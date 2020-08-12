//********************************************************************************
//
//  SYSTEM          : DCP
//  UNIT            : 受信情報解析処理
//
//********************************************************************************
using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using DCP;
using DCP.MngData.File;
using DCP.MngData.Table;

namespace DCP
{
    ///*******************************************************************************
    ///
    /// CLASS NAME      : 受信情報解析クラス
    /// CLASS ID        : CAnalyzeRecvData
    ///
    /// FUNCTION        : 
    /// <summary>
    /// NeXUS受信情報を取得し状態を解析するためのクラスとする。
    /// (アンサ系の伝文種別は別途アンサ解析クラスで解析するため対象外とする)
    /// </summary>
    /// 
    ///*******************************************************************************
    class CAnalyzeRecvData : IDisposable
    {
        #region <定数>

        /// <summary>処理起動要求番号</summary>
        private enum REQUESTID : ushort
        {
            /// <summary>スレッド停止処理起動要求</summary>
            THREADSTOP = 0,
            /// <summary>(PRC)列番情報解析処理起動要求</summary>
            RETUBAN,
            /// <summary>PRC状態情報解析処理起動要求</summary>
            PRCDATA,
            /// <summary>CTC状態情報解析処理(A-Sys)起動要求</summary>
            CTCADATA,
            /// <summary>CTC状態情報解析処理(B-Sys)起動要求</summary>
            CTCBDATA,
            /// <summary>CTC状態情報解析処理(C-Sys)起動要求</summary>
            CTCCDATA,
            /// <summary>CTC状態情報解析処理(D-Sys)起動要求</summary>
            CTCDDATA,
            /// <summary>警報情報解析処理起動要求</summary>
            ALARM,
            /// <summary>ダイヤ要求アンサ解析処理起動要求</summary>
            DIAREQANS,
            /// <summary>提案通知解析処理起動要求</summary>
            TEIAN,
            /// <summary>提案設定情報解析処理起動要求</summary>
            SETTEIAN,
            /// <summary>(TRA)列番情報解析処理起動要求</summary>
            TRARETUBAN,
            ///// <summary>(TID)列番情報解析処理起動要求</summary>
            //TIDIFRETUBAN,
            /// <summary>列番情報解析処理起動要求(他線区IF装置1)</summary>
            TIDIF1RETUBAN,
            /// <summary>列番情報解析処理起動要求(他線区IF装置2)</summary>
            TIDIF2RETUBAN,
            /// <summary>列番情報解析処理起動要求(他線区IF装置3)</summary>
            TIDIF3RETUBAN,
            /// <summary>列番情報解析処理起動要求(他線区IF装置4)</summary>
            TIDIF4RETUBAN,
            /// <summary>他線区IF状態情報解析処理(他線区IF装置1)起動要求</summary>
            TIDIF1DATA,
            /// <summary>他線区IF状態情報解析処理(他線区IF装置2)起動要求</summary>
            TIDIF2DATA,
            /// <summary>他線区IF状態情報解析処理(他線区IF装置3)起動要求</summary>
            TIDIF3DATA,
            /// <summary>他線区IF状態情報解析処理(他線区IF装置4)起動要求</summary>
            TIDIF4DATA,
            /// <summary>日替わり処理起動要求</summary>
            CHANGEDAYS,
            /// <summary>てこ／表示情報解析処理(A-Sys)起動要求</summary>
            TEKOHYOJIA,
            /// <summary>てこ／表示情報解析処理(B-Sys)起動要求</summary>
            TEKOHYOJIB,
            /// <summary>てこ／表示情報解析処理(C-Sys)起動要求</summary>
            TEKOHYOJIC,
            /// <summary>てこ／表示情報解析処理(D-Sys)起動要求</summary>
            TEKOHYOJID,
            /// <summary>起動要求リスト総数</summary>
            REQUESTIDCOUNT,
        };
        /// <summary>列番情報：表示フラグ</summary>
        [Flags]
        private enum DISPFLG : ushort 
        {
            /// <summary>ダイヤなし</summary>
            DIANASI = 0x0001,
            /// <summary>提案応答待ち</summary>
            TEIANOUTOU = 0x0002,
            /// <summary>列車抑止</summary>
            RESYAYOKUSI = 0x0004,
            /// <summary>順序保留</summary>
            JYUNJYOHORYU = 0x0008,
            /// <summary>滞泊</summary>
            TAIHAKU = 0x0010,
            /// <summary>仮列番</summary>
            KARIRETUBAN = 0x0020
        };
        /// <summary>窓状態(窓文字)：状態番号</summary>
        public enum MADOSTAT : ushort
        {
            /// <summary>該当なし</summary>
            UNKNOWN = 0,
            /// <summary>列車抑止</summary>
            RESYAYOKUSI,
            /// <summary>仮列番</summary>
            KARIRETUBAN,
            ///// <summary>ダイヤなし</summary>
            //DIANASI,
            ///// <summary>提案応答待ち</summary>
            //TEIANOUTOU,
            /// <summary>提案応答待ち</summary>
            TEIANOUTOU,
            /// <summary>ダイヤなし</summary>
            DIANASI,
            /// <summary>順序保留</summary>
            JYUNJYOHORYU,
            /// <summary>施行日前日</summary>
            YESTERDAY,
            /// <summary>施行日当日</summary>
            TODAY,
            /// <summary>施行日翌日</summary>
            TOMORROW,
            /// <summary>滞泊</summary>
            TAIHAKU,
            /// <summary>ダイヤあり</summary>
            DIAARI,
        };

        /// <summary>窓状態文字列</summary>
        public static readonly String[] MadoStatName =
        {
            @"通常状態",
            @"列車抑止扱い列番",
            @"仮列番",
            @"提案応答待ち",
            @"ダイヤなし",
            @"順序保留中",
            @"前日列番",
            @"当日列番",
            @"翌日列番",
            @"滞泊列番",
            @"通常ダイヤあり",
        };

        /// <summary>窓状態(窓文字)：状態番号</summary>
        private enum MADOCHIENSTAT : ushort
        {
            /// <summary>該当なし</summary>
            UNKNOWN = 0,
            /// <summary>遅延状態１</summary>
            LIMITSTAT1,
            /// <summary>遅延状態２</summary>
            LIMITSTAT2,
            /// <summary>遅延状態３</summary>
            LIMITSTAT3,
            /// <summary>遅延状態４</summary>
            LIMITSTAT4,
            /// <summary>遅延状態５</summary>
            LIMITSTAT5,
        };

        /// <summary>遅延判定時分段階番号</summary>
        private enum MADOCHIENLEVEL : ushort
        {
            /// <summary>該当なし</summary>
            UNKNOWN = 0,
            /// <summary>遅延時分が遅延判定時分１以上～遅延判定時分２未満</summary>
            LIMITLEVEL1,
            /// <summary>遅延時分が遅延判定時分２以上～遅延判定時分３未満</summary>
            LIMITLEVEL2,
            /// <summary>遅延時分が遅延判定時分３以上～遅延判定時分４未満</summary>
            LIMITLEVEL3,
            /// <summary>遅延時分が遅延判定時分４以上～遅延判定時分５未満</summary>
            LIMITLEVEL4,
            /// <summary>遅延時分が遅延判定時分５以上</summary>
            LIMITLEVEL5,
        };

        /// <summary>警報ビット情報チェック種別</summary>
        private enum ALMBITCHKTYPE : ushort
        {
            /// <summary>通常チェック</summary>
            NORMAL = 0,
            /// <summary>前回状態チェック</summary>
            BEFORE,
            /// <summary>今回状態チェック</summary>
            NOW,
            /// <summary>状変用前回状態チェック</summary>
            JH_BEFORE,
            /// <summary>状変用今回状態チェック</summary>
            JH_NOW,
        };

        /// <summary>ビット情報条件解析検知種別</summary>
        private enum BITDETECT : ushort
        {
            /// <summary>検出なし</summary>
            NODETECT = 0,
            /// <summary>初回フラグ（0=初回ではない、1=初回）</summary>
            FIRSTFLAG = 0x0001,
            /// <summary>初回検知フラグ（0=初回検知なし、1=初回検知あり）</summary>
            FIRSTDETECT = 0x0002,
            /// <summary>全ビット検出</summary>
            DETECTALL = (FIRSTFLAG | FIRSTDETECT),
        };

        #endregion

        #region <メンバ変数>

        /// <summary>スレッドオブジェクト</summary>
        private Thread m_Thread = null;
        /// <summary>スレッド起動フラグ</summary>
        private bool m_IsThreadStarted = false;
        /// <summary>スレッド停止用の待機イベント</summary>
        private AutoResetEvent m_ThreadStopped = null;

        /// <summary>前回CTC状態情報格納テーブル</summary>
        private CNxCTCInfoMng[] m_CTCInfoMngOldT = null;
        /// <summary>CTC状態情報格納テーブル</summary>
        private CNxCTCInfoMng[] m_CTCInfoMngT = null;
        /// <summary>PRC状態情報格納テーブル</summary>
        private CNxPRCInfoMng m_PRCInfoMngT = null;
        /// <summary>列番情報格納テーブル</summary>
        private CNxRetuInfoMng m_RetuInfoMngT = null;
        /// <summary>警報情報格納テーブル</summary>
        private List<CNxKeihouInfoMng> m_KeihouInfoMngT = null;
        /// <summary>提案通知格納テーブル</summary>
        private List<CNxTeianMng> m_TeianMngT = null;
        /// <summary>提案設定情報格納テーブル</summary>
        private CNxTeianSetRecvMng m_TeianSetRecvMngT = null;
        /// <summary>前回てこ／表示情報格納テーブル</summary>
        private CNxTekoHyojiInfoMng[] m_TekoHyojiInfoMngOldT = null;
        /// <summary>てこ／表示情報格納テーブル</summary>
        private CNxTekoHyojiInfoMng[] m_TekoHyojiInfoMngT = null;
        /// <summary>他線区Ｉ／Ｆ状態情報格納テーブル</summary>
        private CNxCTCInfoMng[] m_OtherIFStatInfoMngT = null;
        /// <summary>前回他線区Ｉ／Ｆ状態情報格納テーブル</summary>
        private CNxCTCInfoMng[] m_OtherIFStatInfoMngOldT = null;
        /// <summary>列番情報格納テーブル（他線区IF装置）</summary>
        public static CNxRetuInfoMng[] m_OtherIFRetuInfoMngT = null;
        /// <summary>前回列番情報格納テーブル（他線区IF装置）</summary>
        public static CNxRetuInfoMng[] m_OtherIFRetuInfoMngOldT = null;
        /// <summary>15秒後に表示される警報</summary>
        private DelayedAlert da = null;
        #endregion

        ///*******************************************************************************
        /// MODULE NAME         : コンストラクタ
        /// MODULE ID           : CAnalyzeRecvData
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
        public CAnalyzeRecvData()
        {
            try
            {
                // 起動フラグに「起動」をセット
                m_IsThreadStarted = true;
                // 停止用の待機イベントを生成
                m_ThreadStopped = new AutoResetEvent(false);

                // メンバテーブルの生成
                m_KeihouInfoMngT = new List<CNxKeihouInfoMng>();
                m_TeianMngT = new List<CNxTeianMng>();
                m_CTCInfoMngT = new CNxCTCInfoMng[CAppDat.CTCMAX];
                m_CTCInfoMngOldT = new CNxCTCInfoMng[CAppDat.CTCMAX];
                for (UInt16 i = 0; i < CAppDat.CTCMAX; i++)
                {
                    m_CTCInfoMngT[i] = new CNxCTCInfoMng();
                    m_CTCInfoMngOldT[i] = new CNxCTCInfoMng();
                }
                m_TekoHyojiInfoMngT = new CNxTekoHyojiInfoMng[CAppDat.PRCDENSOMAX];
                m_TekoHyojiInfoMngOldT = new CNxTekoHyojiInfoMng[CAppDat.PRCDENSOMAX];
                for (UInt16 i = 0; i < CAppDat.PRCDENSOMAX; i++)
                {
                    m_TekoHyojiInfoMngT[i] = new CNxTekoHyojiInfoMng();
                    m_TekoHyojiInfoMngOldT[i] = new CNxTekoHyojiInfoMng();
                }
                m_OtherIFStatInfoMngT = new CNxCTCInfoMng[CAppDat.TIDMAX];
                m_OtherIFStatInfoMngOldT = new CNxCTCInfoMng[CAppDat.TIDMAX];
                for (UInt16 i = 0; i < CAppDat.TIDMAX; i++)
                {
                    m_OtherIFStatInfoMngT[i] = new CNxCTCInfoMng();
                    m_OtherIFStatInfoMngOldT[i] = new CNxCTCInfoMng();
                }
                m_OtherIFRetuInfoMngT = new CNxRetuInfoMng[CAppDat.TIDMAX];
                m_OtherIFRetuInfoMngOldT = new CNxRetuInfoMng[CAppDat.TIDMAX];
                for (UInt16 i = 0; i < CAppDat.TIDMAX; i++)
                {
                    m_OtherIFRetuInfoMngT[i] = new CNxRetuInfoMng();
                    m_OtherIFRetuInfoMngOldT[i] = new CNxRetuInfoMng();
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
        /// MODULE NAME         : 受信情報解析処理
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
        /// 共有メモリ監視処理より起動イベントを受信したときに、受信情報の状態解析を行い
        /// 必要であれば表示処理に対して起動イベントを発行する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void Main()
        {
            AutoResetEvent[] WaitEvent = new AutoResetEvent[(UInt32)REQUESTID.REQUESTIDCOUNT];
            bool bRetUpdate = false;
            Int32 intRequestno = 0;             // 要求番号
            UInt16 ctcSysNo = 0;                // CTCシステム番号（1-4）
            UInt16 dcpmode = 0;                 // DCPモード
            UInt32 RequestFlg = 0;              // 要求フラグ
            String logMsg = String.Empty;       // ログ出力メッセージ

            try
            {
                // 待機ハンドルのコントロール配列を作成する
                WaitEvent[(UInt32)REQUESTID.RETUBAN] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.RETUBANRECVANALYZE];
                WaitEvent[(UInt32)REQUESTID.PRCDATA] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.PRCDATARECVANALYZE];
                WaitEvent[(UInt32)REQUESTID.CTCADATA] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.CTCADATARECVANALYZE];
                WaitEvent[(UInt32)REQUESTID.CTCBDATA] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.CTCBDATARECVANALYZE];
                WaitEvent[(UInt32)REQUESTID.CTCCDATA] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.CTCCDATARECVANALYZE];
                WaitEvent[(UInt32)REQUESTID.CTCDDATA] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.CTCDDATARECVANALYZE];
                WaitEvent[(UInt32)REQUESTID.ALARM] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.KEIHOUANALYZE];
                WaitEvent[(UInt32)REQUESTID.DIAREQANS] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.DIAREQANSANANALYZE];
                WaitEvent[(UInt32)REQUESTID.TEIAN] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.TEIANANALYZE];
                WaitEvent[(UInt32)REQUESTID.SETTEIAN] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.SETTEIANANALYZE];
                WaitEvent[(UInt32)REQUESTID.TRARETUBAN] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.TRARETUBANRECVANALYZE];
                //WaitEvent[(UInt32)REQUESTID.TIDIFRETUBAN] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.TIDIFRETUBANRECVANALYZE];
                WaitEvent[(UInt32)REQUESTID.TIDIF1RETUBAN] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.TIDIF1RETUBANRECVANALYZE];
                WaitEvent[(UInt32)REQUESTID.TIDIF2RETUBAN] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.TIDIF2RETUBANRECVANALYZE];
                WaitEvent[(UInt32)REQUESTID.TIDIF3RETUBAN] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.TIDIF3RETUBANRECVANALYZE];
                WaitEvent[(UInt32)REQUESTID.TIDIF4RETUBAN] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.TIDIF4RETUBANRECVANALYZE];
                WaitEvent[(UInt32)REQUESTID.TIDIF1DATA] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.TIDIF1DATARECVANALYZE];
                WaitEvent[(UInt32)REQUESTID.TIDIF2DATA] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.TIDIF2DATARECVANALYZE];
                WaitEvent[(UInt32)REQUESTID.TIDIF3DATA] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.TIDIF3DATARECVANALYZE];
                WaitEvent[(UInt32)REQUESTID.TIDIF4DATA] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.TIDIF4DATARECVANALYZE];
                WaitEvent[(UInt32)REQUESTID.CHANGEDAYS] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.CHANGEDDAYS];
                WaitEvent[(UInt32)REQUESTID.TEKOHYOJIA] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.TEKOHYOJIARECVANALYZE];
                WaitEvent[(UInt32)REQUESTID.TEKOHYOJIB] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.TEKOHYOJIBRECVANALYZE];
                WaitEvent[(UInt32)REQUESTID.TEKOHYOJIC] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.TEKOHYOJICRECVANALYZE];
                WaitEvent[(UInt32)REQUESTID.TEKOHYOJID] = CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.TEKOHYOJIDRECVANALYZE];
                WaitEvent[(UInt32)REQUESTID.THREADSTOP] = m_ThreadStopped;

                da = new DelayedAlert(this);
                da.Elapsed += (s, a) =>
                {
                    this.DispTekoLabelStatusAnalyze();                               // 表示てこラベル状態更新
                    CAppDat.HandleCtrl[(UInt16)CAppDat.WAITID.SHOWDISP].Set();       // 表示内容更新
                    var args = (DelayedAlertArgs)a;
                    CKeihouCtrl.OutputCtcKeihou((ushort)args.iCnt1, args.isAllJyokenD1);
                };

                da.CountStart();

                while (m_IsThreadStarted)
                {
                    // シグナル状態まで待機する
                    intRequestno = WaitHandle.WaitAny(WaitEvent);
                    // クラス変数に受信情報を展開する
                    ctcSysNo = 0;
                    bRetUpdate = GetRecvAnalyzeDataUpdate(intRequestno, ref ctcSysNo);
                    // 受信情報の展開成功？
                    if (true == bRetUpdate)
                    {
                        // 処理なし
                    }
                    else
                    {
                        logMsg = String.Format("受信処理の展開失敗 解析要求={0}", intRequestno);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                        continue;
                    }

                    // 表示更新要求フラグの初期化
                    RequestFlg = 0;

                    // ＤＣＰ設定の起動モードを取得する
                    dcpmode = CCommon.GetEqpActMode();                  // 装置動作モードを取得

                    // 要求に応じて解析処理を実施
                    switch (intRequestno)
                    {
                        case (Int32)REQUESTID.RETUBAN:                  // 列番情報(PRC)解析処理
                        case (Int32)REQUESTID.PRCDATA:                  // PRC状態情報解析処理
                        case (Int32)REQUESTID.CTCADATA:                 // CTC状態情報解析(A-Sys)処理
                        case (Int32)REQUESTID.CTCBDATA:                 // CTC状態情報解析(B-Sys)処理
                        case (Int32)REQUESTID.CTCCDATA:                 // CTC状態情報解析(C-Sys)処理
                        case (Int32)REQUESTID.CTCDDATA:                 // CTC状態情報解析(D-Sys)処理
                        case (Int32)REQUESTID.TEKOHYOJIA:               // てこ表示情報解析(A-Sys)処理
                        case (Int32)REQUESTID.TEKOHYOJIB:               // てこ表示情報解析(B-Sys)処理
                        case (Int32)REQUESTID.TEKOHYOJIC:               // てこ表示情報解析(C-Sys)処理
                        case (Int32)REQUESTID.TEKOHYOJID:               // てこ表示情報解析(D-Sys)処理
                            if (((Int32)REQUESTID.CTCADATA == intRequestno) ||
                                ((Int32)REQUESTID.CTCBDATA == intRequestno) ||
                                ((Int32)REQUESTID.CTCCDATA == intRequestno) ||
                                ((Int32)REQUESTID.CTCDDATA == intRequestno))
                            {
                                CtcEqpAnalyze(ctcSysNo);                // ＣＴＣ装置状態解析処理
                                CtcKeihouAnalyze(ctcSysNo);             // ＣＴＣ状態情報警報解析処理
                                CtcModeKeihouAnalyze(ctcSysNo);         // ＣＴＣモード警報解析処理
                                CtcModeAnalyze(ctcSysNo);               // ＣＴＣモード解析処理

                                // 接近毎に「鳴動する／しない」を設定できる「接近鳴動設定機能」を追加する改修対応
                                SekkinAnalyze(ctcSysNo);                // 接近警報解析処理

                            }
                            else if ((Int32)REQUESTID.RETUBAN == intRequestno)
                            {
                                MadoStatusAnalyze();                    // 窓解析処理
                            }
                            else if (((Int32)REQUESTID.TEKOHYOJIA == intRequestno) ||
                                     ((Int32)REQUESTID.TEKOHYOJIB == intRequestno) ||
                                     ((Int32)REQUESTID.TEKOHYOJIC == intRequestno) ||
                                     ((Int32)REQUESTID.TEKOHYOJID == intRequestno))
                            {
                                PrcDensoEqpAnalyze(ctcSysNo);           // PRC伝送部装置状態解析処理
                                PrcDensoKeihouAnalyze(ctcSysNo);        // PRC伝送部装置状態情報警報解析処理
                                PrcDensoModeKeihouAnalyze(ctcSysNo);    // PRC伝送部モード警報解析処理
                                PrcDensoModeAnalyze(ctcSysNo);          // PRC伝送部モード解析処理

                                // 接近毎に「鳴動する／しない」を設定できる「接近鳴動設定機能」を追加する改修対応
                                SekkinAnalyze(ctcSysNo);                       // 接近警報解析処理
                            }
                            else
                            {
                                // 処理なし
                            }

                            KasoPointAnalyze();                         // 仮想転てつ器解析処理
                            LabelStatusAnalyze();                       // 表示ラベル解析処理
                            EkiLabelStatusAnalyze();                    // 駅名ラベル解析処理
                            GraphicStatusAnalyze();                     // 表示グラフィック解析処理
                            SinroStatusAnalyze();                       // 進路解析処理
                            KidouStatusAnalyze();                       // 軌道回路解析処理
                            PointStatusAnalyze();                       // 転てつ器解析処理
                            HatuChakuStatusAnalyze();                   // 発着てこラベル解析処理

                            //CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DISPUPDATEREQUEST].WaitOne();
                            //RequestFlg = CAppDat.DispUpdateRequestT;
                            //CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DISPUPDATEREQUEST].ReleaseMutex();
                            RequestFlg = CCommon.ReadDispUpdateRequestT();
                            if (0 != RequestFlg)
                            {
                                // 表示要求フラグありでイベントセット
                                CAppDat.HandleCtrl[(UInt16)CAppDat.WAITID.SHOWDISP].Set();
                            }
                            else
                            {
                                // 要求なしは処理なし
                            }
                            break;

                        case (Int32)REQUESTID.ALARM:                    // 警報情報解析処理
                            PrcKeihouAnalyze();                         // 警報情報解析処理
                            break;

                        case (Int32)REQUESTID.DIAREQANS:                // ダイヤ要求アンサ解析処理
                        case (Int32)REQUESTID.TRARETUBAN:               // 列番情報(TRA)解析処理
                        //case (Int32)REQUESTID.TIDIFRETUBAN:             // 列番情報(TID)解析処理
                            logMsg = String.Format("解析処理なし 要求番号={0}", intRequestno);
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.INFORMATION, MethodBase.GetCurrentMethod(), logMsg);
                            break;

                        case (Int32)REQUESTID.TIDIF1DATA:               // 他線区IF状態情報(他線区IF装置1)解析処理
                        case (Int32)REQUESTID.TIDIF2DATA:               // 他線区IF状態情報(他線区IF装置2)解析処理
                        case (Int32)REQUESTID.TIDIF3DATA:               // 他線区IF状態情報(他線区IF装置3)解析処理
                        case (Int32)REQUESTID.TIDIF4DATA:               // 他線区IF状態情報(他線区IF装置4)解析処理
                        case (Int32)REQUESTID.TIDIF1RETUBAN:            // 列番情報(他線区IF装置1)解析処理
                        case (Int32)REQUESTID.TIDIF2RETUBAN:            // 列番情報(他線区IF装置2)解析処理
                        case (Int32)REQUESTID.TIDIF3RETUBAN:            // 列番情報(他線区IF装置3)解析処理
                        case (Int32)REQUESTID.TIDIF4RETUBAN:            // 列番情報(他線区IF装置4)解析処理

                            if (((Int32)REQUESTID.TIDIF1DATA == intRequestno) ||
                                ((Int32)REQUESTID.TIDIF2DATA == intRequestno) ||
                                ((Int32)REQUESTID.TIDIF3DATA == intRequestno) ||
                                ((Int32)REQUESTID.TIDIF4DATA == intRequestno))
                            {
                                TidIfStatusModeKeihouAnalyze(ctcSysNo); // モード警報解析処理
                                TidifEqpAnalyze(ctcSysNo);              // 他線区IF装置状態解析処理
                            }
                            else if (((Int32)REQUESTID.TIDIF1RETUBAN == intRequestno) ||
                                     ((Int32)REQUESTID.TIDIF2RETUBAN == intRequestno) ||
                                     ((Int32)REQUESTID.TIDIF3RETUBAN == intRequestno) ||
                                     ((Int32)REQUESTID.TIDIF4RETUBAN == intRequestno))
                            {

                                TidIfRetUntenKeihouAnalyze(ctcSysNo);   // 運転状態警報解析処理

                                // ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
                                // 特情機能定義による処理分岐
                                if (CAppDat.IFMADO_USED)                // 列番情報(他線区IF装置)反映機能
                                {
                                    TidMadoStatusAnalyze(ctcSysNo);     // TID窓解析処理
                                }
                                else
                                {
                                    // 処理なし
                                }
                                // ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
                            }
                            else
                            {
                                // 処理なし
                            }

                            KasoPointAnalyze();                         // 仮想転てつ器解析処理
                            LabelStatusAnalyze();                       // 表示ラベル解析処理
                            EkiLabelStatusAnalyze();                    // 駅名ラベル解析処理
                            GraphicStatusAnalyze();                     // 表示グラフィック解析処理
                            SinroStatusAnalyze();                       // 進路解析処理
                            KidouStatusAnalyze();                       // 軌道回路解析処理
                            PointStatusAnalyze();                       // 転てつ器解析処理
                            HatuChakuStatusAnalyze();                   // 発着てこラベル解析処理

                            RequestFlg = CCommon.ReadDispUpdateRequestT();
                            if (0 != RequestFlg)
                            {
                                // 表示要求フラグありでイベントセット
                                CAppDat.HandleCtrl[(UInt16)CAppDat.WAITID.SHOWDISP].Set();
                            }
                            else
                            {
                                // 要求なしは処理なし
                            }
                            break;

                        case (Int32)REQUESTID.TEIAN:                    // 提案通知解析処理
                        case (Int32)REQUESTID.SETTEIAN:                 // 提案設定情報解析処理
                            if (((UInt16)CDCPiniMng.MODE.SDP == dcpmode) &&
                                ((Int32)REQUESTID.TEIAN == intRequestno))
                            {
                                // 処理なし
                            }
                            else
                            {
                                PrcRecvAnalyze(intRequestno);           // PRC受信情報解析処理
                            }
                            break;

                        case (Int32)REQUESTID.CHANGEDAYS:               // 日替わり処理
                            MadoStatusAnalyze();                        // 窓解析処理

                            //CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DISPUPDATEREQUEST].WaitOne();
                            //RequestFlg = CAppDat.DispUpdateRequestT;
                            //CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DISPUPDATEREQUEST].ReleaseMutex();
                            RequestFlg = CCommon.ReadDispUpdateRequestT();
                            if (0 != RequestFlg)
                            {
                                // 表示要求フラグありでイベントセット
                                CAppDat.HandleCtrl[(UInt16)CAppDat.WAITID.SHOWDISP].Set();
                            }
                            else
                            {
                                // 要求なしは処理なし
                            }
                            break;

                        case (Int32)REQUESTID.THREADSTOP:               // スレッド停止処理
                            break;

                        default:                                        // その他の処理
                            logMsg = String.Format("想定外要求={0}", intRequestno);
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                            break;
                    }

                    Thread.Sleep(10);
                }
            }
            catch (ThreadAbortException ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.INFORMATION, MethodBase.GetCurrentMethod(), ex.Message);
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 表示ラベル解析処理
        /// MODULE ID           : DispLabelStatusAnalyze
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
        /// 受信情報を解析して表示ラベルパーツの現状態を判断し状態テーブルに格納する。
        /// 1件以上のレコード更新がある場合は表示ラベル表示処理を起動する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void DispLabelStatusAnalyze()
        {
            UInt16 updatecnt = 0;
            UInt16 stateno = 0;
            CSettingMng setData = null;
            CStatusLabelData work = new CStatusLabelData();
            bool IsAgree = false;
            CStatusLabelMng locallabelt = null;
            CPartsLinkMng partslink = null;
            String syncErrorMsg = String.Empty;
            String logMsg = String.Empty;

            try
            {
                ///////////////////////////////////////////////////////////
                // ローカルに現状の表示ラベル状態テーブルを取得
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.LABELSTATUS].WaitOne();
                try
                {
                    locallabelt = (CStatusLabelMng)CAppDat.LabelStatusT.Clone();
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.LABELSTATUS].ReleaseMutex();
                }

                ///////////////////////////////////////////////////////////
                // 状態テーブルの更新条件をチェック
                foreach (UInt16 Crno in CAppDat.LabelPPLF.Keys)
                {
                    foreach (String labelkey in CAppDat.LabelPPLF[Crno].Keys)
                    {
                        partslink = CAppDat.LabelPPLF[Crno][labelkey];
                        work.Clear();
                        IsAgree = false;
                        stateno = 0;
                        // 設備情報ファイルにPPLファイルで設定した区分番号（キー）が存在するか？
                        // 存在する場合は続行、存在しない場合は次データに移行する
                        bool KeyExist = CAppDat.LabelInfoF.ContainsKey(partslink.Kbno);
                        if (true == KeyExist)
                        {
                            // 処理なし
                        }
                        else
                        {
                            continue;
                        }

                        ///////////////////////////////////////////////////////////
                        // 表示情報の状態チェック
                        bool bRet = CAppDat.LabelInfoF[partslink.Kbno].Disp.TryGetValue(0, out setData);
                        // 通常名称を取得
                        if (true == bRet)
                        {
                            work.LabelString = setData.Name;
                        }
                        else
                        {
                            work.LabelString = String.Empty;
                        }

                        // 表示情報に属する状態数分繰り返す
                        foreach (UInt16 keyid in CAppDat.LabelInfoF[partslink.Kbno].Disp.Keys)
                        {
                            // キーIDの状態番号が０は無効とし処理なし
                            if (0 == keyid)
                            {
                                continue;
                            }
                            // キーIDの状態番号が０以外は条件情報を取得する
                            else
                            {
                                setData = CAppDat.LabelInfoF[partslink.Kbno].Disp[keyid];
                                stateno = keyid;
                            }

                            // 表示条件に属する設定条件のステータスをチェック
                            IsAgree = this.CheckSettingDictionary(setData.Condition);
                            // 表示条件が一致あり→[状態件判定終了]
                            if (true == IsAgree)
                            {
                                // 状態番号を取得
                                work.State = stateno;
                                // 表示文字列を取得
                                work.LabelString = setData.Name;
                                break;
                            }
                            else
                            {
                                // 処理なし
                            }
                        }

                        ///////////////////////////////////////////////////////////
                        // 前回情報と比較して状態に変化ありのときテーブル更新
                        // 状態番号、状態文字列のいずれかが不一致？
                        if ((locallabelt.Condition[Crno][labelkey].State != work.State) ||
                            (locallabelt.Condition[Crno][labelkey].LabelString != work.LabelString))
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.LABELSTATUS].WaitOne();
                            try
                            {
                                if (true == CAppDat.LabelStatusT.Condition[Crno].ContainsKey(labelkey))
                                {
                                    CAppDat.LabelStatusT.Condition[Crno][labelkey].State = work.State;
                                    CAppDat.LabelStatusT.Condition[Crno][labelkey].LabelString = work.LabelString;
                                    // 状態を変化させたので変化フラグをオン
                                    CAppDat.LabelStatusT.Condition[Crno][labelkey].IsChangeable = true;
                                    updatecnt++;
                                }
                                else
                                {
                                    logMsg = String.Format("LabelStatusTキー未登録 : ({0})", labelkey);
                                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
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
                                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.LABELSTATUS].ReleaseMutex();
                            }
                        }
                        else
                        {
                            // 処理なし
                        }
                    }
                }

                ///////////////////////////////////////////////////////////
                // １件以上の更新ありのとき、表示ラベル表示処理起動
                if (0 < updatecnt)
                {
                    CCommon.WriteDispUpdateRequest((UInt32)CAppDat.REQUESTFLG.LABELDISP, 1);
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
            finally
            {
                work = null;
                setData = null;
                locallabelt = null;
                partslink = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 駅名ラベル解析処理
        /// MODULE ID           : EkiLabelStatusAnalyze
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
        /// 受信情報を解析して駅名ラベルパーツの現状態を判断し状態テーブルに格納する。
        /// １件以上のレコード更新がある場合は駅名ラベル表示処理を起動する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void EkiLabelStatusAnalyze()
        {
            UInt16 updatecnt = 0;
            UInt16 stateno = 0;
            CSettingMng setData = null;
            CStatusLabelData work = new CStatusLabelData();
            bool IsAgree = false;
            CStatusLabelMng localekilabelt = null;
            CPartsLinkMng partslink = null;
            String syncErrorMsg = String.Empty;
            String logMsg = String.Empty;

            try
            {
                ///////////////////////////////////////////////////////////
                // ローカルに現状の駅名ラベル状態テーブルを取得
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.EKILABELSTATUS].WaitOne();
                try
                {
                    localekilabelt = (CStatusLabelMng)CAppDat.EkiLabelStatusT.Clone();
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.EKILABELSTATUS].ReleaseMutex();
                }

                ///////////////////////////////////////////////////////////
                // 状態テーブルの更新条件をチェック
                foreach (UInt16 Crno in CAppDat.EkiLabelPPLF.Keys)
                {
                    foreach (String ekilabelkey in CAppDat.EkiLabelPPLF[Crno].Keys)
                    {
                        partslink = CAppDat.EkiLabelPPLF[Crno][ekilabelkey];
                        work.Clear();
                        IsAgree = false;
                        stateno = 0;
                        // 設備情報ファイルにPPLファイルで設定した区分番号（キー）が存在するか？
                        // 存在する場合は続行、存在しない場合は次データに移行する
                        bool KeyExist = CAppDat.EkiLabelInfoF.ContainsKey(partslink.Kbno);
                        if (true == KeyExist)
                        {
                            // 処理なし
                        }
                        else
                        {
                            //logMsg = String.Format("ppr_駅名ラベル.xml 区分NO.={0} 未登録, 画面NO.={1}", partslink.Kbno, partslink.Crno);
                            //CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                            continue;
                        }

                        ///////////////////////////////////////////////////////////
                        // 表示情報の状態チェック
                        bool bRet = CAppDat.EkiLabelInfoF[partslink.Kbno].Disp.TryGetValue(0, out setData);
                        // 通常名称を取得
                        if (true == bRet)
                        {
                            work.LabelString = setData.Name;
                        }
                        else
                        {
                            work.LabelString = String.Empty;
                        }

                        // 表示情報に属する状態数分繰り返す
                        foreach (UInt16 keyid in CAppDat.EkiLabelInfoF[partslink.Kbno].Disp.Keys)
                        {
                            setData = null;
                            // キーIDの状態番号が０は無効とし処理なし
                            if (0 == keyid)
                            {
                                continue;
                            }
                            // キーIDの状態番号が０以外は条件情報を取得する
                            else
                            {
                                setData = CAppDat.EkiLabelInfoF[partslink.Kbno].Disp[keyid];
                                stateno = keyid;
                            }

                            // 表示条件に属する設定条件のステータスをチェック
                            IsAgree = this.EkiCheckSettingDictionary(setData.Condition);
                            // 表示条件が一致あり→[状態件判定終了]
                            if (true == IsAgree)
                            {
                                // 状態番号を取得
                                work.State = stateno;
                                // 表示文字列を取得
                                work.LabelString = setData.Name;

                                if (work.LabelString == "CENTER")
                                {
                                    CEqpStateDisp.DispEqpStatus((UInt16)CEqpStateDisp.RENEWKIND.EQPSTATUS, 1, (UInt16)CAreaMng.EQPLABELNO.CENTERPOWER, true);
                                }
                                else if (work.LabelString == "UPS")
                                {
                                    CEqpStateDisp.DispEqpStatus((UInt16)CEqpStateDisp.RENEWKIND.EQPSTATUS, 1, (UInt16)CAreaMng.EQPLABELNO.UPS, true);
                                }
                                else if (work.LabelString == "駅不能")
                                {
                                    CEqpStateDisp.DispEqpStatus((UInt16)CEqpStateDisp.RENEWKIND.EQPSTATUS, 1, (UInt16)CAreaMng.EQPLABELNO.CTCEki, true);
                                }
                                break;
                            }
                            else
                            {
                                if (work.LabelString == "CENTER")
                                {
                                    CEqpStateDisp.DispEqpStatus((UInt16)CEqpStateDisp.RENEWKIND.EQPSTATUS, 1, (UInt16)CAreaMng.EQPLABELNO.CENTERPOWER, false);
                                }
                                else if (work.LabelString == "UPS")
                                {
                                    CEqpStateDisp.DispEqpStatus((UInt16)CEqpStateDisp.RENEWKIND.EQPSTATUS, 1, (UInt16)CAreaMng.EQPLABELNO.UPS, false);
                                }
                                else if (work.LabelString == "駅不能")
                                {
                                    CEqpStateDisp.DispEqpStatus((UInt16)CEqpStateDisp.RENEWKIND.EQPSTATUS, 1, (UInt16)CAreaMng.EQPLABELNO.CTCEki, false);
                                }
                            }
                        }

                        ///////////////////////////////////////////////////////////
                        // 前回情報と比較して状態に変化ありのときテーブル更新
                        // 状態番号、状態文字列のいずれかが不一致？
                        if ((localekilabelt.Condition[Crno][ekilabelkey].State != work.State) ||
                            (localekilabelt.Condition[Crno][ekilabelkey].LabelString != work.LabelString))
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.EKILABELSTATUS].WaitOne();
                            try
                            {
                                if (true == CAppDat.EkiLabelStatusT.Condition[Crno].ContainsKey(ekilabelkey))
                                {
                                    CAppDat.EkiLabelStatusT.Condition[Crno][ekilabelkey].State = work.State;
                                    CAppDat.EkiLabelStatusT.Condition[Crno][ekilabelkey].LabelString = work.LabelString;
                                    // 状態を変化させたので変化フラグをオン
                                    CAppDat.EkiLabelStatusT.Condition[Crno][ekilabelkey].IsChangeable = true;
                                    updatecnt++;
                                }
                                else
                                {
                                    logMsg = String.Format("EkiLabelStatusTキー未登録 : ({0})", ekilabelkey);
                                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
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
                                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.EKILABELSTATUS].ReleaseMutex();
                            }
                        }
                        else
                        {
                            // 処理なし
                        }
                    }
                }

                ///////////////////////////////////////////////////////////
                // １件以上の更新ありのとき、駅名ラベル表示処理起動
                if (0 < updatecnt)
                {
                    CCommon.WriteDispUpdateRequest((UInt32)CAppDat.REQUESTFLG.EKILABELDISP, 1);
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
            finally
            {
                work = null;
                setData = null;
                localekilabelt = null;
                partslink = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 表示グラフィック解析処理
        /// MODULE ID           : GraphicStatusAnalyze
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
        /// 受信情報を解析して表示グラフィックパーツの現状態を判断し状態テーブルに格納する。
        /// １件以上のレコード更新がある場合は表示ラベル表示処理を起動する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void GraphicStatusAnalyze()
        {
            UInt16 updatecnt = 0;
            UInt16 stateno = 0;
            CSettingMng setData = null;
            CStatusGraphicData work = new CStatusGraphicData();
            bool IsAgree = false;
            CStatusGraphicMng localgraphict = null;
            CPartsLinkMng partslink = null;
            String syncErrorMsg = String.Empty;
            String logMsg = String.Empty;

            try
            {
                ///////////////////////////////////////////////////////////
                // ローカルに現状の表示グラフィック状態テーブルを取得
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DISPGRASTATUS].WaitOne();
                try
                {
                    localgraphict = (CStatusGraphicMng)CAppDat.DispGraStatusT.Clone();
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DISPGRASTATUS].ReleaseMutex();
                }

                ///////////////////////////////////////////////////////////
                // 状態テーブルの更新条件をチェック
                foreach (UInt16 Crno in CAppDat.DispGraPPLF.Keys)
                {
                    foreach (String graphickey in CAppDat.DispGraPPLF[Crno].Keys)
                    {
                        partslink = CAppDat.DispGraPPLF[Crno][graphickey];
                        work.Clear();
                        IsAgree = false;
                        stateno = 0;
                        // 設備情報ファイルにPPLファイルで設定した区分番号（キー）が存在するか？
                        // 存在する場合は続行、存在しない場合は次データに移行する
                        bool KeyExist = CAppDat.DispGraInfoF.ContainsKey(partslink.Kbno);
                        if (true == KeyExist)
                        {
                            // 処理なし
                        }
                        else
                        {
                            continue;
                        }

                        ///////////////////////////////////////////////////////////
                        // 表示情報の状態チェック
                        // 表示情報に属する状態数分繰り返す
                        foreach (UInt16 keyid in CAppDat.DispGraInfoF[partslink.Kbno].Disp.Keys)
                        {
                            // キーIDの状態番号が０は無効とし処理なし
                            if (0 == keyid)
                            {
                                continue;
                            }
                            // キーIDの状態番号が０以外は条件情報を取得する
                            else
                            {
                                setData = CAppDat.DispGraInfoF[partslink.Kbno].Disp[keyid];
                                stateno = keyid;
                            }

                            // 表示条件に属する設定条件のステータスをチェック
                            IsAgree = this.CheckSettingDictionary(setData.Condition);
                            // 表示条件が一致あり→[状態件判定終了]
                            if (true == IsAgree)
                            {
                                // 状態番号を取得
                                work.State = stateno;
                                break;
                            }
                            else
                            {
                                // 処理なし
                            }
                        }

                        ///////////////////////////////////////////////////////////
                        // 前回情報と比較して状態に変化ありのときテーブル更新
                        // 状態番号が不一致？
                        if (localgraphict.Condition[Crno][graphickey].State != work.State)
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DISPGRASTATUS].WaitOne();
                            try
                            {
                                if (true == CAppDat.DispGraStatusT.Condition[Crno].ContainsKey(graphickey))
                                {
                                    CAppDat.DispGraStatusT.Condition[Crno][graphickey].State = work.State;
                                    // 状態を変化させたので変化フラグをオン
                                    CAppDat.DispGraStatusT.Condition[Crno][graphickey].IsChangeable = true;
                                    updatecnt++;
                                }
                                else
                                {
                                    logMsg = String.Format("DispGraStatusTキー未登録 : ({0})", graphickey);
                                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
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
                                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DISPGRASTATUS].ReleaseMutex();
                            }
                        }
                        else
                        {
                            // 処理なし
                        }
                    }
                }

                ///////////////////////////////////////////////////////////
                // １件以上の更新ありのとき、表示グラフィック表示処理起動
                if (0 < updatecnt)
                {
                    CCommon.WriteDispUpdateRequest((UInt32)CAppDat.REQUESTFLG.GRAPHICDISP, 1);
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
            finally
            {
                work = null;
                setData = null;
                localgraphict = null;
                partslink = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 進路解析処理
        /// MODULE ID           : SinroStatusAnalyze
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
        /// 受信情報を解析して進路パーツの現状態を判断し状態テーブルに格納する。
        /// １件以上のレコード更新がある場合は進路表示処理を起動する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void SinroStatusAnalyze()
        {
            CPropertySinroData property = null;
            UInt16 updatecnt = 0;
            UInt16 stateno = 0;
            List<CSetData> conditionlist = null;
            CStatusSinroData work = new CStatusSinroData();
            bool IsAgree = false;
            CStatusSinroMng localsinrot = null;
            CPartsLinkMng partslink = null;
            String syncErrorMsg = String.Empty;
            String logMsg = String.Empty;

            try
            {
                ///////////////////////////////////////////////////////////
                // ローカルに現状の進路状態テーブルを取得
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SINROSTATUS].WaitOne();
                try
                {
                    localsinrot = (CStatusSinroMng)CAppDat.SinroStatusT.Clone();
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SINROSTATUS].ReleaseMutex();
                }

                ///////////////////////////////////////////////////////////
                // 状態テーブルの更新条件をチェック
                
                foreach (UInt16 Crno in CAppDat.SinroPPLF.Keys)
                {
                    foreach (String sinrokey in CAppDat.SinroPPLF[Crno].Keys)
                    {
                        partslink = CAppDat.SinroPPLF[Crno][sinrokey];
                        work.Clear();
                        IsAgree = false;
                        stateno = 0;
                        // 設備情報ファイルにPPLファイルで設定した区分番号（キー）が存在するか？
                        // 存在する場合は続行、存在しない場合は次データに移行する
                        bool KeyExist = CAppDat.SinroInfoF.ContainsKey(partslink.Kbno);
                        if (true == KeyExist)
                        {
                            // 処理なし
                        }
                        else
                        {
                            continue;
                        }

                        ///////////////////////////////////////////////////////////
                        // 進路区分のチェック
                        // 進路情報に属する進路区分数分繰り返す
                        for (UInt16 cnt1 = 0; cnt1 < CAppDat.SinroInfoF[partslink.Kbno].SinroKbn.Count; cnt1++)
                        {
                            IsAgree = false;
                            stateno = 0;

                            ///////////////////////////////////////////////////////////
                            // 表示情報のチェック
                            // 進路区分情報を取得
                            property = CAppDat.SinroInfoF[partslink.Kbno].SinroKbn[cnt1];

                            // 進路区分に属する表示情報数分繰り返す
                            foreach (UInt16 keyid in property.Disp.Keys)
                            {
                                // キーIDの状態番号が０は無効とし処理なし
                                if (0 == keyid)
                                {
                                    continue;
                                }
                                // キーIDの状態番号が０以外は記憶する
                                else
                                {
                                    stateno = keyid;
                                }

                                foreach (UInt16 keys in property.Disp[stateno].Condition.Keys)
                                {
                                    // 表示条件リストを取得
                                    conditionlist = property.Disp[stateno].Condition[keys];
                                    // 表示条件に属する設定条件のステータスをチェックする
                                    if (true == this.CheckSettingStateList(conditionlist))
                                    {
                                        // 状態番号の優先度が前回より高い、または初期値？
                                        if ((stateno < work.State) || (0 == work.State))
                                        {
                                            // 状態一致→[設定条件判定終了]
                                            IsAgree = true;
                                            break;
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
                                // 表示条件が一致あり→[状態件判定終了]
                                if (true == IsAgree)
                                {
                                    // 状態番号を取得
                                    work.State = stateno;
                                    break;
                                }
                                else
                                {
                                    // 処理なし
                                }
                            }

                            ///////////////////////////////////////////////////////////
                            // 進路情報(抑止状態)のチェック
                            IsAgree = false;
                            stateno = 0;
                            foreach (UInt16 keyid in property.Sinro.Keys)
                            {
                                // キーIDの状態番号が０は無効とし処理なし
                                if (0 == keyid)
                                {
                                    continue;
                                }
                                // キーIDの状態番号が０以外は記憶する
                                else
                                {
                                    stateno = keyid;
                                }

                                foreach (UInt16 keys in property.Sinro[stateno].Condition.Keys)
                                {
                                    // 表示条件リストを取得
                                    conditionlist = property.Sinro[stateno].Condition[keys];
                                    // 表示条件に属する設定条件のステータスをチェックする
                                    if (true == this.CheckSettingStateList(conditionlist))
                                    {
                                        // 状態一致→[設定条件判定終了]
                                        IsAgree = true;
                                        break;
                                    }
                                    else
                                    {
                                        // 処理なし
                                    }
                                }

                                // 表示条件が一致あり→[状態件判定終了]
                                if (true == IsAgree)
                                {
                                    // 進路抑止有無を設定
                                    work.DispYokusi = 1;
                                    break;
                                }
                                else
                                {
                                    // 処理なし
                                }
                            }
                        }

                        ///////////////////////////////////////////////////////////
                        // 前回情報と比較して状態に変化ありのときテーブル更新
                        // 状態番号、進路抑止有無のいずれかが不一致？
                        if ((localsinrot.Condition[Crno][sinrokey].State != work.State) ||
                            (localsinrot.Condition[Crno][sinrokey].DispYokusi != work.DispYokusi))
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SINROSTATUS].WaitOne();
                            try
                            {
                                if (true == CAppDat.SinroStatusT.Condition[Crno].ContainsKey(sinrokey))
                                {
                                    CAppDat.SinroStatusT.Condition[Crno][sinrokey].State = work.State;
                                    CAppDat.SinroStatusT.Condition[Crno][sinrokey].DispYokusi = work.DispYokusi;
                                    // 状態を変化させたので変化フラグをオン
                                    CAppDat.SinroStatusT.Condition[Crno][sinrokey].IsChangeable = true;
                                    updatecnt++;
                                }
                                else
                                {
                                    logMsg = String.Format("SinroStatusTキー未登録 : ({0})", sinrokey);
                                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
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
                                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SINROSTATUS].ReleaseMutex();
                            }
                        }
                        else
                        {
                            // 処理なし
                        }
                    }
                }

                ///////////////////////////////////////////////////////////
                // １件以上の更新ありのとき、進路表示処理起動
                if (0 < updatecnt)
                {
                    //CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DISPUPDATEREQUEST].WaitOne();
                    //CAppDat.DispUpdateRequestT |= (UInt32)CAppDat.REQUESTFLG.SINRODISP;
                    //CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DISPUPDATEREQUEST].ReleaseMutex();
                    CCommon.WriteDispUpdateRequest((UInt32)CAppDat.REQUESTFLG.SINRODISP, 1);
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
            finally
            {
                work = null;
                property = null;
                conditionlist = null;
                localsinrot = null;
                partslink = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 軌道回路解析処理
        /// MODULE ID           : KidouStatusAnalyze
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
        /// 受信情報を解析して軌道回路パーツの現状態を判断し状態テーブルに格納する。
        /// １件以上のレコード更新がある場合は軌道回路表示処理を起動する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void KidouStatusAnalyze()
        {
            UInt16 updatecnt = 0;
            UInt16 stateno = 0;
            UInt16 patternno = 0;
            UInt16 senpeistateno = 0;
            CSettingMng setData = null;
            Dictionary<UInt16, CSettingMng> senpeilist = null;
            Dictionary<UInt16, CSettingMng> patternlist = null;
            bool IsAgree = false;
            CStatusKidouMng localkidout = null;
            CPartsLinkMng partslink = null;
            bool IsTrackCurcuit = false;
            bool IsShinroCurcuit = false;
            String syncErrorMsg = String.Empty;
            String logMsg = String.Empty;

            try
            {
                ///////////////////////////////////////////////////////////
                // ローカルに現状の軌道回路状態テーブルを取得
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.KIDOUSTATUS].WaitOne();
                try
                {
                    localkidout = (CStatusKidouMng)CAppDat.KidouStatusT.Clone();
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.KIDOUSTATUS].ReleaseMutex();
                }

                ///////////////////////////////////////////////////////////
                // 状態テーブルの更新条件をチェック
                
                foreach (UInt16 Crno in CAppDat.KidouPPLF.Keys)
                {
                    foreach (String kidoukey in CAppDat.KidouPPLF[Crno].Keys)
                    {
                        partslink = CAppDat.KidouPPLF[Crno][kidoukey];
                        // 設備情報ファイルにPPLファイルで設定した区分番号（キー）が存在するか？
                        // 存在する場合は続行、存在しない場合は次データに移行する
                        bool KeyExist = CAppDat.KidouInfoF.ContainsKey(partslink.Kbno);
                        if (true == KeyExist)
                        {
                            // 処理なし
                        }
                        else
                        {
                            continue;
                        }

                        CStatusKidouMng checkKidout = new CStatusKidouMng();
                        checkKidout.Clear();
                        ///////////////////////////////////////////////////////////
                        IsAgree = false;
                        // 線路閉鎖条件のチェック
                        UInt16 localSenpeiState = 0;
                        foreach (UInt16 keyid in CAppDat.KidouInfoF[partslink.Kbno].Senpei.Keys)
                        {
                            senpeistateno = keyid;
                            senpeilist = CAppDat.KidouInfoF[partslink.Kbno].Senpei;
                            // 表示条件に属する設定条件のステータスをチェック
                            IsAgree = this.CheckSettingDictionary(senpeilist[keyid].Condition);
                            // 表示条件が一致あり→[状態件判定終了]
                            if (true == IsAgree)
                            {
                                // 線路閉鎖有無を取得
                                localSenpeiState = senpeistateno;
                                break;
                            }
                            else
                            {
                                // 処理なし
                            }
                        }

                        ///////////////////////////////////////////////////////////
                        // パターン別情報のチェック
                        foreach (UInt16 keyid in CAppDat.KidouInfoF[partslink.Kbno].Pattern.Keys)
                        {
                            IsAgree = false;
                            patternno = keyid;
                            patternlist = CAppDat.KidouInfoF[partslink.Kbno].Pattern[keyid];
                            // パターン別情報に属する状態数分繰り返す
                            UInt16 localKidouState = 0;
                            foreach (UInt16 ptKeyid in patternlist.Keys)
                            {
                                // キーIDの状態番号が０は無効とし処理なし
                                if (0 == ptKeyid)
                                {
                                    continue;
                                }
                                // キーIDの状態番号が０以外は記憶する
                                else
                                {
                                    setData = patternlist[ptKeyid];
                                    stateno = ptKeyid;
                                }

                                // 表示条件に属する設定条件のステータスをチェック
                                IsAgree = this.CheckSettingDictionary(setData.Condition);
                                // 表示条件が一致あり→[状態件判定終了]
                                if (true == IsAgree)
                                {
                                    // 状態番号を取得
                                    localKidouState = stateno;
                                    break;
                                }
                                else
                                {
                                    // 処理なし
                                }
                            }

                            CStatusKidouData work = new CStatusKidouData();
                            work.SenpeiState = localSenpeiState;
                            work.State = localKidouState;
                            Tuple<String, UInt16> patternkey = new Tuple<String, UInt16>(kidoukey, patternno);
                            if (true == checkKidout.Condition.ContainsKey(Crno))
                            {
                                Dictionary<Tuple<String, UInt16>, CStatusKidouData> value = checkKidout.Condition[Crno];
                                if (true == value.ContainsKey(patternkey))
                                {
                                    value[patternkey] = work;
                                }
                                else
                                {
                                    value.Add(patternkey, work);
                                }
                                checkKidout.Condition[Crno] = value;
                            }
                            else
                            {
                                Dictionary<Tuple<String, UInt16>, CStatusKidouData> value = new Dictionary<Tuple<string, ushort>, CStatusKidouData>();
                                value.Add(patternkey, work);
                                checkKidout.Condition.Add(Crno, value);
                            }
                            patternkey = null;
                            work = null;
                        }

                        // 軌道回路の状態を中間ファイルを使用して割付ける
                        // 登録キーが軌道パターン定義に存在しない場合は次に読み飛ばす
                        bool trackKeyExist = CAppDat.TrackPatternF.ContainsKey(partslink.Kbno);
                        if (true == trackKeyExist)
                        {
                            // 処理なし
                        }
                        else
                        {
                            //logMsg = String.Format("sec_軌道パターン.xml 区分NO.={0} 未登録", partslink.Kbno);
                            //CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                            continue;
                        }

                        // 設備情報のルートパターン解析結果は html 上のラインパターンに合致しない。
                        // 中間ファイルを使用して解析した結果で一番優先度の高いものを採用し状態テーブルに反映させる
                        foreach (UInt16 trackPatternKey in CAppDat.TrackPatternF[partslink.Kbno].PartsList.Keys)
                        {
                            UInt16 senpeiState = 0xFFFF;        // 線路閉鎖状態値
                            UInt16 kidouState = 0xFFFF;         // 軌道回路状態値
                            for (UInt16 cnt = 0; cnt < CAppDat.TrackPatternF[partslink.Kbno].PartsList[trackPatternKey].Count; cnt++)
                            {
                                UInt16 trackno = CAppDat.TrackPatternF[partslink.Kbno].PartsList[trackPatternKey][cnt];
                                Tuple<String, UInt16> chkKeyValue = new Tuple<String, UInt16>(kidoukey, trackno);

                                if (true == checkKidout.Condition.ContainsKey(Crno))
                                {
                                    // 処理なし
                                }
                                else
                                {
                                    continue;
                                }

                                // チェック対象のルートパターンが解析テーブルに存在しない場合は次データに移行
                                bool checkKeyExist = checkKidout.Condition[Crno].ContainsKey(chkKeyValue);
                                if (true == checkKeyExist)
                                {
                                    // 処理なし
                                }
                                else
                                {
                                    continue;
                                }

                                // 線路閉鎖状態の状態番号更新
                                if ((0 < checkKidout.Condition[Crno][chkKeyValue].SenpeiState) && (checkKidout.Condition[Crno][chkKeyValue].SenpeiState < senpeiState))
                                {
                                    senpeiState = checkKidout.Condition[Crno][chkKeyValue].SenpeiState;
                                }
                                else
                                {
                                    // 処理なし
                                }
                                // 軌道回路の状態番号更新
                                if ((0 < checkKidout.Condition[Crno][chkKeyValue].State) && (checkKidout.Condition[Crno][chkKeyValue].State < kidouState))
                                {
                                    kidouState = checkKidout.Condition[Crno][chkKeyValue].State;
                                }
                                else
                                {
                                    // 処理なし
                                }
                                chkKeyValue = null;
                            }

                            // 線路閉鎖状態が初期値のままなら通常状態に初期化
                            if (0xFFFF == senpeiState)
                            {
                                senpeiState = 0;
                            }
                            else
                            {
                                // 処理なし
                            }
                            // 軌道回路状態が初期値のままなら通常状態に初期化
                            if (0xFFFF == kidouState)
                            {
                                kidouState = 0;
                            }
                            else
                            {
                                // 処理なし
                            }

                            // 前回情報と比較して状態に変化ありのときテーブル更新
                            // 状態番号、線路閉鎖状態有無のいずれかが不一致？
                            Tuple<String, UInt16> keyValue = new Tuple<String, UInt16>(kidoukey, trackPatternKey);
                            if ((localkidout.Condition[Crno][keyValue].State != kidouState) || (localkidout.Condition[Crno][keyValue].SenpeiState != senpeiState))
                            {
                                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.KIDOUSTATUS].WaitOne();
                                try
                                {
                                    if (true == CAppDat.KidouStatusT.Condition[Crno].ContainsKey(keyValue))
                                    {
                                        CAppDat.KidouStatusT.Condition[Crno][keyValue].State = kidouState;
                                        CAppDat.KidouStatusT.Condition[Crno][keyValue].SenpeiState = senpeiState;
                                        // 状態を変化させたので変化フラグをオン
                                        CAppDat.KidouStatusT.Condition[Crno][keyValue].IsChangeable = true;
                                        updatecnt++;
                                    }
                                    else
                                    {
                                        logMsg = String.Format("KidouStatusTキー未登録 : ({0})({1})", keyValue.Item1, keyValue.Item2);
                                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
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
                                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.KIDOUSTATUS].ReleaseMutex();
                                }
                            }
                            else
                            {
                                // 処理なし
                            }
                            keyValue = null;
                        }

                        UInt16 dis_priorityno = 1;           //逆最優先番号、一番後ろにやりたいやつ

                        ///////////////////////////////////////////////////////////
                        // 最低落下条件および最前面表示有無のチェック
                        IsTrackCurcuit = this.CheckSettingDictionary(CAppDat.KidouInfoF[partslink.Kbno].TrackData.Condition);

                        ///////////////////////////////////////////////////////////
                        // 最低進路鎖錠条件および最前面表示有無のチェック
                        IsShinroCurcuit = this.CheckSettingDictionary(CAppDat.KidouInfoF[partslink.Kbno].ShinroData.Condition);

                        // 最優先度の状態番号を取得する
                        UInt16 priorityno = 0xFFFF;
                        foreach (UInt16 ptncnt in CAppDat.TrackPatternF[partslink.Kbno].PartsList.Keys)
                        {
                            Tuple<String, UInt16> KeyValue = new Tuple<String, UInt16>(kidoukey, ptncnt);
                            UInt16 statno = 0;
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.KIDOUSTATUS].WaitOne();

                            try
                            {
                                bool bContainsKey = CAppDat.KidouStatusT.Condition[Crno].ContainsKey(KeyValue);
                                if (true == bContainsKey)
                                {
                                    statno = CAppDat.KidouStatusT.Condition[Crno][KeyValue].State;
                                }
                                else
                                {
                                    stateno = 0;
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
                                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.KIDOUSTATUS].ReleaseMutex();
                            }

                            // 状態番号が優先度値より小さい？
                            if ((0 < statno) && (statno < priorityno))
                            {
                                priorityno = statno;
                            }
                            else
                            {
                                // 処理なし
                            }

                            // 最背面にしたい状態idが "0" 、すでに通常表示であれば再設定の必要はない
                            // 状態idが通常状態なら、それを最背面に設定する
                            // 最後に、今持ってるdis_priorityと今の状態idを比べて、更新なるかチェックする
                            if ((dis_priorityno != 0) && ((statno == 0) || ((0 < statno) && (statno > dis_priorityno))))
                            {
                                dis_priorityno = statno;     //最背面状態番号を更新
                            }
                            else
                            {
                                // 処理なし
                            }
                        }

                        // 最優先度の状態番号と最低落下条件をチェックし最前面の表示有無と状態番号を補正する
                        foreach (UInt16 ptncnt in CAppDat.TrackPatternF[partslink.Kbno].PartsList.Keys)
                        {
                            Tuple<String, UInt16> KeyValue = new Tuple<String, UInt16>(kidoukey, ptncnt);
                            UInt16 statno = 0;
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.KIDOUSTATUS].WaitOne();


                            try
                            {
                                bool bContainsKey = CAppDat.KidouStatusT.Condition[Crno].ContainsKey(KeyValue);
                                if (true == bContainsKey)
                                {
                                    statno = CAppDat.KidouStatusT.Condition[Crno][KeyValue].State;
                                    // 優先度が落下条件でないが、最低落下条件は成立中？
                                    if ((1 != priorityno) && (true == IsTrackCurcuit))
                                    {
                                        CAppDat.KidouStatusT.Condition[Crno][KeyValue].State = 1;
                                        CAppDat.KidouStatusT.Condition[Crno][KeyValue].DispFront = (UInt16)CAppDat.PRIORITYFLG.FIRST_PRIORITY;
                                        CAppDat.KidouStatusT.Condition[Crno][KeyValue].IsChangeable = true;
                                        updatecnt++;
                                    }
                                    // 優先度が落下条件・進路鎖錠条件でないが、最低進路鎖錠条件は成立中？
                                    else if ((1 != priorityno) && (2 != priorityno) && (true == IsShinroCurcuit))
                                    {
                                        CAppDat.KidouStatusT.Condition[Crno][KeyValue].State = 2;
                                        CAppDat.KidouStatusT.Condition[Crno][KeyValue].DispFront = (UInt16)CAppDat.PRIORITYFLG.FIRST_PRIORITY;
                                        CAppDat.KidouStatusT.Condition[Crno][KeyValue].IsChangeable = true;
                                        updatecnt++;
                                    }
                                    // 優先度と現状態が一致かつ初期状態ではない？
                                    else if ((priorityno == statno) && (0 != statno))
                                    {
                                        CAppDat.KidouStatusT.Condition[Crno][KeyValue].DispFront = (UInt16)CAppDat.PRIORITYFLG.FIRST_PRIORITY;
                                        CAppDat.KidouStatusT.Condition[Crno][KeyValue].IsChangeable = true;
                                    }
                                    // 最背面にするもの かつ 最優先のものが更新されている？
                                    else if ((dis_priorityno == statno) && (priorityno != 0xFFFF))
                                    {
                                        CAppDat.KidouStatusT.Condition[Crno][KeyValue].DispFront = (UInt16)CAppDat.PRIORITYFLG.DIS_PRIORITY;
                                        CAppDat.KidouStatusT.Condition[Crno][KeyValue].IsChangeable = true;
                                    }
                                    // 上記状態に一致しなければ最前面表示有無を初期化
                                    else
                                    {
                                        CAppDat.KidouStatusT.Condition[Crno][KeyValue].DispFront = (UInt16)CAppDat.PRIORITYFLG.NON;
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
                                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.KIDOUSTATUS].ReleaseMutex();
                            }
                        }
                        checkKidout = null;
                    }
                }

                //////////////////////////////////////////////////////
                // １件以上の更新ありのとき、軌道回路表示処理起動
                if (0 < updatecnt)
                {
                    CCommon.WriteDispUpdateRequest((UInt32)CAppDat.REQUESTFLG.KIDOUDISP, 1);
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
            finally
            {
                setData = null;
                senpeilist = null;
                patternlist = null;
                localkidout = null;
                partslink = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 転てつ器解析処理
        /// MODULE ID           : PointStatusAnalyze
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
        /// 受信情報を解析して転てつ器パーツの現状態を判断し状態テーブルに格納する。
        /// １件以上のレコード更新がある場合は転てつ器表示処理を起動する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void PointStatusAnalyze()
        {
            UInt16 updatecnt = 0;
            UInt16 stateno = 0;
            UInt16 pattern = 0;
            CSettingMng setData = null;
            CStatusPointData work = new CStatusPointData();
            Dictionary<UInt16, CSettingMng> patternlist = null;
            bool IsAgree = false;
            CStatusPointMng localpointt = null;
            CPartsLinkMng partslink = null;
            bool IsTrackCurcuit = false;
            String syncErrorMsg = String.Empty;
            String logMsg = String.Empty;

            try
            {
                ///////////////////////////////////////////////////////////
                // ローカルに現状の転てつ器状態テーブルを取得
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.POINTSTATUS].WaitOne();
                try
                {
                    localpointt = (CStatusPointMng)CAppDat.PointStatusT.Clone();
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.POINTSTATUS].ReleaseMutex();
                }

                ///////////////////////////////////////////////////////////
                // 状態テーブルの更新条件をチェック
                
                foreach (UInt16 Crno in CAppDat.PointPPLF.Keys)
                {
                    foreach (String pointkey in CAppDat.PointPPLF[Crno].Keys)
                    {
                        partslink = CAppDat.PointPPLF[Crno][pointkey];
                        work.Clear();
                        IsAgree = false;
                        stateno = 0;
                        // 設備情報ファイルにPPLファイルで設定した区分番号（キー）が存在するか？
                        // 存在する場合は続行、存在しない場合は次データに移行する
                        bool KeyExist = CAppDat.PointInfoF.ContainsKey(partslink.Kbno);
                        if (true == KeyExist)
                        {
                            // 処理なし
                        }
                        else
                        {
                            continue;
                        }

                        ///////////////////////////////////////////////////////////
                        // 線路閉鎖条件のチェック
                        foreach (UInt16 keyid in CAppDat.PointInfoF[partslink.Kbno].Senpei.Keys)
                        {
                            Dictionary<UInt16, CSettingMng> senpeilist = CAppDat.PointInfoF[partslink.Kbno].Senpei;
                            // 表示条件に属する設定条件のステータスをチェック
                            IsAgree = this.CheckSettingDictionary(senpeilist[keyid].Condition);
                            // 表示条件が一致あり→[状態件判定終了]
                            if (true == IsAgree)
                            {
                                // 線路閉鎖有無を取得
                                work.SenpeiState = keyid;
                                break;
                            }
                            else
                            {
                                // 処理なし
                            }
                        }

                        ///////////////////////////////////////////////////////////
                        // パターン別情報のチェック
                        pattern = 0;
                        foreach (UInt16 keyid in CAppDat.PointInfoF[partslink.Kbno].Pattern.Keys)
                        {
                            work.State = 0;
                            pattern = keyid;
                            IsAgree = false;
                            patternlist = CAppDat.PointInfoF[partslink.Kbno].Pattern[keyid];
                            // パターン別情報に属する状態数分繰り返す
                            foreach (UInt16 ptKeyid in patternlist.Keys)
                            {
                                // キーIDの状態番号が０は無効とし処理なし
                                if (0 == ptKeyid)
                                {
                                    continue;
                                }
                                // キーIDの状態番号が０以外は記憶する
                                else
                                {
                                    setData = patternlist[ptKeyid];
                                    stateno = ptKeyid;
                                }

                                // 表示条件に属する設定条件のステータスをチェック
                                IsAgree = this.CheckSettingDictionary(setData.Condition);
                                // 表示条件が一致あり→[状態件判定終了]
                                if (true == IsAgree)
                                {
                                    // 状態番号を取得
                                    work.State = stateno;
                                    break;
                                }
                                else
                                {
                                    // 処理なし
                                }
                            }

                            ///////////////////////////////////////////////////////////
                            Tuple<String, UInt16> pointpatternkey = new Tuple<String, UInt16>(pointkey, pattern);
                            // 前回情報と比較して状態に変化ありのときテーブル更新
                            // 状態番号が不一致？
                            if ((localpointt.Condition[Crno][pointpatternkey].State != work.State) || (localpointt.Condition[Crno][pointpatternkey].SenpeiState != work.SenpeiState))
                            {
                                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.POINTSTATUS].WaitOne();
                                try
                                {
                                    bool bContainsKey = CAppDat.PointStatusT.Condition[Crno].ContainsKey(pointpatternkey);
                                    if (true == bContainsKey)
                                    {
                                        CAppDat.PointStatusT.Condition[Crno][pointpatternkey].SenpeiState = work.SenpeiState;
                                        CAppDat.PointStatusT.Condition[Crno][pointpatternkey].State = work.State;
                                        CAppDat.PointStatusT.Condition[Crno][pointpatternkey].DispFront = (UInt16)CAppDat.PRIORITYFLG.NON;
                                        // 状態を変化させたので変化フラグをオン
                                        CAppDat.PointStatusT.Condition[Crno][pointpatternkey].IsChangeable = true;
                                        updatecnt++;
                                    }
                                    else
                                    {
                                        logMsg = String.Format("PointStatusTキー未登録 : ({0})({1})", pointpatternkey.Item1, pointpatternkey.Item2);
                                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
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
                                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.POINTSTATUS].ReleaseMutex();
                                }
                            }
                            else
                            {
                                // 処理なし
                            }
                        }
                        ///////////////////////////////////////////////////////////
                        // 最低落下条件および最前面表示有無のチェック
                        IsTrackCurcuit = this.CheckSettingDictionary(CAppDat.PointInfoF[partslink.Kbno].TrackData.Condition);
                        // 最優先度の状態番号を取得する
                        UInt16 priorityno = 0xFFFF;
                        for (UInt16 cnt = 1; cnt <= CAppDat.PointInfoF[partslink.Kbno].Pattern.Count; cnt++)
                        {
                            Tuple<String, UInt16> KeyValue = new Tuple<String, UInt16>(pointkey, cnt);
                            UInt16 statno = 0;
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.POINTSTATUS].WaitOne();
                            try
                            {
                                bool bContainsKey = CAppDat.PointStatusT.Condition[Crno].ContainsKey(KeyValue);
                                if (true == bContainsKey)
                                {
                                    statno = CAppDat.PointStatusT.Condition[Crno][KeyValue].State;
                                }
                                else
                                {
                                    stateno = 0;
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
                                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.POINTSTATUS].ReleaseMutex();
                            }

                            if ((0 < statno) && (statno < priorityno))
                            {
                                priorityno = statno;
                            }
                            else
                            {
                                // 処理なし
                            }
                        }
                        // 最優先度の状態番号と最低落下条件をチェックし最前面の表示有無と状態番号を補正する
                        for (UInt16 cnt = 1; cnt <= CAppDat.PointInfoF[partslink.Kbno].Pattern.Count; cnt++)
                        {
                            Tuple<String, UInt16> KeyValue = new Tuple<String, UInt16>(pointkey, cnt);
                            UInt16 statno = 0;
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.POINTSTATUS].WaitOne();
                            try
                            {
                                bool bContainsKey = CAppDat.PointStatusT.Condition[Crno].ContainsKey(KeyValue);
                                if (true == bContainsKey)
                                {
                                    statno = CAppDat.PointStatusT.Condition[Crno][KeyValue].State;
                                    // 優先度が落下条件でないが、最低落下条件は成立中？
                                    if ((1 != priorityno) && (true == IsTrackCurcuit))
                                    {
                                        CAppDat.PointStatusT.Condition[Crno][KeyValue].State = 1;
                                        CAppDat.PointStatusT.Condition[Crno][KeyValue].DispFront = (UInt16)CAppDat.PRIORITYFLG.FIRST_PRIORITY;
                                        CAppDat.PointStatusT.Condition[Crno][KeyValue].IsChangeable = true;
                                        updatecnt++;
                                    }
                                    // 優先度と現状態が一致かつ初期状態ではない？
                                    else if ((priorityno == statno) && (0 != statno))
                                    {
                                        CAppDat.PointStatusT.Condition[Crno][KeyValue].DispFront = (UInt16)CAppDat.PRIORITYFLG.FIRST_PRIORITY;
                                        CAppDat.PointStatusT.Condition[Crno][KeyValue].IsChangeable = true;
                                    }

                                    // 上記状態に一致しなければ最前面表示有無を初期化
                                    else
                                    {
                                        CAppDat.PointStatusT.Condition[Crno][KeyValue].DispFront = (UInt16)CAppDat.PRIORITYFLG.NON;
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
                                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.POINTSTATUS].ReleaseMutex();
                            }
                        }
                    }
                }

                //////////////////////////////////////////////////////
                // １件以上の更新ありのとき、転てつ器表示処理起動
                if (0 < updatecnt)
                {
                    CCommon.WriteDispUpdateRequest((UInt32)CAppDat.REQUESTFLG.POINTDISP, 1);
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
            finally
            {
                setData = null;
                work = null;
                patternlist = null;
                localpointt = null;
                partslink = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 窓解析処理
        /// MODULE ID           : MadoStatusAnalyze
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
        /// 受信情報を解析して窓パーツの現状態を判断し状態テーブルに格納する。
        /// 1件以上のレコード更新がある場合は窓表示処理を起動する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void MadoStatusAnalyze()
        {
            UInt16 updatecnt = 0;
            CStatusMadoData work = new CStatusMadoData();
            CRetuData retuData = null;
            CStatusMadoMng localmadot = null;
            CPartsLinkMng partslink = null;
            String syncErrorMsg = String.Empty;
            String logMsg = String.Empty;

            try
            {
                ///////////////////////////////////////////////////////////
                // ローカルに現状の窓状態テーブルを取得
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.MADOSTATUS].WaitOne();
                try
                {
                    localmadot = (CStatusMadoMng)CAppDat.MadoStatusT.Clone();
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.MADOSTATUS].ReleaseMutex();
                }

                ///////////////////////////////////////////////////////////
                // 状態テーブルの更新条件をチェック
                
                foreach (UInt16 Crno in CAppDat.MadoPPLF.Keys)
                {
                    foreach (String madokey in CAppDat.MadoPPLF[Crno].Keys)
                    {
                        partslink = CAppDat.MadoPPLF[Crno][madokey];
                        work.Clear();
                        // 設備情報ファイルにPPLファイルで設定した区分番号（キー）が存在するか？
                        // 存在する場合は続行、存在しない場合は次データに移行する
                        bool KeyExist = CAppDat.MadoInfoF.ContainsKey(partslink.Kbno);
                        if (true == KeyExist)
                        {
                            // 処理なし
                        }
                        else
                        {
                            continue;
                        }

                        ///////////////////////////////////////////////////////////
                        // 窓の列番情報解析
                        // 列番情報の指定窓に列番が存在するか？
                        bool bRet = m_RetuInfoMngT.GetRecvDataRecord(ref retuData, CAppDat.MadoInfoF[partslink.Kbno].MdNo);
                        if (true == bRet)
                        {
                            // 窓種別が隠し窓、現発窓、システム外窓か？
                            // 上記窓種別の場合は窓状態、背景状態を常に固定とする
                            if ((3 == partslink.SubKind) || (4 == partslink.SubKind) || (6 == partslink.SubKind))
                            {
                                work.SekobiState = this.ConvertToMadoStatDay(retuData);
                                work.State = 1;
                                work.BackState = 1;
                            }
                            else if (1 == partslink.SubKind && 0 == retuData.tnflg && 0 == retuData.dispflg)
                            {
                                work.SekobiState = this.ConvertToMadoStatDay(retuData);
                                work.State = 11;
                                work.BackState = 11;
                            }
                            // 上記以外か？
                            else
                            {
                                // 列番情報を解析して状態番号、背景状態番号を抽出する
                                UInt16 state = this.ConvertToMadoStat(retuData);
                                if ((UInt16)MADOSTAT.UNKNOWN == state)
                                {
                                    work.SekobiState = this.ConvertToMadoStatDay(retuData);
                                    work.State = work.SekobiState;
                                    work.BackState = work.SekobiState;
                                }
                                else
                                {
                                    work.SekobiState = this.ConvertToMadoStatDay(retuData);
                                    work.State = state;
                                    work.BackState = state;
                                }
                            }

                            // 列番文字列の設定
                            Byte bySeko = retuData.seko;             // 施行日
                            Byte byPrefix = retuData.kanmuri;        // 頭冠記号部
                            UInt16 wTrainNo = retuData.trno;         // 数字部
                            Byte byEnd = retuData.kigo;              // 末尾記号部
                            // タイプ２で列番文字列を変換
                            work.RetuString = CCommon.CreateTrainNo(bySeko, byPrefix, wTrainNo, byEnd, false, DateTime.Now, 2);

                            // 列番情報を解析して遅延状態番号を抽出する
                            // 窓種別がシステム外窓か？
                            // 上記窓種別の場合は遅延状態を常に固定とする
                            if (6 == partslink.SubKind)
                            {
                                UInt16 chienState = this.ConvertToMadoChienStat(retuData);
                                if (0 == chienState)
                                {
                                    work.ChienState = (UInt16)MADOCHIENSTAT.UNKNOWN;
                                }
                                else
                                {
                                    work.ChienState = 1;
                                }
                            }
                            // 上記以外か？
                            else
                            {
                                work.ChienState = this.ConvertToMadoChienStat(retuData);
                            }

                            // 遅延文字列の設定
                            UInt16 latetime = (UInt16)((retuData.latetim * 5) / 60);
                            work.ChienString = latetime.ToString();
                            // 遅延時分が最大遅延時分を超えている？
                            if (latetime > CAppDat.TypeF.Disp.ChienMax)
                            {
                                work.ChienString = CAppDat.TypeF.Disp.ChienMax.ToString();
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

                        ///////////////////////////////////////////////////////////
                        // 前回情報と比較して状態に変化ありのときテーブル更新
                        // 窓状態番号, 窓施行日状態, 背景状態番号, 遅延状態番号, 列番文字列, 遅延文字列のいずれかが不一致？
                        if ((localmadot.Condition[Crno][madokey].State != work.State)               ||
                            (localmadot.Condition[Crno][madokey].SekobiState != work.SekobiState)   ||
                            (localmadot.Condition[Crno][madokey].BackState != work.BackState)       ||
                            (localmadot.Condition[Crno][madokey].ChienState != work.ChienState)     ||
                            (localmadot.Condition[Crno][madokey].RetuString != work.RetuString)     ||
                            (localmadot.Condition[Crno][madokey].ChienString != work.ChienString))
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.MADOSTATUS].WaitOne();
                            try
                            {
                                bool bContainsKey = CAppDat.MadoStatusT.Condition[Crno].ContainsKey(madokey);
                                if (true == bContainsKey)
                                {
                                    CAppDat.MadoStatusT.Condition[Crno][madokey].State = work.State;
                                    CAppDat.MadoStatusT.Condition[Crno][madokey].SekobiState = work.SekobiState;
                                    CAppDat.MadoStatusT.Condition[Crno][madokey].BackState = work.BackState;
                                    CAppDat.MadoStatusT.Condition[Crno][madokey].ChienState = work.ChienState;
                                    CAppDat.MadoStatusT.Condition[Crno][madokey].RetuString = work.RetuString;
                                    CAppDat.MadoStatusT.Condition[Crno][madokey].ChienString = work.ChienString;
                                    // 窓内の文字がない場合、窓枠を初期表示にする
                                    if ((CAppDat.MadoStatusT.Condition[Crno][madokey].RetuString == String.Empty) &&
                                        (CAppDat.IopcoT.DkNo == 0))
                                    {
                                        CAppDat.MadoStatusT.Condition[Crno][madokey].OperateState = 0;
                                    }
                                    // 状態を変化させたので変化フラグをオン
                                    CAppDat.MadoStatusT.Condition[Crno][madokey].IsChangeable = true;

                                    updatecnt++;
                                }
                                else
                                {
                                    logMsg = String.Format("MadoStatusTキー未登録 : ({0})", madokey);
                                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
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
                                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.MADOSTATUS].ReleaseMutex();
                            }
                        }
                        else
                        {
                            // 処理なし
                        }
                    }
                }

                //////////////////////////////////////////////////////
                // １件以上の更新ありのとき、窓表示処理起動
                if (0 < updatecnt)
                {
                    CCommon.WriteDispUpdateRequest((UInt32)CAppDat.REQUESTFLG.MADODISP, 1);
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
            finally
            {
                work = null;
                retuData = null;
                localmadot = null;
                partslink = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 窓解析処理
        /// MODULE ID           : TidMadoStatusAnalyze
        ///
        /// PARAMETER IN        : 
        /// <param name="sysNo">(in)システム番号（1-4）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// 受信情報を解析して窓パーツの現状態を判断し状態テーブルに格納する。
        /// 1件以上のレコード更新がある場合は窓表示処理を起動する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void TidMadoStatusAnalyze(UInt16 sysNo)
        {
            UInt16 updatecnt = 0;
            CStatusMadoData work = new CStatusMadoData();
            CRetuData retuData = null;
            CStatusMadoMng localmadot = null;
            CPartsLinkMng partslink = null;
            String syncErrorMsg = String.Empty;
            String logMsg = String.Empty;

            try
            {
                ///////////////////////////////////////////////////////////
                // ローカルに現状の窓状態テーブルを取得
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.MADOSTATUS].WaitOne();
                try
                {
                    localmadot = (CStatusMadoMng)CAppDat.MadoStatusT.Clone();
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.MADOSTATUS].ReleaseMutex();
                }

                ///////////////////////////////////////////////////////////
                // 状態テーブルの更新条件をチェック
                
                foreach (UInt16 Crno in CAppDat.MadoPPLF.Keys)
                {
                    foreach (String madokey in CAppDat.MadoPPLF[Crno].Keys)
                    {
                        partslink = CAppDat.MadoPPLF[Crno][madokey];
                        work.Clear();
                        // 設備情報ファイルにPPLファイルで設定した区分番号（キー）が存在するか？
                        // 存在する場合は続行、存在しない場合は次データに移行する
                        bool KeyExist = CAppDat.MadoInfoF.ContainsKey(partslink.Kbno);
                        if (true == KeyExist)
                        {
                            // 処理なし
                        }
                        else
                        {
                            continue;
                        }

                        ///////////////////////////////////////////////////////////
                        // 窓の列番情報解析
                        // 列番情報の指定窓に列番が存在するか？
                        bool bRet = m_OtherIFRetuInfoMngT[sysNo - 1].GetRecvDataRecord(ref retuData, CAppDat.MadoInfoF[partslink.Kbno].MdNo);
                        if (true == bRet)
                        {
                            // 窓種別が隠し窓、現発窓、システム外窓か？
                            // 上記窓種別の場合は窓状態、背景状態を常に固定とする
                            if ((3 == partslink.SubKind) || (4 == partslink.SubKind) || (6 == partslink.SubKind))
                            {
                                work.SekobiState = this.ConvertToMadoStatDay(retuData);
                                work.State = 1;
                                work.BackState = 1;
                            }
                            // 上記以外か？
                            else
                            {
                                // 列番情報を解析して状態番号、背景状態番号を抽出する
                                UInt16 state = this.ConvertToMadoStat(retuData);
                                if ((UInt16)MADOSTAT.UNKNOWN == state)
                                {
                                    work.SekobiState = this.ConvertToMadoStatDay(retuData);
                                    work.State = work.SekobiState;
                                    work.BackState = work.SekobiState;
                                }
                                else
                                {
                                    work.SekobiState = this.ConvertToMadoStatDay(retuData);
                                    work.State = state;
                                    work.BackState = state;
                                }
                            }

                            // 列番文字列の設定
                            Byte bySeko = retuData.seko;             // 施行日
                            Byte byPrefix = retuData.kanmuri;        // 頭冠記号部
                            UInt16 wTrainNo = retuData.trno;         // 数字部
                            Byte byEnd = retuData.kigo;              // 末尾記号部
                            // タイプ２で列番文字列を変換
                            work.RetuString = CCommon.CreateTrainNo(bySeko, byPrefix, wTrainNo, byEnd, false, DateTime.Now, 2);

                            // 列番情報を解析して遅延状態番号を抽出する
                            // 窓種別がシステム外窓か？
                            // 上記窓種別の場合は遅延状態を常に固定とする
                            if (6 == partslink.SubKind)
                            {
                                UInt16 chienState = this.ConvertToMadoChienStat(retuData);
                                if (0 == chienState)
                                {
                                    work.ChienState = (UInt16)MADOCHIENSTAT.UNKNOWN;
                                }
                                else
                                {
                                    work.ChienState = 1;
                                }
                            }
                            // 上記以外か？
                            else
                            {
                                work.ChienState = this.ConvertToMadoChienStat(retuData);
                            }

                            // 遅延文字列の設定
                            UInt16 latetime = (UInt16)((retuData.latetim * 5) / 60);
                            work.ChienString = latetime.ToString();
                            // 遅延時分が最大遅延時分を超えている？
                            if (latetime > CAppDat.TypeF.Disp.ChienMax)
                            {
                                work.ChienString = CAppDat.TypeF.Disp.ChienMax.ToString();
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

                        ///////////////////////////////////////////////////////////
                        // 前回情報と比較して状態に変化ありのときテーブル更新
                        // 窓状態番号, 窓施行日状態, 背景状態番号, 遅延状態番号, 列番文字列, 遅延文字列のいずれかが不一致？
                        if ((localmadot.Condition[Crno][madokey].State != work.State)               ||
                            (localmadot.Condition[Crno][madokey].SekobiState != work.SekobiState)   ||
                            (localmadot.Condition[Crno][madokey].BackState != work.BackState)       ||
                            (localmadot.Condition[Crno][madokey].ChienState != work.ChienState)     ||
                            (localmadot.Condition[Crno][madokey].RetuString != work.RetuString)     ||
                            (localmadot.Condition[Crno][madokey].ChienString != work.ChienString))
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.MADOSTATUS].WaitOne();
                            try
                            {
                                bool bContainsKey = CAppDat.MadoStatusT.Condition[Crno].ContainsKey(madokey);
                                if (true == bContainsKey)
                                {
                                    CAppDat.MadoStatusT.Condition[Crno][madokey].State = work.State;
                                    CAppDat.MadoStatusT.Condition[Crno][madokey].SekobiState = work.SekobiState;
                                    CAppDat.MadoStatusT.Condition[Crno][madokey].BackState = work.BackState;
                                    CAppDat.MadoStatusT.Condition[Crno][madokey].ChienState = work.ChienState;
                                    CAppDat.MadoStatusT.Condition[Crno][madokey].RetuString = work.RetuString;
                                    CAppDat.MadoStatusT.Condition[Crno][madokey].ChienString = work.ChienString;
                                    // 窓内の文字がない場合、窓枠を初期表示にする
                                    if ((CAppDat.MadoStatusT.Condition[Crno][madokey].RetuString == String.Empty) &&
                                        (CAppDat.IopcoT.DkNo == 0))
                                    {
                                        CAppDat.MadoStatusT.Condition[Crno][madokey].OperateState = 0;
                                    }
                                    // 状態を変化させたので変化フラグをオン
                                    CAppDat.MadoStatusT.Condition[Crno][madokey].IsChangeable = true;

                                    updatecnt++;
                                }
                                else
                                {
                                    logMsg = String.Format("MadoStatusTキー未登録 : ({0})", madokey);
                                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
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
                                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.MADOSTATUS].ReleaseMutex();
                            }
                        }
                        else
                        {
                            // 処理なし
                        }
                    }
                }

                //////////////////////////////////////////////////////
                // １件以上の更新ありのとき、窓表示処理起動
                if (0 < updatecnt)
                {
                    CCommon.WriteDispUpdateRequest((UInt32)CAppDat.REQUESTFLG.MADODISP, 1);
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
            finally
            {
                work = null;
                retuData = null;
                localmadot = null;
                partslink = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 発着てこラベル解析処理
        /// MODULE ID           : HatuChakuStatusAnalyze
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
        /// 受信情報を解析して発着てこラベルパーツの現状態を判断し状態テーブルに格納する。
        /// 1件以上のレコード更新がある場合は発着てこラベル表示処理を起動する。
        /// 発進路着点てこ、進路てこ以外は表示反映がないため処理なしとする。
        /// </summary>
        ///
        ///*******************************************************************************
        private void HatuChakuStatusAnalyze()
        {
            try
            {
                // 進路選択式を取得
                UInt16 SinroSelect = CAppDat.TypeF.Seigyo.SinroSelect;
                UInt16 IrekaeSelect = CAppDat.TypeF.Seigyo.IreSinroSelect;
                UInt16 YudoSelect = CAppDat.TypeF.Seigyo.YudoSinroSelect;
                UInt16 OtherSelect = CAppDat.TypeF.Seigyo.OtherSelect;

                if (((UInt16)CStatusSALabelMng.SELECTTYPE.HATUSINRO == SinroSelect) ||
                    ((UInt16)CStatusSALabelMng.SELECTTYPE.HATUSINRO == IrekaeSelect) ||
                    ((UInt16)CStatusSALabelMng.SELECTTYPE.HATUSINRO == YudoSelect) || 
                    ((UInt16)CStatusSALabelMng.SELECTTYPE.HATUSINRO == OtherSelect))
                {
                    this.HatuSinroStatusAnalyze();
                }
                else
                {
                    // 処理なし
                }

                if (((UInt16)CStatusSALabelMng.SELECTTYPE.SINROTEKO == SinroSelect) ||
                    ((UInt16)CStatusSALabelMng.SELECTTYPE.SINROTEKO == IrekaeSelect) ||
                    ((UInt16)CStatusSALabelMng.SELECTTYPE.SINROTEKO == YudoSelect) ||
                    ((UInt16)CStatusSALabelMng.SELECTTYPE.SINROTEKO == OtherSelect))
                {
                    this.SinroTekoStatusAnalyze();
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
        /// MODULE NAME         : 発着てこラベル解析処理（発進路着点てこ式表示解析）
        /// MODULE ID           : HatuSinroStatusAnalyze
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
        /// 受信情報を解析して発着てこラベルパーツの現状態を判断し状態テーブルに格納する。
        /// 1件以上のレコード更新がある場合は発着てこラベル表示処理を起動する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void HatuSinroStatusAnalyze()
        {
            UInt16 updatecnt = 0;
            UInt16 stateno = 0;
            CSettingMng setData = null;
            CStatusSALabelData work = new CStatusSALabelData();
            bool IsAgree = false;
            CStatusSALabelMng locasalabelt = null;
            CPartsLinkMng partslink = null;
            String syncErrorMsg = String.Empty;
            String logMsg = String.Empty;

            try
            {
                ///////////////////////////////////////////////////////////
                // ローカルに現状の発着てこラベル状態テーブルを取得
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DATEKOLABELSTATUS].WaitOne();
                try
                {
                    locasalabelt = (CStatusSALabelMng)CAppDat.DATekoLabelStatusT.Clone();
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DATEKOLABELSTATUS].ReleaseMutex();
                }

                ///////////////////////////////////////////////////////////
                // 状態テーブルの更新条件をチェック
                
                foreach (UInt16 Crno in CAppDat.DATekoLabelPPLF.Keys)
                {
                    foreach (String satekolabelkey in CAppDat.DATekoLabelPPLF[Crno].Keys)
                    {
                        partslink = CAppDat.DATekoLabelPPLF[Crno][satekolabelkey];
                        work.Clear();
                        IsAgree = false;
                        stateno = 0;
                        // 設備情報ファイルにPPLファイルで設定した区分番号（キー）が存在するか？
                        // 存在する場合は続行、存在しない場合は次データに移行する（種別番号でフィルタリング）
                        if (((UInt16)CStatusSALabelMng.HATUCHAKU_KBMAX < partslink.SubKind) && (partslink.SubKind <= (UInt16)CStatusSALabelMng.HATUSHINRO_KBMAX))
                        {
                            bool KeyExist = CAppDat.SATekoLabelInfoF.ContainsKey(partslink.Kbno);
                            if (true == KeyExist)
                            {
                                // 処理なし
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }

                        ///////////////////////////////////////////////////////////
                        // 表示情報の状態チェック
                        // 通常名称を取得
                        work.LabelString = CAppDat.SATekoLabelInfoF[partslink.Kbno].DefaultName;
                        // 表示情報に属する状態数分繰り返す
                        foreach (UInt16 keyid in CAppDat.SATekoLabelInfoF[partslink.Kbno].Disp.Keys)
                        {
                            // キーIDの状態番号が０は無効とし処理なし
                            if (0 == keyid)
                            {
                                continue;
                            }
                            // キーIDの状態番号が０以外は条件情報を取得する
                            else
                            {
                                setData = CAppDat.SATekoLabelInfoF[partslink.Kbno].Disp[keyid];
                                stateno = keyid;
                            }

                            // 表示条件に属する設定条件のステータスをチェック
                            IsAgree = this.CheckSettingDictionary(setData.Condition);
                            // 表示条件が一致あり→[状態件判定終了]
                            if (true == IsAgree)
                            {
                                // 状態番号を取得
                                work.State = stateno;
                                break;
                            }
                            else
                            {
                                // 処理なし
                            }
                        }

                        ///////////////////////////////////////////////////////////
                        // 前回情報と比較して状態に変化ありのときテーブル更新
                        // 状態番号、状態文字列のいずれかが不一致？
                        if ((locasalabelt.Condition[Crno][satekolabelkey].State != work.State) ||
                            (locasalabelt.Condition[Crno][satekolabelkey].LabelString != work.LabelString))
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DATEKOLABELSTATUS].WaitOne();
                            try
                            {
                                bool bContainsKey = CAppDat.DATekoLabelStatusT.Condition[Crno].ContainsKey(satekolabelkey);
                                if (true == bContainsKey)
                                {
                                    CAppDat.DATekoLabelStatusT.Condition[Crno][satekolabelkey].State = work.State;
                                    CAppDat.DATekoLabelStatusT.Condition[Crno][satekolabelkey].LabelString = work.LabelString;
                                    // 状態を変化させたので変化フラグをオン
                                    CAppDat.DATekoLabelStatusT.Condition[Crno][satekolabelkey].IsChangeable = true;
                                    updatecnt++;
                                }
                                else
                                {
                                    logMsg = String.Format("DATekoLabelStatusTキー未登録 : ({0})", satekolabelkey);
                                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
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
                                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DATEKOLABELSTATUS].ReleaseMutex();
                            }
                        }
                        else
                        {
                            // 処理なし
                        }
                    }
                }

                //////////////////////////////////////////////////////
                // １件以上の更新ありのとき、発着てこラベル表示処理起動
                if (0 < updatecnt)
                {
                    CCommon.WriteDispUpdateRequest((UInt32)CAppDat.REQUESTFLG.HATUCHAKUTEKOLABELDISP, 1);
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
            finally
            {
                work = null;
                setData = null;
                locasalabelt = null;
                partslink = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 発着てこラベル解析処理（進路てこ式表示解析）
        /// MODULE ID           : SinroTekoStatusAnalyze
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
        /// 受信情報を解析して発着てこラベルパーツの現状態を判断し状態テーブルに格納する。
        /// 1件以上のレコード更新がある場合は発着てこラベル表示処理を起動する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void SinroTekoStatusAnalyze()
        {
            UInt16 updatecnt = 0;
            UInt16 stateno = 0;
            CSettingMng setData = null;
            CStatusSALabelData work = new CStatusSALabelData();
            bool IsAgree = false;
            CStatusSALabelMng locasalabelt = null;
            CPartsLinkMng partslink = null;
            String syncErrorMsg = String.Empty;
            String logMsg = String.Empty;

            try
            {
                ///////////////////////////////////////////////////////////
                // ローカルに現状の発着てこラベル状態テーブルを取得
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DATEKOLABELSTATUS].WaitOne();
                try
                {
                    locasalabelt = (CStatusSALabelMng)CAppDat.DATekoLabelStatusT.Clone();
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DATEKOLABELSTATUS].ReleaseMutex();
                }

                ///////////////////////////////////////////////////////////
                // 状態テーブルの更新条件をチェック
                foreach (UInt16 Crno in CAppDat.DATekoLabelPPLF.Keys)
                {
                    foreach (String satekolabelkey in CAppDat.DATekoLabelPPLF[Crno].Keys)
                    {
                        partslink = CAppDat.DATekoLabelPPLF[Crno][satekolabelkey];
                        work.Clear();
                        IsAgree = false;
                        stateno = 0;
                        // 設備情報ファイルにPPLファイルで設定した区分番号（キー）が存在するか？
                        // 存在する場合は続行、存在しない場合は次データに移行する（種別番号でフィルタリング）
                        if (((UInt16)CStatusSALabelMng.HATUSHINRO_KBMAX < partslink.SubKind) && (partslink.SubKind <= (UInt16)CStatusSALabelMng.SHINROTEKO_KBMAX))
                        {
                            bool KeyExist = CAppDat.SRTekoLabelInfoF.ContainsKey(partslink.Kbno);
                            if (true == KeyExist)
                            {
                                // 処理なし
                            }
                            else
                            {
                                //logMsg = String.Format("ppr_進路てこラベル.xml 区分NO.={0} 未登録, 画面NO.={1}", partslink.Kbno, partslink.Crno);
                                //CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }

                        ///////////////////////////////////////////////////////////
                        // 表示情報の状態チェック
                        // 通常名称を取得
                        work.LabelString = CAppDat.SRTekoLabelInfoF[partslink.Kbno].DefaultName;
                        // 表示情報に属する状態数分繰り返す
                        foreach (UInt16 keyid in CAppDat.SRTekoLabelInfoF[partslink.Kbno].Disp.Keys)
                        {
                            // キーIDの状態番号が０は無効とし処理なし
                            if (0 == keyid)
                            {
                                continue;
                            }
                            // キーIDの状態番号が０以外は条件情報を取得する
                            else
                            {
                                setData = CAppDat.SRTekoLabelInfoF[partslink.Kbno].Disp[keyid];
                                stateno = keyid;
                            }

                            // 表示条件に属する設定条件のステータスをチェック
                            IsAgree = this.CheckSettingDictionary(setData.Condition);
                            // 表示条件が一致あり→[状態件判定終了]
                            if (true == IsAgree)
                            {
                                // 状態番号を取得
                                work.State = stateno;
                                break;
                            }
                            else
                            {
                                // 処理なし
                            }
                        }

                        ///////////////////////////////////////////////////////////
                        // 前回情報と比較して状態に変化ありのときテーブル更新
                        // 状態番号、状態文字列のいずれかが不一致？
                        if ((locasalabelt.Condition[Crno][satekolabelkey].State != work.State) ||
                            (locasalabelt.Condition[Crno][satekolabelkey].LabelString != work.LabelString))
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DATEKOLABELSTATUS].WaitOne();
                            try
                            {
                                bool bContainsKey = CAppDat.DATekoLabelStatusT.Condition[Crno].ContainsKey(satekolabelkey);
                                if (true == bContainsKey)
                                {
                                    CAppDat.DATekoLabelStatusT.Condition[Crno][satekolabelkey].State = work.State;
                                    CAppDat.DATekoLabelStatusT.Condition[Crno][satekolabelkey].LabelString = work.LabelString;
                                    // 状態を変化させたので変化フラグをオン
                                    CAppDat.DATekoLabelStatusT.Condition[Crno][satekolabelkey].IsChangeable = true;
                                    updatecnt++;
                                }
                                else
                                {
                                    logMsg = String.Format("DATekoLabelStatusTキー未登録 : ({0})", satekolabelkey);
                                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
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
                                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DATEKOLABELSTATUS].ReleaseMutex();
                            }
                        }
                        else
                        {
                            // 処理なし
                        }
                    }
                }

                //////////////////////////////////////////////////////
                // １件以上の更新ありのとき、発着てこラベル表示処理起動
                if (0 < updatecnt)
                {
                    CCommon.WriteDispUpdateRequest((UInt32)CAppDat.REQUESTFLG.HATUCHAKUTEKOLABELDISP, 1);
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
            finally
            {
                work = null;
                setData = null;
                locasalabelt = null;
                partslink = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 警報情報解析処理
        /// MODULE ID           : PrcKeihouAnalyze
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
        /// 警報情報を解析して警報出力を行う。
        /// 警報情報は受信した情報分の出力が必要なため、リストに積んである情報数分解析する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void PrcKeihouAnalyze()
        {
            try
            {
                // 警報情報格納リストに積まれているデータ分解析する
                for (UInt16 cnt = 0; cnt < m_KeihouInfoMngT.Count; cnt++)
                {
                    // ＰＲＣ警報出力処理
                    CKeihouCtrl.OutputPrcKeihou(m_KeihouInfoMngT[cnt]);
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 仮想転てつ器解析処理
        /// MODULE ID           : KasoPointAnalyze
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
        /// 仮想転てつ器共通定義ファイルに設定されている転てつ器の設定条件の状態を参照し、
        /// 仮想転てつ器状態テーブルの状態を更新を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void KasoPointAnalyze()
        {
            UInt16 ekno = 0;                            // 駅番号
            UInt16 kbno = 0;                            // 軌道回路区分番号
            bool IsTeiiValue = false;                   // 仮想転てつ器の定位条件結果
            bool IsHaniValue = false;                   // 仮想転てつ器の反位条件結果
            bool IsKeepTeiiValue = false;               // 仮想転てつ器の定位保持条件結果
            bool IsKeepHaniValue = false;               // 仮想転てつ器の反位保持条件結果
            Tuple<UInt16, String> nkeyValue = null;     // キー名称
            Tuple<UInt16, String> rkeyValue = null;     // キー名称

            try
            {
                // 仮想転てつ器共通定義ファイル数分ループする
                foreach (UInt16 kasoDataKey in CAppDat.KasouPointF.Keys)
                {
                    // 転てつ器情報から駅番号、関連軌道回路区分番号を取得
                    ekno = CAppDat.KasouPointF[kasoDataKey].TenInfo.Ekino;
                    bool bKidouExists = false;
                    for (UInt16 cnt = 0; cnt < CAppDat.KasouPointF[kasoDataKey].TenInfo.RelateKidou.Count; cnt++)
                    {
                        kbno = CAppDat.KasouPointF[kasoDataKey].TenInfo.RelateKidou[cnt];
                        bool bRetStat = this.CheckKidouStatus(kbno);
                        if (true == bRetStat)
                        {
                            bKidouExists = true;
                            break;
                        }
                        else
                        {
                            // 処理なし
                        }
                    }

                    // 定位条件情報、定位保持条件情報をチェック
                    IsTeiiValue = this.CheckSettingDictionary(CAppDat.KasouPointF[kasoDataKey].Teii);
                    IsKeepTeiiValue = this.CheckSettingDictionary(CAppDat.KasouPointF[kasoDataKey].KeepTeii);

                    // 反位条件情報、反位保持条件情報をチェック
                    IsHaniValue = this.CheckSettingDictionary(CAppDat.KasouPointF[kasoDataKey].Hani);
                    IsKeepHaniValue = this.CheckSettingDictionary(CAppDat.KasouPointF[kasoDataKey].KeepHani);

                    // 駅番号と転てつ器名称からキーを生成
                    nkeyValue = new Tuple<UInt16, String>(ekno, CAppDat.KasouPointF[kasoDataKey].TenInfo.N_Name);
                    rkeyValue = new Tuple<UInt16, String>(ekno, CAppDat.KasouPointF[kasoDataKey].TenInfo.R_Name);
                    if ((true == IsTeiiValue) && (true == IsHaniValue))
                    {
                        // 前回状態を保持
                        CAppDat.KasouPointJyoutaiT[nkeyValue].BeforeState = CAppDat.KasouPointJyoutaiT[nkeyValue].State;
                        CAppDat.KasouPointJyoutaiT[rkeyValue].BeforeState = CAppDat.KasouPointJyoutaiT[rkeyValue].State;
                        // 今回状態に更新
                        CAppDat.KasouPointJyoutaiT[nkeyValue].State = 1;
                        CAppDat.KasouPointJyoutaiT[rkeyValue].State = 1;
                    }
                    else if (true == IsTeiiValue)
                    {
                        if ((0 == CAppDat.KasouPointJyoutaiT[nkeyValue].BeforeState) && (1 == CAppDat.KasouPointJyoutaiT[rkeyValue].BeforeState) && IsKeepHaniValue)
                        {
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.INFORMATION, MethodBase.GetCurrentMethod(), "仮想転てつ器反位保持条件確定中");
                        }
                        else
                        {
                            if (true == IsKeepHaniValue)
                            {
                                // 前回状態を保持
                                CAppDat.KasouPointJyoutaiT[nkeyValue].BeforeState = CAppDat.KasouPointJyoutaiT[nkeyValue].State;
                                CAppDat.KasouPointJyoutaiT[rkeyValue].BeforeState = CAppDat.KasouPointJyoutaiT[rkeyValue].State;
                            }
                            // 
                            else
                            {
                                // 転換状態不一致により転換する場合、前回状態を転換後の状態にする
                                CAppDat.KasouPointJyoutaiT[nkeyValue].BeforeState = 1;
                                CAppDat.KasouPointJyoutaiT[rkeyValue].BeforeState = 0;
                            }

                            // 今回状態に更新
                            CAppDat.KasouPointJyoutaiT[nkeyValue].State = 1;
                            CAppDat.KasouPointJyoutaiT[rkeyValue].State = 0;
                        }
                    }
                    else if (true == IsHaniValue)
                    {
                        if ((1 == CAppDat.KasouPointJyoutaiT[nkeyValue].BeforeState) && (0 == CAppDat.KasouPointJyoutaiT[rkeyValue].BeforeState) && IsKeepTeiiValue)
                        {
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.INFORMATION, MethodBase.GetCurrentMethod(), "仮想転てつ器定位保持条件確定中");
                        }
                        else
                        {
                            if (true == IsKeepTeiiValue)
                            {
                                // 前回状態を保持
                                CAppDat.KasouPointJyoutaiT[nkeyValue].BeforeState = CAppDat.KasouPointJyoutaiT[nkeyValue].State;
                                CAppDat.KasouPointJyoutaiT[rkeyValue].BeforeState = CAppDat.KasouPointJyoutaiT[rkeyValue].State;
                            }
                            else
                            {
                                // 転換するならば前回状態に転換後の状態にする
                                CAppDat.KasouPointJyoutaiT[nkeyValue].BeforeState = 0;
                                CAppDat.KasouPointJyoutaiT[rkeyValue].BeforeState = 1;
                            }
                            // 2014/10/03 suzutomo 仮想転てつ器転換状態保持がうまく動かないバグ対応 End

                            // 今回状態に更新
                            CAppDat.KasouPointJyoutaiT[nkeyValue].State = 0;
                            CAppDat.KasouPointJyoutaiT[rkeyValue].State = 1;
                        }
                    }
                    else
                    {
                        if (true == bKidouExists)
                        {
                            // 落下状態を記憶
                            CAppDat.KasouPointJyoutaiT[nkeyValue].IsKidouState = true;
                            CAppDat.KasouPointJyoutaiT[rkeyValue].IsKidouState = true;
                        }
                        else
                        {
                            // 落下状態を記憶ありは、動作状態で判断し転てつ器状態を解除
                            if ((true == CAppDat.KasouPointJyoutaiT[nkeyValue].IsKidouState) && (true == CAppDat.KasouPointJyoutaiT[rkeyValue].IsKidouState))
                            {
                                switch (CAppDat.TypeF.Disp.Koujyou)
                                {
                                    case 0:
                                        CAppDat.KasouPointJyoutaiT[nkeyValue].State = 0;
                                        CAppDat.KasouPointJyoutaiT[rkeyValue].State = 0;
                                        break;
                                    case 1:
                                        CAppDat.KasouPointJyoutaiT[nkeyValue].State = 1;
                                        CAppDat.KasouPointJyoutaiT[rkeyValue].State = 0;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            // 落下状態を記憶なしは、変更なし
                            else
                            {
                                // 処理なし
                            }
                            // 落下状態を記憶リセット
                            CAppDat.KasouPointJyoutaiT[nkeyValue].IsKidouState = false;
                            CAppDat.KasouPointJyoutaiT[rkeyValue].IsKidouState = false;
                            // 前回開通状態のリセット
                            CAppDat.KasouPointJyoutaiT[nkeyValue].BeforeState = 0;
                            CAppDat.KasouPointJyoutaiT[rkeyValue].BeforeState = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
            finally
            {
                nkeyValue = null;
                rkeyValue = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : PRC受信情報解析処理
        /// MODULE ID           : PrcRecvAnalyze
        ///
        /// PARAMETER IN        : 
        /// <param name="requestId">(in)要求ID</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// ＰＲＣからの受信電文を解析して該当処理を行う。
        /// 情報種別が「提案通知(6AH)」の場合は提案出力処理を行う。
        /// 情報種別が「提案設定情報(71H)」の場合は提案設定情報受信処理を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void PrcRecvAnalyze(Int32 requestId)
        {
            try
            {
                switch (requestId)
                {
                    case (Int32)REQUESTID.TEIAN:                    // 提案出力処理
                        // 提案通知格納リストに積まれているデータ分解析する
                        for (UInt16 cnt = 0; cnt < m_TeianMngT.Count; cnt++)
                        {
                            // １件分ずつ提案通知情報を解析
                            CTeianCtrl.OutputTeian(m_TeianMngT[cnt]);
                        }
                        break;

                    case (Int32)REQUESTID.SETTEIAN:
                        CTeianCtrl.RecvTeianSettei();               // 提案設定情報受信処理
                        break;

                    default:                                        // その他の処理
                        String logMsg = String.Format("想定外要求={0}", requestId);
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
        /// MODULE NAME         : ＣＴＣ装置状態解析処理
        /// MODULE ID           : CtcEqpAnalyze
        ///
        /// PARAMETER IN        : 
        /// <param name="sysNo">(in)CTCシステム番号（1-4）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// ＣＴＣ状態情報を解析して、ＣＴＣ中央伝送部不良／不能，駅不良／不能の検出を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void CtcEqpAnalyze(UInt16 sysNo)
        {
            UInt16  eqpCnt = 0;                         // ループカウンタ
            UInt16  ekiCnt = 0;                         // ループカウンタ
            UInt16  cnt = 0;                            // ループカウンタ
            UInt16  ekiNo = 0;                          // 駅番号
            UInt16  ekiKosu = 0;                        // 駅個数
            UInt16  ctcDenKosu = 0;                     // CTC中央伝送部有効数

            try
            {
                // ノード情報定数ファイルの装置情報数分ループして、ＣＴＣ中央の伝送部状態を設定する
                for (eqpCnt = 0; eqpCnt < (UInt16)CAppDat.NodeInfoF.Count; eqpCnt++)
                {
                    // 装置種別がCTC中央装置で、CTC6D種別がCTC6DN型で、システム番号が引数指定と一致する場合
                    if (((UInt16)CNodeInfoMng.MCKIND.CTC == CAppDat.NodeInfoF[eqpCnt].Kind) &&
                        (2 == CAppDat.NodeInfoF[eqpCnt].CTC6D_TYPE) &&
                        (sysNo == CAppDat.NodeInfoF[eqpCnt].SysNo))
                    {
                        // CTC中央伝送部有効数を取得
                        ctcDenKosu = CAppDat.NodeInfoF[eqpCnt].CtcDenKosu;

                        for (cnt = 1; cnt <= ctcDenKosu; cnt++)
                        {
                            // CTC中央伝送部の不良状態を設定
                            SetCtcEqpJyoutai(sysNo, 3, (UInt16)CAppDat.SETID.CTCCENTER, cnt, 2);

                            // CTC中央伝送部の不能状態を設定
                            SetCtcEqpJyoutai(sysNo, 4, (UInt16)CAppDat.SETID.CTCCENTER, cnt, 2);
                        }
                    }
                    // 上記以外の場合
                    else
                    {
                        // 処理なし
                    }
                }


                // 駅番号変換設定定数ファイルの変換情報の個数を取得
                ekiKosu = (UInt16)CAppDat.EkiConvF.Count;

                // 駅装置情報数分ループして、駅状態を設定する
                for (ekiCnt = 0; ekiCnt < ekiKosu; ekiCnt++)
                {
                    // システム番号が一致していない場合、解析しない
                    // ※システム数が多くなることを考慮し、今後、駅番号変換ファイルに装置種別を追加し、判定に加える対応が必要。
                    if (CAppDat.EkiConvF[(UInt16)(ekiCnt + 1)].SysNo != sysNo)
                    {
                        continue;
                    }

                    ekiNo = CAppDat.EkiConvF[(UInt16)(ekiCnt + 1)].EkiNoCon.Totaleki;
                                                        // 通算駅番号を取得
                    if (0 != ekiNo)
                    {
                        // 駅不良状態を設定
                        SetCtcEqpJyoutai(sysNo, 1, (UInt16)CAppDat.SETID.KEIHOU, ekiNo, 2);

                        // 駅不能状態を設定
                        SetCtcEqpJyoutai(sysNo, 2, (UInt16)CAppDat.SETID.KEIHOU, ekiNo, 2);
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

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : PRC伝送部装置状態解析処理
        /// MODULE ID           : PrcDensoEqpAnalyze
        ///
        /// PARAMETER IN        : 
        /// <param name="sysNo">(in)システム番号（1-4）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// PRC伝送部状態を解析して、装置異常／正常の検出を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void PrcDensoEqpAnalyze(UInt16 sysNo)
        {
            UInt16 eqpCnt = 0;                         // ループカウンタ

            try
            {
                // ノード情報定数ファイルの装置情報数分ループして、連動装置状態を設定する
                for (eqpCnt = 0; eqpCnt < (UInt16)CAppDat.NodeInfoF.Count; eqpCnt++)
                {
                    // 装置種別がPRC伝送部装置で、CTC6D種別が未使用で、システム番号が引数指定と一致する場合
                    if (((UInt16)CNodeInfoMng.MCKIND.PRCDENSO == CAppDat.NodeInfoF[eqpCnt].Kind) &&
                        (0 == CAppDat.NodeInfoF[eqpCnt].CTC6D_TYPE) &&
                        (sysNo == CAppDat.NodeInfoF[eqpCnt].SysNo))
                    {
                        // 連動装置有効個数分ループ
                        for (ushort cnt = 1; cnt <= CAppDat.NodeInfoF[eqpCnt].CtcDenKosu; cnt++)
                        {
                            // 連動装置の状態を設定
                            SetRendoEqpJyoutai(sysNo, cnt);
                        }
                    }
                    // 上記以外の場合
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

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 他線区ＩＦ装置状態解析処理
        /// MODULE ID           : TidifEqpAnalyze
        ///
        /// PARAMETER IN        : 
        /// <param name="sysNo">(in)CTCシステム番号（1-4）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// 他線区ＩＦ状態情報を解析して、他線区ＩＦ装置駅不良／不能の検出を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void TidifEqpAnalyze(UInt16 sysNo)
        {
            UInt16 ekiCnt = 0;                         // ループカウンタ
            UInt16 ekiNo = 0;                          // 駅番号
            UInt16 ekiKosu = 0;                        // 駅個数

            try
            {
                // 駅番号変換設定定数ファイルの変換情報の個数を取得
                ekiKosu = (UInt16)CAppDat.EkiConvF.Count;

                // 駅装置情報数分ループして、駅状態を設定する
                for (ekiCnt = 0; ekiCnt < ekiKosu; ekiCnt++)
                {
                    // システム番号が一致していない場合、解析しない
                    // ※システム数が多くなることを考慮し、今後、駅番号変換ファイルに装置種別を追加し、判定に加える対応が必要。
                    if (CAppDat.EkiConvF[(UInt16)(ekiCnt + 1)].SysNo != sysNo)
                    {
                        continue;
                    }

                    ekiNo = CAppDat.EkiConvF[(UInt16)(ekiCnt + 1)].EkiNoCon.Totaleki;
                    // 通算駅番号を取得
                    if (0 != ekiNo)
                    {
                        // 駅不良状態を設定
                        SetTidifEqpJyoutai(sysNo, 1, (UInt16)CAppDat.SETID.KEIHOU, ekiNo, 2);

                        // 駅不能状態を設定
                        SetTidifEqpJyoutai(sysNo, 2, (UInt16)CAppDat.SETID.KEIHOU, ekiNo, 2);
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

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 他線区IF状態情報 モード状態・警報解析処理
        /// MODULE ID           : TidIfStatusModeKeihouAnalyze
        ///
        /// PARAMETER IN        : 
        /// <param name="sysNo">(in)システム番号（1-4）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// 他線区IF状態情報を解析して、装置異常／正常、警報の検出を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void TidIfStatusModeKeihouAnalyze(UInt16 sysNo)
        {
            UInt16 eqpKind = 0;                     // 装置種別
            UInt16 oldEqpState, newEqpState;        // 
            byte trueBit = 0x00;                    // 異常反転ビット
            byte falseBit = 0x02;                   // 異常ビット
            String syncErrorMsg = String.Empty;
            ushort[] checkBit = { 0x0001, 0x0002, 0x0004, 0x0008,
                                  0x0010, 0x0020, 0x0040, 0x0080 };

            try
            {
                // 装置状態テーブル排他制御開始
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SOUTISTATUS].WaitOne();
                try
                {
                    if (1 == sysNo)
                    {
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_TIDIF1;    // 装置種別を設定
                    }
                    else if (2 == sysNo)
                    {
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_TIDIF2;    // 装置種別を設定
                    }
                    else if (3 == sysNo)
                    {
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_TIDIF3;    // 装置種別を設定
                    }
                    else if (4 == sysNo)
                    {
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_TIDIF4;    // 装置種別を設定
                    }
                    else
                    {
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.NONE;
                    }

                    // 他線区IF状態情報で警報出力に使用する参照ビットを取得し、解析する
                    foreach (ushort key in CAppDat.AlarmInfoF.TidifAlarm[sysNo - 1].IfStatus.BitStatus.Keys)
                    {
                        // 状態値を初期化
                        oldEqpState = 0;
                        newEqpState = 0;

                        // 参照するビット情報を取得
                        CAlarmBitStatus status = CAppDat.AlarmInfoF.TidifAlarm[sysNo - 1].IfStatus.BitStatus[key];

                        // 前回状態を解析
                        if(m_OtherIFStatInfoMngOldT[sysNo - 1].RecvBuffData.Length != 0)
                        {
                            oldEqpState = (ushort)(m_OtherIFStatInfoMngOldT[sysNo - 1].RecvBuffData[2] & checkBit[key]);
                        }

                        // 今回状態を解析
                        if(m_OtherIFStatInfoMngT[sysNo - 1].RecvBuffData.Length != 0)
                        {
                            newEqpState = (ushort)(m_OtherIFStatInfoMngT[sysNo - 1].RecvBuffData[2] & checkBit[key]);
                        }

                        // 前回値と異なる場合
                        if (oldEqpState != newEqpState)
                        {
                            // 正常値と比較し、モード状態を更新
                            if (newEqpState == status.trueValue)
                            {
                                // 正常値の場合
                                CAppDat.SotiJyoutaiT.TidifJyoutai[sysNo - 1].TidIfModeJyoutai[key] = trueBit;
                            }
                            else
                            {
                                // 異常値の場合
                                CAppDat.SotiJyoutaiT.TidifJyoutai[sysNo - 1].TidIfModeJyoutai[key] = falseBit;
                            }

                            // 装置状態表示更新要求処理
                            IntPtr wParam = new IntPtr((UInt16)CAppDat.UPDATEREQUEST.OTHERSTATEUPDATE);
                            CCommon.LocalPostMessage(true, CAppDat.WM_USER_MAINFORMUPDATE, wParam, IntPtr.Zero);


                            // 警報出力なしの場合
                            if (false == CCommon.CheckOutputKeihou(eqpKind, (ushort)CKeihouCom.TIDIFKIND.IF_STATUS_BIT, key, (newEqpState != status.trueValue)))
                            {
                                continue;
                            }

                            // 正常値と比較し、警報出力
                            if (newEqpState == status.trueValue)
                            {
                                // 正常値の場合

                                // 他装置故障警報出力処理（復旧）
                                CKeihouCtrl.OutputDenBitKeihou(eqpKind, (UInt16)CKeihouCom.TIDIFKIND.IF_STATUS, 0, key, 0);
                            }
                            else
                            {
                                // 異常値の場合

                                // 他装置故障警報出力処理（発生）
                                CKeihouCtrl.OutputDenBitKeihou(eqpKind, (UInt16)CKeihouCom.TIDIFKIND.IF_STATUS, 0, key, 1);
                            }
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
                    // 装置状態テーブル排他制御終了
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SOUTISTATUS].ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 他線区IF列番情報 運転状態・警報解析処理
        /// MODULE ID           : TidIfRetUntenKeihouAnalyze
        ///
        /// PARAMETER IN        : 
        /// <param name="sysNo">(in)システム番号（1-4）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// 他線区IF列番情報を解析して、装置異常／正常、警報の検出を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void TidIfRetUntenKeihouAnalyze(UInt16 sysNo)
        {
            UInt16 eqpKind = 0;                     // 装置種別
            UInt16 oldEqpState, newEqpState;        // 
            byte trueBit = 0x00;                    // 異常反転ビット
            byte falseBit = 0x02;                   // 異常ビット
            String syncErrorMsg = String.Empty;
            ushort[] checkBit = { 0x0001, 0x0002, 0x0004, 0x0008,
                                  0x0010, 0x0020, 0x0040, 0x0080,
                                  0x0100, 0x0200, 0x0400, 0x0800,
                                  0x1000, 0x2000, 0x4000, 0x8000 };
            
            try
            {
                // 装置状態テーブル排他制御開始
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SOUTISTATUS].WaitOne();
                try
                {
                    if (1 == sysNo)
                    {
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_TIDIF1;    // 装置種別を設定
                    }
                    else if (2 == sysNo)
                    {
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_TIDIF2;    // 装置種別を設定
                    }
                    else if (3 == sysNo)
                    {
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_TIDIF3;    // 装置種別を設定
                    }
                    else if (4 == sysNo)
                    {
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_TIDIF4;    // 装置種別を設定
                    }
                    else
                    {
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.NONE;
                    }

                    // 他線区IF状態情報で警報出力に使用する参照ビットを取得し、解析する
                    foreach (ushort key in CAppDat.AlarmInfoF.TidifAlarm[sysNo - 1].RetsuInf.BitStatus.Keys)
                    {
                        // 状態値を初期化
                        oldEqpState = 0;
                        newEqpState = 0;

                        // 参照するビット情報を取得
                        CAlarmBitStatus status = CAppDat.AlarmInfoF.TidifAlarm[sysNo - 1].RetsuInf.BitStatus[key];

                        // 前回状態を解析
                        if (m_OtherIFRetuInfoMngOldT[sysNo - 1].RecvBuffData.Length != 0)
                        {
                            oldEqpState = (ushort)(m_OtherIFRetuInfoMngOldT[sysNo - 1].RecvBuffData[1] & checkBit[key]);
                        }

                        // 今回状態を解析
                        if (m_OtherIFRetuInfoMngT[sysNo - 1].RecvBuffData.Length != 0)
                        {
                            newEqpState = (ushort)(m_OtherIFRetuInfoMngT[sysNo - 1].RecvBuffData[1] & checkBit[key]);
                        }

                        // 前回値と異なる場合
                        if (oldEqpState != newEqpState)
                        {
                            // 正常値と比較し、モード状態を更新
                            if (newEqpState == status.trueValue)
                            {
                                // 正常値の場合
                                CAppDat.SotiJyoutaiT.TidifJyoutai[sysNo - 1].TidIfRetUntenJyoutai[key] = trueBit;
                            }
                            else
                            {
                                // 異常値の場合
                                CAppDat.SotiJyoutaiT.TidifJyoutai[sysNo - 1].TidIfRetUntenJyoutai[key] = falseBit;
                            }

                            // 装置状態表示更新要求処理
                            IntPtr wParam = new IntPtr((UInt16)CAppDat.UPDATEREQUEST.OTHERSTATEUPDATE);
                            CCommon.LocalPostMessage(true, CAppDat.WM_USER_MAINFORMUPDATE, wParam, IntPtr.Zero);


                            // 警報出力なしの場合
                            if (false == CCommon.CheckOutputKeihou(eqpKind, (ushort)CKeihouCom.TIDIFKIND.IF_RETSUBAN_BIT, key, (newEqpState != status.trueValue)))
                            {
                                continue;
                            }

                            // 正常値と比較し、警報出力
                            if (newEqpState == status.trueValue)
                            {
                                // 正常値の場合

                                // 他装置故障警報出力処理（復旧）
                                CKeihouCtrl.OutputDenBitKeihou(eqpKind, (UInt16)CKeihouCom.TIDIFKIND.IF_RETSUBAN, 0, key, 0);
                            }
                            else
                            {
                                // 異常値の場合

                                // 他装置故障警報出力処理（発生）
                                CKeihouCtrl.OutputDenBitKeihou(eqpKind, (UInt16)CKeihouCom.TIDIFKIND.IF_RETSUBAN, 0, key, 1);
                            }
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
                    // 装置状態テーブル排他制御終了
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SOUTISTATUS].ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : ＣＴＣ装置状態設定処理
        /// MODULE ID           : SetCtcEqpJyoutai
        ///
        /// PARAMETER IN        : 
        /// <param name="sysNo">(in)CTCシステム番号（1-4）</param>
        /// <param name="type">(in)タイプ（1=駅不良、2=駅不能、3=CTC中央伝送部不良、4=CTC中央伝送部不能）</param>
        /// <param name="kbNo">(in)設定区分番号</param>
        /// <param name="ekiNo">(in)駅番号</param>
        /// <param name="hoko">(in)方向</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// 駅不良／不能を指定された場合は、ＣＴＣ状態情報内の各種警報状態を解析して、
        /// 装置状態テーブルの駅装置状態に不良／不能状態の設定を行う。
        /// CTC中央伝送部駅不良／不能を指定された場合は、ＣＴＣ状態情報内のＣＴＣ中央
        /// 装置状態を解析して、装置状態テーブルのCTC中央伝送部状態に不良／不能状態の
        /// 設定を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void SetCtcEqpJyoutai(UInt16 sysNo, UInt16 type, UInt16 kbNo, UInt16 ekiNo, UInt16 hoko)
        {
            CSetData bitInfo = null;                    // 装置状態ビット情報
            UInt16  eqpKind = 0;                        // 装置種別
            UInt16  denKind = 0;                        // 伝送部種別
            UInt16  uiRet = 0;                          // 戻り値取得用
            Byte    funouBit = 0x02;                    // 不能ビット
            Byte    setBit = 0;                         // 状態設定ビット
            Byte    oldEqpState = 0;                    // 前回装置状態
            Byte    nowEqpState = 0;                    // 今回装置状態
            String  strAppLog = String.Empty;           // ログメッセージ
            String  syncErrorMsg = String.Empty;        // 同期エラーメッセージ

            try
            {
                oldEqpState = 0;                        // 前回装置状態を初期化
                nowEqpState = 0;                        // 今回装置状態を初期化

                if ((0 < sysNo) && (sysNo <= CAppDat.CTCMAX))
                {
                    // 処理なし
                }
                else
                {
                    strAppLog = String.Format("システム番号異常 no={0}", sysNo);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    return;
                }

                // 駅番号が範囲外
                if ((ekiNo < 1) || (ekiNo > CAppDat.TOTALEkimAX))
                {
                    strAppLog = String.Format("駅番号が範囲外：{0:D}", ekiNo);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    return;
                }
                // 駅番号が範囲内
                else
                {
                    // 処理なし
                }

                if ((1 <= type) && (type <= 4))         // タイプが範囲内の場合
                {
                    // 処理なし
                }
                else
                {
                    strAppLog = String.Format("タイプが範囲外：{0:D}", type);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    return;
                }

                // 解析用のビット情報を作成する
                bitInfo = new CSetData();

                bitInfo.KbNo  = kbNo;                   // 設定区分番号
                bitInfo.Ekino = ekiNo;                  // 駅番号
                bitInfo.Hoko  = hoko;                   // 方向

                if ((1 == type) || (3 == type))         // 駅不良／CTC中央伝送部不良を指定された場合
                {
                    bitInfo.Kigou = @"FLK";             // 不良
                    setBit = 0x01;
                }
                else                                    // 駅不能／CTC中央伝送部不能を指定された場合
                {
                    bitInfo.Kigou = @"SFL";             // 不能
                    setBit = funouBit;
                }

                // ビット情報の解析を行い、ＣＴＣ中央伝送部不能／不良状態を設定する
                uiRet = AnalyzeBitInfo(ALMBITCHKTYPE.NORMAL, bitInfo, sysNo);  // ビット情報条件解析処理を行う

                if ((1 != uiRet) && (2 != uiRet))       // 条件成立、不成立以外の場合
                {
                    // 処理なし
                }
                else if ((1 == type) || (2 == type))    // 駅不良／駅不能を指定された場合
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SOUTISTATUS].WaitOne();
                    try
                    {
                        oldEqpState = CAppDat.SotiJyoutaiT.EkiJyoutai[bitInfo.Ekino - 1];   // 前回装置状態を取得

                        // 駅装置状態を設定
                        if (1 == uiRet)                 // 条件成立の場合
                        {
                            CAppDat.SotiJyoutaiT.EkiJyoutai[bitInfo.Ekino - 1] |= setBit;
                        }
                        else if (2 == uiRet)            // 条件不成立の場合
                        {
                            CAppDat.SotiJyoutaiT.EkiJyoutai[bitInfo.Ekino - 1] &= (Byte)~setBit;
                        }
                        else                            // 上記以外の場合
                        {
                            // 処理なし
                        }

                        nowEqpState = CAppDat.SotiJyoutaiT.EkiJyoutai[bitInfo.Ekino - 1];   // 今回装置状態を取得
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
                }
                else                                    // CTC中央伝送部不良／不能を指定された場合
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SOUTISTATUS].WaitOne();
                    try
                    {
                        switch(bitInfo.Ekino)
                        {
                            case  1:
                                denKind = (UInt16)CKeihouCom.DENKIND.DEN_A; // 伝送部種別を設定
                                oldEqpState = CAppDat.SotiJyoutaiT.CtcJyoutai[sysNo - 1].CtcDenAJyoutai;  // 前回装置状態を取得

                                // CTC中央伝送部A状態を設定
                                if (1 == uiRet)         // 条件成立の場合
                                {
                                    CAppDat.SotiJyoutaiT.CtcJyoutai[sysNo - 1].CtcDenAJyoutai |= setBit;
                                }
                                else if (2 == uiRet)    // 条件不成立の場合
                                {
                                    CAppDat.SotiJyoutaiT.CtcJyoutai[sysNo - 1].CtcDenAJyoutai &= (Byte)~setBit;
                                }
                                else                    // 上記以外の場合
                                {
                                    // 処理なし
                                }

                                nowEqpState = CAppDat.SotiJyoutaiT.CtcJyoutai[sysNo - 1].CtcDenAJyoutai;  // 今回装置状態を取得

                                break;

                            case  2:
                                denKind = (UInt16)CKeihouCom.DENKIND.DEN_B; // 伝送部種別を設定
                                oldEqpState = CAppDat.SotiJyoutaiT.CtcJyoutai[sysNo - 1].CtcDenBJyoutai;  // 前回装置状態を取得

                                // CTC中央伝送部B状態を設定
                                if (1 == uiRet)         // 条件成立の場合
                                {
                                    CAppDat.SotiJyoutaiT.CtcJyoutai[sysNo - 1].CtcDenBJyoutai |= setBit;
                                }
                                else if (2 == uiRet)    // 条件不成立の場合
                                {
                                    CAppDat.SotiJyoutaiT.CtcJyoutai[sysNo - 1].CtcDenBJyoutai &= (Byte)~setBit;
                                }
                                else                    // 上記以外の場合
                                {
                                    // 処理なし
                                }

                                nowEqpState = CAppDat.SotiJyoutaiT.CtcJyoutai[sysNo - 1].CtcDenBJyoutai;  // 今回装置状態を取得

                                break;

                            case  3:
                                denKind = (UInt16)CKeihouCom.DENKIND.DEN_C; // 伝送部種別を設定
                                oldEqpState = CAppDat.SotiJyoutaiT.CtcJyoutai[sysNo - 1].CtcDenCJyoutai;  // 前回装置状態を取得

                                // CTC中央伝送部C状態を設定
                                if (1 == uiRet)         // 条件成立の場合
                                {
                                    CAppDat.SotiJyoutaiT.CtcJyoutai[sysNo - 1].CtcDenCJyoutai |= setBit;
                                }
                                else if (2 == uiRet)    // 条件不成立の場合
                                {
                                    CAppDat.SotiJyoutaiT.CtcJyoutai[sysNo - 1].CtcDenCJyoutai &= (Byte)~setBit;
                                }
                                else                    // 上記以外の場合
                                {
                                    // 処理なし
                                }

                                nowEqpState = CAppDat.SotiJyoutaiT.CtcJyoutai[sysNo - 1].CtcDenCJyoutai;  // 今回装置状態を取得

                                break;

                            case  4:
                                denKind = (UInt16)CKeihouCom.DENKIND.DEN_D; // 伝送部種別を設定
                                oldEqpState = CAppDat.SotiJyoutaiT.CtcJyoutai[sysNo - 1].CtcDenDJyoutai;  // 前回装置状態を取得

                                // CTC中央伝送部D状態を設定
                                if (1 == uiRet)         // 条件成立の場合
                                {
                                    CAppDat.SotiJyoutaiT.CtcJyoutai[sysNo - 1].CtcDenDJyoutai |= setBit;
                                }
                                else if (2 == uiRet)    // 条件不成立の場合
                                {
                                    CAppDat.SotiJyoutaiT.CtcJyoutai[sysNo - 1].CtcDenDJyoutai &= (Byte)~setBit;
                                }
                                else                    // 上記以外の場合
                                {
                                    // 処理なし
                                }

                                nowEqpState = CAppDat.SotiJyoutaiT.CtcJyoutai[sysNo - 1].CtcDenDJyoutai;  // 今回装置状態を取得

                                break;

                            default:
                                strAppLog = String.Format("ＣＴＣ中央伝送部指定不正：{0:D}", bitInfo.Ekino);
                                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                                break;
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
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SOUTISTATUS].ReleaseMutex();
                    }
                }

                // 表示更新と警報出力を行う
                if ((oldEqpState & funouBit) != (nowEqpState & funouBit))   // 状態変化ありの場合
                {
                    if ((1 == type) || (2 == type))     // 駅不良／駅不能を指定された場合
                    {
                        if ((nowEqpState & funouBit) != 0)  // 不能の場合
                        {
                            // 駅故障警報出力処理（発生）
                            CKeihouCtrl.OutputEkiKeihou(bitInfo.Ekino, 1);
                        }
                        else                            // 不能ではない場合
                        {
                            // 駅故障警報出力処理（復旧）
                            CKeihouCtrl.OutputEkiKeihou(bitInfo.Ekino, 0);
                        }
                    }
                    else if ((3 == type) || (4 == type))    // CTC中央伝送部不良／CTC中央伝送部不能を指定された場合
                    {
                        // 装置状態表示更新要求処理
                        //[未使用] CKeihouCom.RequestEqpStatusDisp((UInt16)CAppDat.WAITID.OTHERSTATEUPDATE);
                        IntPtr wParam = new IntPtr((UInt16)CAppDat.UPDATEREQUEST.OTHERSTATEUPDATE);
                        CCommon.LocalPostMessage(true, CAppDat.WM_USER_MAINFORMUPDATE, wParam, IntPtr.Zero);

                        if (1 == sysNo)
                        {
                            eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_CTC1;  // 装置種別を設定
                        }
                        else if (2 == sysNo)
                        {
                            eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_CTC2;  // 装置種別を設定
                        }
                        else if (3 == sysNo)
                        {
                            eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_CTC3;  // 装置種別を設定
                        }
                        else if (4 == sysNo)
                        {
                            eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_CTC4;  // 装置種別を設定
                        }
                        else
                        {
                            eqpKind = (UInt16)CKeihouCom.EQPKIND.NONE;
                        }
                            
                        // 出力なしの場合
                        if (false == CCommon.CheckOutputKeihou(eqpKind, bitInfo.Ekino, 0, ((nowEqpState & funouBit) != 0)))
                        {
                            return;
                        }

                        if ((nowEqpState & funouBit) != 0)  // 不能の場合
                        {
                            // 他装置故障警報出力処理（発生）
                            CKeihouCtrl.OutputEqpKeihou(eqpKind, denKind, 1);
                        }
                        else                            // 不能ではない場合
                        {
                            // 他装置故障警報出力処理（復旧）
                            CKeihouCtrl.OutputEqpKeihou(eqpKind, denKind, 0);
                        }
                    }
                    else                                // CTC中央伝送部不良・CTC中央伝送部不能ではない場合
                    {
                        // 処理なし
                    }
                }
                else                                    // 状態変化なしの場合
                {
                    // 処理なし
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
            finally
            {
                bitInfo = null;
            }

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 連動装置状態設定処理
        /// MODULE ID           : SetRendoEqpJyoutai
        ///
        /// PARAMETER IN        : 
        /// <param name="sysNo">(in)CTCシステム番号（1-4）</param>
        /// <param name="renIndex">(in)連動装置有効個数</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// てこ／表示情報内のPRC伝送部状態を解析して、
        /// 装置状態テーブルの連動装置状態に異常／正常状態の設定を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void SetRendoEqpJyoutai(UInt16 sysNo, UInt16 renIndex)
        {
            Byte state = 0;                                     // 運転状態
            Byte[] rendoFunouBit = { 0x01, 0x02, 0x04, 0x08 };  // 各連動装置 不能参照ビット
            Byte funouBit = 0x02;                               // 不能ビット
            Byte oldEqpState = 0;                               // 前回装置状態
            Byte nowEqpState = 0;                               // 今回装置状態
            UInt16 eqpKind = 0;                                 // 装置種別
            UInt16 denKind = 0;                                 // 伝送部種別
            String strAppLog = String.Empty;                    // ログメッセージ
            String syncErrorMsg = String.Empty;                 // 同期エラーメッセージ
            String logMsg = String.Empty;

            try
            {
                oldEqpState = 0;                        // 前回装置状態を初期化
                nowEqpState = 0;                        // 今回装置状態を初期化

                if ((0 < sysNo) && (sysNo <= CAppDat.PRCDENSOMAX))
                {
                    // 処理なし
                }
                else
                {
                    strAppLog = String.Format("システム番号異常 no={0}", sysNo);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    return;
                }

                if (null == m_TekoHyojiInfoMngT)
                {
                    logMsg = String.Format("パラメータエラー");
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                    return;
                }
                else
                {
                    // 処理なし
                }

                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SOUTISTATUS].WaitOne();
                try
                {
                    // 運転状態を取得
                    state = (Byte)m_TekoHyojiInfoMngT[sysNo - 1].RecvBuffData[2];

                    switch (renIndex)
                    {
                        case 1:
                            denKind = (UInt16)CKeihouCom.RENKIND.REN_A;                                     // 伝送部種別を設定
                            oldEqpState = CAppDat.SotiJyoutaiT.PrcDensoJyoutai[sysNo - 1].RendoAJyoutai;    // 前回装置状態を取得

                            // 正常・不能の判定処理
                            if ((state & rendoFunouBit[renIndex - 1]) != 0x00)
                            {   // 解析値結果が異常の場合

                                CAppDat.SotiJyoutaiT.PrcDensoJyoutai[sysNo - 1].RendoAJyoutai |= funouBit;
                            }
                            else
                            {   // 解析値結果が正常の場合

                                CAppDat.SotiJyoutaiT.PrcDensoJyoutai[sysNo - 1].RendoAJyoutai &= (Byte)~funouBit;
                            }

                            nowEqpState = CAppDat.SotiJyoutaiT.PrcDensoJyoutai[sysNo - 1].RendoAJyoutai;    // 今回装置状態を取得

                            break;

                        case 2:
                            denKind = (UInt16)CKeihouCom.RENKIND.REN_B;                                     // 伝送部種別を設定
                            oldEqpState = CAppDat.SotiJyoutaiT.PrcDensoJyoutai[sysNo - 1].RendoBJyoutai;    // 前回装置状態を取得

                            // 正常・不能の判定処理
                            if ((state & rendoFunouBit[renIndex - 1]) != 0x00)
                            {   // 解析値結果が異常の場合

                                CAppDat.SotiJyoutaiT.PrcDensoJyoutai[sysNo - 1].RendoBJyoutai |= funouBit;
                            }
                            else
                            {   // 解析値結果が正常の場合

                                CAppDat.SotiJyoutaiT.PrcDensoJyoutai[sysNo - 1].RendoBJyoutai &= (Byte)~funouBit;
                            }

                            nowEqpState = CAppDat.SotiJyoutaiT.PrcDensoJyoutai[sysNo - 1].RendoBJyoutai;    // 今回装置状態を取得

                            break;

                        case 3:
                            denKind = (UInt16)CKeihouCom.RENKIND.REN_C;                                     // 伝送部種別を設定
                            oldEqpState = CAppDat.SotiJyoutaiT.PrcDensoJyoutai[sysNo - 1].RendoCJyoutai;    // 前回装置状態を取得

                            // 正常・不能の判定処理
                            if ((state & rendoFunouBit[renIndex - 1]) != 0x00)
                            {   // 解析値結果が異常の場合

                                CAppDat.SotiJyoutaiT.PrcDensoJyoutai[sysNo - 1].RendoCJyoutai |= funouBit;
                            }
                            else
                            {   // 解析値結果が正常の場合

                                CAppDat.SotiJyoutaiT.PrcDensoJyoutai[sysNo - 1].RendoCJyoutai &= (Byte)~funouBit;
                            }

                            nowEqpState = CAppDat.SotiJyoutaiT.PrcDensoJyoutai[sysNo - 1].RendoCJyoutai;    // 今回装置状態を取得

                            break;

                        case 4:
                            denKind = (UInt16)CKeihouCom.RENKIND.REN_D;                                     // 伝送部種別を設定
                            oldEqpState = CAppDat.SotiJyoutaiT.PrcDensoJyoutai[sysNo - 1].RendoDJyoutai;    // 前回装置状態を取得

                            // 正常・不能の判定処理
                            if ((state & rendoFunouBit[renIndex - 1]) != 0x00)
                            {   // 解析値結果が異常の場合

                                CAppDat.SotiJyoutaiT.PrcDensoJyoutai[sysNo - 1].RendoDJyoutai |= funouBit;
                            }
                            else
                            {   // 解析値結果が正常の場合

                                CAppDat.SotiJyoutaiT.PrcDensoJyoutai[sysNo - 1].RendoDJyoutai &= (Byte)~funouBit;
                            }

                            nowEqpState = CAppDat.SotiJyoutaiT.PrcDensoJyoutai[sysNo - 1].RendoDJyoutai;    // 今回装置状態を取得

                            break;

                        default:
                            strAppLog = String.Format("連動装置指定不正：{0:D}", renIndex);
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                            break;
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
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SOUTISTATUS].ReleaseMutex();
                }

                // 表示更新と警報出力を行う
                if ((oldEqpState & funouBit) != (nowEqpState & funouBit))       // 状態変化ありの場合
                {
                    // 装置状態表示更新要求処理
                    IntPtr wParam = new IntPtr((UInt16)CAppDat.UPDATEREQUEST.OTHERSTATEUPDATE);
                    CCommon.LocalPostMessage(true, CAppDat.WM_USER_MAINFORMUPDATE, wParam, IntPtr.Zero);

                    if (1 == sysNo)
                    {
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_PRCDENSO1;     // 装置種別を設定
                    }
                    else if (2 == sysNo)
                    {
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_PRCDENSO2;     // 装置種別を設定
                    }
                    else if (3 == sysNo)
                    {
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_PRCDENSO3;     // 装置種別を設定
                    }
                    else if (4 == sysNo)
                    {
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.EQP_PRCDENSO4;     // 装置種別を設定
                    }
                    else
                    {
                        eqpKind = (UInt16)CKeihouCom.EQPKIND.NONE;
                    }

                    if ((nowEqpState & funouBit) != 0)  // 不能の場合
                    {
                        // 他装置故障警報出力処理（発生）
                        CKeihouCtrl.OutputEqpKeihou(eqpKind, denKind, 1);
                    }
                    else                                // 不能ではない場合
                    {
                        // 他装置故障警報出力処理（復旧）
                        CKeihouCtrl.OutputEqpKeihou(eqpKind, denKind, 0);
                    }
                }
                else                                    // 状態変化なしの場合
                {
                    // 処理なし
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 他線区ＩＦ装置状態設定処理
        /// MODULE ID           : SetTidifEqpJyoutai
        ///
        /// PARAMETER IN        : 
        /// <param name="sysNo">(in)システム番号（1-4）</param>
        /// <param name="type">(in)タイプ（1=駅不良、2=駅不能）</param>
        /// <param name="kbNo">(in)設定区分番号</param>
        /// <param name="ekiNo">(in)駅番号</param>
        /// <param name="hoko">(in)方向</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// 駅不良／不能を指定された場合は、他線区ＩＦ装置状状態情報内の各種警報状態を解析して、
        /// 装置状態テーブルの駅装置状態に不良／不能状態の設定を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void SetTidifEqpJyoutai(UInt16 sysNo, UInt16 type, UInt16 kbNo, UInt16 ekiNo, UInt16 hoko)
        {
            CSetData bitInfo = null;                    // 装置状態ビット情報
            UInt16  uiRet = 0;                          // 戻り値取得用
            Byte    funouBit = 0x02;                    // 不能ビット
            Byte    setBit = 0;                         // 状態設定ビット
            Byte    oldEqpState = 0;                    // 前回装置状態
            Byte    nowEqpState = 0;                    // 今回装置状態
            String  strAppLog = String.Empty;           // ログメッセージ
            String  syncErrorMsg = String.Empty;        // 同期エラーメッセージ

            try
            {
                oldEqpState = 0;                        // 前回装置状態を初期化
                nowEqpState = 0;                        // 今回装置状態を初期化

                if ((0 < sysNo) && (sysNo <= CAppDat.TIDMAX))
                {
                    // 処理なし
                }
                else
                {
                    strAppLog = String.Format("システム番号異常 no={0}", sysNo);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    return;
                }

                // 駅番号が範囲外
                if ((ekiNo < 1) || (ekiNo > CAppDat.TOTALEkimAX))
                {
                    strAppLog = String.Format("駅番号が範囲外：{0:D}", ekiNo);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    return;
                }
                // 駅番号が範囲内
                else
                {
                    // 処理なし
                }

                if ((1 <= type) && (type <= 2))         // タイプが範囲内の場合
                {
                    // 処理なし
                }
                else
                {
                    strAppLog = String.Format("タイプが範囲外：{0:D}", type);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    return;
                }

                // 解析用のビット情報を作成する
                bitInfo = new CSetData();

                bitInfo.KbNo = kbNo;                    // 設定区分番号
                bitInfo.Ekino = ekiNo;                  // 駅番号
                bitInfo.Hoko = hoko;                    // 方向

                if (1 == type)                          // 駅不良を指定された場合
                {
                    bitInfo.Kigou = @"FLK";             // 不良
                    setBit = 0x01;
                }
                else                                    // 駅不能を指定された場合
                {
                    bitInfo.Kigou = @"SFL";             // 不能
                    setBit = funouBit;
                }

                // ビット情報の解析を行い、不能／不良状態を設定する
                uiRet = AnalyzeBitInfoTidif(ALMBITCHKTYPE.NORMAL, bitInfo, sysNo);          // ビット情報条件解析処理を行う

                if ((1 != uiRet) && (2 != uiRet))       // 条件成立、不成立以外の場合
                {
                    // 処理なし
                }
                else                                    // 駅不良／駅不能を指定された場合
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.SOUTISTATUS].WaitOne();
                    try
                    {
                        oldEqpState = CAppDat.SotiJyoutaiT.EkiJyoutai[bitInfo.Ekino - 1];   // 前回装置状態を取得

                        // 駅装置状態を設定
                        if (1 == uiRet)                 // 条件成立の場合
                        {
                            CAppDat.SotiJyoutaiT.EkiJyoutai[bitInfo.Ekino - 1] |= setBit;
                        }
                        else if (2 == uiRet)            // 条件不成立の場合
                        {
                            CAppDat.SotiJyoutaiT.EkiJyoutai[bitInfo.Ekino - 1] &= (Byte)~setBit;
                        }
                        else                            // 上記以外の場合
                        {
                            // 処理なし
                        }

                        nowEqpState = CAppDat.SotiJyoutaiT.EkiJyoutai[bitInfo.Ekino - 1];   // 今回装置状態を取得
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
                }

                // 表示更新と警報出力を行う
                if ((oldEqpState & funouBit) != (nowEqpState & funouBit))                   // 状態変化ありの場合
                {
                    if ((1 == type) || (2 == type))     // 駅不良／駅不能を指定された場合
                    {
                        if ((nowEqpState & funouBit) != 0)  // 不能の場合
                        {
                            // 駅故障警報出力処理（発生）
                            CKeihouCtrl.OutputEkiKeihou(bitInfo.Ekino, 1);
                        }
                        else                                // 不能ではない場合
                        {
                            // 駅故障警報出力処理（復旧）
                            CKeihouCtrl.OutputEkiKeihou(bitInfo.Ekino, 0);
                        }
                    }
                    else
                    {
                        // 処理なし
                    }
                }
                else                                    // 状態変化なしの場合
                {
                    // 処理なし
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
            finally
            {
                bitInfo = null;
            }

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : ＣＴＣ警報解析処理
        /// MODULE ID           : CtcKeihouAnalyze
        ///
        /// PARAMETER IN        : 
        /// <param name="sysNo">(in)CTCシステム番号（1-4）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// ＣＴＣ状態情報を解析して警報出力を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void CtcKeihouAnalyze(UInt16 sysNo)
        {
            UInt16 uiRetS;                              // 戻り値取得用（１ビット警報）
            UInt16 uiRetD;                              // 戻り値取得用（複数ビット警報）
            bool isAllJyokenS = false;                  // 全ビット情報条件成立フラグ（true=成立、false=不成立、１ビット警報）
            bool isAllJyokenD = false;                  // 全ビット情報条件成立フラグ（true=成立、false=不成立、複数ビット警報）
            bool isAllJyokenD1 = false;                 // 全ビット情報条件成立フラグ（true=成立、false=不成立、複数ビット警報）
            Int32 isJouhenCntS = 0;                     // 状変有カウンタ（１ビット警報）
            Int32 isJouhenCntD = 0;                     // 状変有カウンタ（複数ビット警報）
            Int32 iCnt = 0;                             // ループカウンタ
            Int32 iCnt1 = 0;                            // ループカウンタ（保存）
            String strAppLog = String.Empty;            // ログメッセージ

            try
            {
                // 警報監視設定定数ファイルのステータスビット情報数分ループ
                for (iCnt = 0; iCnt < CAppDat.AlarmInfoF.StateBitAlarm.Count; iCnt++)
                {
                    isJouhenCntS = 0;                    // 状変有カウンタに０を設定
                    isJouhenCntD = 0;                    // 状変有カウンタに０を設定
                    isAllJyokenS = false;                // 全ビット情報条件成立フラグに不成立を設定
                    isAllJyokenD = false;                // 全ビット情報条件成立フラグに不成立を設定

                    // 警報監視設定定数ファイルのステータスビット情報のビット情報数が０（ビット情報がない）場合
                    if (0 == CAppDat.AlarmInfoF.StateBitAlarm[iCnt].BitInfo.Count)
                    {
                        strAppLog = String.Format("警報監視設定定数ファイルのステータスビット情報にビット情報がない：{0:D}", iCnt);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    }
                    // 警報監視設定定数ファイルのステータスビット情報のビット情報数が０ではない（ビット情報がある）場合
                    else
                    {
                        // 警報監視設定定数ファイルのステータスビット情報のビット情報が１つだけある（１ビット警報）場合
                        if (1 == CAppDat.AlarmInfoF.StateBitAlarm[iCnt].BitInfo.Count)
                        {
                            uiRetS = AnalyzeBitInfo(ALMBITCHKTYPE.NORMAL, CAppDat.AlarmInfoF.StateBitAlarm[iCnt].BitInfo[0], sysNo);
                                                        // ビット情報条件解析処理を行う
                            if (1 == uiRetS)                 // 条件成立の場合
                            {
                                isJouhenCntS++;              // 状変有カウンタをカウントＵＰ
                                isAllJyokenS = true;         // 全ビット情報条件成立フラグに成立を設定
                            }
                            else if (2 == uiRetS)            // 条件不成立の場合
                            {
                                isJouhenCntS++;              // 状変有カウンタをカウントＵＰ
                                isAllJyokenS = false;        // 全ビット情報条件成立フラグに不成立を設定
                            }
                            else                            // 上記以外の場合
                            {
                                // 処理なし
                            }
                        }
                        // 警報監視設定定数ファイルのステータスビット情報のビット情報が複数ある（複数ビット警報）場合
                        else
                        {
                            uiRetD = AnalyzeNbitAlarmInfo(0, iCnt, CAppDat.AlarmInfoF.StateBitAlarm[iCnt], sysNo);
                                                        // Ｎビット情報条件解析処理を行う
                            if (1 == uiRetD)                 // 条件成立の場合
                            {
                                isJouhenCntD++;              // 状変有カウンタをカウントＵＰ
                                isAllJyokenD = true;         // 全ビット情報条件成立フラグに成立を設定
                            }
                            else if (2 == uiRetD)            // 条件不成立の場合
                            {
                                isJouhenCntD++;              // 状変有カウンタをカウントＵＰ
                                isAllJyokenD = false;        // 全ビット情報条件成立フラグに不成立を設定
                            }
                            else                            // 上記以外の場合
                            {
                                // 処理なし
                            }

                            if (uiRetD != 0 &&
                                (CAppDat.AlarmInfoF.StateBitAlarm[iCnt].OnMsgDefNo == CAppDat.ALERTMESSAGE_NO1 ||
                                 CAppDat.AlarmInfoF.StateBitAlarm[iCnt].OnMsgDefNo == CAppDat.ALERTMESSAGE_NO2))
                            {
                                iCnt1 = iCnt;
                                uiRetD = 0;
                                isJouhenCntD = 0;
                                isAllJyokenD1 = isAllJyokenD;

                                da.Register(sysNo, iCnt1, isAllJyokenD1, CAppDat.DELAYED_TIME);
                            }
                            else
                            {
                                // 処理なし
                            }
                        }
                    }

                    // 状変有カウンタが０（＝状変がない）場合
                    if (0 == isJouhenCntS)
                    {
                        // 処理なし
                    }
                    // 状変有カウンタが０ではない（＝状変がある）場合
                    else
                    {
                        // ＣＴＣ警報出力処理を行う
                        CKeihouCtrl.OutputCtcKeihou((UInt16)iCnt, isAllJyokenS);
                    }

                    // 状変有カウンタが０（＝状変がない）場合
                    if (0 == isJouhenCntD)
                    {
                        // 処理なし
                    }
                    // 状変有カウンタが０ではない（＝状変がある）場合
                    else
                    {
                        // ＣＴＣ警報出力処理を行う
                        CKeihouCtrl.OutputCtcKeihou((UInt16)iCnt, isAllJyokenD);
                    }
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : PRC伝送部警報解析処理
        /// MODULE ID           : PrcDensoKeihouAnalyze
        ///
        /// PARAMETER IN        : 
        /// <param name="sysNo">(in)CTCシステム番号（1-4）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// てこ／表示情報を解析して警報出力を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void PrcDensoKeihouAnalyze(UInt16 sysNo)
        {
            UInt16  uiRet;                              // 戻り値取得用
            bool    isAllJyoken = false;                // 全ビット情報条件成立フラグ（true=成立、false=不成立）
            Int32   isJouhenCnt = 0;                    // 状変有カウンタ
            Int32   iCnt = 0;                           // ループカウンタ
            String  strAppLog = String.Empty;           // ログメッセージ

            try
            {
                // 警報監視設定定数ファイルのステータスビット情報数分ループ
                for (iCnt = 0; iCnt < CAppDat.AlarmInfoF.StateBitAlarm.Count; iCnt++)
                {
                    isJouhenCnt = 0;                    // 状変有カウンタに０を設定
                    isAllJyoken = false;                // 全ビット情報条件成立フラグに不成立を設定

                    // 警報監視設定定数ファイルのステータスビット情報のビット情報数が０（ビット情報がない）場合
                    if (0 == CAppDat.AlarmInfoF.StateBitAlarm[iCnt].BitInfo.Count)
                    {
                        strAppLog = String.Format("警報監視設定定数ファイルのステータスビット情報にビット情報がない：{0:D}", iCnt);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    }
                    // 警報監視設定定数ファイルのステータスビット情報のビット情報数が０ではない（ビット情報がある）場合
                    else
                    {
                        // 警報監視設定定数ファイルのステータスビット情報のビット情報が１つだけある（１ビット警報）場合
                        if (1 == CAppDat.AlarmInfoF.StateBitAlarm[iCnt].BitInfo.Count)
                        {
                            uiRet = AnalyzeBitInfo(ALMBITCHKTYPE.NORMAL, CAppDat.AlarmInfoF.StateBitAlarm[iCnt].BitInfo[0], sysNo);
                            // ビット情報条件解析処理を行う
                        }
                        // 警報監視設定定数ファイルのステータスビット情報のビット情報が複数ある（複数ビット警報）場合
                        else
                        {
                            uiRet = AnalyzeNbitAlarmInfo(0, iCnt, CAppDat.AlarmInfoF.StateBitAlarm[iCnt], sysNo);
                            // Ｎビット情報条件解析処理を行う
                        }

                        if (1 == uiRet)                 // 条件成立の場合
                        {
                            isJouhenCnt++;              // 状変有カウンタをカウントＵＰ
                            isAllJyoken = true;         // 全ビット情報条件成立フラグに成立を設定
                        }
                        else if (2 == uiRet)            // 条件不成立の場合
                        {
                            isJouhenCnt++;              // 状変有カウンタをカウントＵＰ
                            isAllJyoken = false;        // 全ビット情報条件成立フラグに不成立を設定
                        }
                        else                            // 上記以外の場合
                        {
                            // 処理なし
                        }
                    }

                    // 状変有カウンタが０（＝状変がない）場合
                    if (0 == isJouhenCnt)
                    {
                        // 処理なし
                    }
                    // 状変有カウンタが０ではない（＝状変がある）場合
                    else
                    {
                        // PRC伝送部警報出力処理を行う
                        CKeihouCtrl.OutputCtcKeihou((UInt16)iCnt, isAllJyoken);
                    }
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : ビット情報条件解析処理
        /// MODULE ID           : AnalyzeBitInfo
        ///
        /// PARAMETER IN        : 
        /// <param name="chkType">(in)警報ビット情報チェック種別（詳細はenum ALMBITCHKTYPEを参照）</param>
        /// <param name="bitInfo">(in)ビット情報</param>
        /// <param name="sysNo">(in)CTCシステム番号（1-4）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>解析結果（0=該当なし、1=条件成立、2=条件不成立）</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// 警報監視設定定数ファイルのステータスビット情報のビット情報を元にして
        /// CTC状態情報を解析し、ビット情報の条件が成立したか否かを返す。
        /// </summary>
        ///
        ///*******************************************************************************
        private UInt16 AnalyzeBitInfo(ALMBITCHKTYPE chkType, CSetData bitInfo, UInt16 sysNo)
        {
            UInt16  kekka = 0;                          // 戻り値
            UInt16  detect = 0;                         // 検知種別

            try
            {
                kekka = AnalyzeBitInfoDetect(chkType, bitInfo, sysNo, ref detect);
                                                        // ビット情報条件解析処理

                if (0 != kekka)                         // 結果が「該当なし」ではない場合
                {
                    // 検知種別が「初回」で「初回検知なし」の場合
                    if((detect & (UInt16)BITDETECT.DETECTALL) == (UInt16)BITDETECT.FIRSTFLAG)
                    {
                        kekka = 0;                      // 結果に該当なしを設定
                    }
                    // 上記以外の場合
                    else
                    {
                        // 処理なし
                    }
                }
                else                                    // 結果が「該当なし」の場合
                {
                    // 処理なし
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                kekka = 0;
            }

            return kekka;
        }

        ///*******************************************************************************
        /// MODULE NAME         : ビット情報条件解析処理（他線区ＩＦ装置）
        /// MODULE ID           : AnalyzeBitInfoTidif
        ///
        /// PARAMETER IN        : 
        /// <param name="chkType">(in)警報ビット情報チェック種別（詳細はenum ALMBITCHKTYPEを参照）</param>
        /// <param name="bitInfo">(in)ビット情報</param>
        /// <param name="sysNo">(in)システム番号（1-4）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>解析結果（0=該当なし、1=条件成立、2=条件不成立）</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// 警報監視設定定数ファイルのステータスビット情報のビット情報を元にして
        /// 他線区ＩＦ状態情報を解析し、ビット情報の条件が成立したか否かを返す。
        /// </summary>
        ///
        ///*******************************************************************************
        private UInt16 AnalyzeBitInfoTidif(ALMBITCHKTYPE chkType, CSetData bitInfo, UInt16 sysNo)
        {
            UInt16 kekka = 0;                          // 戻り値
            UInt16 detect = 0;                         // 検知種別

            try
            {
                kekka = AnalyzeBitInfoDetectTidif(chkType, bitInfo, sysNo, ref detect);
                // ビット情報条件解析処理

                if (0 != kekka)                         // 結果が「該当なし」ではない場合
                {
                    // 検知種別が「初回」で「初回検知なし」の場合
                    if ((detect & (UInt16)BITDETECT.DETECTALL) == (UInt16)BITDETECT.FIRSTFLAG)
                    {
                        kekka = 0;                      // 結果に該当なしを設定
                    }
                    // 上記以外の場合
                    else
                    {
                        // 処理なし
                    }
                }
                else                                    // 結果が「該当なし」の場合
                {
                    // 処理なし
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                kekka = 0;
            }

            return kekka;
        }

        ///*******************************************************************************
        /// MODULE NAME         : ビット情報条件解析処理
        /// MODULE ID           : AnalyzeBitInfoDetect
        ///
        /// PARAMETER IN        : 
        /// <param name="chkType">(in)警報ビット情報チェック種別（詳細はenum ALMBITCHKTYPEを参照）</param>
        /// <param name="bitInfo">(in)ビット情報</param>
        /// <param name="sysNo">(in)CTCシステム番号（1-4）</param>
        /// PARAMETER OUT       : 
        /// <param name="detect">(out)検知種別（FIRSTFLAGﾋﾞｯﾄ: 0=初回ではない、1=初回、FIRSTDETECTﾋﾞｯﾄ: 0=初回検知なし、1=初回検知あり）</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>解析結果（0=該当なし、1=条件成立、2=条件不成立）</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// 警報監視設定定数ファイルのステータスビット情報のビット情報を元にして
        /// CTC状態情報を解析し、ビット情報の条件が成立したか否かを返す。
        /// </summary>
        ///
        ///*******************************************************************************
        private UInt16 AnalyzeBitInfoDetect(ALMBITCHKTYPE chkType, CSetData bitInfo, UInt16 sysNo, ref UInt16 detect)
        {
            UInt16  kekka = 0;                          // 戻り値
            bool    bRet1 = false;                      // 戻り値取得用
            bool    bRet2 = false;                      // 戻り値取得用
            bool    bRet1_0 = false;                    // 戻り値取得用
            bool    bRet1_1 = false;                    // 戻り値取得用
            bool    bRet2_0 = false;                    // 戻り値取得用
            bool    bRet2_1 = false;                    // 戻り値取得用
            bool    isTarget = false;                   // チェック対象フラグ（true=対象、false=対象ではない）
            bool    isFirstCheck = false;               // 初回フラグ（true=初回、false=初回ではない）
            bool    isFirstDetect = false;              // 初回検知フラグ（true=検知あり、false=検知なし）
            bool    isBeforeState = false;              // 前回CTC状態設定指定（true=1、false=0）
            Int32   dataSize = 0;                       // CTC状態情報のデータ長
            Int32   iCnt = 0;                           // ループカウンタ
            UInt16  hoko1 = 0;                          // 前回方向
            UInt16  hoko2 = 0;                          // 今回方向
            UInt16  chkHoko_0 = 0;                      // チェック方向値：0
            UInt16  chkHoko_1 = 0;                      // チェック方向値：1
            UInt16  bitVal = 0;                         // ビット状態
            UInt16  kbno = 0;                           // 区分No.
            UInt16  chkPmode = 0;                       // P_MD チェックビット
            UInt16  outHoko = 0;                        // 方向
            String  strKigou = String.Empty;            // 記号名称
            String  strOutKigou = String.Empty;         // 記号名称
            String  strAppLog = String.Empty;           // ログメッセージ
            UInt16[] aryData = null;
            UInt16  targetSysNo = 0;                    // 対象駅のSYS-No
            Int32   ekiIndex = 0;                       // 駅Index
            bool    isSkip = false;                     // 検出処理スキップフラグ

            kekka = 0;                                  // 戻り値に該当なしを設定
            detect = (UInt16)BITDETECT.NODETECT;        // 検知種別になしを設定

            try
            {
                isTarget = false;                       // チェック対象フラグに対象ではないを設定
                isFirstCheck = false;                   // 初回フラグに、初回ではないを設定
                isFirstDetect = false;                  // 初回検知フラグに、検知なしを設定


                isSkip = false;                         // 検出処理スキップフラグにスキップなしを設定

                // 区分が「中央装置状態」の場合
                if (bitInfo.KbNo == (UInt16)CAppDat.SETID.CTCCENTER)
                {
                    isSkip = false;                 // 検出処理スキップフラグにスキップなしを設定
                }
                // 区分が「PRC伝送部」の場合
                else if (bitInfo.KbNo == (UInt16)CAppDat.SETID.PRCDENSO)
                {
                    isSkip = false;                 // 検出処理スキップフラグにスキップなしを設定
                }
                else
                {
                    ekiIndex = CCommon.GetEkiIndex(CCommon.EKINOTYPE.EKINO_TUEKI, bitInfo.Ekino);

                    if (-1 == ekiIndex)
                    {
                        strAppLog = String.Format("通算駅番号 格納テーブル異常 駅番号={0:D}", bitInfo.Ekino);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    }
                    else
                    {
                        targetSysNo = CAppDat.EkiConvF[(UInt16)(ekiIndex + 1)].SysNo;
                                                            // 対象駅のSYS-Noを取得
                        if(targetSysNo == sysNo)            // 処理対象駅のSYS-Noと電文のSYS-Noが同じ場合
                        {
                            isSkip = false;                 // 検出処理スキップフラグにスキップなしを設定
                        }
                        else                                // 処理対象駅のSYS-Noと電文のSYS-Noが違う場合
                        {
                            isSkip = true;                  // 検出処理スキップフラグにスキップありを設定
                        }
                    }
                }

                // モード状態解析の場合
                if ((UInt16)CAppDat.SETID.MODE == bitInfo.KbNo)
                {
                    //[del] isTarget = true;                    // チェック対象フラグに対象を設定
                }
                // 上記以外の場合
                else
                {
                    switch(bitInfo.Hoko)
                    {
                        case 1:                         // 指定ビットが0,0で変化無し
                            hoko1 = 0;
                            hoko2 = 0;
                            isTarget = true;            // チェック対象フラグに対象を設定
                            isFirstDetect = true;       // 初回検知フラグに、検知ありを設定
                            break;

                        case 2:                         // 指定ビットが0から1に変化
                            hoko1 = 0;
                            hoko2 = 1;
                            isTarget = true;            // チェック対象フラグに対象を設定
                            isFirstDetect = true;       // 初回検知フラグに、検知ありを設定
                            break;

                        case 3:                         // 指定ビットが1から0に変化
                            hoko1 = 1;
                            hoko2 = 0;
                            isTarget = true;            // チェック対象フラグに対象を設定
                            isFirstDetect = true;       // 初回検知フラグに、検知ありを設定
                            break;

                        case 4:                         // 指定ビットが1,1で変化無し
                            hoko1 = 1;
                            hoko2 = 1;
                            break;

                        case 5:                         // 指定ビットが0から1に変化（初回検知なし）
                            hoko1 = 0;
                            hoko2 = 1;
                            isTarget = true;            // チェック対象フラグに対象を設定
                            break;

                        case 6:                         // 指定ビットが1から0に変化（初回検知なし）
                            hoko1 = 1;
                            hoko2 = 0;
                            isTarget = true;            // チェック対象フラグに対象を設定
                            break;

                        default:                        // 上記以外
                            strAppLog = String.Format("ビット情報解析対象外：Hoko={0:D}", bitInfo.Hoko);
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.INFORMATION, MethodBase.GetCurrentMethod(), strAppLog);
                            break;
                    }
                }


                // チェック方向値を設定
                chkHoko_0 = 0;
                chkHoko_1 = 1;

                // 記号名称を取得する
                strKigou = bitInfo.Kigou;

                // P_MD No. 名称を取得する
                kbno = CCommon.GetPModeKigou(strKigou, ref strOutKigou, ref outHoko, ref chkPmode);

                //[mod] if (0 == kbno)                          // P_MD No. が取得できない場合（P_MD警報ではない場合）
                if (true == isSkip)                     // 検出処理スキップフラグがスキップありの場合
                {
                    isTarget = false;                   // チェック対象フラグに対象ではないを設定
                    isFirstDetect = false;              // 初回検知フラグに、検知なしを設定
                    isFirstCheck = false;               // 初回フラグに、初回ではないを設定
                    kekka = 0;                          // 戻り値に該当なしを設定
                }
                else if (0 == kbno)                     // P_MD No. が取得できない場合（P_MD警報ではない場合）
                {
                    // モード状態解析の場合
                    if ((UInt16)CAppDat.SETID.MODE == bitInfo.KbNo)
                    {
                        strAppLog = String.Format("モードビット情報解析対象外２：Kigou={0} Hoko={1:X4}", bitInfo.Kigou, bitInfo.Hoko);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    }
                    else                                // P_MD No. が取得できた場合
                    {
                        // 処理なし
                    }
                }
                else                                    // P_MD No. が取得できた場合（P_MD警報の場合）
                {
                    //［注釈］
                    //    モードの場合は特殊で、記号名称に方向付き記号名称が設定され、方向にビット状態が設定されている。

                    bitVal = bitInfo.Hoko;              // ビット状態を取得する

                    // 方向を決める
                    if (0xFFFF == outHoko)              // 方向なしの場合
                    {
                        if (0 == bitInfo.Hoko)          // ビット状態が０の場合
                        {
                            bitVal = chkPmode;
                            hoko1 = 1;
                            hoko2 = 0;
                        }
                        else                            // ビット状態が０ではない場合
                        {
                            bitVal = bitInfo.Hoko;
                            hoko1 = 0;
                            hoko2 = 1;
                        }
                        isTarget = true;                // チェック対象フラグに対象を設定
                        isFirstDetect = true;           // 初回検知フラグに、検知ありを設定
                    }
                    else if (2 == outHoko)              // 「指定ビットが0から1に変化」の場合
                    {
                        hoko1 = 0;
                        hoko2 = 1;
                        isTarget = true;                // チェック対象フラグに対象を設定
                        isFirstDetect = true;           // 初回検知フラグに、検知ありを設定
                    }
                    else if (3 == outHoko)              // 「指定ビットが1から0に変化」の場合
                    {
                        hoko1 = 1;
                        hoko2 = 0;
                        isTarget = true;                // チェック対象フラグに対象を設定
                        isFirstDetect = true;           // 初回検知フラグに、検知ありを設定
                    }
                    else if (5 == outHoko)              // 「指定ビットが0から1に変化（初回検知なし）」の場合
                    {
                        hoko1 = 0;
                        hoko2 = 1;
                        isTarget = true;                // チェック対象フラグに対象を設定
                    }
                    else if (6 == outHoko)              // 「指定ビットが1から0に変化（初回検知なし）」の場合
                    {
                        hoko1 = 1;
                        hoko2 = 0;
                        isTarget = true;                // チェック対象フラグに対象を設定
                    }
                    else                                // 上記以外の場合
                    {
                        strAppLog = String.Format("モードビット情報解析対象外：Kigou={0} outHoko={1:D} Hoko={2:X4}", strKigou, outHoko, bitInfo.Hoko);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);

                        // デフォルトは 0→1 変化扱いにする
                        hoko1 = 0;
                        hoko2 = 1;
                        isTarget = true;                // チェック対象フラグに対象を設定
                    }

                    strKigou = strOutKigou;             // 方向付き記号名称を、方向なし記号名称に置き換える
                }


                if (true == isTarget)                   // チェック対象の場合
                {
                    // てこ／表示情報の場合
                    if (0 != (CAppDat.RecvSoutiKind & 0x00F0))
                    {
                        // 今回てこ／表示情報のビット状態を取得（ビットチェック行う）
                        m_TekoHyojiInfoMngT[sysNo - 1].GetRecvData(ref aryData);
                        bRet2 = CCommon.GetStateTEKOHYOJI(aryData, bitInfo.KbNo, bitInfo.Ekino, bitInfo.TekoHyojiKubun, strKigou, hoko2, sysNo, bitVal);
                        bRet2_0 = CCommon.GetStateTEKOHYOJI(aryData, bitInfo.KbNo, bitInfo.Ekino, bitInfo.TekoHyojiKubun, strKigou, chkHoko_0, sysNo, bitVal);
                        bRet2_1 = CCommon.GetStateTEKOHYOJI(aryData, bitInfo.KbNo, bitInfo.Ekino, bitInfo.TekoHyojiKubun, strKigou, chkHoko_1, sysNo, bitVal);

                        dataSize = aryData.Length;          // 今回てこ／表示情報のデータ長を取得

                        // 前回てこ／表示情報を取得する
                        m_TekoHyojiInfoMngOldT[sysNo - 1].GetRecvData(ref aryData);
                    }
                    // CTC状態情報の場合
                    else
                    {
                        // 今回CTC状態情報のビット状態を取得（ビットチェック行う）
                        m_CTCInfoMngT[sysNo - 1].GetRecvData(ref aryData);
                        bRet2 = CCommon.GetStateCTC(aryData, bitInfo.KbNo, bitInfo.Ekino, strKigou, hoko2, sysNo, bitVal);
                        bRet2_0 = CCommon.GetStateCTC(aryData, bitInfo.KbNo, bitInfo.Ekino, strKigou, chkHoko_0, sysNo, bitVal);
                        bRet2_1 = CCommon.GetStateCTC(aryData, bitInfo.KbNo, bitInfo.Ekino, strKigou, chkHoko_1, sysNo, bitVal);

                        dataSize = aryData.Length;          // 今回CTC状態情報のデータ長を取得

                        // 前回CTC状態情報を取得する
                        m_CTCInfoMngOldT[sysNo - 1].GetRecvData(ref aryData);
                    }

                    if (0 == aryData.Length)            // 前回CTC状態情報のデータがない場合
                    {
                        isFirstCheck = true;            // 初回フラグに、初回を設定

                        // 前回CTC状態情報のデータは０扱いとする（０で初期化する）
                        Array.Resize(ref aryData, dataSize);
                        Array.Clear(aryData, 0, aryData.Length);

                        // 前回CTC状態の設定値を判断する
                        isBeforeState = false;          // 前回CTC状態設定指定に「0」を設定

                        if (0 != hoko1)                 // 前回状態指定が「1」の場合（1→0変化検出の場合）
                        {
                            if (true == isFirstDetect)  // 初回検知フラグが検知ありの場合
                            {
                                isBeforeState = true;   // 前回CTC状態設定指定に「1」を設定
                            }
                            else                        // 初回検知フラグが検知なしの場合
                            {
                                // 処理なし
                            }
                        }
                        else                            // 前回状態指定が「0」の場合（0→1変化検出の場合）
                        {
                            if (true == isFirstDetect)  // 初回検知フラグが検知ありの場合
                            {
                                // 処理なし
                            }
                            else                        // 初回検知フラグが検知なしの場合
                            {
                                isBeforeState = true;   // 前回CTC状態設定指定に「1」を設定
                            }
                        }

                        // 前回CTC状態設定指定が「1」の場合、前回CTC状態情報のデータは１扱いとする
                        if (true == isBeforeState)      // 前回CTC状態設定指定が「1」の場合
                        {
                            for (iCnt = 0; iCnt < aryData.Length; iCnt++)
                            {
                                aryData[iCnt] = 0xFFFF;
                            }
                        }
                        else                            // 前回CTC状態設定指定が「0」の場合
                        {
                            // 処理なし
                        }
                    }
                    else                                // 今回CTC状態情報のデータがある場合
                    {
                        // 処理なし
                    }

                    // てこ／表示情報の場合
                    if (0 != (CAppDat.RecvSoutiKind & 0x00F0))
                    {
                        // 前回CTC状態情報のビット状態を取得（ビットチェック行う）
                        bRet1 = CCommon.GetStateTEKOHYOJI(aryData, bitInfo.KbNo, bitInfo.Ekino, bitInfo.TekoHyojiKubun, strKigou, hoko1, sysNo, bitVal);
                        bRet1_0 = CCommon.GetStateTEKOHYOJI(aryData, bitInfo.KbNo, bitInfo.Ekino, bitInfo.TekoHyojiKubun, strKigou, chkHoko_0, sysNo, bitVal);
                        bRet1_1 = CCommon.GetStateTEKOHYOJI(aryData, bitInfo.KbNo, bitInfo.Ekino, bitInfo.TekoHyojiKubun, strKigou, chkHoko_1, sysNo, bitVal);
                    }
                    // CTC状態情報の場合
                    //else if (0 != (CAppDat.RecvSoutiKind & 0x000F))
                    else
                    {
                        // 前回CTC状態情報のビット状態を取得（ビットチェック行う）
                        bRet1 = CCommon.GetStateCTC(aryData, bitInfo.KbNo, bitInfo.Ekino, strKigou, hoko1, sysNo, bitVal);
                        bRet1_0 = CCommon.GetStateCTC(aryData, bitInfo.KbNo, bitInfo.Ekino, strKigou, chkHoko_0, sysNo, bitVal);
                        bRet1_1 = CCommon.GetStateCTC(aryData, bitInfo.KbNo, bitInfo.Ekino, strKigou, chkHoko_1, sysNo, bitVal);
                    }

                    // 警報ビット情報チェック種別が「通常チェック」指定の場合
                    if (ALMBITCHKTYPE.NORMAL == chkType)
                    {
                        // 「前回が０で今回も０」または「前回が１で今回も１」の場合
                        if (((true == bRet1_0) && (true == bRet2_0)) ||
                            ((true == bRet1_1) && (true == bRet2_1)))
                        {
                            // 処理なし
                        }
                        // 条件成立の場合
                        else if ((true == bRet1) && (true == bRet2))
                        {
                            kekka = 1;                      // 戻り値に条件成立を設定
                        }
                        // 条件不成立の場合
                        else
                        {
                            kekka = 2;                      // 戻り値に条件不成立を設定
                        }
                    }
                    // 警報ビット情報チェック種別が「前回状態チェック」「今回状態チェック」「状変用前回状態チェック」「状変用今回状態チェック」指定の場合
                    else if ((ALMBITCHKTYPE.BEFORE    == chkType) ||
                             (ALMBITCHKTYPE.NOW       == chkType) ||
                             (ALMBITCHKTYPE.JH_BEFORE == chkType) ||
                             (ALMBITCHKTYPE.JH_NOW    == chkType))
                    {
                        if ((ALMBITCHKTYPE.BEFORE    == chkType) || // 「前回状態チェック」指定の場合
                            (ALMBITCHKTYPE.JH_BEFORE == chkType))   // 「状変用前回状態チェック」指定の場合
                        {
                            // てこ／表示情報の場合
                            if (0 != (CAppDat.RecvSoutiKind & 0x00F0))
                            {
                                // 前回てこ／表示情報のビット状態を取得（ビットチェック行う）
                                // （前回状態チェック指定の場合は、前回てこ／表示情報に対して成立条件(hoko2)でチェックをし直す）
                                bRet2 = CCommon.GetStateTEKOHYOJI(aryData, bitInfo.KbNo, bitInfo.Ekino, bitInfo.TekoHyojiKubun, strKigou, hoko2, sysNo, bitVal);
                            }
                            // CTC状態情報の場合
                            //else if (0 != (CAppDat.RecvSoutiKind & 0x000F))
                            else
                            {
                                // 前回CTC状態情報のビット状態を取得（ビットチェック行う）
                                // （前回状態チェック指定の場合は、前回CTC状態情報に対して成立条件(hoko2)でチェックをし直す）
                                bRet2 = CCommon.GetStateCTC(aryData, bitInfo.KbNo, bitInfo.Ekino, strKigou, hoko2, sysNo, bitVal);
                            }
                        }
                        else                                        // 「今回状態チェック」「状変用今回状態チェック」指定の場合
                        {
                            // 処理なし
                        }

                        if (true == bRet2)              // 条件成立の場合
                        {
                            kekka = 1;                  // 戻り値に条件成立を設定
                        }
                        else                            // 条件不成立の場合
                        {
                            kekka = 2;                  // 戻り値に条件不成立を設定
                        }

                        if (bitInfo.Hoko == 1)          //指定ビットが0,0で変化無し
                        {
                            if ((bRet1 == true) && (bRet2 == true)) // 変化無し
                            {
                                kekka = 1;              // 戻り値に条件成立を設定
                            }
                            else
                            {
                                kekka = 2;              // 戻り値に条件不成立を設定
                            }
                        }
                    }
                    // 上記以外の場合
                    else
                    {
                        // 処理なし
                    }
                }
                else                                    // チェック対象ではない場合
                {
                    // 処理なし
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                isFirstDetect = false;                  // 初回検知フラグに、検知なしを設定
                isFirstCheck = false;                   // 初回フラグに、初回ではないを設定
                kekka = 0;                              // 戻り値に該当なしを設定
            }
            finally
            {
                aryData = null;
            }

            try
            {
                // 警報ビット情報チェック種別が「状変用前回状態チェック」「状変用今回状態チェック」指定の場合
                if ((ALMBITCHKTYPE.JH_BEFORE == chkType) ||
                    (ALMBITCHKTYPE.JH_NOW    == chkType))
                {
                    // 初回フラグが「初回」で、初回検知フラグが「検知なし」の場合
                    if ((true == isFirstCheck) && (false == isFirstDetect))
                    {
                        if (0 != kekka)                 // 結果が「該当なし」ではない場合
                        {
                            kekka = 2;                  // 戻り値に条件不成立を設定
                        }
                        else                            // 結果が「該当なし」の場合
                        {
                            // 処理なし
                        }
                    }
                    // 上記以外の場合
                    else
                    {
                        // 処理なし
                    }
                }
                // 上記以外の場合
                else
                {
                    // 処理なし
                }

                if (true == isFirstCheck)               // 初回フラグが、初回の場合
                {
                    detect |= (UInt16)BITDETECT.FIRSTFLAG;   // 検知種別に、初回を設定
                }
                else                                    // 初回フラグが、初回ではない場合
                {
                    // 処理なし
                }

                if (true == isFirstDetect)              // 初回検知フラグが、検知ありの場合
                {
                    detect |= (UInt16)BITDETECT.FIRSTDETECT; // 検知種別に、初回検知ありを設定
                }
                else                                    // 初回検知フラグが、検知なしの場合
                {
                    // 処理なし
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return kekka;
        }

        ///*******************************************************************************
        /// MODULE NAME         : ビット情報条件解析処理（他線区ＩＦ装置）
        /// MODULE ID           : AnalyzeBitInfoDetect
        ///
        /// PARAMETER IN        : 
        /// <param name="chkType">(in)警報ビット情報チェック種別（詳細はenum ALMBITCHKTYPEを参照）</param>
        /// <param name="bitInfo">(in)ビット情報</param>
        /// <param name="sysNo">(in)システム番号（1-4）</param>
        /// PARAMETER OUT       : 
        /// <param name="detect">(out)検知種別（FIRSTFLAGﾋﾞｯﾄ: 0=初回ではない、1=初回、FIRSTDETECTﾋﾞｯﾄ: 0=初回検知なし、1=初回検知あり）</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>解析結果（0=該当なし、1=条件成立、2=条件不成立）</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// 警報監視設定定数ファイルのステータスビット情報のビット情報を元にして
        /// 他線区ＩＦ状態情報を解析し、ビット情報の条件が成立したか否かを返す。
        /// </summary>
        ///
        ///*******************************************************************************
        private UInt16 AnalyzeBitInfoDetectTidif(ALMBITCHKTYPE chkType, CSetData bitInfo, UInt16 sysNo, ref UInt16 detect)
        {
            UInt16 kekka = 0;                           // 戻り値
            bool bRet1 = false;                         // 戻り値取得用
            bool bRet2 = false;                         // 戻り値取得用
            bool bRet1_0 = false;                       // 戻り値取得用
            bool bRet1_1 = false;                       // 戻り値取得用
            bool bRet2_0 = false;                       // 戻り値取得用
            bool bRet2_1 = false;                       // 戻り値取得用
            bool isTarget = false;                      // チェック対象フラグ（true=対象、false=対象ではない）
            bool isFirstCheck = false;                  // 初回フラグ（true=初回、false=初回ではない）
            bool isFirstDetect = false;                 // 初回検知フラグ（true=検知あり、false=検知なし）
            bool isBeforeState = false;                 // 前回CTC状態設定指定（true=1、false=0）
            Int32 dataSize = 0;                         // 他線区ＩＦ状態情報のデータ長
            Int32 iCnt = 0;                             // ループカウンタ
            UInt16 hoko1 = 0;                           // 前回方向
            UInt16 hoko2 = 0;                           // 今回方向
            UInt16 chkHoko_0 = 0;                       // チェック方向値：0
            UInt16 chkHoko_1 = 0;                       // チェック方向値：1
            UInt16 bitVal = 0;                          // ビット状態
            UInt16 kbno = 0;                            // 区分No.
            UInt16 chkPmode = 0;                        // P_MD チェックビット
            UInt16 outHoko = 0;                         // 方向
            String strKigou = String.Empty;             // 記号名称
            String strOutKigou = String.Empty;          // 記号名称
            String strAppLog = String.Empty;            // ログメッセージ
            UInt16[] aryData = null;
            UInt16 targetSysNo = 0;                     // 対象駅のSYS-No
            Int32 ekiIndex = 0;                         // 駅Index
            bool isSkip = false;                        // 検出処理スキップフラグ

            kekka = 0;                                  // 戻り値に該当なしを設定
            detect = (UInt16)BITDETECT.NODETECT;        // 検知種別になしを設定

            try
            {
                isTarget = false;                       // チェック対象フラグに対象ではないを設定
                isFirstCheck = false;                   // 初回フラグに、初回ではないを設定
                isFirstDetect = false;                  // 初回検知フラグに、検知なしを設定
                isSkip = false;                         // 検出処理スキップフラグにスキップなしを設定

                ekiIndex = CCommon.GetEkiIndex(CCommon.EKINOTYPE.EKINO_TUEKI, bitInfo.Ekino);

                if (-1 == ekiIndex)
                {
                    strAppLog = String.Format("通算駅番号 格納テーブル異常 駅番号={0:D}", bitInfo.Ekino);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                }
                else
                {
                    targetSysNo = CAppDat.EkiConvF[(UInt16)(ekiIndex + 1)].SysNo;
                    // 対象駅のSYS-Noを取得
                    if (targetSysNo == sysNo)           // 処理対象駅のSYS-Noと電文のSYS-Noが同じ場合
                    {
                        isSkip = false;                 // 検出処理スキップフラグにスキップなしを設定
                    }
                    else                                // 処理対象駅のSYS-Noと電文のSYS-Noが違う場合
                    {
                        isSkip = true;                  // 検出処理スキップフラグにスキップありを設定
                    }
                }


                // モード状態解析の場合
                if ((UInt16)CAppDat.SETID.MODE == bitInfo.KbNo)
                {
                    //[del] isTarget = true;              // チェック対象フラグに対象を設定
                }
                // 上記以外の場合
                else
                {
                    switch(bitInfo.Hoko)
                    {
                        case 1:                         // 指定ビットが0,0で変化無し
                            hoko1 = 0;
                            hoko2 = 0;
                            break;

                        case 2:                         // 指定ビットが0から1に変化
                            hoko1 = 0;
                            hoko2 = 1;
                            isTarget = true;            // チェック対象フラグに対象を設定
                            isFirstDetect = true;       // 初回検知フラグに、検知ありを設定
                            break;

                        case 3:                         // 指定ビットが1から0に変化
                            hoko1 = 1;
                            hoko2 = 0;
                            isTarget = true;            // チェック対象フラグに対象を設定
                            isFirstDetect = true;       // 初回検知フラグに、検知ありを設定
                            break;

                        case 4:                         // 指定ビットが1,1で変化無し
                            hoko1 = 1;
                            hoko2 = 1;
                            break;

                        case 5:                         // 指定ビットが0から1に変化（初回検知なし）
                            hoko1 = 0;
                            hoko2 = 1;
                            isTarget = true;            // チェック対象フラグに対象を設定
                            break;

                        case 6:                         // 指定ビットが1から0に変化（初回検知なし）
                            hoko1 = 1;
                            hoko2 = 0;
                            isTarget = true;            // チェック対象フラグに対象を設定
                            break;

                        default:                        // 上記以外
                            strAppLog = String.Format("ビット情報解析対象外：Hoko={0:D}", bitInfo.Hoko);
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.INFORMATION, MethodBase.GetCurrentMethod(), strAppLog);
                            break;
                    }
                }


                // チェック方向値を設定
                chkHoko_0 = 0;
                chkHoko_1 = 1;

                // 記号名称を取得する
                strKigou = bitInfo.Kigou;

                // P_MD No. 名称を取得する
                kbno = CCommon.GetPModeKigou(strKigou, ref strOutKigou, ref outHoko, ref chkPmode);

                if (true == isSkip)                     // 検出処理スキップフラグがスキップありの場合
                {
                    isTarget = false;                   // チェック対象フラグに対象ではないを設定
                    isFirstDetect = false;              // 初回検知フラグに、検知なしを設定
                    isFirstCheck = false;               // 初回フラグに、初回ではないを設定
                    kekka = 0;                          // 戻り値に該当なしを設定
                }
                else if (0 == kbno)                     // P_MD No. が取得できない場合（P_MD警報ではない場合）
                {
                    // モード状態解析の場合
                    if ((UInt16)CAppDat.SETID.MODE == bitInfo.KbNo)
                    {
                        strAppLog = String.Format("モードビット情報解析対象外２：Kigou={0} Hoko={1:X4}", bitInfo.Kigou, bitInfo.Hoko);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    }
                    else                                // P_MD No. が取得できた場合
                    {
                        // 処理なし
                    }
                }
                else                                    // P_MD No. が取得できた場合（P_MD警報の場合）
                {
                    //［注釈］
                    //    モードの場合は特殊で、記号名称に方向付き記号名称が設定され、方向にビット状態が設定されている。

                    bitVal = bitInfo.Hoko;              // ビット状態を取得する

                    // 方向を決める
                    if (0xFFFF == outHoko)              // 方向なしの場合
                    {
                        if (0 == bitInfo.Hoko)          // ビット状態が０の場合
                        {
                            bitVal = chkPmode;
                            hoko1 = 1;
                            hoko2 = 0;
                        }
                        else                            // ビット状態が０ではない場合
                        {
                            bitVal = bitInfo.Hoko;
                            hoko1 = 0;
                            hoko2 = 1;
                        }
                        isTarget = true;                // チェック対象フラグに対象を設定
                        isFirstDetect = true;           // 初回検知フラグに、検知ありを設定
                    }
                    else if (2 == outHoko)              // 「指定ビットが0から1に変化」の場合
                    {
                        hoko1 = 0;
                        hoko2 = 1;
                        isTarget = true;                // チェック対象フラグに対象を設定
                        isFirstDetect = true;           // 初回検知フラグに、検知ありを設定
                    }
                    else if (3 == outHoko)              // 「指定ビットが1から0に変化」の場合
                    {
                        hoko1 = 1;
                        hoko2 = 0;
                        isTarget = true;                // チェック対象フラグに対象を設定
                        isFirstDetect = true;           // 初回検知フラグに、検知ありを設定
                    }
                    else if (5 == outHoko)              // 「指定ビットが0から1に変化（初回検知なし）」の場合
                    {
                        hoko1 = 0;
                        hoko2 = 1;
                        isTarget = true;                // チェック対象フラグに対象を設定
                    }
                    else if (6 == outHoko)              // 「指定ビットが1から0に変化（初回検知なし）」の場合
                    {
                        hoko1 = 1;
                        hoko2 = 0;
                        isTarget = true;                // チェック対象フラグに対象を設定
                    }
                    else                                // 上記以外の場合
                    {
                        strAppLog = String.Format("モードビット情報解析対象外：Kigou={0} outHoko={1:D} Hoko={2:X4}", strKigou, outHoko, bitInfo.Hoko);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);

                        // デフォルトは 0→1 変化扱いにする
                        hoko1 = 0;
                        hoko2 = 1;
                        isTarget = true;                // チェック対象フラグに対象を設定
                    }

                    strKigou = strOutKigou;             // 方向付き記号名称を、方向なし記号名称に置き換える
                }


                if (true == isTarget)                   // チェック対象の場合
                {
                    // 今回他線区IF状態情報のビット状態を取得（ビットチェック行う）
                    m_OtherIFStatInfoMngT[sysNo - 1].GetRecvData(ref aryData);
                    bRet2 = CCommon.GetStateTIDIF(aryData, bitInfo.KbNo, bitInfo.Ekino, strKigou, hoko2, sysNo, bitVal);
                    bRet2_0 = CCommon.GetStateTIDIF(aryData, bitInfo.KbNo, bitInfo.Ekino, strKigou, chkHoko_0, sysNo, bitVal);
                    bRet2_1 = CCommon.GetStateTIDIF(aryData, bitInfo.KbNo, bitInfo.Ekino, strKigou, chkHoko_1, sysNo, bitVal);

                    dataSize = aryData.Length;          // 今回他線区IF状態情報のデータ長を取得

                    // 前回他線区IF状態情報を取得する
                    m_OtherIFStatInfoMngOldT[sysNo - 1].GetRecvData(ref aryData);


                    if (0 == aryData.Length)            // 前回他線区IF状態情報のデータがない場合
                    {
                        isFirstCheck = true;            // 初回フラグに、初回を設定

                        // 前回他線区IF状態情報のデータは０扱いとする（０で初期化する）
                        Array.Resize(ref aryData, dataSize);
                        Array.Clear(aryData, 0, aryData.Length);

                        // 前回他線区IF状態の設定値を判断する
                        isBeforeState = false;          // 前回他線区IF状態設定指定に「0」を設定

                        if (0 != hoko1)                 // 前回状態指定が「1」の場合（1→0変化検出の場合）
                        {
                            if (true == isFirstDetect)  // 初回検知フラグが検知ありの場合
                            {
                                isBeforeState = true;   // 前回他線区IF状態設定指定に「1」を設定
                            }
                            else                        // 初回検知フラグが検知なしの場合
                            {
                                // 処理なし
                            }
                        }
                        else                            // 前回状態指定が「0」の場合（0→1変化検出の場合）
                        {
                            if (true == isFirstDetect)  // 初回検知フラグが検知ありの場合
                            {
                                // 処理なし
                            }
                            else                        // 初回検知フラグが検知なしの場合
                            {
                                isBeforeState = true;   // 前回他線区IF状態設定指定に「1」を設定
                            }
                        }

                        // 前回他線区IF状態設定指定が「1」の場合、前回他線区IF状態情報のデータは１扱いとする
                        if (true == isBeforeState)      // 前回他線区IF状態設定指定が「1」の場合
                        {
                            for (iCnt = 0; iCnt < aryData.Length; iCnt++)
                            {
                                aryData[iCnt] = 0xFFFF;
                            }
                        }
                        else                            // 前回他線区IF状態設定指定が「0」の場合
                        {
                            // 処理なし
                        }
                    }
                    else                                // 今回他線区IF状態情報のデータがある場合
                    {
                        // 処理なし
                    }

                    // 前回他線区IF状態情報のビット状態を取得（ビットチェック行う）
                    bRet1 = CCommon.GetStateTIDIF(aryData, bitInfo.KbNo, bitInfo.Ekino, strKigou, hoko1, sysNo, bitVal);
                    bRet1_0 = CCommon.GetStateTIDIF(aryData, bitInfo.KbNo, bitInfo.Ekino, strKigou, chkHoko_0, sysNo, bitVal);
                    bRet1_1 = CCommon.GetStateTIDIF(aryData, bitInfo.KbNo, bitInfo.Ekino, strKigou, chkHoko_1, sysNo, bitVal);

                    // 警報ビット情報チェック種別が「通常チェック」指定の場合
                    if (ALMBITCHKTYPE.NORMAL == chkType)
                    {
                        // 「前回が０で今回も０」または「前回が１で今回も１」の場合
                        if (((true == bRet1_0) && (true == bRet2_0)) ||
                            ((true == bRet1_1) && (true == bRet2_1)))
                        {
                            // 処理なし
                        }
                        // 条件成立の場合
                        else if ((true == bRet1) && (true == bRet2))
                        {
                            kekka = 1;                  // 戻り値に条件成立を設定
                        }
                        // 条件不成立の場合
                        else
                        {
                            kekka = 2;                  // 戻り値に条件不成立を設定
                        }
                    }
                    // 警報ビット情報チェック種別が「前回状態チェック」「今回状態チェック」「状変用前回状態チェック」「状変用今回状態チェック」指定の場合
                    else if ((ALMBITCHKTYPE.BEFORE    == chkType) ||
                             (ALMBITCHKTYPE.NOW       == chkType) ||
                             (ALMBITCHKTYPE.JH_BEFORE == chkType) ||
                             (ALMBITCHKTYPE.JH_NOW    == chkType))
                    {
                        if ((ALMBITCHKTYPE.BEFORE    == chkType)  ||    // 「前回状態チェック」指定の場合
                            (ALMBITCHKTYPE.JH_BEFORE == chkType))       // 「状変用前回状態チェック」指定の場合
                        {
                            // 前回他線区IF状態情報のビット状態を取得（ビットチェック行う）
                            // （前回状態チェック指定の場合は、前回他線区IF状態情報に対して成立条件(hoko2)でチェックをし直す）
                            bRet2 = CCommon.GetStateTIDIF(aryData, bitInfo.KbNo, bitInfo.Ekino, strKigou, hoko2, sysNo, bitVal);
                        }
                        else                                            // 「今回状態チェック」「状変用今回状態チェック」指定の場合
                        {
                            // 処理なし
                        }

                        if (true == bRet2)              // 条件成立の場合
                        {
                            kekka = 1;                  // 戻り値に条件成立を設定
                        }
                        else                            // 条件不成立の場合
                        {
                            kekka = 2;                  // 戻り値に条件不成立を設定
                        }
                    }
                    // 上記以外の場合
                    else
                    {
                        // 処理なし
                    }
                }
                else                                    // チェック対象ではない場合
                {
                    // 処理なし
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                isFirstDetect = false;                  // 初回検知フラグに、検知なしを設定
                isFirstCheck = false;                   // 初回フラグに、初回ではないを設定
                kekka = 0;                              // 戻り値に該当なしを設定
            }
            finally
            {
                aryData = null;
            }

            try
            {
                // 警報ビット情報チェック種別が「状変用前回状態チェック」「状変用今回状態チェック」指定の場合
                if ((ALMBITCHKTYPE.JH_BEFORE == chkType) ||
                    (ALMBITCHKTYPE.JH_NOW    == chkType))
                {
                    // 初回フラグが「初回」で、初回検知フラグが「検知なし」の場合
                    if ((true == isFirstCheck) && (false == isFirstDetect))
                    {
                        if (0 != kekka)                 // 結果が「該当なし」ではない場合
                        {
                            kekka = 2;                  // 戻り値に条件不成立を設定
                        }
                        else                            // 結果が「該当なし」の場合
                        {
                            // 処理なし
                        }
                    }
                    // 上記以外の場合
                    else
                    {
                        // 処理なし
                    }
                }
                // 上記以外の場合
                else
                {
                    // 処理なし
                }

                if (true == isFirstCheck)               // 初回フラグが、初回の場合
                {
                    detect |= (UInt16)BITDETECT.FIRSTFLAG;      // 検知種別に、初回を設定
                }
                else                                    // 初回フラグが、初回ではない場合
                {
                    // 処理なし
                }

                if (true == isFirstDetect)              // 初回検知フラグが、検知ありの場合
                {
                    detect |= (UInt16)BITDETECT.FIRSTDETECT;    // 検知種別に、初回検知ありを設定
                }
                else                                    // 初回検知フラグが、検知なしの場合
                {
                    // 処理なし
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return kekka;
        }

        ///*******************************************************************************
        /// MODULE NAME         : Ｎビット情報条件解析処理
        /// MODULE ID           : AnalyzeNbitAlarmInfo
        ///
        /// PARAMETER IN        : 
        /// <param name="kind">(in)ビット情報の種別（0=ステータスビット情報、1=制御モードビット移行情報）</param>
        /// <param name="stateIndex">(in)警報監視設定定数ファイルのステータスビット情報／制御モードビット移行情報の位置(0-N)</param>
        /// <param name="bitAlarmInfo">(in)警報監視設定定数ファイルのステータスビット情報／制御モードビット移行情報</param>
        /// <param name="sysNo">(in)CTCシステム番号（1-4）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>チェック結果（0=該当なし、1=条件成立、2=条件不成立）</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// ＣＴＣ状態情報を解析してNビット警報の条件成立チェックを行う。
        /// （引数のkindとstateIndexはログに出力する情報としてのみ使用する。）
        /// </summary>
        ///
        ///*******************************************************************************
        internal UInt16 AnalyzeNbitAlarmInfo(UInt16 kind, Int32 stateIndex, CAlarmBitInfo bitAlarmInfo, UInt16 sysNo)
        {
            UInt16  kekka = 0;                          // 戻り値
            UInt16  uiRet;                              // 戻り値取得用
            UInt16  uiRet2;                             // 戻り値取得用
            UInt16  firstDetect = 0;                    // 初回検知種別
            UInt16  detect = 0;                         // 検知種別
            Int32   iBitInfoCnt = 0;                    // ループカウンタ
            Int32   iNotTargetCnt1 = 0;                 // 前回状態対象外カウンタ
            Int32   iOrganizedCnt1 = 0;                 // 前回状態条件成立カウンタ
            Int32   iNotTargetCnt2 = 0;                 // 今回状態対象外カウンタ
            Int32   iOrganizedCnt2 = 0;                 // 今回状態条件成立カウンタ
            UInt16  uiState1 = 0;                       // 前回状態（0=条件不成立、1=条件成立）
            UInt16  uiState2 = 0;                       // 今回状態（0=条件不成立、1=条件成立）
            String  strAppLog = String.Empty;           // ログメッセージ
            Int32[] iBitResultFormer = null;            // 前回状態条件の結果を保存する配列
            Int32[] iBitResultCurrent = null;           // 今回状態条件の結果を保存する配列
            UInt16[] uiCompareResult = null;            // 当該情報において、前回状態条件と今回状態条件の比較結果（0=不成立から成立、1=成立から不成立、2=変化なし）
            Int32 iChangeNG = 0;                        // 変化なし、かつ成立中条件カウンタ
            Int32 iChangeONNG = 0;                      // 成立から不成立条件カウンタ

            kekka = 0;                                  // 戻り値に該当なしを設定

            try
            {
                // 初期化
                iNotTargetCnt1 = 0;                     // 前回状態対象外カウンタ
                iOrganizedCnt1 = 0;                     // 前回状態条件成立カウンタ
                iNotTargetCnt2 = 0;                     // 今回状態対象外カウンタ
                iOrganizedCnt2 = 0;                     // 今回状態条件成立カウンタ

                iBitResultFormer = new Int32[bitAlarmInfo.BitInfo.Count];
                iBitResultCurrent = new Int32[bitAlarmInfo.BitInfo.Count];
                uiCompareResult = new UInt16[bitAlarmInfo.BitInfo.Count];

                firstDetect = 0;                        // 初回検知種別に検知なしを設定

                // 警報監視設定定数ファイルのステータスビット情報／制御モードビット移行情報のビット情報数分ループ
                for (iBitInfoCnt = 0; iBitInfoCnt < bitAlarmInfo.BitInfo.Count; iBitInfoCnt++)
                {
                    //==========  前回CTC状態情報をチェックする  ==========
                    uiRet = AnalyzeBitInfoDetect(ALMBITCHKTYPE.JH_BEFORE, bitAlarmInfo.BitInfo[iBitInfoCnt], sysNo, ref detect);    // ビット情報条件解析処理を行う（前回CTC状態チェック指定）

                    iBitResultFormer[iBitInfoCnt] = uiRet;

                    if (1 == uiRet)                     // 条件成立の場合
                    {
                        iOrganizedCnt1++;               // 前回状態条件成立カウンタをカウントＵＰ
                    }
                    else if (2 == uiRet)                // 条件不成立の場合
                    {
                        // 処理なし
                    }
                    else                                // 上記以外の場合
                    {
                        iNotTargetCnt1++;               // 前回状態対象外カウンタをカウントＵＰ
                        strAppLog = String.Format("警報監視設定定数ファイルのステータスビット情報のビット情報／制御モードビット移行情報(前回CTC)チェック結果が対象外：Kind={0:D} StatusBit位置={1:D} BitInfo位置={2:D} ret={3:D}", kind, stateIndex, iBitInfoCnt, uiRet);
                    }

                    //==========  今回CTC状態情報をチェックする  ==========
                    uiRet2 = AnalyzeBitInfoDetect(ALMBITCHKTYPE.JH_NOW, bitAlarmInfo.BitInfo[iBitInfoCnt], sysNo, ref detect);      // ビット情報条件解析処理を行う（今回CTC状態チェック指定）
                    iBitResultCurrent[iBitInfoCnt] = uiRet2;

                    firstDetect |= detect;              // 検知種別を保存

                    if (1 == uiRet2)                     // 条件成立の場合
                    {
                        iOrganizedCnt2++;               // 今回状態条件成立カウンタをカウントＵＰ
                    }
                    else if (2 == uiRet2)                // 条件不成立の場合
                    {
                        // 処理なし
                    }
                    else                                // 上記以外の場合
                    {
                        iNotTargetCnt2++;               // 今回状態対象外カウンタをカウントＵＰ
                        strAppLog = String.Format("警報監視設定定数ファイルのステータスビット情報のビット情報／制御モードビット移行情報(今回CTC)チェック結果が対象外：Kind={0:D} StatusBit位置={1:D} BitInfo位置={2:D} ret={3:D}", kind, stateIndex, iBitInfoCnt, uiRet2);
                    }

                    if (iBitResultCurrent[iBitInfoCnt] == iBitResultFormer[iBitInfoCnt])         //変化なし
                    {
                        uiCompareResult[iBitInfoCnt] = 2;
                        if(iBitResultCurrent[iBitInfoCnt] == 1)
                        {
                            iChangeNG++;
                        }
                    }
                    else if (iBitResultCurrent[iBitInfoCnt] == 1)    //不成立から成立へ
                    {
                        uiCompareResult[iBitInfoCnt] = 0;
                    }
                    else if (iBitResultCurrent[iBitInfoCnt] == 2)    //成立から不成立へ
                    {
                        uiCompareResult[iBitInfoCnt] = 1;
                        iChangeONNG++;
                    }
                }


                // 前回状態対象外あり、または、今回状態対象外ありの場合
                if ((0 != iNotTargetCnt1) || (0 != iNotTargetCnt2))
                {
                    // 処理なし
                }
                // 前回状態対象外なし、かつ、今回状態対象外なしの場合
                else
                {
                    if (bitAlarmInfo.BitInfoType == 0)    //複数のビット情報はAND条件として判定する場合
                    {

                        // 前回CTC状態で、警報監視設定定数ファイルのステータスビット情報／制御モードビット移行情報の全ビット情報条件が成立した場合
                        if (iOrganizedCnt1 == bitAlarmInfo.BitInfo.Count)
                        {
                            uiState1 = 1;                   // 前回状態に条件成立を設定
                        }
                        else
                        {
                            uiState1 = 0;                   // 前回状態に条件不成立を設定
                        }

                        // 今回CTC状態で、警報監視設定定数ファイルのステータスビット情報／制御モードビット移行情報の全ビット情報条件が成立した場合
                        if (iOrganizedCnt2 == bitAlarmInfo.BitInfo.Count)
                        {
                            uiState2 = 1;                   // 今回状態に条件成立を設定
                        }
                        else
                        {
                            uiState2 = 0;                   // 今回状態に条件不成立を設定
                        }

                        if (uiState1 == uiState2)           // 前回状態と今回状態が同じ場合
                        {
                            // 処理なし（状変なしの為、処理なし）
                        }
                        else                                // 前回状態と今回状態が違う場合
                        {
                            if (0 != uiState2)              // 今回状態が条件成立の場合
                            {
                                kekka = 1;                  // 戻り値に条件成立を設定
                            }
                            else                            // 今回状態が条件不成立の場合
                            {
                                kekka = 2;                  // 戻り値に条件不成立を設定
                            }
                        }

                    }
                    else           //複数のビット情報はOR条件として判定する場合
                    {
                        for (iBitInfoCnt = 0; iBitInfoCnt < bitAlarmInfo.BitInfo.Count; iBitInfoCnt++)
                        {
                            if (iChangeNG != 0)             // 成立中の条件がある場合の変化なし
                            {
                                kekka = 0;
                                break;
                            }
                            else if (uiCompareResult[iBitInfoCnt] == 0 && iChangeONNG == 0)     //不成立から成立へ
                            {
                                kekka = 1;
                                break;                                      //不成立から成立の条件が一つでもあれば、戻り値に条件成立を設定
                            }
                            else if (uiCompareResult[iBitInfoCnt] == 1)     //成立から不成立へ
                            {
                                kekka = 2;                                  //成立から不成立の条件であれば、戻り値に条件成立を設定し、ループし続ける
                            }
                            else if (uiCompareResult[iBitInfoCnt] == 2 && kekka != 2)  //変化なし
                            {
                                kekka = 0;                                  //変化なし、かつ成立から不成立の条件がなければ、戻り値に該当なしを設定
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
            finally 
            {
                iBitResultFormer = null;
                iBitResultCurrent = null;
                uiCompareResult = null;
            }
            return kekka;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 設定条件のチェック処理
        /// MODULE ID           : CheckSettingDictionary
        ///
        /// PARAMETER IN        : 
        /// <param name="settingDataList">(in)設定条件</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>true=状態一致 / false=状態不一致</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// 設定条件中に一致する条件が存在するかを判断し結果を返す。
        /// 一致する条件が存在する場合は"true"、一致する条件が存在しない場合は"false"を返す。
        /// </summary>
        ///
        ///*******************************************************************************
        private bool CheckSettingDictionary(Dictionary<UInt16, List<CSetData>> settingDataList)
        {
            bool bRetValue = false;             // リターン値
            bool IsCheckValue = false;          // チェック結果

            try
            {
                if (null == settingDataList)
                {
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.ERROR, MethodBase.GetCurrentMethod(), "パラメータエラー");
                    return false;
                }
                else
                {
                    // 処理なし
                }

                foreach (List<CSetData> setDataList in settingDataList.Values)
                {
                    IsCheckValue = this.CheckSettingStateList(setDataList);
                    if (IsCheckValue)
                    {
                        bRetValue = true;
                        break;
                    }
                    else
                    {
                        // 処理なし
                    }
                }
                return bRetValue;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                return false;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 駅設定条件のチェック処理
        /// MODULE ID           : EkiCheckSettingDictionary
        ///
        /// PARAMETER IN        : 
        /// <param name="settingDataList">(in)設定条件</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>true=状態一致 / false=状態不一致</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// 駅の設定条件中に一致する条件が存在するかを判断し結果を返す。
        /// 一致する条件が存在する場合は"true"、一致する条件が存在しない場合は"false"を返す。
        /// </summary>
        ///
        ///*******************************************************************************
        private bool EkiCheckSettingDictionary(Dictionary<UInt16, List<CSetData>> settingDataList)
        {
            bool bRetValue = false;             // リターン値
            bool IsCheckValue = false;          // チェック結果

            try
            {
                if (null == settingDataList)
                {
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.ERROR, MethodBase.GetCurrentMethod(), "パラメータエラー");
                    return false;
                }
                else
                {
                    // 処理なし
                }

                foreach (List<CSetData> setDataList in settingDataList.Values)
                {
                    IsCheckValue = this.CheckSettingStateListKai(setDataList, false);
                    if (IsCheckValue)
                    {
                        bRetValue = true;
                        break;
                    }
                    else
                    {
                        // 処理なし
                    }
                }
                return bRetValue;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                return false;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 設定条件解析処理(リスト単位)
        /// MODULE ID           : CheckSettingStateList
        ///
        /// PARAMETER IN        : 
        /// <param name="settingData">(in)設定条件リスト</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>true=状態一致 / false=状態不一致</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// 設定条件リストを取得し、リストに登録済みの設定条件を解析する。
        /// </summary>
        ///
        ///*******************************************************************************
        private bool CheckSettingStateList(List<CSetData> settingData)
        {
            bool bRetValue = false;             // リターン値

            try
            {
                bRetValue = CheckSettingStateListKai(settingData, false);
                return bRetValue;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                return false;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 設定条件解析処理(リスト単位)
        /// MODULE ID           : CheckSettingStateListKai
        ///
        /// PARAMETER IN        : 
        /// <param name="settingData">(in)設定条件リスト</param>
        /// <param name="isEkiConv">(in)駅変換必要有無（true=有、false=無）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>true=状態一致 / false=状態不一致</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// 設定条件リストを取得し、リストに登録済みの設定条件を解析する。
        /// </summary>
        ///
        ///*******************************************************************************
        private bool CheckSettingStateListKai(List<CSetData> settingData, bool isEkiConv)
        {
            bool bRetValue = false;             // リターン値

            try
            {
                if (null == settingData)
                {
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.ERROR, MethodBase.GetCurrentMethod(), "パラメータエラー");
                    return false;
                }
                else
                {
                    // 処理なし
                }

                foreach (CSetData setdata in settingData)
                {
                    bRetValue = CheckSettingState(setdata, isEkiConv);
                    if (false == bRetValue)
                    {
                        // 状態不一致の場合は処理終了
                        break;
                    }
                    else
                    {
                        // 状態一致の場合は処理続行
                        // 最終的に全て状態一致の場合の戻り値は[true]となる
                        // 処理なし
                    }
                }
                return bRetValue;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                return false;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 設定条件解析処理(個別) 
        /// MODULE ID           : CheckSettingState
        ///
        /// PARAMETER IN        : 
        /// <param name="setData">(in)設定条件</param>
        /// <param name="isEkiConv">(in)駅変換必要有無（true=有、false=無）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>true=一致 / false=不一致</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// 設定条件を取得し状態解析を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private bool CheckSettingState(CSetData setData, bool isEkiConv)
        {
            bool bRet = false;
            UInt16[] aryData = null;
            String logMsg = String.Empty;
            UInt16 ekino = 0;                   // 駅番号

            try
            {
                // 設定区分番号で処理を分岐
                switch (setData.KbNo)
                {
                    // CTC状態情報格納テーブルの状態を解析する
                    case (UInt16)CAppDat.SETID.SEIGYO:
                    case (UInt16)CAppDat.SETID.HYOUJI:
                    case (UInt16)CAppDat.SETID.MODE:
                    case (UInt16)CAppDat.SETID.KEIHOU:

                        Int32 ekiIndex = CCommon.GetEkiIndex(CCommon.EKINOTYPE.EKINO_TUEKI, setData.Ekino);

                        if ((-1 == ekiIndex) || (null == m_CTCInfoMngT) || (null == m_TekoHyojiInfoMngT))
                        {
                            logMsg = String.Format("通算駅番号・格納テーブル異常 駅番号={0}, 状態テーブル={1:X}, 状態テーブル={2:X}", setData.Ekino, m_CTCInfoMngT, m_TekoHyojiInfoMngT);
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                        }
                        else
                        {
                            UInt16 sysno = CAppDat.EkiConvF[(UInt16)(ekiIndex + 1)].SysNo;

                            if ((0 != (CAppDat.RecvSoutiKind & 0x000F)) && 
                                ((0 < sysno) && (sysno <= CAppDat.CTCMAX)))
                            {
                                if (true == isEkiConv)  // 駅変換必要有りの場合
                                {
                                    ekino = CAppDat.EkiConvF[(UInt16)(ekiIndex + 1)].EkiNoCon.Ctceki;
                                }
                                else                    // 駅変換必要無しの場合
                                {
                                    ekino = setData.Ekino;
                                }

                                m_CTCInfoMngT[sysno - 1].GetRecvData(ref aryData);

                                if ((null == aryData) || (0 == aryData.Length))
                                {
                                    bRet = false;
                                }
                                else
                                {
                                    bRet = CCommon.GetStateCTC(aryData, setData.KbNo, ekino, setData.Kigou, setData.Hoko, sysno, 0xFFFF);
                                }
                            }
                            else if ((0 != (CAppDat.RecvSoutiKind & 0x00F0)) &&
                                ((0 < sysno) && (sysno <= CAppDat.PRCDENSOMAX)))
                            {
                                if (true == isEkiConv)  // 駅変換必要有りの場合
                                {
                                    ekino = CAppDat.EkiConvF[(UInt16)(ekiIndex + 1)].EkiNoCon.Ctceki;
                                }
                                else                    // 駅変換必要無しの場合
                                {
                                    ekino = setData.Ekino;
                                }

                                m_TekoHyojiInfoMngT[sysno - 1].GetRecvData(ref aryData);

                                if ((null == aryData) || (0 == aryData.Length))
                                {
                                    bRet = false;
                                }
                                else
                                {
                                    bRet = CCommon.GetStateTEKOHYOJI(aryData, setData.KbNo, ekino, setData.TekoHyojiKubun, setData.Kigou, setData.Hoko, sysno, 0xFFFF);
                                }
                            }
                            else
                            {
                                logMsg = String.Format("システム番号異常 ノード情報INDEX={0}, システム番号={1}", ekiIndex, sysno);
                                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                            }
                        }
                        break;

                    case (UInt16)CAppDat.SETID.TIDIFSEIGYO:
                    case (UInt16)CAppDat.SETID.TIDIFHYOUJI:
                    case (UInt16)CAppDat.SETID.TIDIFMODE:
                    case (UInt16)CAppDat.SETID.TIDIFKEIHOU:
                        Int32 stationInd = CCommon.GetEkiIndex(CCommon.EKINOTYPE.EKINO_TUEKI, setData.Ekino);

                        if ((-1 == stationInd) || (null == m_OtherIFStatInfoMngT))
                        {
                            logMsg = String.Format("通算駅番号・格納テーブル異常 駅番号={0}, 状態テーブル={1:X}", setData.Ekino, m_OtherIFStatInfoMngT);
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                        }
                        else
                        {
                            UInt16 sysno = CAppDat.EkiConvF[(UInt16)(stationInd + 1)].SysNo;

                            if ((0 < sysno) && (sysno <= CAppDat.TIDMAX))
                            {
                                if (true == isEkiConv)  // 駅変換必要有りの場合
                                {
                                    ekino = CAppDat.EkiConvF[(UInt16)(stationInd + 1)].EkiNoCon.Ctceki;
                                }
                                else                    // 駅変換必要無しの場合
                                {
                                    ekino = setData.Ekino;
                                }

                                m_OtherIFStatInfoMngT[sysno - 1].GetRecvData(ref aryData);

                                if ((null == aryData) || (0 == aryData.Length))
                                {
                                    bRet = false;
                                }
                                else
                                {
                                    bRet = CCommon.GetStateCTC(aryData, setData.KbNo, ekino, setData.Kigou, setData.Hoko, sysno, 0xFFFF);
                                }
                            }
                            else
                            {
                                logMsg = String.Format("他システム番号異常 ノード情報INDEX={0}, 他システム番号={1}", stationInd, sysno);
                                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                            }
                        }

                        break;

                    // PRC状態情報格納テーブルの状態を解析する
                    case (UInt16)CAppDat.SETID.SHINROYOKUSI:
                    case (UInt16)CAppDat.SETID.YOKOKUYUUSEN:
                    case (UInt16)CAppDat.SETID.YOMIDASHITEISHI:
                        if (null == m_PRCInfoMngT)
                        {
                            // 処理なし
                        }
                        else
                        {
                            m_PRCInfoMngT.GetRecvData(ref aryData);

                            if (CAppDat.TypeF.PrcStatusAnalyzeFormat == (UInt16)CAppDat.PRCSTATUSANALYZEFORMAT.NOPRCDENSO1)
                            {
                                bRet = CCommon.GetStatePRC(aryData, setData.KbNo, setData.Ekino, setData.Kigou, setData.Hoko);
                            }
                            else if (CAppDat.TypeF.PrcStatusAnalyzeFormat == (UInt16)CAppDat.PRCSTATUSANALYZEFORMAT.PRCDENSO1)
                            {
                                bRet = CCommon.GetStatePRC2(aryData, setData.KbNo, setData.Ekino, setData.Kigou, setData.Hoko);
                            }
                            else
                            {
                                bRet = CCommon.GetStatePRC(aryData, setData.KbNo, setData.Ekino, setData.Kigou, setData.Hoko);
                            }
                        }
                        break;

                    // 列番情報格納テーブルの状態を解析する
                    case (UInt16)CAppDat.SETID.MADO:
                        if (null == m_RetuInfoMngT)
                        {
                            // 処理なし
                        }
                        else
                        {
                            m_RetuInfoMngT.GetRecvData(ref aryData);
                            // 地点情報解析の場合
                            if (true == setData.Kigou.StartsWith(CAppDat.CHITENSTR))
                            {
                                bRet = CCommon.GetStateCHITEN(aryData, setData.KbNo, setData.Ekino, setData.Kigou, setData.Hoko);
                            }
                            // 表示フラグ解析の場合
                            else if (true == setData.Kigou.StartsWith(CAppDat.DISPFLGSTR))
                            {
                                bRet = CCommon.GetStateDISPFLG(aryData, setData.KbNo, setData.Ekino, setData.Kigou, setData.Hoko);
                            }
                            // 列万情報解析の場合
                            else
                            {
                                bRet = CCommon.GetStateRETU(aryData, setData.KbNo, setData.Ekino, setData.Kigou, setData.Hoko);
                            }
                        }
                        break;

                    // 仮想転てつ器状態テーブルの状態を解析する
                    case (UInt16)CAppDat.SETID.KASOTENTETUKI:
                        bRet = CCommon.GetStateKASOU(setData.KbNo, setData.Ekino, setData.Kigou, setData.Hoko);
                        break;

                    // 装置状態テーブルの状態を解析する
                    case (UInt16)CAppDat.SETID.SOUTI:
                        UInt16[] sysnoCTC = new UInt16[CAppDat.CTCMAX]{0,0,0,0};    //CTCSys番号を使用しない時に０を格納
                        bRet = CCommon.GetStateSOUTI(setData.KbNo, setData.Ekino, setData.Kigou, setData.Hoko, 0, sysnoCTC);
                        sysnoCTC = null;
                        break;

                    // 該当する解析情報なし
                    default:
                        if ((0 == setData.KbNo) && (0 == setData.Ekino) && (String.Empty == setData.Kigou) && (0 == setData.Hoko))
                        {
                            // 「設定区分番号=0, 駅番号=0, 記号名称="", 方向=0」の場合は、処理なし
                        }
                        else
                        {
                            logMsg = String.Format("ERROR 設定区分番号={0}, 駅番号={1}, 記号名称={2}, 方向={3}", setData.KbNo, setData.Ekino, setData.Kigou, setData.Hoko);
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                        }
                        break;
                }
                return bRet;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                return false;
            }
            finally
            {
                aryData = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 受信情報展開処理
        /// MODULE ID           : GetRecvAnalyzeDataUpdate
        ///
        /// PARAMETER IN        : 
        /// <param name="request">(in)要求番号</param>
        /// PARAMETER OUT       : 
        /// <param name="sysNo">(out)CTCシステム番号</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>true=成功 / false=失敗</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// D-NeXUSから受信した情報を解析するために解析クラスの変数に展開する。
        /// </summary>
        ///
        ///*******************************************************************************
        private bool GetRecvAnalyzeDataUpdate(Int32 request, ref UInt16 sysNo)
        {
            String syncErrorMsg = string.Empty;

            try
            {
                sysNo = 0;
                switch (request)
                {
                    case (Int32)REQUESTID.RETUBAN:          // 列番情報解析要求
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.RECVRETUINFO].WaitOne();
                        try
                        {
                            m_RetuInfoMngT = null;
                            m_RetuInfoMngT = (CNxRetuInfoMng)CAppDat.RetuInfoMngT.Clone();
                        }
                        catch (Exception ex)
                        {
                            // ミューテックス取得中に発生した例外の捕捉
                            syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                        }
                        finally
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.RECVRETUINFO].ReleaseMutex();
                        }
                        break;

                    case (Int32)REQUESTID.PRCDATA:          // PRC状態情報解析要求
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.RECVPRCINFO].WaitOne();
                        try
                        {
                            m_PRCInfoMngT = null;
                            m_PRCInfoMngT = (CNxPRCInfoMng)CAppDat.PRCInfoMngT.Clone();
                        }
                        catch (Exception ex)
                        {
                            // ミューテックス取得中に発生した例外の捕捉
                            syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                        }
                        finally
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.RECVPRCINFO].ReleaseMutex();
                        }
                        break;

                    case (Int32)REQUESTID.CTCADATA:          // CTC状態情報解析要求(A-Sys)
                    case (Int32)REQUESTID.CTCBDATA:          // CTC状態情報解析要求(B-Sys)
                    case (Int32)REQUESTID.CTCCDATA:          // CTC状態情報解析要求(C-Sys)
                    case (Int32)REQUESTID.CTCDDATA:          // CTC状態情報解析要求(D-Sys)
                        switch (request)
                        {
                            case (Int32)REQUESTID.CTCBDATA:
                                sysNo = 2;
                                break;
                            case (Int32)REQUESTID.CTCCDATA:
                                sysNo = 3;
                                break;
                            case (Int32)REQUESTID.CTCDDATA:
                                sysNo = 4;
                                break;
                            default:
                                sysNo = 1;
                                break;
                        }

                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.RECVCTCINFO].WaitOne();
                        try
                        {
                            m_CTCInfoMngOldT[sysNo - 1] = null;
                            if (null == m_CTCInfoMngT[sysNo - 1])
                            {
                                m_CTCInfoMngOldT[sysNo - 1] = new CNxCTCInfoMng();
                            }
                            else
                            {
                                m_CTCInfoMngOldT[sysNo - 1] = (CNxCTCInfoMng)m_CTCInfoMngT[sysNo - 1].Clone();
                            }
                            m_CTCInfoMngT[sysNo - 1] = null;
                            m_CTCInfoMngT[sysNo - 1] = (CNxCTCInfoMng)CAppDat.CTCInfoMngT[sysNo - 1].Clone();
                        }
                        catch (Exception ex)
                        {
                            // ミューテックス取得中に発生した例外の捕捉
                            syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                        }
                        finally
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.RECVCTCINFO].ReleaseMutex();
                        }

                        // ビット反転設定定数ファイルに従い、CTC状態情報のビット反転を行う
                        ReverseBitCTCInfo(m_CTCInfoMngT[sysNo - 1], CAppDat.BitXorF[sysNo]);   // ビット反転処理
                        break;
                    case (Int32)REQUESTID.TIDIF1DATA:          // 他線区I/F状態情報解析要求(Sys1)
                    case (Int32)REQUESTID.TIDIF2DATA:          // 他線区I/F状態情報解析要求(Sys2)
                    case (Int32)REQUESTID.TIDIF3DATA:          // 他線区I/F状態情報解析要求(Sys3)
                    case (Int32)REQUESTID.TIDIF4DATA:          // 他線区I/F状態情報解析要求(Sys4)
                        switch (request)
                        {
                            case (Int32)REQUESTID.TIDIF2DATA:
                                sysNo = 2;
                                break;
                            case (Int32)REQUESTID.TIDIF3DATA:
                                sysNo = 3;
                                break;
                            case (Int32)REQUESTID.TIDIF4DATA:
                                sysNo = 4;
                                break;
                            default:
                                sysNo = 1;
                                break;
                        }

                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.RECVTIDIFINFO].WaitOne();
                        try
                        {
                            m_OtherIFStatInfoMngOldT[sysNo - 1] = null;
                            if (null == m_OtherIFStatInfoMngT[sysNo - 1])
                            {
                                m_OtherIFStatInfoMngOldT[sysNo - 1] = new CNxCTCInfoMng();
                            }
                            else
                            {
                                m_OtherIFStatInfoMngOldT[sysNo - 1] = (CNxCTCInfoMng)m_OtherIFStatInfoMngT[sysNo - 1].Clone();
                            }
                            m_OtherIFStatInfoMngT[sysNo - 1] = null;
                            m_OtherIFStatInfoMngT[sysNo - 1] = (CNxCTCInfoMng)CAppDat.OtherCTCInfoMngT[sysNo - 1].Clone();
                        }
                        catch (Exception ex)
                        {
                            // ミューテックス取得中に発生した例外の捕捉
                            syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                        }
                        finally
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.RECVTIDIFINFO].ReleaseMutex();
                        }
                        break;

                    case (Int32)REQUESTID.TIDIF1RETUBAN:        // 他線区I/F列番情報解析要求(Sys1)
                    case (Int32)REQUESTID.TIDIF2RETUBAN:        // 他線区I/F列番情報解析要求(Sys2)
                    case (Int32)REQUESTID.TIDIF3RETUBAN:        // 他線区I/F列番情報解析要求(Sys3)
                    case (Int32)REQUESTID.TIDIF4RETUBAN:        // 他線区I/F列番情報解析要求(Sys4)
                        switch (request)
                        {
                            case (Int32)REQUESTID.TIDIF2RETUBAN:
                                sysNo = 2;
                                break;
                            case (Int32)REQUESTID.TIDIF3RETUBAN:
                                sysNo = 3;
                                break;
                            case (Int32)REQUESTID.TIDIF4RETUBAN:
                                sysNo = 4;
                                break;
                            default:
                                sysNo = 1;
                                break;
                        }

                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.RECVTIDIFINFO].WaitOne();
                        try
                        {
                            m_OtherIFRetuInfoMngOldT[sysNo - 1] = null;
                            if (null == m_OtherIFRetuInfoMngT[sysNo - 1])
                            {
                                m_OtherIFRetuInfoMngOldT[sysNo - 1] = new CNxRetuInfoMng();
                            }
                            else
                            {
                                m_OtherIFRetuInfoMngOldT[sysNo - 1] = (CNxRetuInfoMng)m_OtherIFRetuInfoMngT[sysNo - 1].Clone();
                            }
                            m_OtherIFRetuInfoMngT[sysNo - 1] = null;
                            m_OtherIFRetuInfoMngT[sysNo - 1] = (CNxRetuInfoMng)CAppDat.OtherRetuInfoMngT[sysNo - 1].Clone();
                        }
                        catch (Exception ex)
                        {
                            // ミューテックス取得中に発生した例外の捕捉
                            syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                        }
                        finally
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.RECVTIDIFINFO].ReleaseMutex();
                        }
                        break;

                    case (Int32)REQUESTID.ALARM:            // 警報情報解析要求
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.KEIHOUINFO].WaitOne();
                        try
                        {
                            m_KeihouInfoMngT.Clear();
                            for (UInt16 cnt = 0; cnt < CAppDat.KeihouInfoMngT.Count; cnt++)
                            {
                                m_KeihouInfoMngT.Add((CNxKeihouInfoMng)CAppDat.KeihouInfoMngT[cnt].Clone());
                                CAppDat.KeihouInfoMngT[cnt] = null;
                            }
                            CAppDat.KeihouInfoMngT.Clear();
                        }
                        catch (Exception ex)
                        {
                            // ミューテックス取得中に発生した例外の捕捉
                            syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                        }
                        finally
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.KEIHOUINFO].ReleaseMutex();
                        }
                        break;

                    case (Int32)REQUESTID.TEIAN:            // 提案通知解析要求
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.TEIANINFO].WaitOne();
                        try
                        {
                            m_TeianMngT.Clear();
                            for (UInt16 cnt = 0; cnt < CAppDat.TeianMngT.Count; cnt++)
                            {
                                m_TeianMngT.Add((CNxTeianMng)CAppDat.TeianMngT[cnt].Clone());
                                CAppDat.TeianMngT[cnt] = null;
                            }
                            CAppDat.TeianMngT.Clear();
                        }
                        catch (Exception ex)
                        {
                            // ミューテックス取得中に発生した例外の捕捉
                            syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                        }
                        finally
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.TEIANINFO].ReleaseMutex();
                        }
                        break;

                    case (Int32)REQUESTID.SETTEIAN:         // 提案設定情報解析要求
                        m_TeianSetRecvMngT = null;
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.TEIANSETINFO].WaitOne();
                        try
                        {
                            m_TeianSetRecvMngT = (CNxTeianSetRecvMng)CAppDat.TeianSetRecvMngT.Clone();
                        }
                        catch (Exception ex)
                        {
                            // ミューテックス取得中に発生した例外の捕捉
                            syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                        }
                        finally
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.TEIANSETINFO].ReleaseMutex();
                        }
                        break;

                    case (Int32)REQUESTID.TEKOHYOJIA:          // てこ／表示情報解析要求(A-Sys)
                    case (Int32)REQUESTID.TEKOHYOJIB:          // てこ／表示情報解析要求(B-Sys)
                    case (Int32)REQUESTID.TEKOHYOJIC:          // てこ／表示情報解析要求(C-Sys)
                    case (Int32)REQUESTID.TEKOHYOJID:          // てこ／表示情報解析要求(D-Sys)
                        switch (request)
                        {
                            case (Int32)REQUESTID.TEKOHYOJIB:
                                sysNo = 2;
                                break;
                            case (Int32)REQUESTID.TEKOHYOJIC:
                                sysNo = 3;
                                break;
                            case (Int32)REQUESTID.TEKOHYOJID:
                                sysNo = 4;
                                break;
                            default:
                                sysNo = 1;
                                break;
                        }

                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.RECVTEKOHYOJIINFO].WaitOne();
                        try
                        {
                            m_TekoHyojiInfoMngOldT[sysNo - 1] = null;
                            if (null == m_TekoHyojiInfoMngT[sysNo - 1])
                            {
                                m_TekoHyojiInfoMngOldT[sysNo - 1] = new CNxTekoHyojiInfoMng();
                            }
                            else
                            {
                                m_TekoHyojiInfoMngOldT[sysNo - 1] = (CNxTekoHyojiInfoMng)m_TekoHyojiInfoMngT[sysNo - 1].Clone();
                            }
                            m_TekoHyojiInfoMngT[sysNo - 1] = null;
                            m_TekoHyojiInfoMngT[sysNo - 1] = (CNxTekoHyojiInfoMng)CAppDat.TekoHyojiInfoMngT[sysNo - 1].Clone();
                        }
                        catch (Exception ex)
                        {
                            // ミューテックス取得中に発生した例外の捕捉
                            syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                        }
                        finally
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.RECVTEKOHYOJIINFO].ReleaseMutex();
                        }

                        // ビット反転設定定数ファイルに従い、てこ／表示情報のビット反転を行う
                        ReverseBitTekoHyojiInfo(m_TekoHyojiInfoMngT[sysNo - 1], CAppDat.BitXorF[sysNo]);   // ビット反転処理
                        break;

                    case (Int32)REQUESTID.DIAREQANS:        // ダイヤ要求アンサ解析要求
                    case (Int32)REQUESTID.THREADSTOP:       // スレッド停止
                        break;

                    case (Int32)REQUESTID.CHANGEDAYS:       // 日替わり処理起動要求
                        break;
                    
                    default:                                // その他要求
                        String message = String.Format("想定外要求={0}", request);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), message);
                        break;
                }
                return true;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                return false;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 状態番号変換処理（施行日を取得）
        /// MODULE ID           : ConvertToMadoStatDay
        ///
        /// PARAMETER IN        : 
        /// <param name="retuData">(in)解析する列番</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>窓状態番号</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// 列番の施行日を解析して窓状態番号を出力する。
        /// 中規模版と小規模版では解析状態が異なるため初期起動タイプに応じて処理を分岐する。
        /// </summary>
        ///
        ///*******************************************************************************
        private UInt16 ConvertToMadoStatDay(CRetuData retuData)
        {
            UInt16 state = (UInt16)MADOSTAT.UNKNOWN;

            try
            {
                // 施行曜日と現在の曜日の差分を取得
                Byte nowsekoubi = CCommon.GetDayOfWeek();
                Byte retsekobi = retuData.seko;
                Int16 sekobi = (Int16)(retsekobi - nowsekoubi);
                // 取得する状態文字列
                String statName = String.Empty;

                // 小規模では以下の条件で解析
                if ((UInt16)CAppDat.ENFLAG.FLAG_OFF == CAppDat.TypeF.RetuSekoExist)
                {
                    // 常にダイヤありとする
                    statName = MadoStatName[(UInt16)MADOSTAT.DIAARI];
                }
                else
                {
                    // 差分がなし？
                    if ((0 == sekobi) || (0 == retsekobi))
                    {
                        statName = MadoStatName[(UInt16)MADOSTAT.TODAY];
                    }
                    // 差分が１日前（施行日は１～７：日～土）？
                    else if ((-1 == sekobi) || (6 == sekobi))
                    {
                        statName = MadoStatName[(UInt16)MADOSTAT.YESTERDAY];
                    }
                    // 差分が１日後（施行日は１～７：日～土）？
                    else if ((1 == sekobi) || (-6 == sekobi))
                    {
                        statName = MadoStatName[(UInt16)MADOSTAT.TOMORROW];
                    }
                    else
                    {
                        // 処理なし
                    }
                }

                if (statName != String.Empty)
                {
                    // 該当する状態文字列
                    foreach (ushort key in CAppDat.MadoStateInfoDef.Keys)
                    {
                        if (CAppDat.MadoStateInfoDef[key] == statName)
                        {
                            state = key;
                            break;
                        }
                    }
                }
                return state;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                return (UInt16)MADOSTAT.UNKNOWN;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 状態番号変換処理
        /// MODULE ID           : ConvertToMadoStat
        ///
        /// PARAMETER IN        : 
        /// <param name="retuData">(in)解析する列番</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>窓状態番号</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// 列番の表示フラグを解析して窓状態番号（背景も含む）を出力する。
        /// 中規模版と小規模版では解析状態が異なるため初期起動タイプに応じて処理を分岐する。
        /// </summary>
        ///
        ///*******************************************************************************
        private UInt16 ConvertToMadoStat(CRetuData retuData)
        {
            UInt16 state = (UInt16)MADOSTAT.UNKNOWN;

            try
            {
                // 列番情報から表示フラグを取得
                UInt16 dispflg = (UInt16)retuData.dispflg;
                
                // 窓状態定義を参照しながら、優先度の該当する高い処理を行う
                for (ushort cnt = 0; cnt < CAppDat.MadoStateInfoDef.Count; cnt++)
                {
                    // 通常状態は無視
                    if (cnt == 0)
                    {
                        continue;
                    }

                    // 状態番号を含まなければ continue
                    if (false == CAppDat.MadoStateInfoDef.ContainsKey(cnt))
                    {
                        continue;
                    }

                    // 該当する状態文字列の処理を行う
                    // 列車抑止扱い列番
                    if (CAppDat.MadoStateInfoDef[cnt] == MadoStatName[(UInt16)MADOSTAT.RESYAYOKUSI])
                    {
                        if ((UInt16)DISPFLG.RESYAYOKUSI == (dispflg & (UInt16)DISPFLG.RESYAYOKUSI))
                        {
                            state = cnt;
                        }
                    }
                    // 仮列番
                    else if (CAppDat.MadoStateInfoDef[cnt] == MadoStatName[(UInt16)MADOSTAT.KARIRETUBAN])
                    {
                        if ((UInt16)DISPFLG.KARIRETUBAN == (dispflg & (UInt16)DISPFLG.KARIRETUBAN))
                        {
                            state = cnt;
                        }
                    }
                    // 提案応答待ち
                    else if (CAppDat.MadoStateInfoDef[cnt] == MadoStatName[(UInt16)MADOSTAT.TEIANOUTOU])
                    {
                        if ((UInt16)DISPFLG.TEIANOUTOU == (dispflg & (UInt16)DISPFLG.TEIANOUTOU))
                        {
                            state = cnt;
                        }
                    }
                    // ダイヤなし
                    else if (CAppDat.MadoStateInfoDef[cnt] == MadoStatName[(UInt16)MADOSTAT.DIANASI])
                    {
                        if ((UInt16)DISPFLG.DIANASI == (dispflg & (UInt16)DISPFLG.DIANASI))
                        {
                            state = cnt;
                        }
                    }
                    // 順序保留中
                    else if (CAppDat.MadoStateInfoDef[cnt] == MadoStatName[(UInt16)MADOSTAT.JYUNJYOHORYU])
                    {
                        if ((UInt16)DISPFLG.JYUNJYOHORYU == (dispflg & (UInt16)DISPFLG.JYUNJYOHORYU))
                        {
                            state = cnt;
                        }
                    }
                    // 滞泊列番
                    else if (CAppDat.MadoStateInfoDef[cnt] == MadoStatName[(UInt16)MADOSTAT.TAIHAKU])
                    {
                        if ((UInt16)DISPFLG.TAIHAKU == (dispflg & (UInt16)DISPFLG.TAIHAKU))
                        {
                            state = cnt;
                        }
                    }
                    else
                    {
                        // 処理なし
                    }

                    // 窓状態が通常状態でない（何かしらの状態になっている）場合
                    if (state != (UInt16)MADOSTAT.UNKNOWN)
                    {
                        break;
                    }
                }
                return state;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                return (UInt16)MADOSTAT.UNKNOWN;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 遅延状態番号変換処理
        /// MODULE ID           : ConvertToMadoChienStat
        ///
        /// PARAMETER IN        : 
        /// <param name="retuData">(in)解析する列番</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>窓遅延状態番号</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// 列番の遅延時分を解析して窓遅延状態番号を出力する。
        /// </summary>
        ///
        ///*******************************************************************************
        private UInt16 ConvertToMadoChienStat(CRetuData retuData)
        {
            Int32  kosu = 0;
            Int32  index = 0;
            Int32  posi = 0;
            UInt16 chienState = (UInt16)MADOCHIENSTAT.UNKNOWN;
            UInt16 chienLevel = (UInt16)MADOCHIENLEVEL.UNKNOWN;
            UInt16[] chienKubunList = { 0, 0, 0, 0, 0 };
            UInt16[] chienCheckList = { 0, 0, 0, 0, 0 };
            UInt16[] chienKubun = { 0, 0, 0, 0, 0 };
            String  strAppLog = String.Empty;           // ログメッセージ

            try
            {
                // 遅延時分を分で取得
                UInt16 latetime = (UInt16)((retuData.latetim * 5) / 60);

                // 遅延状態を設定
                chienKubunList[0] = (UInt16)MADOCHIENSTAT.LIMITSTAT1;
                chienKubunList[1] = (UInt16)MADOCHIENSTAT.LIMITSTAT2;
                chienKubunList[2] = (UInt16)MADOCHIENSTAT.LIMITSTAT3;
                chienKubunList[3] = (UInt16)MADOCHIENSTAT.LIMITSTAT4;
                chienKubunList[4] = (UInt16)MADOCHIENSTAT.LIMITSTAT5;

                // 遅延判定時分を設定
                chienCheckList[0] = (UInt16)CAppDat.TypeF.Disp.ChienCheck1;
                chienCheckList[1] = (UInt16)CAppDat.TypeF.Disp.ChienCheck2;
                chienCheckList[2] = (UInt16)CAppDat.TypeF.Disp.ChienCheck3;
                chienCheckList[3] = (UInt16)CAppDat.TypeF.Disp.ChienCheck4;
                chienCheckList[4] = (UInt16)CAppDat.TypeF.Disp.ChienCheck5;

                // 遅延状態区分を設定
                chienKubun[0] = (UInt16)MADOCHIENSTAT.UNKNOWN;
                chienKubun[1] = (UInt16)MADOCHIENSTAT.UNKNOWN;
                chienKubun[2] = (UInt16)MADOCHIENSTAT.UNKNOWN;
                chienKubun[3] = (UInt16)MADOCHIENSTAT.UNKNOWN;
                chienKubun[4] = (UInt16)MADOCHIENSTAT.UNKNOWN;

                kosu = chienCheckList.Length;
                posi = 0;
                for (index = (kosu - 1); index >= 0; index--)
                {
                    if (0 < chienCheckList[index])
                    {
                        chienKubun[index] = chienKubunList[posi];
                        posi++;
                    }
                    else
                    {
                        // 処理なし
                    }
                }

                // 遅延時分が判定時分５以上か？
                if ((0 < CAppDat.TypeF.Disp.ChienCheck5) && (latetime >= CAppDat.TypeF.Disp.ChienCheck5))
                {
                    // 遅延レベルに限界基準５をセット
                    chienLevel = (UInt16)MADOCHIENLEVEL.LIMITLEVEL5;
                }
                // 遅延時分が判定時分５未満、且つ、判定時分４以上か？
                else if ((0 < CAppDat.TypeF.Disp.ChienCheck4) && (latetime >= CAppDat.TypeF.Disp.ChienCheck4))
                {
                    // 遅延レベルに限界基準４をセット
                    chienLevel = (UInt16)MADOCHIENLEVEL.LIMITLEVEL4;
                }
                // 遅延時分が判定時分４未満、且つ、判定時分３以上か？
                else if ((0 < CAppDat.TypeF.Disp.ChienCheck3) && (latetime >= CAppDat.TypeF.Disp.ChienCheck3))
                {
                    // 遅延レベルに限界基準３をセット
                    chienLevel = (UInt16)MADOCHIENLEVEL.LIMITLEVEL3;
                }
                // 遅延時分が判定時分３未満、且つ、判定時分２以上か？
                else if ((0 < CAppDat.TypeF.Disp.ChienCheck2) && (latetime >= CAppDat.TypeF.Disp.ChienCheck2))
                {
                    // 遅延レベルに限界基準２をセット
                    chienLevel = (UInt16)MADOCHIENLEVEL.LIMITLEVEL2;
                }
                // 遅延時分が判定時分２未満、且つ、判定時分１以上か？
                else if ((0 < CAppDat.TypeF.Disp.ChienCheck1) && (latetime >= CAppDat.TypeF.Disp.ChienCheck1))
                {
                    // 遅延レベルに限界基準１をセット
                    chienLevel = (UInt16)MADOCHIENLEVEL.LIMITLEVEL1;
                }
                // 遅延時分が判定時分１未満か？
                else if ((latetime >= 0) && (latetime < CAppDat.TypeF.Disp.ChienCheck1))
                {
                    // 遅延レベルにUNKNOWNをセット
                    chienLevel = (UInt16)MADOCHIENLEVEL.UNKNOWN;
                }
                else
                {
                    // 遅延レベルにUNKNOWNをセット
                    chienLevel = (UInt16)MADOCHIENLEVEL.UNKNOWN;

                     // 上記のif文が成立していない場合、定数エラーの可能性があるのでログ出力する
                    strAppLog = String.Format("retuLateTime={0} ChienCheck1={1} ChienCheck2={2} ChienCheck3={3} ChienCheck4={4} ChienCheck5={5}",
                                                retuData.latetim, CAppDat.TypeF.Disp.ChienCheck1, CAppDat.TypeF.Disp.ChienCheck2,
                                                CAppDat.TypeF.Disp.ChienCheck3, CAppDat.TypeF.Disp.ChienCheck4, CAppDat.TypeF.Disp.ChienCheck5);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                }

                // 遅延レベルに応じた遅延状態区分を取得
                if ((UInt16)MADOCHIENLEVEL.UNKNOWN == chienLevel)
                {
                    chienState = (UInt16)MADOCHIENSTAT.UNKNOWN;
                }
                else if((chienLevel >= (UInt16)MADOCHIENLEVEL.LIMITLEVEL1) && (chienLevel <= (UInt16)MADOCHIENLEVEL.LIMITLEVEL5))
                {
                    chienState = chienKubun[chienLevel - 1];
                }
                else
                {
                    chienState = (UInt16)MADOCHIENSTAT.UNKNOWN;

                     // ここにはこないはず
                    strAppLog = String.Format("chienLevel={0}", chienLevel);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                }

                return chienState;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                return (UInt16)MADOCHIENSTAT.UNKNOWN;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : ＣＴＣモード警報解析処理
        /// MODULE ID           : CtcModeKeihouAnalyze
        ///
        /// PARAMETER IN        : 
        /// <param name="sysNo">(in)CTCシステム番号（1-4）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// 制御モード情報（モード状態）を解析して警報出力を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void CtcModeKeihouAnalyze(UInt16 sysNo)
        {
            UInt16  uiRet;                              // 戻り値取得用
            bool    isAllJyoken = false;                // 全ビット情報条件成立フラグ（true=成立、false=不成立）
            Int32   isJouhenCnt = 0;                    // 状変有カウンタ
            Int32   iCnt = 0;                           // ループカウンタ
            bool    isKeihou = true;                    // 警報対象フラグ
            UInt16  ekno = 0;                           // 駅No.
            UInt16  kbno = 0;                           // 区分No.
            UInt16  chkPmode = 0;                       // P_MD チェックビット
            String  strTmp = String.Empty;              // 編集用
            String  strKigou = String.Empty;            // 記号名称
            String  strAppLog = String.Empty;           // ログメッセージ
            String  syncErrorMsg = String.Empty;        // 同期エラーメッセージ

            try
            {
                // 警報監視設定定数ファイルの制御モード移行情報数分ループ
                for (iCnt = 0; iCnt < CAppDat.AlarmInfoF.ModeBitAlarm.Count; iCnt++)
                {
                    isJouhenCnt = 0;                    // 状変有カウンタに０を設定
                    isAllJyoken = false;                // 全ビット情報条件成立フラグに不成立を設定

                    // 警報監視設定定数ファイルの制御モード移行情報のビット情報数が０（ビット情報がない）場合
                    if (0 == CAppDat.AlarmInfoF.ModeBitAlarm[iCnt].BitInfo.Count)
                    {
                        strAppLog = String.Format("警報監視設定定数ファイルの制御モード移行情報にビット情報がない：{0:D}", iCnt);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    }
                    // 警報監視設定定数ファイルの制御モード移行情報のビット情報数が０ではない（ビット情報がある）場合
                    else
                    {
                        {
                            uiRet = AnalyzeNbitAlarmInfo(1, iCnt, CAppDat.AlarmInfoF.ModeBitAlarm[iCnt], sysNo);
                                                        // Ｎビット情報条件解析処理を行う
                        }

                        if (1 == uiRet)                 // 条件成立の場合
                        {
                            isJouhenCnt++;              // 状変有カウンタをカウントＵＰ
                            isAllJyoken = true;         // 全ビット情報条件成立フラグに成立を設定
                        }
                        else if (2 == uiRet)            // 条件不成立の場合
                        {
                            isJouhenCnt++;              // 状変有カウンタをカウントＵＰ
                            isAllJyoken = false;        // 全ビット情報条件成立フラグに不成立を設定
                        }
                        else                            // 上記以外の場合
                        {
                            // 処理なし
                        }
                    }

                    // 状変有カウンタが０（＝状変がない）場合
                    if (0 == isJouhenCnt)
                    {
                        // 処理なし
                    }
                    // 状変有カウンタが０ではない（＝状変がある）場合
                    else
                    {
                        // 駅No.と記号名称を取得する
                        ekno     = CAppDat.AlarmInfoF.ModeBitAlarm[iCnt].BitInfo[0].Ekino;
                        strKigou = CAppDat.AlarmInfoF.ModeBitAlarm[iCnt].BitInfo[0].Kigou;

                        // 警報対象かどうかを判別する
                        isKeihou = true;                // 警報対象フラグに対象を設定

                        // P_MD No.を取得する
                        kbno = CCommon.GetPModeNo(strKigou, ref chkPmode);

                        if (0 != kbno)                  // P_MD No. が取得できた場合（P_MD警報の場合）
                        {
                            // 駅No.と区分No.が有効範囲内の場合
                            Int32 ekiIndex = CCommon.GetEkiIndex(CCommon.EKINOTYPE.EKINO_TUEKI, ekno);
                            UInt16 ctcEkiNo = 0;
                            if (-1 == ekiIndex)
                            {
                                strAppLog = String.Format("通算駅番号(CTC)格納テーブル異常 駅番号={0:D}", ekno);
                                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                            }
                            else
                            {
                                ctcEkiNo = CAppDat.EkiConvF[(UInt16)(ekiIndex + 1)].EkiNoCon.Ctceki;            //eknoを通算駅番号からＣＴＣ駅番号に変換  
                            }
                            if (((0 < ekno) && (CAppDat.TypeF.CTCEkimax[sysNo - 1] >= ctcEkiNo)) && ((0 < kbno) && (4 >= kbno)))

                            {
                                // 操作内容テーブル排他制御開始
                                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].WaitOne();

                                try
                                {
                                    // モード切替指示が操作ありか？

                                    if (0 != CAppDat.IopcoT.ModeKiriSiji[ekno - 1, kbno - 1])
                                    {
                                        isKeihou = false;        // 警報対象フラグに対象ではないを設定
                                        strAppLog = String.Format("モード切替指示が操作あり：ekno={0:D} kbno={1:D} Kigou={2}", ekno, kbno, strKigou);
                                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.INFORMATION, MethodBase.GetCurrentMethod(), strAppLog);
                                    }
                                    // モード切替指示が操作なしか？
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
                                    // 操作内容テーブル排他制御終了
                                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].ReleaseMutex();
                                }
                            }
                            // 駅No.、または、区分No.が有効範囲外の場合
                            else
                            {
                                strAppLog = String.Format("制御モード警報解析処理で駅No／区分Noが範囲外：ekno={0:D} kbno={1:D}", ekno, kbno);
                                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                            }
                        }
                        else                            // P_MD No. が取得できない場合（P_MD警報ではない場合）
                        {
                            // 処理なし
                        }

                        // 警報対象なら警報出力を行う
                        if (true == isKeihou)           // 警報対象フラグが対象の場合
                        {
                            // ＣＴＣモード警報出力処理を行う
                            CKeihouCtrl.OutputCtcModeKeihou((UInt16)iCnt, isAllJyoken);
                        }
                        else                            // 警報対象フラグが対象ではない場合
                        {
                            // 処理なし
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : PRC伝送部モード警報解析処理
        /// MODULE ID           : PrcDensoModeKeihouAnalyze
        ///
        /// PARAMETER IN        : 
        /// <param name="sysNo">(in)CTCシステム番号（1-4）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// 制御モード情報（モード状態）を解析して警報出力を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void PrcDensoModeKeihouAnalyze(UInt16 sysNo)
        {
            UInt16  uiRet;                              // 戻り値取得用
            bool    isAllJyoken = false;                // 全ビット情報条件成立フラグ（true=成立、false=不成立）
            Int32   isJouhenCnt = 0;                    // 状変有カウンタ
            Int32   iCnt = 0;                           // ループカウンタ
            bool    isKeihou = true;                    // 警報対象フラグ
            UInt16  ekno = 0;                           // 駅No.
            String  strTmp = String.Empty;              // 編集用
            String  strKigou = String.Empty;            // 記号名称
            String  strAppLog = String.Empty;           // ログメッセージ
            String  syncErrorMsg = String.Empty;        // 同期エラーメッセージ

            try
            {
                // 警報監視設定定数ファイルの制御モード移行情報数分ループ
                for (iCnt = 0; iCnt < CAppDat.AlarmInfoF.ModeBitAlarm.Count; iCnt++)
                {
                    isJouhenCnt = 0;                    // 状変有カウンタに０を設定
                    isAllJyoken = false;                // 全ビット情報条件成立フラグに不成立を設定

                    // 警報監視設定定数ファイルの制御モード移行情報のビット情報数が０（ビット情報がない）場合
                    if (0 == CAppDat.AlarmInfoF.ModeBitAlarm[iCnt].BitInfo.Count)
                    {
                        strAppLog = String.Format("警報監視設定定数ファイルの制御モード移行情報にビット情報がない：{0:D}", iCnt);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    }
                    // 警報監視設定定数ファイルの制御モード移行情報のビット情報数が０ではない（ビット情報がある）場合
                    else
                    {
                        // 警報監視設定定数ファイルの制御モード移行情報のビット情報が１つだけある（１ビット警報）場合
                        uiRet = AnalyzeNbitAlarmInfo(1, iCnt, CAppDat.AlarmInfoF.ModeBitAlarm[iCnt], sysNo);
                                                                    // Ｎビット情報条件解析処理を行う

                        if (1 == uiRet)                 // 条件成立の場合
                        {
                            isJouhenCnt++;              // 状変有カウンタをカウントＵＰ
                            isAllJyoken = true;         // 全ビット情報条件成立フラグに成立を設定
                        }
                        else if (2 == uiRet)            // 条件不成立の場合
                        {
                            isJouhenCnt++;              // 状変有カウンタをカウントＵＰ
                            isAllJyoken = false;        // 全ビット情報条件成立フラグに不成立を設定
                        }
                        else                            // 上記以外の場合
                        {
                            // 処理なし
                        }
                    }

                    // 状変有カウンタが０（＝状変がない）場合
                    if (0 == isJouhenCnt)
                    {
                        // 処理なし
                    }
                    // 状変有カウンタが０ではない（＝状変がある）場合
                    else
                    {
                        // 駅No.と記号名称を取得する
                        ekno = CAppDat.AlarmInfoF.ModeBitAlarm[iCnt].BitInfo[0].Ekino;
                        strKigou = CAppDat.AlarmInfoF.ModeBitAlarm[iCnt].BitInfo[0].Kigou;

                        // 警報対象かどうかを判別する
                        isKeihou = true;                // 警報対象フラグに対象を設定

                        Int32 ekiIndex = CCommon.GetEkiIndex(CCommon.EKINOTYPE.EKINO_TUEKI, ekno);
                        UInt16 rendoEkiNo = 0;
                        if (-1 == ekiIndex)
                        {
                            strAppLog = String.Format("通算駅番号格納テーブル異常 駅番号={0:D}", ekno);
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                        }
                        else
                        {
                            rendoEkiNo = CAppDat.EkiConvF[(UInt16)(ekiIndex + 1)].EkiNoCon.Ctceki;
                        }

                        if ((0 < ekno) && (CAppDat.TypeF.RendoEkimax[sysNo] >= rendoEkiNo))
                        {
                            // 操作内容テーブル排他制御開始
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].WaitOne();

                            try
                            {
                                // モード切替指示が操作ありか？
                                if (0 != CAppDat.IopcoT.ModeKiriSiji[ekno - 1, 0])
                                {
                                    isKeihou = false;        // 警報対象フラグに対象ではないを設定
                                    strAppLog = String.Format("モード切替指示が操作あり：ekno={0:D} Kigou={1}", ekno, strKigou);
                                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.INFORMATION, MethodBase.GetCurrentMethod(), strAppLog);
                                }
                                // モード切替指示が操作なしか？
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
                                // 操作内容テーブル排他制御終了
                                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].ReleaseMutex();
                            }
                        }
                        // 駅No.、または、区分No.が有効範囲外の場合
                        else
                        {
                            strAppLog = String.Format("制御モード警報解析処理で駅No／区分Noが範囲外：ekno={0:D}", ekno);
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                        }

                        // 警報対象なら警報出力を行う
                        if (true == isKeihou)           // 警報対象フラグが対象の場合
                        {
                            // てこ／表示情報モード警報出力処理を行う
                            CKeihouCtrl.OutputCtcModeKeihou((UInt16)iCnt, isAllJyoken);
                        }
                        else                            // 警報対象フラグが対象ではない場合
                        {
                            // 処理なし
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : ＣＴＣモード解析処理
        /// MODULE ID           : CtcModeAnalyze
        ///
        /// PARAMETER IN        : 
        /// <param name="sysNo">(in)CTCシステム番号（1-4）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// CTC状態情報を解析してモード状態を判断する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void CtcModeAnalyze(UInt16 sysNo)
        {
            String syncErrorMsg = String.Empty;
            String logMsg = String.Empty;
            Int32   ekiIndex = 0;                       // 駅Index

            try
            {
                if ((null == m_CTCInfoMngT) || (0 == sysNo) || (CAppDat.CTCMAX < sysNo))
                {
                    logMsg = String.Format("パラメータエラー sysNo={0}", sysNo);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                    return;
                }
                else
                {
                    // 処理なし
                }

                UInt16[] kbstat = new UInt16[4];
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].WaitOne();
                try
                {
                    for (UInt16 cnt = 0; cnt < CAppDat.TypeF.CTCEkimax[sysNo - 1]; cnt++)
                    {
                        // データレングスが必要レングス存在するかチェック
                        if (3 + ((cnt + 1) * 24) < m_CTCInfoMngT[sysNo - 1].RecvBuffData.Length)
                        {
                            // 処理なし
                        }
                        else
                        {
                            break;
                        }

                        UInt16 ekno = m_CTCInfoMngT[sysNo - 1].RecvBuffData[3 + (cnt * 24)];                               // 駅No.
                        kbstat[0] = (UInt16)((m_CTCInfoMngT[sysNo - 1].RecvBuffData[3 + (cnt * 24) + 3] & 0xFF00) >> 8);   // モード状態（区分１）
                        kbstat[1] = (UInt16)(m_CTCInfoMngT[sysNo - 1].RecvBuffData[3 + (cnt * 24) + 3] & 0x00FF);          // モード状態（区分２）
                        kbstat[2] = (UInt16)((m_CTCInfoMngT[sysNo - 1].RecvBuffData[3 + (cnt * 24) + 4] & 0xFF00) >> 8);   // モード状態（区分３）
                        kbstat[3] = (UInt16)(m_CTCInfoMngT[sysNo - 1].RecvBuffData[3 + (cnt * 24) + 4] & 0x00FF);          // モード状態（区分４）

                        ekiIndex = CCommon.GetEkiIndex(CCommon.EKINOTYPE.EKINO_CTC, ekno, sysNo);

                        if (-1 == ekiIndex)
                        {

                        }
                        else
                        {
                            ekno = CAppDat.EkiConvF[(UInt16)(ekiIndex + 1)].EkiNoCon.Totaleki;
                                                        // 駅番号を通算駅番号に置き換える
                        }

                        for (UInt16 ii = 0; ii < kbstat.Length; ii++)
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DCPSTATUS].WaitOne();
                            try
                            {
                                // 前回モード状態と今回モード状態が不一致なら指示フラグクリア
                                if (kbstat[ii] != CAppDat.DcpStatusT.ModeState[ekno - 1, ii])
                                {
                                    // モード切替指示フラグが指示ありのときのみ指示なしに更新
                                    if (1 == CAppDat.IopcoT.ModeKiriSiji[ekno - 1, ii])
                                    {
                                        CAppDat.IopcoT.ModeKiriSiji[ekno - 1, ii] = 0;
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
                                CAppDat.DcpStatusT.ModeState[ekno - 1, ii] = kbstat[ii];
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
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : ＰＲＣ伝送部モード解析処理
        /// MODULE ID           : PrcDensoModeAnalyze
        ///
        /// PARAMETER IN        : 
        /// <param name="sysNo">(in)CTCシステム番号（1-4）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// てこ／表示情報の扱い所てこを解析してモード状態を判断する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void PrcDensoModeAnalyze(UInt16 sysNo)
        {
            String syncErrorMsg = String.Empty;
            String logMsg = String.Empty;

            // 取り扱い最大駅数
            UInt16 ekMax = 16;

            // 各駅のモード状態を確認するための配列
            UInt16[] CHECK_BIT = {0x0100, 0x0200, 0x0400, 0x0800, 0x1000, 0x2000, 0x4000, 0x8000, 
                                  0x0001, 0x0002, 0x0004, 0x0008, 0x0010, 0x0020, 0x0040, 0x0080};

            try
            {
                if ((null == m_TekoHyojiInfoMngT) || (0 == sysNo) || (CAppDat.PRCDENSOMAX < sysNo))
                {
                    logMsg = String.Format("パラメータエラー sysNo={0}", sysNo);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                    return;
                }
                else
                {
                    // 処理なし
                }

                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].WaitOne();
                try
                {
                    // データの更新
                    for (UInt16 ekno = 0; ekno < ekMax; ekno++)
                    {
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DCPSTATUS].WaitOne();
                        try
                        {
                            // 今回モード状態の取得（０：手動、１：自動）
                            UInt16 state = 0;
                            if((m_TekoHyojiInfoMngT[sysNo - 1].RecvBuffData[3] & CHECK_BIT[ekno]) != 0)
                            {   // 自動の場合

                                state = 0x0400;
                            }
                            else
                            {   // 手動の場合

                                state = 0x0000;
                            }

                            // 前回モード状態と今回モード状態が不一致なら指示フラグクリア
                            if (CAppDat.DcpStatusT.ModeState[ekno, 0] != state)
                            {
                                // モード切替指示フラグが指示ありのときのみ指示なしに更新
                                if (1 == CAppDat.IopcoT.ModeKiriSiji[ekno, 0])
                                {
                                    CAppDat.IopcoT.ModeKiriSiji[ekno, 0] = 0;
                                }
                                else
                                {
                                    // 処理なし
                                }

                                // モード状態の更新
                                CAppDat.DcpStatusT.ModeState[ekno, 0] = state;
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
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DCPSTATUS].ReleaseMutex();
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
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 軌道回路状態チェック処理
        /// MODULE ID           : CheckKidouStatus
        ///
        /// PARAMETER IN        : 
        /// <param name="chkKbno">(in)チェック対象の軌道回路の区分番号</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>true=通常状態以外 / false=通常状態</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// パラメータで指定した軌道回路が通常状態以外の状態を保有するかチェックする。
        /// </summary>
        ///
        ///*******************************************************************************
        private bool CheckKidouStatus(UInt16 chkKbno)
        {
            CSettingMng setData = null;
            bool IsNonStateValue = false;
            String syncErrorMsg = String.Empty;
            String logMsg = String.Empty;
            CPartsLinkMng partslink = null;

            try
            {
                // 状態テーブルの更新条件をチェック
                foreach (UInt16 Crno in CAppDat.KidouPPLF.Keys)
                {
                    foreach (String kidoukey in CAppDat.KidouPPLF[Crno].Keys)
                    {
                        partslink = CAppDat.KidouPPLF[Crno][kidoukey];
                        if (chkKbno == partslink.Kbno)
                        {
                            // 区分番号がパラメータと一致する情報のみ対象
                            bool KeyExist = CAppDat.KidouInfoF.ContainsKey(chkKbno);
                            if (true == KeyExist)
                            {
                                // 処理なし
                            }
                            else
                            {
                                logMsg = String.Format("軌道回路設備情報ファイルに区分番号 {0} 未登録", chkKbno);
                                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }

                        // パターン別情報のチェック
                        foreach (UInt16 keyid in CAppDat.KidouInfoF[chkKbno].Pattern.Keys)
                        {
                            bool bRet = CAppDat.KidouInfoF[chkKbno].Pattern[keyid].ContainsKey(1);
                            if (true == bRet)
                            {
                                setData = CAppDat.KidouInfoF[chkKbno].Pattern[keyid][1];
                            }
                            else
                            {
                                continue;
                            }

                            foreach (UInt16 condkey in setData.Condition.Keys)
                            {
                                // 表示条件リストを取得
                                if (true == this.CheckSettingStateList(setData.Condition[condkey]))
                                {
                                    // 状態一致→[設定条件判定終了]
                                    IsNonStateValue = true;
                                    break;
                                }
                                else
                                {
                                    // 処理なし
                                }
                            }
                        }
                    }
                }
                return IsNonStateValue;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                return false;
            }
            finally
            {
                setData = null;
                partslink = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 接近てこラベルチェック処理
        /// MODULE ID           : CheckSekkinTeko
        ///
        /// PARAMETER IN        : 
        /// <param name="dcpset">(in)DCP設定情報オブジェクト</param>
        /// <param name="labelStatus">(in)状態テーブル値</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>true=接近てこラベル / false=接近てこラベルではない</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// 接近のてこラベルかどうかを判断し結果を返す。
        /// 接近てこラベルの場合は"true"、接近てこラベルではない場合は"false"を返す。
        /// </summary>
        ///
        ///*******************************************************************************
        private bool CheckSekkinTeko(CDCPSETTEIiniMng dcpset, CStatusLabelData labelStatus)
        {
            const UInt16 SEKKIN_TEKONO = 58;    // 接近てこの情報種別番号
            bool bRetValue = false;             // リターン値
            UInt16 cnt = 0;                     // ループカウンタ

            try
            {
                bRetValue = false;              // リターン値に接近てこラベルではないを設定

                if (null == dcpset)
                {
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.ERROR, MethodBase.GetCurrentMethod(), "パラメータエラー１");
                    return false;
                }
                else if (null == labelStatus)
                {
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.ERROR, MethodBase.GetCurrentMethod(), "パラメータエラー２");
                    return false;
                }
                else
                {
                    // 処理なし
                }

                for (cnt = 0; cnt < CAppDat.EkiListF.Sekkin.Count; cnt++)
                {
                    if ((CAppDat.EkiListF.Sekkin[(UInt16)(cnt + 1)].Kbno == labelStatus.KubunNo) &&
                        (SEKKIN_TEKONO == labelStatus.Kind))
                    {
                        // 接近抑止の対象区分番号が一致、表示部品の種別が「接近てこ」
                        bRetValue = true;       // リターン値に接近てこラベルを設定
                        break;
                    }
                    else
                    {
                        // 処理なし
                    }
                }

                return bRetValue;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                return false;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 接近ボタン押下接近解除チェック処理
        /// MODULE ID           : CheckSekkinOffState
        ///
        /// PARAMETER IN        : 
        /// <param name="dcpset">(in)DCP設定情報オブジェクト</param>
        /// <param name="state">(in)状態</param>
        /// <param name="labelStatus">(in)状態テーブル値</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>true=接近ボタン押下で接近解除する / false=接近ボタン押下で接近解除ではない</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// 接近ボタン押下で接近解除、かつ、接近条件がOFFになったかどうかを判断し結果を返す。
        /// 条件一致した場合（接近ボタン押下で接近解除、接近条件がOFF時）は"true"、条件不一致の場合は"false"を返す。
        /// </summary>
        ///
        ///*******************************************************************************
        private bool CheckSekkinOffState(CDCPSETTEIiniMng dcpset, UInt16 state, CStatusLabelData labelStatus)
        {
            bool bRetValue = false;             // リターン値
            bool blRet = false;                 // 戻り値取得用

            try
            {
                bRetValue = false;              // リターン値に接近ボタン押下で接近解除ではないを設定

                if (null == dcpset)
                {
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.ERROR, MethodBase.GetCurrentMethod(), "パラメータエラー１");
                    return false;
                }
                else if (null == labelStatus)
                {
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.ERROR, MethodBase.GetCurrentMethod(), "パラメータエラー２");
                    return false;
                }
                else
                {
                    // 処理なし
                }


                // 接近てこラベルかどうかをチェックする
                blRet = this.CheckSekkinTeko(dcpset, labelStatus);

                // 「接近のてこラベル」の場合
                if (true == blRet)
                {
                    // 接近解除区分が「接近ボタン押下で接近を解除する」場合
                    if (1 == CAppDat.TypeF.SekkinOffKbn)
                    {
                        if (0 == state)       // 接近条件がOFFになった場合
                        {
                            bRetValue = true; // リターン値に接近ボタン押下で接近解除するを設定
                        }
                        else                  // 接近条件がONになった場合
                        {
                            // 処理なし
                        }
                    }
                    // 上記以外の場合
                    else
                    {
                        // 処理なし
                    }
                }
                // 「接近のてこラベル」ではない場合
                else
                {
                    // 処理なし
                }

                return bRetValue;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                return false;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : ビット反転処理
        /// MODULE ID           : ReverseBitCTCInfo
        ///
        /// PARAMETER IN        : 
        /// <param name="nxCTCInfoMngT">(in)CTC状態情報</param>
        /// <param name="bitXorInfo">(in)ビット反転情報</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// ビット反転設定定数ファイルに従い、受信したCTC状態情報のビット反転を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void ReverseBitCTCInfo(CNxCTCInfoMng nxCTCInfoMngT, CBitXorInfoMng bitXorInfo)
        {
            bool    isUpdate = false;                   // 更新有無フラグ
            Int32   kosu = 0;                           // 反転情報の個数
            Int32   cnt = 0;                            // ループカウンタ
            UInt16  userHead = 0;                       // ユーザーデータヘッダサイズ
            UInt16  otherHead = 0;                      // その他ヘッダサイズ
            UInt16  headSize = 0;                       // ヘッダサイズ
            UInt16  byteinfo = 0;                       // バイト情報
            Byte    bitInfo = 0;                        // ビット情報
            Int32   posi = 0;                           // 対象位置
            UInt16  bit = 0;                            // 反転ビット
            UInt16[] aryData = null;                    // 受信情報
            String  strAppLog = String.Empty;           // ログメッセージ

            try
            {
                isUpdate = false;                       // 更新有無フラグに更新なしを設定

                userHead  = 18;                         // ユーザーデータヘッダサイズ
                otherHead = 64;                         // その他ヘッダサイズ
                headSize = (UInt16)(userHead + otherHead);
                                                        // ヘッダサイズを算出
                // CTC状態情報の受信情報を取得
                nxCTCInfoMngT.GetRecvData(ref aryData);

                // 反転情報の個数を取得する
                if (0 == aryData.Length)                // CTC状態情報のデータがない場合
                {
                    // CTC状態情報のデータがない時は反転できない為、反転情報の個数に０を設定する
                    kosu = 0;
                }
                else
                {
                    kosu = bitXorInfo.XorData.Count;    // 反転情報の個数を取得
                }

                // 反転情報の個数分ループして、CTC状態情報のビット反転を行う
                for (cnt = 0; cnt < kosu; cnt++)
                {
                    byteinfo = bitXorInfo.XorData[cnt].Byteinfo; // バイト情報を取得
                    bitInfo  = bitXorInfo.XorData[cnt].BitInfo;  // ビット情報を取得

                    if (0 == bitInfo)                   // ビット情報が０の場合
                    {
                        continue;
                    }
                    else if(byteinfo < headSize)        // バイト情報（バイト位置）がヘッダサイズより小さい場合
                    {
                        strAppLog = String.Format("ビット反転設定定数バイト情報不正：index={0:D} バイト情報={1:D} ビット情報={2:D}", cnt, byteinfo, bitInfo);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                        continue;
                    }
                    else                                // 上記以外の場合
                    {
                        // 処理なし
                    }

                    // 受信データをWORD単位で見るため、バイト位置とビット位置をWORD用に変換する
                    posi = ((byteinfo - headSize) / 2); // バイト情報（バイト位置）から WORD位置を算出
                    if ((byteinfo % 2) == 0)
                    {
                        // ビックエンディアンのデータ時のHigh側のバイト位置の場合
                        // （ここに来た時はリトルエンディアン用にLow-High逆転されているので、ビット位置はLow側にする）
                        bit = (UInt16)bitInfo;
                    }
                    else
                    {
                        // ビックエンディアンのデータ時のLow側のバイト位置の場合
                        // （ここに来た時はリトルエンディアン用にLow-High逆転されているので、ビット位置はHigh側にする）
                        bit = (UInt16)(bitInfo << 8);
                    }

                    // 反転情報の対象データのビット反転を行う
                    aryData[posi] ^= bit;

                    isUpdate = true;                    // 更新有無フラグに更新ありを設定
                }


                if (true == isUpdate)                   // 更新ありの場合
                {
                    // CTC状態情報の受信情報を設定
                    nxCTCInfoMngT.SetRecvData(aryData);
                }
                else                                    // 更新なしの場合
                {
                    // 処理なし
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
            finally
            {
                aryData = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : ビット反転処理
        /// MODULE ID           : ReverseBitTekoHyojiInfo
        ///
        /// PARAMETER IN        : 
        /// <param name="nxTekoHyojiInfoMngT">(in)てこ／表示情報</param>
        /// <param name="bitXorInfo">(in)ビット反転情報</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// ビット反転設定定数ファイルに従い、受信したてこ／表示情報のビット反転を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void ReverseBitTekoHyojiInfo(CNxTekoHyojiInfoMng nxTekoHyojiInfoMngT, CBitXorInfoMng bitXorInfo)
        {
            bool isUpdate = false;              // 更新有無フラグ
            Int32 kosu = 0;                     // 反転情報の個数
            Int32 cnt = 0;                      // ループカウンタ
            UInt16 userHead = 0;                // ユーザーデータヘッダサイズ
            UInt16 otherHead = 0;               // その他ヘッダサイズ
            UInt16 headSize = 0;                // ヘッダサイズ
            UInt16 byteinfo = 0;                // バイト情報
            Byte bitInfo = 0;                   // ビット情報
            Int32 posi = 0;                     // 対象位置
            UInt16 bit = 0;                     // 反転ビット
            UInt16[] aryData = null;            // 受信情報
            String strAppLog = String.Empty;    // ログメッセージ

            try
            {
                isUpdate = false;                       // 更新有無フラグに更新なしを設定

                userHead = 18;                         // ユーザーデータヘッダサイズ
                otherHead = 64;                         // その他ヘッダサイズ
                headSize = (UInt16)(userHead + otherHead);
                // ヘッダサイズを算出

                // てこ／表示情報の受信情報を取得
                nxTekoHyojiInfoMngT.GetRecvData(ref aryData);

                // 反転情報の個数を取得する
                if (0 == aryData.Length)                // てこ／表示情報のデータがない場合
                {
                    // てこ／表示情報のデータがない時は反転できない為、反転情報の個数に０を設定する
                    kosu = 0;
                }
                else
                {
                    kosu = bitXorInfo.XorData.Count;    // 反転情報の個数を取得
                }

                // 反転情報の個数分ループして、てこ／表示情報のビット反転を行う
                for (cnt = 0; cnt < kosu; cnt++)
                {
                    byteinfo = bitXorInfo.XorData[cnt].Byteinfo;    // バイト情報を取得
                    bitInfo = bitXorInfo.XorData[cnt].BitInfo;      // ビット情報を取得

                    if (0 == bitInfo)                   // ビット情報が０の場合
                    {
                        continue;
                    }
                    else if (byteinfo < headSize)       // バイト情報（バイト位置）がヘッダサイズより小さい場合
                    {
                        strAppLog = String.Format("ビット反転設定定数バイト情報不正：index={0:D} バイト情報={1:D} ビット情報={2:D}", cnt, byteinfo, bitInfo);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                        continue;
                    }
                    else                                // 上記以外の場合
                    {
                        // 処理なし
                    }

                    // 受信データをWORD単位で見るため、バイト位置とビット位置をWORD用に変換する
                    posi = ((byteinfo - headSize) / 2); // バイト情報（バイト位置）から WORD位置を算出
                    if ((byteinfo % 2) == 0)
                    {
                        // ビックエンディアンのデータ時のHigh側のバイト位置の場合
                        // （ここに来た時はリトルエンディアン用にLow-High逆転されているので、ビット位置はLow側にする）
                        bit = (UInt16)bitInfo;
                    }
                    else
                    {
                        // ビックエンディアンのデータ時のLow側のバイト位置の場合
                        // （ここに来た時はリトルエンディアン用にLow-High逆転されているので、ビット位置はHigh側にする）
                        bit = (UInt16)(bitInfo << 8);
                    }

                    // 反転情報の対象データのビット反転を行う
                    aryData[posi] ^= bit;

                    isUpdate = true;                    // 更新有無フラグに更新ありを設定
                }


                if (true == isUpdate)                   // 更新ありの場合
                {
                    // てこ／表示情報の受信情報を設定
                    nxTekoHyojiInfoMngT.SetRecvData(aryData);
                }
                else                                    // 更新なしの場合
                {
                    // 処理なし
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
            finally
            {
                aryData = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : ラベル解析処理
        /// MODULE ID           : LabelStatusAnalyze
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
        /// 初期起動タイプが小規模版の場合は表示てこラベル解析処理を実行する。
        /// 上記以外の場合は中規模版のラベル表示解析処理を実行する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void LabelStatusAnalyze()
        {
            UInt16  dispTekoLabelCnt = 0;           // 表示てこラベル数
            UInt16  dispLabelCnt = 0;               // 表示ラベル数

            try
            {
                dispTekoLabelCnt = CCommon.GetDispTekoLabelCount();
                dispLabelCnt = CCommon.GetDispLabelCount();

                // 表示てこラベルあり
                if (0 != dispTekoLabelCnt)
                {
                    this.DispTekoLabelStatusAnalyze();
                }
                else
                {
                    // 処理なし
                }

                // 表示ラベルあり
                if (0 != dispLabelCnt)
                {
                    this.DispLabelStatusAnalyze();
                }
                else
                {
                    // 処理なし
                }

                return;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 表示てこラベル解析処理
        /// MODULE ID           : DispTekoLabelStatusAnalyze
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
        /// 受信情報を解析して表示てこラベルパーツの現状態を判断し状態テーブルに格納する。
        /// 1件以上のレコード更新がある場合は表示てこラベル表示処理を起動する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void DispTekoLabelStatusAnalyze()
        {
            bool blRet = false;                     // 戻り値取得用
            CDCPSETTEIiniMng dcpset = null;         // DCP設定情報オブジェクト
            UInt16 updatecnt = 0;                   // 更新情報数
            UInt16 stateno = 0;                     // 状態番号
            CSettingMng setData = null;             // 設定条件情報
            bool IsAgree = false;                   // 一致フラグ
            CStatusLabelMng locallabelt = null;     // 状態テーブルコピーオブジェクト
            CPartsLinkMng partslink = null;         // リンク情報
            String syncErrorMsg = String.Empty;     // 同期エラーメッセージ
            String logMsg = String.Empty;           // ログ出力メッセージ
            CStatusLabelData work = new CStatusLabelData();     // 作業エリア

            try
            {
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DCPSET].WaitOne();
                try
                {
                    dcpset = (CDCPSETTEIiniMng)CAppDat.DCPSETTEI.Clone();
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DCPSET].ReleaseMutex();
                }

                ///////////////////////////////////////////////////////////
                // ローカルに現状の表示てこラベル状態テーブルを取得
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DISPTEKOLABELSTATUS].WaitOne();
                try
                {
                    locallabelt = (CStatusLabelMng)CAppDat.DispTekoLabelStatusT.Clone();
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DISPTEKOLABELSTATUS].ReleaseMutex();
                }

                ///////////////////////////////////////////////////////////
                // 状態テーブルの更新条件をチェック
                foreach (UInt16 Crno in CAppDat.DispTekoLabelPPLF.Keys)
                {
                    foreach (String labelkey in CAppDat.DispTekoLabelPPLF[Crno].Keys)
                    {
                        partslink = CAppDat.DispTekoLabelPPLF[Crno][labelkey];
                        work.Clear();
                        IsAgree = false;
                        stateno = 0;
                        // 設備情報ファイルにPPLファイルで設定した区分番号（キー）が存在するか？
                        // 存在する場合は続行、存在しない場合は次データに移行する
                        bool IsKeyExist = CAppDat.LabelInfoF.ContainsKey(partslink.Kbno);
                        if (IsKeyExist)
                        {
                            // 処理なし
                        }
                        else
                        {
                            //logMsg = String.Format("ppr_表示ラベル.xml 区分NO.={0} 未登録, 画面NO.={1}", partslink.Kbno, partslink.Crno);
                            //CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                            continue;
                        }

                        ///////////////////////////////////////////////////////////
                        // 表示情報の状態チェック
                        bool IsGetValue = CAppDat.LabelInfoF[partslink.Kbno].Disp.TryGetValue(0, out setData);
                        // 通常名称を取得
                        if (IsGetValue)
                        {
                            work.LabelString = setData.Name;
                        }
                        else
                        {
                            work.LabelString = String.Empty;
                        }

                        // 表示情報に属する状態数分繰り返す
                        foreach (UInt16 keyid in CAppDat.LabelInfoF[partslink.Kbno].Disp.Keys)
                        {
                            // キーIDの状態番号が０は無効とし処理なし
                            if (0 == keyid)
                            {
                                continue;
                            }
                            // キーIDの状態番号が０以外は条件情報を取得する
                            else
                            {
                                setData = CAppDat.LabelInfoF[partslink.Kbno].Disp[keyid];
                                stateno = keyid;
                            }
                            // 表示条件に属する設定条件のステータスをチェック
                            IsAgree = false;
                            // 在線不一致ラベルの場合
                            if (locallabelt.Condition[Crno][labelkey].Kind == CAppDat.DISPTEKOLABEL_NO1)
                            {
                                // 在線不一致状態確認
                                foreach (List<CSetData> setDataList in setData.Condition.Values)
                                {
                                    if (1 < setDataList.Count)
                                    {
                                        var ekno = setDataList[0].Ekino;				// 表示条件の駅No
                                        var bitname1 = setDataList[0].Kigou;			// 表示条件の記号名称1
                                        var bitname2 = setDataList[1].Kigou;			// 表示条件の記号名称2
                                        var k = new Tuple<UInt16, String, String>(ekno, bitname1, bitname2);		// キー設定
                                        if (CAppDat.Zaifustt.ContainsKey(k))										// 在線状態テーブルに表示条件に対応するデータ有
                                        {
                                            if ((stateno == 1) && (((CAppDat.Zaifustt[k] & 0x0001) == 1)))          // ラベル表示状態が表示に変化 かつ 在線不一致状態が異常
                                            {
                                                IsAgree = true;														// 表示条件が致あり
                                                break;
                                            }
                                            else if ((stateno == 2) && (((CAppDat.Zaifustt[k] & 0x0001) == 0)))     // ラベル表示状態が非表示に変化 かつ 在線不一致状態が正常
                                            {
                                                IsAgree = true;														// 表示条件が致あり
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            // 転てつ器密着ラベル または、異常検知(遅延)ラベルの場合
                            else if ((locallabelt.Condition[Crno][labelkey].Kind == CAppDat.DISPTEKOLABEL_NO2) ||
                                     (locallabelt.Condition[Crno][labelkey].Kind == CAppDat.DISPTEKOLABEL_NO3))
                            {
                                // 転てつ器密着状態確認
                                foreach (List<CSetData> setDataList in setData.Condition.Values)
                                {
                                    if (1 < setDataList.Count)
                                    {
                                        var ekno = setDataList[0].Ekino;				// 表示条件の駅No
                                        var bitname1 = setDataList[0].Kigou;			// 表示条件の記号名称1
                                        var bitname2 = setDataList[1].Kigou;			// 表示条件の記号名称2
                                        var k = new Tuple<UInt16, String, String>(ekno, bitname1, bitname2);		// 転てつ器密着状態テーブルに表示条件に対応するデータ有
                                        if (CAppDat.Tenmitustt.ContainsKey(k))
                                        {
                                            if ((stateno == 1) && (((CAppDat.Tenmitustt[k] & 0x0001) == 1)))        // ラベル表示状態が表示に変化 かつ 転てつ器密着状態が異常
                                            {
                                                IsAgree = true;														// 表示条件が致あり
                                                break;
                                            }
                                            else if ((stateno == 2) && (((CAppDat.Tenmitustt[k] & 0x0001) == 0)))   // ラベル表示状態が非表示に変化 かつ 転てつ器密着状態が正常
                                            {
                                                IsAgree = true;														// 表示条件が致あり
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            // 上記以外のラベルの場合
                            else
                            {
                                IsAgree = this.CheckSettingDictionary(setData.Condition);
                            }
                            // 表示条件が一致あり→[状態件判定終了]
                            if (IsAgree)
                            {
                                // 状態番号を取得
                                work.State = stateno;
                                // 表示文字列を取得
                                work.LabelString = setData.Name;
                                break;
                            }
                            else
                            {
                                // 処理なし
                            }
                        }

                        ///////////////////////////////////////////////////////////
                        // 前回情報と比較して状態に変化ありのときテーブル更新
                        // 状態番号、状態文字列のいずれかが不一致？
                        if ((locallabelt.Condition[Crno][labelkey].State != work.State) ||
                            (locallabelt.Condition[Crno][labelkey].LabelString != work.LabelString))
                        {
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DISPTEKOLABELSTATUS].WaitOne();
                            try
                            {
                                if (true == CAppDat.DispTekoLabelStatusT.Condition[Crno].ContainsKey(labelkey))
                                {
                                    // 接近ボタン押下で接近解除するかチェックする
                                    blRet = this.CheckSekkinOffState(dcpset, work.State, CAppDat.DispTekoLabelStatusT.Condition[Crno][labelkey]);

                                    if (true == blRet)      // 接近ボタン押下解除の場合
                                    {
                                        CAppDat.DispTekoLabelStatusT.Condition[Crno][labelkey].State = work.State;
                                        CAppDat.DispTekoLabelStatusT.Condition[Crno][labelkey].LabelString = work.LabelString;
                                    }
                                    else                    // 通常の場合
                                    {
                                        CAppDat.DispTekoLabelStatusT.Condition[Crno][labelkey].SekkinOn = work.State;
                                        CAppDat.DispTekoLabelStatusT.Condition[Crno][labelkey].State = work.State;
                                        CAppDat.DispTekoLabelStatusT.Condition[Crno][labelkey].LabelString = work.LabelString;
                                        // 状態を変化させたので変化フラグをオン
                                        CAppDat.DispTekoLabelStatusT.Condition[Crno][labelkey].IsChangeable = true;
                                        updatecnt++;
                                    }
                                }
                                else
                                {
                                    logMsg = String.Format("DispTekoLabelStatusTキー未登録 : ({0})", labelkey);
                                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
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
                                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DISPTEKOLABELSTATUS].ReleaseMutex();
                            }
                        }
                        else
                        {
                            // 処理なし
                        }
                    }
                }

                ///////////////////////////////////////////////////////////
                // １件以上の更新ありのとき、表示ラベル表示処理起動
                if (0 < updatecnt)
                {
                    CCommon.WriteDispUpdateRequest((UInt32)CAppDat.REQUESTFLG.DISPTEKOLABELDISP, 1);
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
            finally
            {
                dcpset = null;
                work = null;
                setData = null;
                locallabelt = null;
                partslink = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 接近警報解析処理
        /// MODULE ID           : SekkinAnalyze
        ///
        /// PARAMETER IN        : 
        /// <param name="sysno">(in)CTCシステム番号（1-4）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            : 
        /// <summary>
        /// ＣＴＣ状態情報を解析して接近警報出力を行う。
        /// 接近毎に「鳴動する／しない」を設定できる「接近鳴動設定機能」を追加する改修対応
        /// </summary>
        ///
        ///*******************************************************************************
        private void SekkinAnalyze(UInt16 sysno)
        {
            const UInt16 CHECK_OK = 1;              // 条件成立
            const UInt16 CHECK_NG = 2;              // 条件不成立
            bool IsAllJyoken = false;               // 全ビット情報条件成立フラグ（true=成立、false=不成立）
            UInt32 isJouhenCnt = 0;                 // 状変有カウンタ
            String logMsg = String.Empty;           // ログ出力メッセージ
            CDCPSETTEIiniMng dcpsettei = null;      // DCP_SETTEI.iniオブジェクト
            String syncErrorMsg = String.Empty;     // 同期エラーメッセージ

            try
            {
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DCPSET].WaitOne();
                try
                {
                    dcpsettei = (CDCPSETTEIiniMng)CAppDat.DCPSETTEI.Clone();
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DCPSET].ReleaseMutex();
                }

                for (UInt16 cnt = 0; cnt < CAppDat.AlarmInfoF.SekkinAlarm.Count; cnt++)
                {
                    IsAllJyoken = false;                // 全ビット情報条件成立フラグを初期化
                    isJouhenCnt = 0;                    // 状変有カウンタに０を初期化

                    if (0 == CAppDat.AlarmInfoF.SekkinAlarm[cnt].BitInfo.Count)
                    {
                        logMsg = String.Format("警報監視設定定数ファイルの接近監視情報にビット情報がない：{0:D}", cnt);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                    }
                    else
                    {
                        UInt16 uiRet;                   // 戻り値取得用
                        if (1 == CAppDat.AlarmInfoF.SekkinAlarm[cnt].BitInfo.Count)
                        {
                            uiRet = AnalyzeBitInfo(ALMBITCHKTYPE.NORMAL, CAppDat.AlarmInfoF.SekkinAlarm[cnt].BitInfo[0], sysno);
                        }
                        else
                        {
                            uiRet = AnalyzeNbitAlarmInfo(0, cnt, CAppDat.AlarmInfoF.SekkinAlarm[cnt], sysno);
                        }

                        // 条件成立の場合
                        if (CHECK_OK == uiRet)
                        {
                            isJouhenCnt++;              // 状変有カウンタをカウントＵＰ
                            IsAllJyoken = true;         // 全ビット情報条件成立フラグに成立を設定
                        }
                        // 条件不成立の場合
                        else if (CHECK_NG == uiRet)
                        {
                            isJouhenCnt++;              // 状変有カウンタをカウントＵＰ
                            IsAllJyoken = false;        // 全ビット情報条件成立フラグに不成立を設定
                        }
                        // 上記以外の場合
                        else
                        {
                            // 処理なし
                        }
                    }

                    if (0 == isJouhenCnt)
                    {
                        // 処理なし
                    }
                    else
                    {
                        // 接近解除区分が「接近ボタン押下で接近を解除する」で、情報条件成立フラグが「不成立」の場合
                        if ((1 == CAppDat.TypeF.SekkinOffKbn) && (false == IsAllJyoken))
                        {
                            // 処理なし
                        }
                        // 上記以外の場合
                        else
                        {
                            CKeihouCtrl.OutputSekkin((UInt16)cnt, IsAllJyoken);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
            finally
            {
                dcpsettei = null;
            }
            return;
        }
    }
}
