//********************************************************************************
//
//  SYSTEM          : DCP
//  UNIT            : アンサ解析処理
//
//********************************************************************************
using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using DCP.MngData.File;
using DCP.MngData.Table;

namespace DCP
{
    ///*******************************************************************************
    ///
    /// CLASS NAME      : アンサ解析クラス
    /// CLASS ID        : CAnalyzeAnswer
    ///
    /// FUNCTION        : 
    /// <summary>
    /// NeXUS受信情報を取得し状態を解析するためのクラスとする。
    /// アンサ系の伝文種別のみを対象とする。
    /// </summary>
    /// 
    ///*******************************************************************************
    class CAnalyzeAnswer : IDisposable
    {
        #region <定数>

        /// <summary>処理起動要求番号</summary>
        private enum REQUESTID : ushort
        {
            /// <summary>スレッド停止処理起動要求</summary>
            THREADSTOP = 0,
            /// <summary>アンサ解析処理起動要求</summary>
            ANSWER,
            /// <summary>起動要求リスト総数</summary>
            REQUESTIDCOUNT,
        };

        #endregion

        #region <メンバ変数>

        /// <summary>スレッドオブジェクト</summary>
        private Thread m_Thread = null;
        /// <summary>スレッド起動フラグ</summary>
        private bool m_IsThreadStarted = false;
        /// <summary>スレッド停止用の待機イベント</summary>
        private AutoResetEvent m_ThreadStopped = null;

        #endregion

        ///*******************************************************************************
        /// MODULE NAME         : コンストラクタ
        /// MODULE ID           : CAnalyzeAnswer
        ///
        /// PARAMETER IN        : 
        /// <param>(in)なし</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// コンストラクタ
        /// </summary>
        ///
        ///*******************************************************************************
        public CAnalyzeAnswer()
        {
            try
            {
                // 起動フラグに「起動」をセット
                m_IsThreadStarted = true;
                // 停止用の待機イベントを生成
                m_ThreadStopped = new AutoResetEvent(false);
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : IDisposableインターフェース
        /// MODULE ID           : Dispose
        ///
        /// PARAMETER IN        : 
        /// <param name="disposing">(in)有効無効フラグ</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// アンマネージ リソースの解放およびリセットに関連付けられているアプリケーション定義のタスクを実行します。
        /// </summary>
        ///
        ///*******************************************************************************
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    m_ThreadStopped.Dispose();          // 待機イベントの破棄およびクリア
                    m_ThreadStopped = null;
                    m_Thread = null;                    // スレッドのクリア
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : IDisposableインターフェース
        /// MODULE ID           : Dispose
        ///
        /// PARAMETER IN        : 
        /// <param>(in)なし</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// アンマネージ リソースの解放およびリセットに関連付けられているアプリケーション定義のタスクを実行します。
        /// </summary>
        ///
        ///*******************************************************************************
        public void Dispose()
        {
            try
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : スレッド開始処理
        /// MODULE ID           : ThreadStart
        ///
        /// PARAMETER IN        : 
        /// <param name="strThreadName">(in)スレッド名称</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// スレッド処理を起動する。
        /// </summary>
        ///
        ///*******************************************************************************
        public void ThreadStart(String strThreadName = "")
        {
            try
            {
                m_Thread = new Thread(new ThreadStart(Main));
                // スレッド名称の設定
                m_Thread.Name = strThreadName;

                // スレッドを起動する
                m_Thread.IsBackground = true;
                m_Thread.Start();
                String logMsg = String.Format("{0}({1})", CAppDat.MESSAGE_THREADSTART, m_Thread.Name);
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.INFORMATION, MethodBase.GetCurrentMethod(), logMsg);
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : スレッド終了処理
        /// MODULE ID           : ThreadEnd
        ///
        /// PARAMETER IN        : 
        /// <param>(in)なし</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// スレッド処理を停止する。
        /// </summary>
        ///
        ///*******************************************************************************
        public void ThreadEnd()
        {
            try
            {
                String logMsg = String.Format("{0}({1})", CAppDat.MESSAGE_THREADEND, m_Thread.Name);
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.INFORMATION, MethodBase.GetCurrentMethod(), logMsg);

                // 起動フラグに「終了」をセット
                m_IsThreadStarted = false;

                // 起動待ちを防止するためシグナル状態をセットする
                m_ThreadStopped.Set();
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : アンサ解析処理
        /// MODULE ID           : Main
        ///
        /// PARAMETER IN        : 
        /// <param>(in)なし</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 制御情報を出力した後、それに応じる制御アンサの受信を監視する。
        /// アンサ対象判断、アンサエリアへの出力や監視時素等の詳細は、制御種別毎に異なる。
        /// </summary>
        ///
        ///*******************************************************************************
        private void Main()
        {
            AutoResetEvent[] WaitEvent = new AutoResetEvent[(UInt32)REQUESTID.REQUESTIDCOUNT];
            Int32 intRequestno = 0;                     // イベント番号

            try
            {
                WaitEvent[(UInt32)REQUESTID.ANSWER] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.ANSWERANALYZE];
                WaitEvent[(UInt32)REQUESTID.THREADSTOP] = m_ThreadStopped;

                while (m_IsThreadStarted)
                {
                    // シグナル状態まで待機する
                    intRequestno = WaitHandle.WaitAny(WaitEvent);

                    // 要求指示の対応処理を実施する
                    switch (intRequestno)
                    {
                        case (Int32)REQUESTID.ANSWER:                   // アンサ解析処理
                            AnalyzeAnswer();
                            break;

                        case (Int32)REQUESTID.THREADSTOP:               // スレッド停止処理
                            break;

                        default:                                        // その他の処理
                            String logMsg = String.Format("想定外要求={0}", intRequestno);
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                            break;
                    }

                    Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : アンサ情報解析処理
        /// MODULE ID           : AnalyzeAnswer
        ///
        /// PARAMETER IN        : 
        /// <param>(in)なし</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// アンサ情報を解析する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void AnalyzeAnswer()
        {
            List<CNxDataAnsMng> AnsList = new List<CNxDataAnsMng>();
            bool bRet = false;
            String logMsg = String.Empty;
            String syncErrorMsg = String.Empty;

            try
            {
                // アンサ情報のデータ取得
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.ANSWERINFO].WaitOne();
                try
                {
                    for (UInt16 cnt = 0; cnt < CAppDat.AnswerMngT.Count; cnt++)
                    {
                        AnsList.Add((CNxDataAnsMng)CAppDat.AnswerMngT[cnt].Clone());
                        CAppDat.AnswerMngT[cnt] = null;
                    }
                    CAppDat.AnswerMngT.Clear();
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.ANSWERINFO].ReleaseMutex();
                }

                // 情報種別毎にアンサ情報数分ループ
                for (UInt16 cnt = 0; cnt < AnsList.Count; cnt++)
                {
                    // 結果の初期化
                    bRet = false;
                    // 情報種別に応じて処理を分岐
                    switch (AnsList[cnt].DataKind)
                    {
                        case (UInt16)CAppDat.NXDATA.RETUBANSYUSEIANSWER:     // 列番修正アンサ情報
                            bRet = CheckRetubanSyusei(AnsList[cnt]);
                            break;

                        case (UInt16)CAppDat.NXDATA.PRCANSWER:               // PRCアンサ情報
                            bRet = CheckPrcAnswer(AnsList[cnt]);
                            break;

                        case (UInt16)CAppDat.NXDATA.SEIGYOANSWER:            // 制御アンサ情報
                            bRet = CheckSeigyoAnswer(AnsList[cnt]);
                            break;

                        default:                                             // 上記以外
                            break;
                    }

                    // 制御の残なしでメインフォームにメッセージを通知
                    UInt16 sendListCount = CCommon.GetSendListCount();
                    if ((0 == sendListCount) && (true == bRet))
                    {
                        bool IsSeigyoAnsFlg = false;
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].WaitOne();
                        try
                        {
                            IsSeigyoAnsFlg = CAppDat.IopcoT.SetAnsFlg(2);
                        }
                        catch (Exception ex)
                        {
                            // ミューテックス取得中に発生した例外の捕捉
                            syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                        }
                        finally
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].ReleaseMutex();
                        }

                        if (true == IsSeigyoAnsFlg)
                        {
                            CCommon.LocalPostMessage(true, CAppDat.WM_USER_ANSWERRECV, IntPtr.Zero, IntPtr.Zero);
                        }
                        else
                        {
                            // ### DEBUG-LOG OUTPUT ###############################################################################################
                            logMsg = String.Format("PostMessageなし 情報種別={0}, アンサフラグ={1}", AnsList[cnt].DataKind, IsSeigyoAnsFlg);
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.INFORMATION, MethodBase.GetCurrentMethod(), logMsg);
                            // ####################################################################################################################
                        }
                    }
                    else
                    {
                        // 処理なし
                    }
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
            finally
            {
                for (UInt16 cnt = 0; cnt < AnsList.Count; cnt++)
                {
                    AnsList[cnt] = null;
                }
                AnsList.Clear();
                AnsList = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 制御アンサ解析処理
        /// MODULE ID           : CheckSeigyoAnswer
        ///
        /// PARAMETER IN        : 
        /// <param name="recvdata">(in)受信データ</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>true=更新あり / false=更新なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 制御アンサ情報を解析し、出力制御管理テーブルを更新する。
        /// </summary>
        ///
        ///*******************************************************************************
        private bool CheckSeigyoAnswer(CNxDataAnsMng recvdata)
        {
            const UInt16 OK_CODE = 0;
            UInt16 datacount = 0;
            UInt16 anskind = 0;
            bool bRet = false;
            bool IsMyNodeData = false;
            bool IsErrorFlg = false;
            String ansCodeText = String.Empty;
            String syncErrorMsg = String.Empty;
            String logMsg = String.Empty;
            IntPtr wParam = IntPtr.Zero;                    // メッセージパラメータ
            String HandlingMsg = String.Empty;              // 扱いメッセ―ジ文字列
            UInt16 ctcsysno = 1;                            //ＣＴＣシステム番号
            bool isAnsClear = false;                        // アンサ表示クリア有無フラグ（true=クリア有、false=クリア無）
            UInt16 allOthers = (UInt16)CAppDat.ALLOTHERS.UNKNOWN;   // 制御送信方法

            try
            {
                // 受信データのユーザーデータありか（最低限個数とヘッダアンサ）？
                if (2 < recvdata.RecvBuffData.Length)
                {
                    // ヘッダアンサコードがＯＫか？
                    if (recvdata.RecvBuffData[1] == OK_CODE)
                    {
                        // アンサ種別を取得
                        anskind = recvdata.Yobi2Value;
                        // 制御情報個数を取得
                        datacount = recvdata.RecvBuffData[0];
                    }
                    else
                    {
                        UInt16 ansKind = recvdata.Yobi2Value;
                        UInt16 ansCode = recvdata.RecvBuffData[1];
                        logMsg = String.Format("制御アンサ情報 種別アンサ=0x{0:X4}, ヘッダアンサコード=0x{1:X4}", ansKind, ansCode);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                        return false;
                    }
                }
                else
                {
                    logMsg = String.Format("制御アンサ情報 データレングス={0}", (recvdata.RecvBuffData.Length + 9));
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                    return false;
                }

                isAnsClear = false;                         // フラグにアンサ表示クリア無しを設定

                // 制御情報個数分ループ
                for (UInt16 i = 0; i < datacount; i++)
                {
                    UInt16 nodeno = (UInt16)((recvdata.RecvBuffData[2 + (i * 6) + 5] & 0xFF00) >> 8);
                    // 受信データのアンサ情報の表示制御卓は自ノード番号か？
                    if (nodeno == CAppDat.DCPSET.MyNode)
                    {
                        // 制御送信方法を取得
                        allOthers = CCommon.GetTekoAllOthers(CAppDat.IopcoT.FuncName);

                        UInt16 ekino = (UInt16)(recvdata.RecvBuffData[2 + (i * 6)]);

                        for (UInt16 cnt = 0; cnt < CAppDat.NodeInfoF.Count; cnt++)
                        {
                            if (recvdata.Node == CAppDat.NodeInfoF[cnt].Node)            //送信先のＣＴＣのノード番号が「ノード情報」テーブルのＣＴＣノード番号と一致する場合
                            {
                                ctcsysno = CAppDat.NodeInfoF[cnt].SysNo;
                                break;
                            }
                        }

                        UInt16 ekiIndex;
                        if (0 != (CAppDat.RecvSoutiKind & 0x00F0))
                        {   // 連動駅の場合
                            ekiIndex = (UInt16)CCommon.GetEkiIndex(CCommon.EKINOTYPE.EKINO_CTC, ekino, ctcsysno);
                        }
                        else
                        {   // CTC駅の場合
                            ekiIndex = (UInt16)CCommon.GetEkiIndex(CCommon.EKINOTYPE.EKINO_CTC, ekino, ctcsysno);
                        }

                        UInt16 gunno = (UInt16)(recvdata.RecvBuffData[2 + (i * 6) + 1]);
                        UInt16 bitno = (UInt16)(recvdata.RecvBuffData[2 + (i * 6) + 2]);
                        UInt16 anscode = (UInt16)(recvdata.RecvBuffData[2 + (i * 6) + 4]);
                        UInt16 jnlno = (UInt16)(recvdata.RecvBuffData[2 + (i * 6) + 5]);
                        // 自ノード情報をセット
                        IsMyNodeData = true;

                        bool IsTargetDataFlg = false;
                        UInt32 SendMngTimeID = 0;
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SENDMANAGE].WaitOne();
                        try
                        {
                            // アンサデータに対応する出力制御情報をループし一致するレコードを削除する
                            for (UInt16 cnt = 0; cnt < (UInt16)CAppDat.SendMngT.Count; cnt++)
                            {
                                // 出力制御の情報種別・ジャーナル通番が一致なら管理テーブルのレコード削除
                                if ((CAppDat.SendMngT[cnt].DataKind == anskind) && (CAppDat.SendMngT[cnt].Jnlno == jnlno))
                                {
                                    IsTargetDataFlg = true;
                                    SendMngTimeID = CAppDat.SendMngT[cnt].TimeID;
                                    HandlingMsg = CAppDat.SendMngT[cnt].HandlingMsg;
                                    // 出力制御アンサテーブルの削除
                                    CAppDat.SendMngT.RemoveAt(cnt);
                                    bRet = true;
                                    break;
                                }
                                else
                                {
                                    // 処理なし
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // ミューテックス取得中に発生した例外の捕捉
                            syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                        }
                        finally
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SENDMANAGE].ReleaseMutex();
                        }

                        if (true == IsTargetDataFlg)
                        {
                            // メインフォームにメッセージを通知
                            wParam = new IntPtr(SendMngTimeID);
                            CCommon.LocalPostMessage(true, CAppDat.WM_USER_TIMESTOP, wParam, IntPtr.Zero);
                        }
                        else
                        {
                            continue;
                        }

                        // エラーアンサコードがＯＫか？
                        if (OK_CODE == anscode)
                        {
                            // 処理なし
                        }
                        else
                        {
                            if (true == IsErrorFlg)
                            {
                                // 一度、出力していれば処理なし
                            }
                            else
                            {
                                //*******************************************************************************
                                // 【アンサ表示エリア更新】

                                if (0 != (CAppDat.RecvSoutiKind & 0x00F0))
                                {   // 連動駅の場合
                                    ansCodeText = this.GetRENDOAnsCodeText(anskind, anscode);
                                }
                                else
                                {   // CTC駅の場合
                                    ansCodeText = this.GetCTCAnsCodeText(anskind, anscode);
                                }

                                if ((UInt16)CAppDat.DISPLAYANSWER.ANSWER_MESSAGEBOX == CAppDat.TypeF.DisplayAnswer)
                                {
                                    if (String.Empty == ansCodeText)
                                    {
                                        // 反応文字列が空文字の場合はメッセージボックスは表示しない
                                    }
                                    else
                                    {
                                        // メッセージボックスに反応文字列を表示
                                        String messageText = String.Format("{0}\r\n\r\n{1}", HandlingMsg, ansCodeText);
                                        CCommon.ShowMessageBox(CFormMessage.ICONIMAGE.WARNING, CAppDat.DCPMSG_RESULTTITLE, messageText);
                                        // 再考音の鳴動要求セット
                                        CKeihouCom.OutputMeidou((Byte)CMeidouReqMng.ACTKIND.RECONSIDER, (Byte)0, "", 0, 0);
                                    }
                                }
                                else if ((UInt16)CAppDat.DISPLAYANSWER.GUIDANCE_ANSWER_AREA == CAppDat.TypeF.DisplayAnswer)
                                {
                                    // ガイダンス表示エリアのアンサ表示エリアに反応文字列を表示
                                    CAppDat.AreaKanriT.SetAreaLabelText(CAreaUserControl.AREANAME_GUIDANCE, (UInt16)CAreaMng.GUIDELABELNO.ANSWER, ansCodeText);
                                    // 再考音の鳴動要求セット
                                    CKeihouCom.OutputMeidou((Byte)CMeidouReqMng.ACTKIND.RECONSIDER, (Byte)0, "", 0, 0);
                                }
                                else if ((UInt16)CAppDat.DISPLAYANSWER.ATSUKAI_ALARM_AREA == CAppDat.TypeF.DisplayAnswer)
                                {
                                    // 扱い警報出力
                                    CKeihouCom.OutputAtsukaiKeihou(anskind, anscode, 0xFFFF);
                                }
                                else if ((UInt16)CAppDat.DISPLAYANSWER.ALARM_AREA == CAppDat.TypeF.DisplayAnswer)
                                {
                                    // 扱い警報出力
                                    CKeihouCom.OutputAtsukaiKeihou(anskind, anscode, 0xFFFF);
                                }
                                else
                                {
                                    // 処理なし
                                }
                                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].WaitOne();
                                try
                                {
                                    CAppDat.IopcoT.AnsErrFlg = 1;               // エラー発生を記憶
                                }
                                catch (Exception ex)
                                {
                                    // ミューテックス取得中に発生した例外の捕捉
                                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                                }
                                finally
                                {
                                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].ReleaseMutex();
                                }
                            }

                            if ((UInt16)CAppDat.DISPLAYANSWER.ATSUKAI_ALARM_AREA == CAppDat.TypeF.DisplayAnswer)
                            {
                                // 扱い警報エリアに出力する場合、操作履歴に操作アンサを出力しない
                            }
                            else if ((UInt16)CAppDat.DISPLAYANSWER.ALARM_AREA == CAppDat.TypeF.DisplayAnswer)
                            {
                                // 警報エリアに出力する場合、操作履歴に操作アンサを出力しない
                            }
                            else
                            {
                                // ▼▼▼【操作履歴対応】▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
                                // 制御アンサ情報に対する操作履歴出力に対応

                                CSousaMng rirekiData = new CSousaMng();
                                // 操作履歴データの登録
                                rirekiData.FuncName = CAppDat.FuncName[(UInt16)CAppDat.FUNCID.CONTROL_ANSWER];      // 機能名称

                                CSousaTekoMng TekoData = new CSousaTekoMng();
                                TekoData.Ekino = ekiIndex;                                    // 所属駅番号(駅番号変換設定定数インデックス)
                                String TekoName = String.Empty;

                                if (0 != (CAppDat.RecvSoutiKind & 0x00F0))
                                {
                                    // てこ／表示情報の場合

                                    foreach (CBitDataMng bitData in CAppDat.BitF.Code.Values)
                                    {
                                        // 登録キーの駅番号、区分番号、ビット番号、状態情報番号（０固定）が一致？
                                        if ((bitData.Eki == ekino) && (bitData.Kubun == gunno) && (bitData.Bit == bitno) && (bitData.Status == ctcsysno))
                                        {
                                            TekoName = bitData.Name;        // 一致した名称を取得
                                            break;
                                        }
                                        else
                                        {
                                            // 処理なし
                                        }
                                    }
                                }
                                else
                                {
                                    // ＣＴＣ状態情報の場合
                                    foreach (CBitDataMng bitData in CAppDat.BitF.Code.Values)
                                    {
                                        // 登録キーの状態情報番号（てこステータスの場合、状態情報番号は０）、駅番号、区分番号、ビット番号が一致？
                                        if ((bitData.Status == 0) && (bitData.Eki == ekino) && (bitData.Kubun == gunno) && (bitData.Bit == bitno))
                                        {
                                            TekoName = bitData.Name;        // 一致した名称を取得
                                            break;
                                        }
                                        else
                                        {
                                            // 処理なし
                                        }
                                    }
                                }
                                TekoData.Name = TekoName;                                   // てこ名称
                                rirekiData.Teko.Add(TekoData);                              // 選択てこ
                                TekoData = null;
                                rirekiData.AnswerCode = ansCodeText;                        // 扱いアンサ

                                // 操作履歴のデータセット
                                CCommon.AddSousaRirekiT(rirekiData);

                                // 操作履歴ログ出力処理
                                CKeihouCom.OutputOpeRireki();
                                rirekiData = null;
                            }

                            if ((UInt16)CAppDat.ALLOTHERS.ALLNOTSEND == allOthers)
                            {
                                IsErrorFlg = true;
                            }
                            else
                            {
                                // 情報ごとに出力する
                            }
                            logMsg = String.Format("制御アンサ 駅番号=0x{0:X2}, 群番号=0x{1:X2}, ﾋﾞｯﾄ番号=0x{2:X2}, ｱﾝｻｺｰﾄﾞ=0x{3:X4}, ｼﾞｬｰﾅﾙ通番=0x{4:X4}", ekino, gunno, bitno, anscode, jnlno);
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                        }
                    }
                    else
                    {
                        // 処理なし
                    }
                }

                // エラーデータなし、且つ自ノード情報か？
                if ((false == IsErrorFlg) && (true == IsMyNodeData))
                {
                    //*******************************************************************************
                    // 【アンサ表示エリア更新】
                    if (0 == CCommon.GetSendListCount())
                    {
                        bool sisyou = false;
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DCPSTATUS].WaitOne();
                        try
                        {
                            sisyou = CAppDat.DcpStatusT.SisyoState;
                        }
                        catch (Exception ex)
                        {
                            // ミューテックス取得中に発生した例外の捕捉
                            syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                        }
                        finally
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DCPSTATUS].ReleaseMutex();
                        }

                        String FuncName = String.Empty;
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].WaitOne();
                        try
                        {
                            FuncName = CAppDat.IopcoT.FuncName;
                        }
                        catch (Exception ex)
                        {
                            // ミューテックス取得中に発生した例外の捕捉
                            syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                        }
                        finally
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].ReleaseMutex();
                        }

                        if ((true == sisyou) && ((UInt16)CAppDat.ALLOTHERS.ALLNOTSEND == allOthers))
                        {
                            // 処理なし
                        }
                        else
                        {
                            if (0 != (CAppDat.RecvSoutiKind & 0x00F0))
                            {   // 連動駅の場合
                                ansCodeText = this.GetRENDOAnsCodeText(anskind, 0x0000);
                            }
                            else
                            {   // CTC駅の場合
                                ansCodeText = this.GetCTCAnsCodeText(anskind, 0x0000);
                            }
                        
                            if ((UInt16)CAppDat.DISPLAYANSWER.ANSWER_MESSAGEBOX == CAppDat.TypeF.DisplayAnswer)
                            {
                                // 処理なし
                            }
                            else if ((UInt16)CAppDat.DISPLAYANSWER.GUIDANCE_ANSWER_AREA == CAppDat.TypeF.DisplayAnswer)
                            {
                                if (true == isAnsClear)
                                {
                                    // ガイダンス表示エリアのアンサ表示エリアに反応文字列を表示
                                    CAppDat.AreaKanriT.SetAreaLabelText(CAreaUserControl.AREANAME_GUIDANCE, (UInt16)CAreaMng.GUIDELABELNO.ANSWER, ansCodeText);
                                }
                                else
                                {
                                    // 処理なし
                                }
                            }
                            else if ((UInt16)CAppDat.DISPLAYANSWER.ATSUKAI_ALARM_AREA == CAppDat.TypeF.DisplayAnswer)
                            {
                                // 扱い警報出力
                                CKeihouCom.OutputAtsukaiKeihou(anskind, 0x0000, 0xFFFF);
                            }
                            else if ((UInt16)CAppDat.DISPLAYANSWER.ALARM_AREA == CAppDat.TypeF.DisplayAnswer)
                            {
                                // 扱い警報出力
                                CKeihouCom.OutputAtsukaiKeihou(anskind, 0x0000, 0xFFFF);
                            }
                            else
                            {
                                // 処理なし
                            }
                        }
                    }
                    else
                    {
                        // 処理なし
                    }
                }
                else
                {
                    // 処理なし
                }

                // 待機ハンドルをシグナルにセット
                wParam = new IntPtr((UInt16)CAppDat.UPDATEREQUEST.GUIDANCEUPDATE);
                CCommon.LocalPostMessage(true, CAppDat.WM_USER_MAINFORMUPDATE, wParam, IntPtr.Zero);

                return bRet;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                return false;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 列番修正アンサ解析処理
        /// MODULE ID           : CheckRetubanSyusei
        ///
        /// PARAMETER IN        : 
        /// <param name="recvdata">(in)受信データ</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>true=更新あり / false=更新なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 列番修正アンサ情報を解析し、出力制御管理テーブルを更新する。
        /// </summary>
        ///
        ///*******************************************************************************
        private bool CheckRetubanSyusei(CNxDataAnsMng recvdata)
        {
            const UInt16 RETUBANSYUUSEI_DATALENGTH = 1;
            const UInt16 OK_CODE = 0;
            bool bRet = false;
            String syncErrorMsg = String.Empty;
            String logMsg = String.Empty;
            IntPtr wParam = IntPtr.Zero;                    // メッセージパラメータ
            String HandlingMsg = String.Empty;              // 扱いメッセ―ジ文字列

            try
            {
                // 受信データのデータサイズは既定値か？
                if (recvdata.RecvBuffData.Length == RETUBANSYUUSEI_DATALENGTH)
                {
                    UInt16 nodeno = (UInt16)((recvdata.RecvBuffData[0] & 0xFF00) >> 8);
                    // 受信データのアンサ情報の表示制御卓番号が一致か？
                    if (nodeno == CAppDat.DCPSET.DcpNo)
                    {
                        // 待機ハンドルをシグナルにセット
                        //[未使用] CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.GUIDANCEDISP].Set();
                        wParam = new IntPtr((UInt16)CAppDat.UPDATEREQUEST.GUIDANCEUPDATE);
                        CCommon.LocalPostMessage(true, CAppDat.WM_USER_MAINFORMUPDATE, wParam, IntPtr.Zero);

                        bool IsTargetDataFlg = false;
                        UInt32 SendMngTimeID = 0;
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SENDMANAGE].WaitOne();
                        try
                        {
                            // アンサデータに対応する出力制御情報をループし一致するレコードを削除する
                            for (UInt16 cnt = 0; cnt < (UInt16)CAppDat.SendMngT.Count; cnt++)
                            {
                                // 出力制御が列番修正情報なら管理テーブルのレコード削除
                                if (CAppDat.SendMngT[cnt].DataKind == (UInt16)CAppDat.NXDATA.RETUBANSYUSEI)
                                {
                                    IsTargetDataFlg = true;
                                    SendMngTimeID = CAppDat.SendMngT[cnt].TimeID;
                                    HandlingMsg = CAppDat.SendMngT[cnt].HandlingMsg;
                                    // 出力制御アンサテーブルの削除
                                    CAppDat.SendMngT.RemoveAt(cnt);
                                    bRet = true;
                                    break;
                                }
                                else
                                {
                                    // 処理なし
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // ミューテックス取得中に発生した例外の捕捉
                            syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                        }
                        finally
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SENDMANAGE].ReleaseMutex();
                        }

                        UInt16 anscode = (UInt16)(recvdata.RecvBuffData[0] & 0x00FF);
                        //*******************************************************************************
                        // 【アンサ表示エリア更新】
                        String ansCodeText = this.GetRETUAnsCodeText(anscode);
                        if ((UInt16)CAppDat.DISPLAYANSWER.ANSWER_MESSAGEBOX == CAppDat.TypeF.DisplayAnswer)
                        {
                            if (String.Empty == ansCodeText)
                            {
                                // 反応文字列が空文字の場合はメッセージボックスは表示しない
                            }
                            else
                            {
                                // メッセージボックスに反応文字列を表示
                                String messageText = String.Format("{0}\r\n\r\n{1}", HandlingMsg, ansCodeText);
                                CCommon.ShowMessageBox(CFormMessage.ICONIMAGE.WARNING, CAppDat.DCPMSG_RESULTTITLE, messageText);

                                //*******************************************************************************
                                // 再考音の鳴動要求セット
                                CKeihouCom.OutputMeidou((Byte)CMeidouReqMng.ACTKIND.RECONSIDER, (Byte)0, "", 0, 0);
                                //*******************************************************************************
                            }
                        }
                        else if ((UInt16)CAppDat.DISPLAYANSWER.GUIDANCE_ANSWER_AREA == CAppDat.TypeF.DisplayAnswer)
                        {
                            // ガイダンス表示エリアのアンサ表示エリアに反応文字列を表示
                            CAppDat.AreaKanriT.SetAreaLabelText(CAreaUserControl.AREANAME_GUIDANCE, (UInt16)CAreaMng.GUIDELABELNO.ANSWER, ansCodeText);

                            //*******************************************************************************
                            // 再考音の鳴動要求セット
                            CKeihouCom.OutputMeidou((Byte)CMeidouReqMng.ACTKIND.RECONSIDER, (Byte)0, "", 0, 0);
                            //*******************************************************************************
                        }
                        else if ((UInt16)CAppDat.DISPLAYANSWER.ATSUKAI_ALARM_AREA == CAppDat.TypeF.DisplayAnswer)
                        {	
                            // 扱い警報出力
                            CKeihouCom.OutputAtsukaiKeihou(recvdata.DataKind, anscode);
                        }
                        else if ((UInt16)CAppDat.DISPLAYANSWER.ALARM_AREA == CAppDat.TypeF.DisplayAnswer)
                        {
                            // 扱い警報出力
                            CKeihouCom.OutputAtsukaiKeihou(recvdata.DataKind, anscode);
                        }
                        else
                        {
                            // 処理なし
                        }

                        if (true == IsTargetDataFlg)
                        {
                            // メインフォームにメッセージを通知
                            wParam = new IntPtr(SendMngTimeID);
                            CCommon.LocalPostMessage(true, CAppDat.WM_USER_TIMESTOP, wParam, IntPtr.Zero);
                        }
                        else
                        {
                            // 処理なし
                        }

                        if ((UInt16)CAppDat.DISPLAYANSWER.ATSUKAI_ALARM_AREA == CAppDat.TypeF.DisplayAnswer)
                        {
                            // 扱い警報エリアに出力する場合、操作履歴に操作アンサを出力しない
                        }
                        else if ((UInt16)CAppDat.DISPLAYANSWER.ALARM_AREA == CAppDat.TypeF.DisplayAnswer)
                        {
                            // 警報エリアに出力する場合、操作履歴に操作アンサを出力しない
                        }
                        else
                        {
                            // 受信データのアンサ情報のアンサコードは正常か？
                            if (OK_CODE == anscode)
                            {
                                // 処理なし
                            }
                            // 受信データのアンサ情報のアンサコードは正常以外か？
                            else
                            {
                                //*******************************************************************************
                                // 再考音の鳴動要求セット
                                CKeihouCom.OutputMeidou((Byte)CMeidouReqMng.ACTKIND.RECONSIDER, (Byte)0, "", 0, 0);
                                //*******************************************************************************

                                // ▼▼▼【操作履歴対応】▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
                                // 列番修正アンサ情報に対する操作履歴出力に対応

                                CSousaMng rirekiData = new CSousaMng();
                                // 操作履歴データの登録
                                rirekiData.FuncName = CAppDat.FuncName[(UInt16)CAppDat.FUNCID.OPERATION_ANSWER];    // 機能名称
                                rirekiData.JudgeName = "自動進路";                          // アンサ判断装置
                                rirekiData.AnswerCode = ansCodeText;                        // 扱いアンサ

                                // 操作履歴のデータセット
                                CCommon.AddSousaRirekiT(rirekiData);

                                // 操作履歴ログ出力処理
                                CKeihouCom.OutputOpeRireki();
                                rirekiData = null;

                                logMsg = String.Format("列番修正アンサ情報 アンサコード=0x{0:X4}, ノード番号={1}", anscode, nodeno);
                                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                            }
                        }
                    }
                    else
                    {
                        // 処理なし
                    }
                }
                else
                {
                    // 処理なし
                }
                return bRet;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                return false;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : PRCアンサ解析処理
        /// MODULE ID           : CheckPrcAnswer
        ///
        /// PARAMETER IN        : 
        /// <param name="recvdata">(in)受信データ</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>true=更新あり / false=更新なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// PRCアンサ情報を解析し、出力制御管理テーブルを更新する。
        /// </summary>
        ///
        ///*******************************************************************************
        private bool CheckPrcAnswer(CNxDataAnsMng recvdata)
        {
            const UInt16 PRCANSWER_DATALENGTH = 1;
            const UInt16 OK_CODE = 0;
            bool bRet = false;
            String syncErrorMsg = String.Empty;
            String logMsg = String.Empty;
            IntPtr wParam = IntPtr.Zero;                    // メッセージパラメータ
            String HandlingMsg = String.Empty;              // 扱いメッセ―ジ文字列

            try
            {
                // 受信データのデータサイズは既定値か？
                if (recvdata.RecvBuffData.Length == PRCANSWER_DATALENGTH)
                {
                    UInt16 nodeno = (UInt16)((recvdata.RecvBuffData[0] & 0xFF00) >> 8);
                    // 受信データのアンサ情報の表示制御卓番号が一致か？
                    if (nodeno == CAppDat.DCPSET.DcpNo)
                    {
                        // 待機ハンドルをシグナルにセット
                        //[未使用] CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.GUIDANCEDISP].Set();
                        wParam = new IntPtr((UInt16)CAppDat.UPDATEREQUEST.GUIDANCEUPDATE);
                        CCommon.LocalPostMessage(true, CAppDat.WM_USER_MAINFORMUPDATE, wParam, IntPtr.Zero);

                        bool IsTargetDataFlg = false;
                        UInt32 SendMngTimeID = 0;
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SENDMANAGE].WaitOne();
                        try
                        {
                            // アンサデータに対応する出力制御情報をループし一致するレコードを削除する
                            for (UInt16 cnt = 0; cnt < (UInt16)CAppDat.SendMngT.Count; cnt++)
                            {
                                // 出力制御がPRCアンサ情報なら管理テーブルのレコード削除
                                if (CAppDat.SendMngT[cnt].DataKind == (UInt16)CAppDat.NXDATA.PRCTUUCHI)
                                {
                                    IsTargetDataFlg = true;
                                    SendMngTimeID = CAppDat.SendMngT[cnt].TimeID;
                                    HandlingMsg = CAppDat.SendMngT[cnt].HandlingMsg;
                                    // 出力制御アンサテーブルの削除
                                    CAppDat.SendMngT.RemoveAt(cnt);
                                    bRet = true;
                                    break;
                                }
                                else
                                {
                                    // 処理なし
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // ミューテックス取得中に発生した例外の捕捉
                            syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                        }
                        finally
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SENDMANAGE].ReleaseMutex();
                        }

                        UInt16 anscode = (UInt16)(recvdata.RecvBuffData[0] & 0x00FF);
                        //*******************************************************************************
                        // 【アンサ表示エリア更新】
                        String ansCodeText = this.GetPRCAnsCodeText(anscode);
                        if ((UInt16)CAppDat.DISPLAYANSWER.ANSWER_MESSAGEBOX == CAppDat.TypeF.DisplayAnswer)
                        {
                            if (String.Empty == ansCodeText)
                            {
                                // 反応文字列が空文字の場合はメッセージボックスは表示しない
                            }
                            else
                            {
                                // メッセージボックスに反応文字列を表示
                                String messageText = String.Format("{0}\r\n\r\n{1}", HandlingMsg, ansCodeText);
                                CCommon.ShowMessageBox(CFormMessage.ICONIMAGE.WARNING, CAppDat.DCPMSG_RESULTTITLE, messageText);

                                //*******************************************************************************
                                // 再考音の鳴動要求セット
                                CKeihouCom.OutputMeidou((Byte)CMeidouReqMng.ACTKIND.RECONSIDER, (Byte)0, "", 0, 0);
                                //*******************************************************************************
                            }
                        }
                        else if ((UInt16)CAppDat.DISPLAYANSWER.GUIDANCE_ANSWER_AREA == CAppDat.TypeF.DisplayAnswer)
                        {
                            // ガイダンス表示エリアのアンサ表示エリアに反応文字列を表示
                            CAppDat.AreaKanriT.SetAreaLabelText(CAreaUserControl.AREANAME_GUIDANCE, (UInt16)CAreaMng.GUIDELABELNO.ANSWER, ansCodeText);

                            //*******************************************************************************
                            // 再考音の鳴動要求セット
                            CKeihouCom.OutputMeidou((Byte)CMeidouReqMng.ACTKIND.RECONSIDER, (Byte)0, "", 0, 0);
                            //*******************************************************************************
                        }
                        else if ((UInt16)CAppDat.DISPLAYANSWER.ATSUKAI_ALARM_AREA == CAppDat.TypeF.DisplayAnswer)
                        {
                            // 扱い警報出力
                            CKeihouCom.OutputAtsukaiKeihou(recvdata.DataKind, anscode, 0xFFFF);
                        }
                        else if ((UInt16)CAppDat.DISPLAYANSWER.ALARM_AREA == CAppDat.TypeF.DisplayAnswer)
                        {
                            // 扱い警報出力
                            CKeihouCom.OutputAtsukaiKeihou(recvdata.DataKind, anscode, 0xFFFF);
                        }
                        else
                        {
                            // 処理なし
                        }

                        if (true == IsTargetDataFlg)
                        {
                            // メインフォームにメッセージを通知
                            wParam = new IntPtr(SendMngTimeID);
                            CCommon.LocalPostMessage(true, CAppDat.WM_USER_TIMESTOP, wParam, IntPtr.Zero);
                        }
                        else
                        {
                            // 処理なし
                        }

                        if ((UInt16)CAppDat.DISPLAYANSWER.ATSUKAI_ALARM_AREA == CAppDat.TypeF.DisplayAnswer)
                        {
                            // 扱い警報エリアに出力する場合、操作履歴に操作アンサを出力しない
                        }
                        else if ((UInt16)CAppDat.DISPLAYANSWER.ALARM_AREA == CAppDat.TypeF.DisplayAnswer)
                        {
                            // 警報エリアに出力する場合、操作履歴に操作アンサを出力しない
                        }
                        else
                        {
                            // 受信データのアンサ情報のアンサコードは正常か？
                            if (OK_CODE == anscode)
                            {
                                // 処理なし
                            }
                            else
                            {
                                //*******************************************************************************
                                // 再考音の鳴動要求セット
                                CKeihouCom.OutputMeidou((Byte)CMeidouReqMng.ACTKIND.RECONSIDER, (Byte)0, "", 0, 0);
                                //*******************************************************************************

                                // ▼▼▼【操作履歴対応】▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
                                // PRCアンサ情報に対する操作履歴出力に対応

                                CSousaMng rirekiData = new CSousaMng();
                                // 操作履歴データの登録
                                rirekiData.FuncName = CAppDat.FuncName[(UInt16)CAppDat.FUNCID.OPERATION_ANSWER];    // 機能名称
                                rirekiData.JudgeName = "自動進路";                          // アンサ判断装置
                                rirekiData.AnswerCode = ansCodeText;                        // 扱いアンサ

                                // 操作履歴のデータセット
                                CCommon.AddSousaRirekiT(rirekiData);

                                // 操作履歴ログ出力処理
                                CKeihouCom.OutputOpeRireki();
                                rirekiData = null;

                                logMsg = String.Format("PRCアンサ情報 アンサコード=0x{0:X4}, ノード番号={1}", anscode, nodeno);
                                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                            }
                        }
                    }
                    else
                    {
                        // 処理なし
                    }
                }
                else
                {
                    // 処理なし
                }
                return bRet;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                return false;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : CTCアンサコード文字列取得処理
        /// MODULE ID           : GetCTCAnsCodeText
        ///
        /// PARAMETER IN        : 
        /// <param name="dataKind">(in)情報種別</param>
        /// <param name="ansCode">(in)アンサコード</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>アンサコードと一致した文字列（不一致の場合は"ＮＧ"）</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 制御アンサ情報のアンサコードを解析して、対応する文字列（アンサ文字列）を返す。
        /// </summary>
        ///
        ///*******************************************************************************
        private String GetCTCAnsCodeText(UInt16 dataKind, UInt16 ansCode)
        {
            String AnsCodeText = CAppDat.MESSAGE_OPERATIONERROR;
            CAnsDataMng AnsData = null;

            try
            {
                for (UInt16 cnt1 = 0; cnt1 < CAppDat.CtcansF.Count; cnt1++)
                {
                    // 情報種別が一致か？
                    if (dataKind == CAppDat.CtcansF[cnt1].DataKind)
                    {
                        for (UInt16 cnt2 = 0; cnt2 < CAppDat.CtcansF[cnt1].AnsCode.Count; cnt2++)
                        {
                            // アンサ情報を取得してアンサコードを比較する
                            // 一致した場合はアンサ文字列を取得する
                            AnsData = CAppDat.CtcansF[cnt1].AnsCode[cnt2];
                            if (ansCode == AnsData.CodeData)
                            {
                                AnsCodeText = AnsData.Moji;
                                break;
                            }
                            else
                            {
                                // アンサコードに任意のコードが存在するため
                                // チェック条件に定義値と受信コードをマスクした結果が定義値と一致していることを追加
                                // 但し、定義コードが "0000"（=正常）のときは除外する
                                if ((ansCode == (ansCode & AnsData.CodeData)) && (0x0000 != AnsData.CodeData))
                                {
                                    AnsCodeText = AnsData.Moji;
                                    break;
                                }
                                else
                                {
                                    // 処理なし
                                }
                            }
                        }
                        break;
                    }
                    // 情報種別が不一致か？
                    else
                    {
                        // 処理なし
                    }
                }
                return AnsCodeText;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                return String.Empty;
            }
            finally
            {
                AnsData = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 連動アンサコード文字列取得処理
        /// MODULE ID           : GetRENDOAnsCodeText
        ///
        /// PARAMETER IN        : 
        /// <param name="dataKind">(in)情報種別</param>
        /// <param name="ansCode">(in)アンサコード</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>アンサコードと一致した文字列（不一致の場合は"ＮＧ"）</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 制御アンサ情報のアンサコードを解析して、対応する文字列（アンサ文字列）を返す。
        /// </summary>
        ///
        ///*******************************************************************************
        private String GetRENDOAnsCodeText(UInt16 dataKind, UInt16 ansCode)
        {
            String AnsCodeText = CAppDat.MESSAGE_OPERATIONERROR;
            CAnsDataMng AnsData = null;

            try
            {
                for (UInt16 cnt1 = 0; cnt1 < CAppDat.RendoansF.Count; cnt1++)
                {
                    // 情報種別が一致か？
                    if (dataKind == CAppDat.RendoansF[cnt1].DataKind)
                    {
                        for (UInt16 cnt2 = 0; cnt2 < CAppDat.RendoansF[cnt1].AnsCode.Count; cnt2++)
                        {
                            // アンサ情報を取得してアンサコードを比較する
                            // 一致した場合はアンサ文字列を取得する
                            AnsData = CAppDat.RendoansF[cnt1].AnsCode[cnt2];
                            if (ansCode == AnsData.CodeData)
                            {
                                AnsCodeText = AnsData.Moji;
                                break;
                            }
                            else
                            {
                                // アンサコードに任意のコードが存在するため
                                // チェック条件に定義値と受信コードをマスクした結果が定義値と一致していることを追加
                                // 但し、定義コードが "0000"（=正常）のときは除外する
                                if ((ansCode == (ansCode & AnsData.CodeData)) && (0x0000 != AnsData.CodeData))
                                {
                                    AnsCodeText = AnsData.Moji;
                                    break;
                                }
                                else
                                {
                                    // 処理なし
                                }
                            }
                        }
                        break;
                    }
                    // 情報種別が不一致か？
                    else
                    {
                        // 処理なし
                    }
                }
                return AnsCodeText;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                return String.Empty;
            }
            finally
            {
                AnsData = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 列番修正アンサコード文字列取得処理
        /// MODULE ID           : GetRETUAnsCodeText
        ///
        /// PARAMETER IN        : 
        /// <param name="ansCode">(in)アンサコード</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>アンサコードと一致した文字列（不一致の場合は"ＮＧ"）</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 列番修正アンサ情報のアンサコードを解析して、対応する文字列（アンサ文字列）を返す。
        /// </summary>
        ///
        ///*******************************************************************************
        private String GetRETUAnsCodeText(UInt16 ansCode)
        {
            String AnsCodeText = CAppDat.MESSAGE_OPERATIONERROR;
            CAnsDataMng AnsData = null;

            try
            {
                for (UInt16 cnt = 0; cnt < CAppDat.PrcansF.Retuans.Count; cnt++)
                {
                    // アンサ情報を取得してアンサコードを比較する
                    // 一致した場合はアンサ文字列を取得する
                    AnsData = CAppDat.PrcansF.Retuans[cnt];
                    if (ansCode == AnsData.CodeData)
                    {
                        AnsCodeText = AnsData.Moji;
                        break;
                    }
                    else
                    {
                        // 処理なし
                    }
                }
                return AnsCodeText;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                return String.Empty;
            }
            finally
            {
                AnsData = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : PRCアンサコード文字列取得処理
        /// MODULE ID           : GetPRCAnsCodeText
        ///
        /// PARAMETER IN        : 
        /// <param name="ansCode">(in)アンサコード</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>アンサコードと一致した文字列（不一致の場合は"ＮＧ"）</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// PRCアンサ情報のアンサコードを解析して、対応する文字列（アンサ文字列）を返す。
        /// </summary>
        ///
        ///*******************************************************************************
        private String GetPRCAnsCodeText(UInt16 ansCode)
        {
            String AnsCodeText = CAppDat.MESSAGE_OPERATIONERROR;
            CAnsDataMng AnsData = null;

            try
            {
                for (UInt16 cnt = 0; cnt < CAppDat.PrcansF.Prcans.Count; cnt++)
                {
                    // アンサ情報を取得してアンサコードを比較する
                    // 一致した場合はアンサ文字列を取得する
                    AnsData = CAppDat.PrcansF.Prcans[cnt];
                    if (ansCode == AnsData.CodeData)
                    {
                        AnsCodeText = AnsData.Moji;
                        break;
                    }
                    else
                    {
                        // 処理なし
                    }
                }
                return AnsCodeText;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                return String.Empty;
            }
            finally
            {
                AnsData = null;
            }
        }
    }
}
