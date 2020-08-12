//********************************************************************************
//
//  SYSTEM          : DCP
//  UNIT            : タイマー解析処理
//
//********************************************************************************
using System;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;
using DCP.MngData.Table;

namespace DCP
{
    ///*******************************************************************************
    ///
    /// CLASS NAME      : タイマー解析クラス
    /// CLASS ID        : CAnalyzeTimer
    ///
    /// FUNCTION        : 
    /// <summary>
    /// 固定タイマーと可変タイマーのタイマーインデックスに従い、
    /// タイムアウト時に対応する処理を行う。
    /// </summary>
    ///
    /// 
    ///*******************************************************************************
    class CAnalyzeTimer
    {
        #region <定数>

        /// <summary>可変タイマー種別::重警報</summary>
        private const Byte VARTIME_JYUKEIHOU = 1;
        /// <summary>可変タイマー種別::軽警報</summary>
        private const Byte VARTIME_KEIKEIHOU = 2;
        /// <summary>可変タイマー種別::回復</summary>
        private const Byte VARTIME_KAIFUKU = 3;
        /// <summary>可変タイマー種別::情報</summary>
        private const Byte VARTIME_INFORMATION = 4;
        /// <summary>可変タイマー種別::その他</summary>
        private const Byte VARTIME_OTHER = 5;
        /// <summary>可変タイマー種別::現場要請</summary>
        private const Byte VARTIME_GENBAYOUSEI = 6;
        /// <summary>可変タイマー種別::接近</summary>
        private const Byte VARTIME_SEKKIN = 7;
        /// <summary>可変タイマー種別::提案</summary>
        private const Byte VARTIME_TEIAN = 8;
        /// <summary>可変タイマー種別::提案自動拒否</summary>
        private const Byte VARTIME_TEIANKYOHI = 9;

        /// <summary>可変タイマー種別::受信電文頁監視</summary>
        //private const Byte VARTIME_RECVPAGEKANSI = 10;
        private const Byte VARTIME_RECVPAGEKANSI = 11;

        /// <summary>可変タイマー種別::扱い警報</summary>
        private const Byte VARTIME_ATSUKAIKEIHOU = 10;

        /// <summary>固定タイマーの停止値</summary>
        private const UInt16 TIMERSTOP = 0xFFFF;

        /// <summary>現在日時文字列のフォーマット</summary>
        private const String RIREKIRENEW_FORMAT = "yyyy/MM/dd";

        /// <summary>装置状態不能</summary>
        private const Byte EQPSTATE_UNABLE = 0x02;

        #endregion

        #region <メンバ変数>

        /// <summary>時刻切替フラグ</summary>
        private bool m_IsDateTimeChanged = false;
        /// <summary>固定タイマーリスト</summary>
        private List<CTimerCtrl> m_FixTimeList = null;
        /// <summary>固定タイマー同期オブジェクト</summary>
        private Object m_FixTimeSyncObject = null; 

        /// <summary>可変タイマーリスト</summary>
        private List<CTimerCtrl> m_VarTimeList = null;
        /// <summary>可変タイマー同期オブジェクト</summary>
        private Object m_VarTimeSyncObject = null; 

        /// <summary>メモリ監視用タイマーカウンタ</summary>
        private UInt32 m_memWatchTmCount = 0;

        #endregion

        ///*******************************************************************************
        /// MODULE NAME         : コンストラクタ
        /// MODULE ID           : CAnalyzeTimer
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
        public CAnalyzeTimer()
        {
            try
            {
                m_IsDateTimeChanged = false;

                m_FixTimeSyncObject = new Object();
                m_FixTimeList = new List<CTimerCtrl>();
                m_FixTimeList.Clear();

                m_VarTimeSyncObject = new Object();
                m_VarTimeList = new List<CTimerCtrl>();
                m_VarTimeList.Clear();
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 固定タイマー停止処理
        /// MODULE ID           : FixTimeStopTimer
        ///
        /// PARAMETER IN        : 
        /// <param name="tmIndex">(in)固定タイマーインデックス</param>
        /// <param name="tmCount">(in)固定タイマー値</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 固定タイマー（２段階目）の管理情報のクリア、およびタイマーの破棄を行う。
        /// 破棄対象のタイマーの場合、固定タイマー解析処理を実施する。
        /// </summary>
        /// 
        ///*******************************************************************************
        private void FixTimeStopTimer(UInt16 tmIndex, UInt16 tmCount)
        {
            String syncErrorMsg = String.Empty;

            try
            {
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.FIXTIMER].WaitOne();
                try
                {
                    lock (m_FixTimeSyncObject)
                    {
                        for (UInt16 cnt = 0; cnt < (UInt16)m_FixTimeList.Count; cnt++)
                        {
                            if (CAppDat.BtmerT[tmIndex].Timerno == m_FixTimeList[cnt].DataKind)
                            {
                                m_FixTimeList[cnt].Dispose();
                                m_FixTimeList.RemoveAt(cnt);
                                break;
                            }
                            else
                            {
                                // 処理なし
                            }
                        }
                    }
                    CAppDat.BtmerT[tmIndex].Timerno = 0;
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.FIXTIMER].ReleaseMutex();
                }
                this.AnalyzeBaseTimer(tmIndex, tmCount);
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 固定タイマータイムアウト処理
        /// MODULE ID           : FixedTime_Tick
        ///
        /// PARAMETER IN        : 
        /// <param name="sender">(in)発生元コントロール</param>
        /// <param name="e">(in)イベント情報</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 固定タイマー（２段階目）のタイムアウト処理を行う。
        /// </summary>
        /// 
        ///*******************************************************************************
        private void FixedTime_Tick(object sender, System.EventArgs e)
        {
            try
            {
                CTimerCtrl FixedTime = (CTimerCtrl)sender;
                UInt16 TimeIndex = (UInt16)((FixedTime.DataKind & 0xFFFF0000) >> 16);

                TimeIndex = (UInt16)(TimeIndex - 1);
                this.FixTimeStopTimer(TimeIndex, 0);
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 可変タイマー停止処理
        /// MODULE ID           : VarTimeStopTimer
        ///
        /// PARAMETER IN        : 
        /// <param name="tmIndex">(in)可変タイマーインデックス</param>
        /// <param name="tmCount">(in)可変タイマー値</param>
        /// <param name="tmKind">(in)可変タイマー種別</param>
        /// <param name="procKind">(in)処理種別</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 可変タイマー（２段階目）の管理情報のクリア、およびタイマーの破棄を行う。
        /// 破棄対象のタイマーの場合、可変タイマー解析処理を実施する。
        /// </summary>
        /// 
        ///*******************************************************************************
        private void VarTimeStopTimer(UInt16 tmIndex, UInt16 tmCount, Byte tmKind, Byte procKind)
        {
            String syncErrorMsg = String.Empty;

            try
            {
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.TIMERINFO].WaitOne();
                try
                {
                    lock (m_VarTimeSyncObject)
                    {
                        for (UInt16 cnt = 0; cnt < (UInt16)m_VarTimeList.Count; cnt++)
                        {
                            if (CAppDat.TimerAryT[tmIndex].Timerno == m_VarTimeList[cnt].DataKind)
                            {
                                m_VarTimeList[cnt].Dispose();
                                m_VarTimeList.RemoveAt(cnt);
                                break;
                            }
                            else
                            {
                                // 処理なし
                            }
                        }
                    }
                    CAppDat.TimerAryT[tmIndex].Timerno = 0;
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.TIMERINFO].ReleaseMutex();
                }
                this.AnalyzeVariableTimer(tmIndex, tmCount, tmKind, procKind);
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 可変タイマータイムアウト処理
        /// MODULE ID           : VariableTime_Tick
        ///
        /// PARAMETER IN        : 
        /// <param name="sender">(in)発生元コントロール</param>
        /// <param name="e">(in)イベント情報</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 可変タイマー（２段階目）のタイムアウト処理を行う。
        /// </summary>
        /// 
        ///*******************************************************************************
        private void VariableTime_Tick(object sender, System.EventArgs e)
        {
            try
            {
                CTimerCtrl VariableTime = (CTimerCtrl)sender;
                UInt16 TimeIndex = (UInt16)((VariableTime.DataKind & 0xFFFF0000) >> 16);
                Byte ProcKind = (Byte)(VariableTime.DataKind & 0xFF);

                TimeIndex = (UInt16)(TimeIndex - 1);
                this.VarTimeStopTimer(TimeIndex, 0, 0, ProcKind);
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 固定タイマー減算処理
        /// MODULE ID           : DecBaseTimer
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
        /// 固定タイマーをチェックしてタイマー値が有効値の場合、２段目タイマーを起動する。
        /// 注意事項、タイマーの誤差の修正に伴い、本処理ではタイマー値の減算を行わない。
        /// </summary>
        ///
        ///*******************************************************************************
        public void DecBaseTimer()
        {
            String syncErrorMsg = String.Empty;

            try
            {
                for (UInt16 idx = 0; idx < CAppDat.BtmerT.Length; idx++)
                {
                    // 固定タイマーの登録有無確認
                    if (null == CAppDat.BtmerT[idx])
                    {
                        // 処理なし
                    }
                    else
                    {
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.FIXTIMER].WaitOne();
                        try
                        {
                            // 固定タイマーが１～0xFFFEの間の値か？
                            if ((0 < CAppDat.BtmerT[idx].TimerCount) && (0xFFFF > CAppDat.BtmerT[idx].TimerCount))
                            {
                                UInt32 TimeCount = (UInt32)((UInt32)CAppDat.BtmerT[idx].TimerCount * (UInt32)100);  // タイマクラス用のタイマ値（単位：ミリ秒）を算出
                                if (0 != CAppDat.BtmerT[idx].Timerno)
                                {
                                    lock (m_FixTimeSyncObject)
                                    {
                                        for (UInt16 cnt = 0; cnt < m_FixTimeList.Count; cnt++)
                                        {
                                            if (CAppDat.BtmerT[idx].Timerno == m_FixTimeList[cnt].DataKind)
                                            {
                                                m_FixTimeList[cnt].ChangeTimer(TimeCount, Timeout.Infinite);
                                                break;
                                            }
                                            else
                                            {
                                                // 処理なし
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    lock (m_FixTimeSyncObject)
                                    {
                                        CAppDat.BtmerT[idx].Timerno = ((UInt32)((idx + 1) * 0x10000)) + 0xFFFF;
                                        CTimerCtrl FixedTime = new CTimerCtrl();
                                        FixedTime.Time += new EventHandler(this.FixedTime_Tick);
                                        FixedTime.StartTimer(TimeCount, Timeout.Infinite, "固定タイマー", CAppDat.BtmerT[idx].Timerno);
                                        m_FixTimeList.Add(FixedTime);
                                        FixedTime = null;
                                    }
                                }
                                CAppDat.BtmerT[idx].TimerCount = 0;
                            }
                            else
                            {
                                // 処理なし
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
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.FIXTIMER].ReleaseMutex();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 可変タイマー減算処理
        /// MODULE ID           : DecVariableTimer
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
        /// 可変タイマーをチェックしてタイマー値が有効値の場合、２段目タイマーを起動する。
        /// 注意事項、タイマーの誤差の修正に伴い、本処理ではタイマー値の減算を行わない。
        /// </summary>
        ///
        ///*******************************************************************************
        public void DecVariableTimer()
        {
            String syncErrorMsg = String.Empty;

            try
            {
                for (UInt16 idx = 0; idx < CAppDat.TimerAryT.Length; idx++)
                {
                    // 可変タイマーの登録有無確認
                    if (null == CAppDat.TimerAryT[idx])
                    {
                        // 処理なし
                    }
                    else
                    {
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.TIMERINFO].WaitOne();
                        try
                        {
                            // 可変タイマーが１～0xFFFDの間の値か？
                            if ((0 < CAppDat.TimerAryT[idx].TimerCount) && (0xFFFE > CAppDat.TimerAryT[idx].TimerCount))
                            {
                                UInt32 TimeCount = (UInt32)((UInt32)CAppDat.TimerAryT[idx].TimerCount * (UInt32)100);   // タイマクラス用のタイマ値（単位：ミリ秒）を算出
                                Byte TimeKind = CAppDat.TimerAryT[idx].TimerKind;
                                Byte ProcKind = CAppDat.TimerAryT[idx].ProcKind;
                                if (0 != CAppDat.TimerAryT[idx].Timerno)
                                {
                                    // 処理なし
                                }
                                else
                                {
                                    lock (m_VarTimeSyncObject)
                                    {
                                        CAppDat.TimerAryT[idx].Timerno = ((UInt32)((idx + 1) * 0x10000)) + ProcKind;
                                        CTimerCtrl VariableTime = new CTimerCtrl();
                                        VariableTime.Time += new EventHandler(this.VariableTime_Tick);
                                        VariableTime.StartTimer(TimeCount, Timeout.Infinite, "可変タイマー", CAppDat.TimerAryT[idx].Timerno);
                                        m_VarTimeList.Add(VariableTime);
                                        VariableTime = null;
                                    }
                                }
                            }
                            // 可変タイマーが0xFFFE、0xFFFFか？
                            else if ((0xFFFE == CAppDat.TimerAryT[idx].TimerCount) || (0xFFFF == CAppDat.TimerAryT[idx].TimerCount))
                            {
                                if (0 != CAppDat.TimerAryT[idx].Timerno)
                                {
                                    UInt16 TimeCount = CAppDat.TimerAryT[idx].TimerCount;
                                    Byte TimeKind = CAppDat.TimerAryT[idx].TimerKind;
                                    Byte ProcKind = CAppDat.TimerAryT[idx].ProcKind;
                                    this.VarTimeStopTimer(idx, TimeCount, TimeKind, ProcKind);
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
                        }
                        catch (Exception ex)
                        {
                            // ミューテックス取得中に発生した例外の捕捉
                            syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                        }
                        finally
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.TIMERINFO].ReleaseMutex();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 固定タイマー解析処理
        /// MODULE ID           : AnalyzeBaseTimer
        ///
        /// PARAMETER IN        : 
        /// <param name="timeIndex">(in)タイマーインデックス</param>
        /// <param name="timeValue">(in)タイマー値</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 固定タイマーのインデックスからタイマー値を解析し処理を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void AnalyzeBaseTimer(UInt16 timeIndex, UInt16 timeValue)
        {
            String syncErrorMsg = String.Empty;
            String logMsg = String.Empty;
            UInt16 uiTimerCount = 0;                    // タイマーカウンタ値
            bool   isTimStop = false;                   // タイマー停止中フラグ

            try
            {
                uiTimerCount = 0;                       // タイマーカウンタ値を初期化
                isTimStop = false;                      // タイマー停止中フラグに停止中ではないを設定

                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.FIXTIMER].WaitOne();
                try
                {
                    uiTimerCount = CAppDat.BtmerT[timeIndex].TimerCount;
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CCommon.WriteLog(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.FIXTIMER].ReleaseMutex();
                }

                if (TIMERSTOP == uiTimerCount)
                {
                    isTimStop = true;                   // タイマー停止中フラグに停止中を設定
                }
                else
                {
                    // 処理なし
                }

                // タイマ値が０より超過の場合は処理なし
                // タイマ値が０以下の場合のみタイマ解析する、このとき対象タイマは停止とする
                if (0 < timeValue)
                {
                    return;
                }
                else
                {
                    // 固定タイマーの停止をセット
                    CCommon.SetBtmerT(timeIndex, true, TIMERSTOP);
                }

                // タイマーインデックスにより処理を分岐する
                switch (timeIndex)
                {
                    case (UInt16)CAppDat.TIMEID.CTC_A:              // CTC-A 監視
                    case (UInt16)CAppDat.TIMEID.CTC_B:              // CTC-B 監視
                    case (UInt16)CAppDat.TIMEID.CTC_C:              // CTC-C 監視
                    case (UInt16)CAppDat.TIMEID.CTC_D:              // CTC-D 監視
                        this.TimeOutCTC(timeIndex);
                        break;

                    case (UInt16)CAppDat.TIMEID.TRARETUBAN:         // TRA列番情報監視
                        this.TimeOutTRARETUBAN(timeIndex);
                        break;

                    case (UInt16)CAppDat.TIMEID.PRCRETUBAN:         // PRC列番情報監視
                        this.TimeOutPRCRETUBAN(timeIndex);
                        break;

                    case (UInt16)CAppDat.TIMEID.TIDIF1:             // 他線区IF装置１ 監視
                    case (UInt16)CAppDat.TIMEID.TIDIF2:             // 他線区IF装置２ 監視
                    case (UInt16)CAppDat.TIMEID.TIDIF3:             // 他線区IF装置３ 監視
                    case (UInt16)CAppDat.TIMEID.TIDIF4:             // 他線区IF装置４ 監視
                        this.TimeOutTIDIF(timeIndex);
                        break;

                    case (UInt16)CAppDat.TIMEID.RIREKIRENEW:        // 履歴ダイアログ表示更新
                        this.TimeOutRIREKIRENEW(timeIndex);
                        break;

                    case (UInt16)CAppDat.TIMEID.ALIVENOTICE:        // 生存通知
                        this.TimeOutALIVENOTICE(timeIndex);
                        break;

                    case (UInt16)CAppDat.TIMEID.WDT:                // ＷＤＴ監視
                        CCommon.WriteDebugLog(MethodBase.GetCurrentMethod(), "ＷＤＴ監視 タイムアウト処理なし");
                        break;

                    case (UInt16)CAppDat.TIMEID.BLINK:              // ブリンク表示更新
                        // 表示ブリンクは dcp.js のスクリプトで制御する
                        break;

                    case (UInt16)CAppDat.TIMEID.MYEQPBLINK:         // 自装置状態ブリンク表示更新
                        this.TimeOutMYEQPBLINK(timeIndex);

                        // 2015/06/01 yang メモリ監視処理を追加 Add Start
                        m_memWatchTmCount++;
                        if (m_memWatchTmCount >= ((1800 * 1000) / 500))
                        {
                            m_memWatchTmCount = 0;

                            // メモリ情報取得処理
                            // （デバッガ起動時は「DCP.vshost.exe」になるので注意
                            //bool blRet = CMemoryInfo.GetMemoInfo("DCP.vshost.exe", CAppDat.DCPSET.LogFileDir, 0, true);
                            bool blRet = CMemoryInfo.GetMemoInfo("DCP.exe", CAppDat.DCPSET.LogFileDir, 0, true);
                        }
                        else
                        {
                            // 処理なし
                        }

                        break;

                    case (UInt16)CAppDat.TIMEID.TIMEDISP:           // 時刻表示更新
                        this.TimeOutTIMEDISP(timeIndex);
                        break;

                    case (UInt16)CAppDat.TIMEID.TEIANSETTEI_ANS:    // 提案設定アンサ監視
                        if (true == isTimStop)                      // タイマー停止中フラグが停止中の場合
                        {
                            logMsg = String.Format("提案設定アンサ監視：タイマ停止中の為タイムアウト処理しない：タイマID={0}", timeIndex);
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.INFORMATION, MethodBase.GetCurrentMethod(), logMsg);
                        }
                        else
                        {
                            this.TimeOutTEIANANS(timeIndex);
                        }
                        break;

                    case (UInt16)CAppDat.TIMEID.TEIAN1_ANS:         // 提案１アンサ監視
                    case (UInt16)CAppDat.TIMEID.TEIAN2_ANS:         // 提案２アンサ監視
                    case (UInt16)CAppDat.TIMEID.TEIAN3_ANS:         // 提案３アンサ監視
                    case (UInt16)CAppDat.TIMEID.TEIAN4_ANS:         // 提案４アンサ監視
                    case (UInt16)CAppDat.TIMEID.TEIAN5_ANS:         // 提案５アンサ監視
                        this.TimeOutTEIANANS(timeIndex);
                        break;

                    case (UInt16)CAppDat.TIMEID.DIAREQ_ANS:         // ダイヤ要求アンサ監視
                        if (true == isTimStop)                      // タイマー停止中フラグが停止中の場合
                        {
                            logMsg = String.Format("ダイヤ要求アンサ監視：タイマ停止中の為タイムアウト処理しない：タイマID={0}", timeIndex);
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.INFORMATION, MethodBase.GetCurrentMethod(), logMsg);
                        }
                        else
                        {
                            this.TimeOutDIAREQANS(timeIndex);
                        }
                        break;

                    case (UInt16)CAppDat.TIMEID.JYUUKEIHOU:         // 重警報停止監視
                    case (UInt16)CAppDat.TIMEID.KEIKEIHOU:          // 軽警報停止監視
                    case (UInt16)CAppDat.TIMEID.KAIFUKUKEIHOU:      // 回復警報停止監視
                        CCommon.WriteDebugLog(MethodBase.GetCurrentMethod(), "警報停止監視 タイムアウト処理なし");
                        break;

                    case (UInt16)CAppDat.TIMEID.ENSEN1:             // 沿線情報：風規制解除１監視
                    case (UInt16)CAppDat.TIMEID.ENSEN2:             // 沿線情報：風規制解除２監視
                    case (UInt16)CAppDat.TIMEID.ENSEN3:             // 沿線情報：風規制解除３監視
                    case (UInt16)CAppDat.TIMEID.ENSEN4:             // 沿線情報：風規制解除４監視
                    case (UInt16)CAppDat.TIMEID.ENSEN5:             // 沿線情報：風規制解除５監視
                    case (UInt16)CAppDat.TIMEID.ENSEN6:             // 沿線情報：風規制解除６監視
                    case (UInt16)CAppDat.TIMEID.ENSEN7:             // 沿線情報：風規制解除７監視
                    case (UInt16)CAppDat.TIMEID.ENSEN8:             // 沿線情報：風規制解除８監視
                    case (UInt16)CAppDat.TIMEID.ENSEN9:             // 沿線情報：風規制解除９監視
                    case (UInt16)CAppDat.TIMEID.ENSEN10:            // 沿線情報：風規制解除１０監視
                    case (UInt16)CAppDat.TIMEID.ENSEN11:            // 沿線情報：風規制解除１１監視
                    case (UInt16)CAppDat.TIMEID.ENSEN12:            // 沿線情報：風規制解除１２監視
                        CCommon.WriteDebugLog(MethodBase.GetCurrentMethod(), "沿線情報：風規制解除監視 タイムアウト処理なし");
                        break;

                    case (UInt16)CAppDat.TIMEID.CTCANS:             // CTCアンサ警報音停止監視
                    case (UInt16)CAppDat.TIMEID.PRCANS:             // PRCアンサ警報音停止監視
                    case (UInt16)CAppDat.TIMEID.ATSUKAI:            // 扱い警報音停止監視
                        CCommon.WriteDebugLog(MethodBase.GetCurrentMethod(), "警報情報：警報音停止監視 タイムアウト処理なし");
                        break;

                    case (UInt16)CAppDat.TIMEID.PRCDENSO_A:              // PRC-A伝送部監視
                    case (UInt16)CAppDat.TIMEID.PRCDENSO_B:              // PRC-B伝送部監視
                    case (UInt16)CAppDat.TIMEID.PRCDENSO_C:              // PRC-C伝送部監視
                    case (UInt16)CAppDat.TIMEID.PRCDENSO_D:              // PRC-D伝送部監視
                        this.TimeOutPRCDenso(timeIndex);
                        break;

                    case (UInt16)CAppDat.TIMEID.TIDIFRET1:              // 他線区IF装置１ 列番情報 監視
                    case (UInt16)CAppDat.TIMEID.TIDIFRET2:              // 他線区IF装置２ 列番情報 監視
                    case (UInt16)CAppDat.TIMEID.TIDIFRET3:              // 他線区IF装置３ 列番情報 監視
                    case (UInt16)CAppDat.TIMEID.TIDIFRET4:              // 他線区IF装置４ 列番情報 監視
                        this.TimeOutTIDIFRET(timeIndex);
                        break;

                    default:                                        // その他の監視
                        logMsg = String.Format("想定外固定タイマーがタイムアウトしました タイマID={0}", timeIndex);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                        break;
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 可変タイマー解析処理
        /// MODULE ID           : AnalyzeVariableTimer
        ///
        /// PARAMETER IN        : 
        /// <param name="timeIndex">(in)タイマーインデックス</param>
        /// <param name="timeValue">(in)タイマー値</param>
        /// <param name="timeKind">(in)タイマー種別</param>
        /// <param name="procKind">(in)処理種別</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 可変タイマーのインデックスからタイマー値を解析し処理を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void AnalyzeVariableTimer(UInt16 timeIndex, UInt16 timeValue, Byte timeKind, Byte procKind)
        {
            bool isStopFlag = false;                    // 停止フラグ
            String syncErrorMsg = String.Empty;         // 同期エラーメッセージ
            String logMsg = String.Empty;               // ログメッセージ

            try
            {
                // タイマ値が０より超過の場合は処理なし
                // タイマ値が０以下の場合のみタイマ解析する
                if (0 < timeValue)
                {
                    return;
                }
                else
                {
                    // 処理なし
                }

                // 処理種別により処理を分岐する
                switch (procKind)
                {
                    case VARTIME_JYUKEIHOU:              // 重警報
                    case VARTIME_KEIKEIHOU:              // 軽警報
                    case VARTIME_KAIFUKU:                // 回復
                    case VARTIME_INFORMATION:            // 情報
                    case VARTIME_OTHER:                  // その他
                    case VARTIME_GENBAYOUSEI:            // 現場要請
                    case VARTIME_SEKKIN:                 // 接近
                    case VARTIME_TEIAN:                  // 提案
                    case VARTIME_ATSUKAIKEIHOU:          // 扱い警報
                        // 可変タイマーの停止（予約停止）
                        CCommon.StopVarTimerHi(timeIndex, true);

                        // 鳴動管理テーブルに鳴動要求データを登録
                        CKeihouCom.OutputMeidou((Byte)CMeidouReqMng.ACTKIND.TIMEREND, procKind, String.Empty, (UInt16)(timeIndex + 1), 0); 
                        break;

                    case VARTIME_TEIANKYOHI:             // 提案自動拒否
                        isStopFlag = false;              // 停止フラグに停止ではないを設定
                        // 提案制御処理の排他制御開始
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.TEIANHAITA].WaitOne();
                        try
                        {
                            // 可変タイマテーブル排他制御開始
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.TIMERINFO].WaitOne();

                            try
                            {
                                // 該当タイマは、停止／予約停止済みの場合
                                if ((0xFFFF == CAppDat.TimerAryT[timeIndex].TimerCount) ||
                                    (0xFFFE == CAppDat.TimerAryT[timeIndex].TimerCount))
                                {
                                    isStopFlag = true;  // 停止フラグに停止を設定
                                }
                                // 該当タイマは、停止・予約停止済みではない場合
                                else
                                {
                                    // 処理なし
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
                                // 可変タイマテーブル排他制御終了
                                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.TIMERINFO].ReleaseMutex();
                            }

                            if (false == isStopFlag)    // 停止フラグが停止ではない場合
                            {
                                // 可変タイマーの停止（予約停止）
                                CCommon.StopVarTimerHi(timeIndex, true);

                                // 提案応答処理
                                CTeianCtrl.WatchRespTeian((UInt16)(timeIndex + 1), (UInt16)CTeianSub.RESPSETTEI.JIDOUKYOHI);
                            }
                            else                        // 停止フラグが停止の場合
                            {
                                // 処理なし
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
                            // 提案制御処理の排他制御終了
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.TEIANHAITA].ReleaseMutex();
                        }
                        break;

                    case VARTIME_RECVPAGEKANSI:         // 受信電文頁監視
                        // 可変タイマーの停止（予約停止）
                        CCommon.StopVarTimerHi(timeIndex, true);
                        break;

                    default:                                        // その他の監視
                        logMsg = String.Format("想定外可変タイマーがタイムアウトしました タイマID={0}", timeIndex);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                        break;
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 装置種別取得処理
        /// MODULE ID           : GetEpqKind
        ///
        /// PARAMETER IN        : 
        /// <param name="timeIndex">(in)タイマーインデックス</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>装置種別</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 固定タイマーのインデックスを解析し該当する装置種別を返す。
        /// 比較した結果一致する装置種別が存在しない場合は、「０」を返す。
        /// </summary>
        ///
        ///*******************************************************************************
        private UInt16 GetEpqKind(UInt16 timeIndex)
        {
            UInt16 eqpKind = 0;
            String logMsg = String.Empty;

            try
            {
                // タイマーインデックスにより装置種別を取得処理を分岐する
                switch (timeIndex)
                {
                    case (UInt16)CAppDat.TIMEID.CTC_A:          // ＣＴＣ-Ａ監視
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_CTC1;
                        break;

                    case (UInt16)CAppDat.TIMEID.CTC_B:          // ＣＴＣ-Ｂ監視
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_CTC2;
                        break;

                    case (UInt16)CAppDat.TIMEID.CTC_C:          // ＣＴＣ-Ｃ監視
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_CTC3;
                        break;

                    case (UInt16)CAppDat.TIMEID.CTC_D:          // ＣＴＣ-Ｄ監視
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_CTC4;
                        break;

                    case (UInt16)CAppDat.TIMEID.PRCRETUBAN:     // ＰＲＣ装置監視
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_PRC;
                        break;

                    case (UInt16)CAppDat.TIMEID.TRARETUBAN:     // ＴＲＡ装置監視
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_TRA;
                        break;

                    case (UInt16)CAppDat.TIMEID.TIDIF1:         // 他線区ＩＦ装置１監視
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_TIDIF1;
                        break;

                    case (UInt16)CAppDat.TIMEID.TIDIF2:         // 他線区ＩＦ装置２監視
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_TIDIF2;
                        break;

                    case (UInt16)CAppDat.TIMEID.TIDIF3:         // 他線区ＩＦ装置３監視
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_TIDIF3;
                        break;

                    case (UInt16)CAppDat.TIMEID.TIDIF4:         // 他線区ＩＦ装置４監視
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_TIDIF4;
                        break;

                    case (UInt16)CAppDat.TIMEID.TIDIFRET1:      // 他線区ＩＦ装置１ 列番情報監視
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_TIDIF1;
                        break;

                    case (UInt16)CAppDat.TIMEID.TIDIFRET2:      // 他線区ＩＦ装置２ 列番情報監視
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_TIDIF2;
                        break;

                    case (UInt16)CAppDat.TIMEID.TIDIFRET3:      // 他線区ＩＦ装置３ 列番情報監視
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_TIDIF3;
                        break;

                    case (UInt16)CAppDat.TIMEID.TIDIFRET4:      // 他線区ＩＦ装置４ 列番情報監視
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_TIDIF4;
                        break;

                    case (UInt16)CAppDat.TIMEID.PRCDENSO_A:     // ＰＲＣ伝送部-Ａ監視
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_PRCDENSO1;
                        break;
                    case (UInt16)CAppDat.TIMEID.PRCDENSO_B:     // ＰＲＣ伝送部-Ｂ監視
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_PRCDENSO2;
                        break;
                    case (UInt16)CAppDat.TIMEID.PRCDENSO_C:     // ＰＲＣ伝送部-Ｃ監視
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_PRCDENSO3;
                        break;
                    case (UInt16)CAppDat.TIMEID.PRCDENSO_D:     // ＰＲＣ伝送部-Ｄ監視
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_PRCDENSO4;
                        break;

                    default:                                // その他の監視
                        logMsg = String.Format("インデックス不正のため装置種別取得に失敗しました TimeIndex={0}", timeIndex);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                        break;
                }
                return eqpKind;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                return 0;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 伝文種別取得処理
        /// MODULE ID           : GetDenKind
        ///
        /// PARAMETER IN        : 
        /// <param name="timeIndex">(in)タイマーインデックス</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>装置種別</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 固定タイマーのインデックスを解析し該当する伝文種別を返す。
        /// 比較した結果一致する装置種別が存在しない場合は、「０」を返す。
        /// </summary>
        ///
        ///*******************************************************************************
        private UInt16 GetDenKind(UInt16 timeIndex)
        {
            UInt16 denKind = 0;
            String logMsg = String.Empty;

            try
            {
                // タイマーインデックスにより装置種別を取得処理を分岐する
                switch (timeIndex)
                {
                    case (UInt16)CAppDat.TIMEID.CTC_A:          // ＣＴＣ-Ａ監視
                    case (UInt16)CAppDat.TIMEID.CTC_B:          // ＣＴＣ-Ｂ監視
                    case (UInt16)CAppDat.TIMEID.CTC_C:          // ＣＴＣ-Ｃ監視
                    case (UInt16)CAppDat.TIMEID.CTC_D:          // ＣＴＣ-Ｄ監視
                        denKind = (UInt16)CKeihouCom.DENKIND.NONE;
                        break;

                    case (UInt16)CAppDat.TIMEID.TIDIF1:         // 他線区ＩＦ装置１監視
                    case (UInt16)CAppDat.TIMEID.TIDIF2:         // 他線区ＩＦ装置２監視
                    case (UInt16)CAppDat.TIMEID.TIDIF3:         // 他線区ＩＦ装置３監視
                    case (UInt16)CAppDat.TIMEID.TIDIF4:         // 他線区ＩＦ装置４監視
                        denKind = (UInt16)CKeihouCom.TIDIFKIND.IF_STATUS;
                        break;

                    case (UInt16)CAppDat.TIMEID.TIDIFRET1:      // 他線区ＩＦ装置１ 列番情報監視
                    case (UInt16)CAppDat.TIMEID.TIDIFRET2:      // 他線区ＩＦ装置２ 列番情報監視
                    case (UInt16)CAppDat.TIMEID.TIDIFRET3:      // 他線区ＩＦ装置３ 列番情報監視
                    case (UInt16)CAppDat.TIMEID.TIDIFRET4:      // 他線区ＩＦ装置４ 列番情報監視
                        denKind = (UInt16)CKeihouCom.TIDIFKIND.IF_RETSUBAN;
                        break;

                    case (UInt16)CAppDat.TIMEID.PRCDENSO_A:     // ＰＲＣ伝送部-Ａ監視
                    case (UInt16)CAppDat.TIMEID.PRCDENSO_B:     // ＰＲＣ伝送部-Ｂ監視
                    case (UInt16)CAppDat.TIMEID.PRCDENSO_C:     // ＰＲＣ伝送部-Ｃ監視
                    case (UInt16)CAppDat.TIMEID.PRCDENSO_D:     // ＰＲＣ伝送部-Ｄ監視
                        denKind = (UInt16)CKeihouCom.RENKIND.NONE;
                        break;

                    default:                                // その他の監視
                        denKind = 0;
                        break;
                }
                return denKind;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                return 0;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 装置状態更新処理
        /// MODULE ID           : SetEqpStatus
        ///
        /// PARAMETER IN        : 
        /// <param name="timeIndex">(in)タイマーインデックス</param>
        /// <param name="nowEqpState">(in)今回装置状態</param>
        /// <param name="oldEqpState">(in)前回装置状態</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 前回装置状態と今回装置状態で正常→不能の場合は表示更新、警報出力を実施する。
        /// 上記以外の状態変化の場合は処理なしとする。
        /// </summary>
        ///
        ///*******************************************************************************
        private void SetEqpStatus(UInt16 timeIndex, UInt16 nowEqpState, UInt16 oldEqpState)
        {
            try
            {
                // 更新前装置状態が不能ではない（正常→不能へ変化）場合
                if (0x00 == ((Byte)oldEqpState & EQPSTATE_UNABLE))
                {
                    // 装置状態表示更新要求処理
                    IntPtr wParam = new IntPtr((UInt16)CAppDat.UPDATEREQUEST.OTHERSTATEUPDATE);
                    CCommon.LocalPostMessage(true, CAppDat.WM_USER_MAINFORMUPDATE, wParam, IntPtr.Zero);

                    // 装置種別の取得
                    UInt16 eqpKind = this.GetEpqKind(timeIndex);

                    // 駅種別の取得
                    UInt16 denKind = this.GetDenKind(timeIndex);

                    // 出力ありの場合
                    if (true == CCommon.CheckOutputKeihou(eqpKind, denKind, 0, true))
                    {
                        // 他装置故障警報出力処理
                        CKeihouCtrl.OutputEqpKeihou(eqpKind, denKind, 1);
                    }
                }
                // 更新前装置状態が不能（不能→不能：変化なし）場合
                else
                {
                    // 処理なし
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : CTC監視タイマタイムアウト処理
        /// MODULE ID           : TimeOutCTC
        ///
        /// PARAMETER IN        : 
        /// <param name="timeIndex">(in)タイマーインデックス</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 固定タイマーのインデックスを解析し該当するCTC装置状態を不能にセットする。
        /// 装置状態の表示更新、警報出力を実施する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void TimeOutCTC(UInt16 timeIndex)
        {
            Byte oldEqpState = 0;                   // 更新前装置状態
            Byte nowEqpState = 0;                   // 更新後装置状態
            UInt16 eqpIndex = 0;                    // 装置インデックス
            bool isCheckFlg = false;                // 装置更新有無
            String syncErrorMsg = String.Empty;     // 同期エラーメッセージ
            String logMsg = String.Empty;           // ログ出力メッセージ

            try
            {
                // タイマーインデックスがＣＴＣ-Ｂ監視？
                if ((UInt16)CAppDat.TIMEID.CTC_B == timeIndex)
                {
                    eqpIndex = 1;
                    isCheckFlg = true;
                }
                // タイマーインデックスがＣＴＣ-Ｃ監視？
                else if ((UInt16)CAppDat.TIMEID.CTC_C == timeIndex)
                {
                    eqpIndex = 2;
                    isCheckFlg = true;
                }
                // タイマーインデックスがＣＴＣ-Ｄ監視？
                else if ((UInt16)CAppDat.TIMEID.CTC_D == timeIndex)
                {
                    eqpIndex = 3;
                    isCheckFlg = true;
                }
                else
                {
                    eqpIndex = 0;
                    isCheckFlg = true;
                }

                // 参照インデックスが取得成功、且つ状態テーブル範囲内？
                if ((true == isCheckFlg) && ((0 <= eqpIndex) && (eqpIndex < CAppDat.CTCMAX)))
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SOUTISTATUS].WaitOne();
                    try
                    {
                        // 更新前装置状態を保存
                        oldEqpState = CAppDat.SotiJyoutaiT.CtcJyoutai[eqpIndex].CtcRonJyoutai;

                        // 装置状態テーブルのCTC中央装置（論理部）に不能をセット
                        CAppDat.SotiJyoutaiT.CtcJyoutai[eqpIndex].CtcRonJyoutai |= EQPSTATE_UNABLE;
                        // 更新後装置状態を保存
                        nowEqpState = CAppDat.SotiJyoutaiT.CtcJyoutai[eqpIndex].CtcRonJyoutai;
                    }
                    catch (Exception ex)
                    {
                        // ミューテックス取得中に発生した例外の捕捉
                        syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                    }
                    finally
                    {
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SOUTISTATUS].ReleaseMutex();
                    }

                    // 装置状態更新処理
                    this.SetEqpStatus(timeIndex, nowEqpState, oldEqpState);
                }
                else
                {
                    logMsg = String.Format("装置状態テーブル[CTC中央装置]の参照インデックス取得失敗 TimeID={0}", timeIndex);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : PRC伝送部監視タイマタイムアウト処理
        /// MODULE ID           : TimeOutPRCDenso
        ///
        /// PARAMETER IN        : 
        /// <param name="timeIndex">(in)タイマーインデックス</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 固定タイマーのインデックスを解析し該当するPRC伝送部装置状態を不能にセットする。
        /// 装置状態の表示更新、警報出力を実施する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void TimeOutPRCDenso(UInt16 timeIndex)
        {
            Byte oldEqpState = 0;                   // 更新前装置状態
            Byte nowEqpState = 0;                   // 更新後装置状態
            String syncErrorMsg = String.Empty;     // 同期エラーメッセージ
            String logMsg = String.Empty;           // ログ出力メッセージ
            UInt16 eqpIndex = 0;                    // 装置インデックス
            bool isCheckFlg = false;                // 装置更新有無

            try
            {
                // タイマーインデックスがＰＲＣ伝送部-Ｂ監視？
                if ((UInt16)CAppDat.TIMEID.PRCDENSO_B == timeIndex)
                {
                    eqpIndex = 1;
                    isCheckFlg = true;
                }
                // タイマーインデックスがＰＲＣ伝送部-Ｃ監視？
                else if ((UInt16)CAppDat.TIMEID.PRCDENSO_C == timeIndex)
                {
                    eqpIndex = 2;
                    isCheckFlg = true;
                }
                // タイマーインデックスがＰＲＣ伝送部-Ｄ監視？
                else if ((UInt16)CAppDat.TIMEID.PRCDENSO_D == timeIndex)
                {
                    eqpIndex = 3;
                    isCheckFlg = true;
                }
                else
                {
                    eqpIndex = 0;
                    isCheckFlg = true;
                }

                // 参照インデックスが取得成功、且つ状態テーブル範囲内？
                if ((true == isCheckFlg) && ((0 <= eqpIndex) && (eqpIndex < CAppDat.PRCDENSOMAX)))
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SOUTISTATUS].WaitOne();
                    try
                    {
                        // 更新前装置状態を保存
                        oldEqpState = CAppDat.SotiJyoutaiT.PrcDensoJyoutai[eqpIndex].PrcDenJyoutai;
                        // 装置状態テーブルのPRC伝送部装置に不能をセット
                        CAppDat.SotiJyoutaiT.PrcDensoJyoutai[eqpIndex].PrcDenJyoutai |= EQPSTATE_UNABLE;
                        // 更新後装置状態を保存
                        nowEqpState = CAppDat.SotiJyoutaiT.PrcDensoJyoutai[eqpIndex].PrcDenJyoutai;
                    }
                    catch (Exception ex)
                    {
                        // ミューテックス取得中に発生した例外の捕捉
                        syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                    }
                    finally
                    {
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SOUTISTATUS].ReleaseMutex();
                    }

                    // 装置状態更新処理
                    this.SetEqpStatus(timeIndex, nowEqpState, oldEqpState);
                }
                else
                {
                    logMsg = String.Format("装置状態テーブル[PRC伝送部装置]の参照インデックス取得失敗 TimeID={0}", timeIndex);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 他線区IF装置列番情報監視タイマタイムアウト処理
        /// MODULE ID           : TimeOutTIDIFRET
        ///
        /// PARAMETER IN        : 
        /// <param name="timeIndex">(in)タイマーインデックス</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 固定タイマーのインデックスを解析し該当する他線区IF装置状態を不能にセットする。
        /// 装置状態の表示更新、警報出力を実施する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void TimeOutTIDIFRET(UInt16 timeIndex)
        {
            Byte oldEqpState = 0;                   // 更新前装置状態
            Byte nowEqpState = 0;                   // 更新後装置状態
            UInt16 eqpIndex = 0;                    // 装置インデックス
            bool isCheckFlg = false;                // 装置更新有無
            String syncErrorMsg = String.Empty;     // 同期エラーメッセージ
            String logMsg = String.Empty;           // ログ出力メッセージ

            try
            {
                // タイマーインデックスが他線区ＩＦ装置２列番情報監視？
                if ((UInt16)CAppDat.TIMEID.TIDIFRET2 == timeIndex)
                {
                    eqpIndex = 1;
                    isCheckFlg = true;
                }
                // タイマーインデックスが他線区ＩＦ装置３列番情報監視？
                else if ((UInt16)CAppDat.TIMEID.TIDIFRET3 == timeIndex)
                {
                    eqpIndex = 2;
                    isCheckFlg = true;
                }
                // タイマーインデックスが他線区ＩＦ装置４列番情報監視？
                else if ((UInt16)CAppDat.TIMEID.TIDIFRET4 == timeIndex)
                {
                    eqpIndex = 3;
                    isCheckFlg = true;
                }
                else
                {
                    eqpIndex = 0;
                    isCheckFlg = true;
                }

                // 参照インデックスが取得成功、且つ状態テーブル範囲内？
                if ((true == isCheckFlg) && ((0 <= eqpIndex) && (eqpIndex < CAppDat.TIDMAX)))
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SOUTISTATUS].WaitOne();
                    try
                    {
                        // 更新前装置状態を保存
                        oldEqpState = CAppDat.SotiJyoutaiT.TidifJyoutai[eqpIndex].TidIfRetRecJyoutai;
                        // 装置状態テーブルのTIDIFRETに不能をセット
                        CAppDat.SotiJyoutaiT.TidifJyoutai[eqpIndex].TidIfRetRecJyoutai |= EQPSTATE_UNABLE;
                        // 更新後装置状態を保存
                        nowEqpState = CAppDat.SotiJyoutaiT.TidifJyoutai[eqpIndex].TidIfRetRecJyoutai;
                    }
                    catch (Exception ex)
                    {
                        // ミューテックス取得中に発生した例外の捕捉
                        syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                    }
                    finally
                    {
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SOUTISTATUS].ReleaseMutex();
                    }

                    // 装置状態更新処理
                    this.SetEqpStatus(timeIndex, nowEqpState, oldEqpState);
                }
                else
                {
                    logMsg = String.Format("装置状態テーブル[他線区IF装置]の参照インデックス取得失敗 TimeID={0}", timeIndex);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : TRA監視タイマタイムアウト処理
        /// MODULE ID           : TimeOutTRARETUBAN
        ///
        /// PARAMETER IN        : 
        /// <param name="timeIndex">(in)タイマーインデックス</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 固定タイマーのインデックスを解析し該当するTRA装置状態を不能にセットする。
        /// 装置状態の表示更新、警報出力を実施する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void TimeOutTRARETUBAN(UInt16 timeIndex)
        {
            Byte oldEqpState = 0;                   // 更新前装置状態
            Byte nowEqpState = 0;                   // 更新後装置状態
            String syncErrorMsg = String.Empty;     // 同期エラーメッセージ

            try
            {
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SOUTISTATUS].WaitOne();
                try
                {
                    // 更新前装置状態を保存
                    oldEqpState = CAppDat.SotiJyoutaiT.Tra;
                    // 装置状態テーブルのTRA装置に不能をセット
                    CAppDat.SotiJyoutaiT.Tra |= EQPSTATE_UNABLE;
                    // 更新後装置状態を保存
                    nowEqpState = CAppDat.SotiJyoutaiT.Tra;
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SOUTISTATUS].ReleaseMutex();
                }

                // 装置状態更新処理
                this.SetEqpStatus(timeIndex, nowEqpState, oldEqpState);
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : PRC監視タイマタイムアウト処理
        /// MODULE ID           : TimeOutPRCRETUBAN
        ///
        /// PARAMETER IN        : 
        /// <param name="timeIndex">(in)タイマーインデックス</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 固定タイマーのインデックスを解析し該当するPRC装置状態を不能にセットする。
        /// 装置状態の表示更新、警報出力を実施する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void TimeOutPRCRETUBAN(UInt16 timeIndex)
        {
            Byte oldEqpState = 0;                   // 更新前装置状態
            Byte nowEqpState = 0;                   // 更新後装置状態
            String syncErrorMsg = String.Empty;     // 同期エラーメッセージ

            try
            {
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SOUTISTATUS].WaitOne();
                try
                {
                    // 更新前装置状態を保存
                    oldEqpState = CAppDat.SotiJyoutaiT.Prc;
                    // 装置状態テーブルのPRC装置に不能をセットする
                    CAppDat.SotiJyoutaiT.Prc |= EQPSTATE_UNABLE;
                    // 更新後装置状態を保存
                    nowEqpState = CAppDat.SotiJyoutaiT.Prc;
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SOUTISTATUS].ReleaseMutex();
                }

                // 装置状態更新処理
                this.SetEqpStatus(timeIndex, nowEqpState, oldEqpState);
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 他線区IF装置監視タイマタイムアウト処理
        /// MODULE ID           : TimeOutTIDIF
        ///
        /// PARAMETER IN        : 
        /// <param name="timeIndex">(in)タイマーインデックス</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 固定タイマーのインデックスを解析し該当する他線区IF装置状態を不能にセットする。
        /// 装置状態の表示更新、警報出力を実施する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void TimeOutTIDIF(UInt16 timeIndex)
        {
            Byte oldEqpState = 0;                   // 更新前装置状態
            Byte nowEqpState = 0;                   // 更新後装置状態
            UInt16 eqpIndex = 0;                    // 装置インデックス
            bool isCheckFlg = false;                // 装置更新有無
            String syncErrorMsg = String.Empty;     // 同期エラーメッセージ
            String logMsg = String.Empty;           // ログ出力メッセージ

            try
            {
                // タイマーインデックスが他線区ＩＦ装置２監視？
                if ((UInt16)CAppDat.TIMEID.TIDIF2 == timeIndex)
                {
                    eqpIndex = 1;
                    isCheckFlg = true;
                }
                // タイマーインデックスが他線区ＩＦ装置３監視？
                else if ((UInt16)CAppDat.TIMEID.TIDIF3 == timeIndex)
                {
                    eqpIndex = 2;
                    isCheckFlg = true;
                }
                // タイマーインデックスが他線区ＩＦ装置４監視？
                else if ((UInt16)CAppDat.TIMEID.TIDIF4 == timeIndex)
                {
                    eqpIndex = 3;
                    isCheckFlg = true;
                }
                else
                {
                    eqpIndex = 0;
                    isCheckFlg = true;
                }

                // 参照インデックスが取得成功、且つ状態テーブル範囲内？
                if ((true == isCheckFlg) && ((0 <= eqpIndex) && (eqpIndex < CAppDat.TIDMAX)))
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SOUTISTATUS].WaitOne();
                    try
                    {
                        // 更新前装置状態を保存
                        oldEqpState = CAppDat.SotiJyoutaiT.TidifJyoutai[eqpIndex].TidIfRecJyoutai;
                        // 装置状態テーブルのTIDIFに不能をセット
                        CAppDat.SotiJyoutaiT.TidifJyoutai[eqpIndex].TidIfRecJyoutai |= EQPSTATE_UNABLE;
                        // 更新後装置状態を保存
                        nowEqpState = CAppDat.SotiJyoutaiT.TidifJyoutai[eqpIndex].TidIfRecJyoutai;
                    }
                    catch (Exception ex)
                    {
                        // ミューテックス取得中に発生した例外の捕捉
                        syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                    }
                    finally
                    {
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SOUTISTATUS].ReleaseMutex();
                    }

                    // 装置状態更新処理
                    this.SetEqpStatus(timeIndex, nowEqpState, oldEqpState);
                }
                else
                {
                    logMsg = String.Format("装置状態テーブル[他線区IF装置]の参照インデックス取得失敗 TimeID={0}", timeIndex);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 履歴ダイアログ表示更新タイマタイムアウト処理
        /// MODULE ID           : TimeOutRIREKIRENEW
        ///
        /// PARAMETER IN        : 
        /// <param name="timeIndex">(in)タイマーインデックス</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 固定タイマーのインデックスを解析し履歴ダイアログの表示更新を行う。
        /// （履歴ダイアログに日付リスト更新を通知する。）
        /// 処理完了後、履歴ダイアログ表示更新タイマーを再設定する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void TimeOutRIREKIRENEW(UInt16 timeIndex)
        {
            String syncErrorMsg = String.Empty;

            try
            {
                // 現在の日付を取得する
                String strDate = DateTime.Now.ToString(RIREKIRENEW_FORMAT);

                // 履歴フォームポインタ更新排他制御開始
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.RIREKIPOINTER].WaitOne();
                try
                {
                    // 保存している日付と違う場合（＝日付が変わっている場合）
                    // 且つ、履歴画面が表示中の場合
                    if ((String.Empty != CAppDat.RirekiDate) && (CAppDat.RirekiDate != strDate) && (null != CAppDat.FormAlarmRireki))
                    {
                        // 日付リスト更新要求処理
                        CAppDat.FormAlarmRireki.SetDateRefresh(true);
                        // 日付を保存
                        CAppDat.RirekiDate = strDate;
                    }
                    // 保存している日付と同じ、または、初期値の場合
                    else
                    {
                        // 処理なし
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
                    // 履歴フォームポインタ更新排他制御終了
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.RIREKIPOINTER].ReleaseMutex();
                }

                // 履歴ダイアログ表示更新タイマーを再設定
                CCommon.SetBtmerT(timeIndex);
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 生存通知タイマタイムアウト処理
        /// MODULE ID           : TimeOutALIVENOTICE
        ///
        /// PARAMETER IN        : 
        /// <param name="timeIndex">(in)タイマーインデックス</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 定周期で生存通知の送信を行う。
        /// 処理完了後、生存通知タイマーを再設定する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void TimeOutALIVENOTICE(UInt16 timeIndex)
        {
            try
            {
                // 生存通知コードを取得
                Byte[] senddata = BitConverter.GetBytes(CAppDat.MainProcSetF.ProcCodeInfo.ProcSurvive);
                // 監視プロセスに生存通知を通知する
                CCommon.WriteQueSendData((UInt16)CAppDat.SHAREMEMORYID.TIDQUE, senddata, 2, 0x000A);

                // 生存通知タイマーを再設定
                UInt16 timerValue = (UInt16)(CAppDat.MainProcSetF.ProcCodeInfo.SurvCyc / 100);
                CCommon.SetBtmerT(timeIndex, true, timerValue);
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 自装置状態ブリンク表示更新タイマタイムアウト処理
        /// MODULE ID           : TimeOutMYEQPBLINK
        ///
        /// PARAMETER IN        : 
        /// <param name="timeIndex">(in)タイマーインデックス</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 自装置状態ブリンク表示要求処理を実施する。
        /// 処理完了後、自装置状態ブリンク表示更新タイマーを再設定する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void TimeOutMYEQPBLINK(UInt16 timeIndex)
        {
            try
            {
                // 装置状態表示更新要求処理（自装置状態表示更新指定）
                IntPtr wParam = new IntPtr((UInt16)CAppDat.UPDATEREQUEST.MYSTATEUPDATE);
                CCommon.LocalPostMessage(true, CAppDat.WM_USER_MAINFORMUPDATE, wParam, IntPtr.Zero);

                // 自装置状態ブリンク表示更新タイマーを再設定
                CCommon.SetBtmerT(timeIndex);
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 時刻表示更新タイマタイムアウト処理
        /// MODULE ID           : TimeOutTIMEDISP
        ///
        /// PARAMETER IN        : 
        /// <param name="timeIndex">(in)タイマーインデックス</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 時刻表示更新要求処理を実施する。処理完了後、時刻表示更新タイマーを再設定する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void TimeOutTIMEDISP(UInt16 timeIndex)
        {
            try
            {
                // 装置状態表示更新要求処理（時刻表示更新指定）
                IntPtr wParam = new IntPtr((UInt16)CAppDat.UPDATEREQUEST.DATETIMEUPDATE);
                CCommon.LocalPostMessage(true, CAppDat.WM_USER_MAINFORMUPDATE, wParam, IntPtr.Zero);

                // 時刻表示更新タイマーを再設定
                CCommon.SetBtmerT(timeIndex);

                // 日替わり時刻を判断し、一致したら日替わり処理を実施
                UInt16 ChangeHour = CAppDat.DCPSET.ChangeHour;
                UInt16 ChangeMinute = CAppDat.DCPSET.ChangeMinute;
                if ((ChangeHour == DateTime.Now.Hour) && (ChangeMinute == DateTime.Now.Minute))
                {
                    if (true == m_IsDateTimeChanged)
                    {
                        // 処理なし
                    }
                    else
                    {
                        m_IsDateTimeChanged = true;
                        CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.CHANGEDDAYS].Set();
                        // DEBUG 2014/04/18
                        CCommon.GetComputerInfomation();
                    }
                }
                else
                {
                    m_IsDateTimeChanged = false;
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 提案アンサ監視タイマタイムアウト処理
        /// MODULE ID           : TimeOutTEIANANS
        ///
        /// PARAMETER IN        : 
        /// <param name="timeIndex">(in)タイマーインデックス</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 提案設定応答監視処理を実施する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void TimeOutTEIANANS(UInt16 timeIndex)
        {
            try
            {
                // 提案設定応答監視処理
                UInt16 kekka = CTeianCtrl.WatchTeianSettei();
                if (0 == kekka)
                {
                    // 処理なし
                }
                else
                {
                    // 処理なし
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : ダイヤ要求アンサ監視タイマタイムアウト処理
        /// MODULE ID           : TimeOutDIAREQANS
        ///
        /// PARAMETER IN        : 
        /// <param name="timeIndex">(in)タイマーインデックス</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// ダイヤ要求応答監視処理を実施する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void TimeOutDIAREQANS(UInt16 timeIndex)
        {
            try
            {
                // ダイヤ要求応答監視処理
                UInt16 kekka = CTeianCtrl.WatchDiaReq();
                if (0 == kekka)
                {
                    // 処理なし
                }
                else
                {
                    // 処理なし
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }
    }
}
