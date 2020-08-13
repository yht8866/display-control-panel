//********************************************************************************
//
//  SYSTEM          : DCP
//  UNIT            : コマンドボタン処理
//
//********************************************************************************

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Forms;
using DCP;
using DCP.MngData.File;
using DCP.MngData.Table;

namespace DCP
{
    ///*******************************************************************************
    ///
    /// CLASS NAME      : コマンドボタン処理クラス
    /// CLASS ID        : CButtonCtrl
    ///
    /// FUNCTION        : 
    /// <summary>
    /// コマンドボタン押下時の各種処理を行う。
    /// </summary>
    /// 
    ///*******************************************************************************
    class CButtonCtrl
    {
        #region <定数>

        /// <summary>キー種別</summary>
        public enum KEYTYPE : ushort
        {
            ///<summary>No.00 エリア部品（ラベル／ボタン）押下</summary>
            AREAPARTS = 0,
            ///<summary>No.01 スペースキー押下</summary>
            SPACE_KEY,
            ///<summary>No.02 マウス左ボタン押下（ＳＤＰモード）</summary>
            MOUSE_LEFT,
            ///<summary>No.03 マウス右ボタン押下（ＳＤＰモード）</summary>
            MOUSE_RIGHT,
            ///<summary>No.04 ＣＴＲＬ＋Ｓキー押下</summary>
            CTRL_S,
            ///<summary>No.05 ＣＴＲＬ＋Ｗキー押下</summary>
            CTRL_W,
            ///<summary>No.06 ＣＴＲＬ＋Ｂキー押下</summary>
            CTRL_B,
            ///<summary>No.07 ＣＴＲＬ＋Ｄキー押下</summary>
            CTRL_D,
            ///<summary>No.08 ＣＴＲＬ＋←キー押下</summary>
            CTRL_LEFT,
            ///<summary>No.09 ＣＴＲＬ＋→キー押下</summary>
            CTRL_RIGHT,
            ///<summary>No.10 （画面選択）メニュー項目押下</summary>
            SCREENMENU,
            ///<summary>No.11 （設定項目）（窓編集）（進路編集）メニュー項目押下</summary>
            SETTINGMENU,
            ///<summary>No.12 マウス右ボタン押下（ＤＣＰモード）</summary>
            MOUSE_RIGHT_DCP,
            ///<summary>No.13 ＣＴＲＬ＋↑キー押下</summary>
            CTRL_UP,
            ///<summary>No.14 ＣＴＲＬ＋↓キー押下</summary>
            CTRL_DOWN,
            ///<summary>No.15 HOMEキー押下</summary>
            HOME_KEY,
            ///<summary>No.16 BackSpaceキー押下</summary>
            BACK_KEY,
        };

        /// <summary>入力種別</summary>
        public enum INPUTTYPE : ushort
        {
            // 以下、マウス入力系
            ///<summary>マウスの左ボタンクリック</summary>
            LEFT_CLICK = 101,
            ///<summary>マウスの右ボタンクリック</summary>
            RIGHT_CLICK,
            // 以下、キーボード入力系
            ///<summary>キーボードの「Ctrl+W」キーボタンの同時押下</summary>
            CTRL_W = 1,
            ///<summary>キーボードの「Ctrl+B」キーボタンの同時押下</summary>
            CTRL_B,
            ///<summary>キーボードの「Ctrl+S」キーボタンの同時押下</summary>
            CTRL_S,
            ///<summary>キーボードの「Ctrl+D」キーボタンの同時押下</summary>
            CTRL_D,
            ///<summary>キーボードの「Ctrl+→」キーボタンの同時押下</summary>
            CTRL_RIGHT,
            ///<summary>キーボードの「Ctrl+←」キーボタンの同時押下</summary>
            CTRL_LEFT,
            ///<summary>キーボードの「スペース」キーボタンの押下</summary>
            SPACE,
            ///<summary>キーボードの「Ctrl+↑」キーボタンの同時押下</summary>
            CTRL_UP,
            ///<summary>キーボードの「Ctrl+↓」キーボタンの同時押下</summary>
            CTRL_DOWN,
            ///<summary>キーボードの「HOME」キーボタンの押下</summary>
            HOME,
            ///<summary>キーボードの「BackSpace」キーボタンの押下</summary>
            BACK,
        };

        /// <summary>遷移種別</summary>
        public enum SENIKIND : ushort
        {
            ///<summary>No.00 遷移なし</summary>
            NONE = 0,
            ///<summary>No.01 次遷移あり</summary>
            NEXTSENNI,
            ///<summary>No.02 制御次遷移あり</summary>
            SEIGYONEXTSENNI,
            ///<summary>No.03 初期メニュー</summary>
            TOPMENU,
            ///<summary>No.04 制御初期メニュー</summary>
            SEIGYOTOPMENU,
            ///<summary>No.05 １つ前に戻る</summary>
            STEPBACK,
            ///<summary>No.06 制御１つ前に戻る</summary>
            SEIGYOSTEPBACK,
            ///<summary>No.07 画面切替</summary>
            CHGGAMEN,
            ///<summary>No.08 (総数)</summary>
            SENIKINDCOUNT
        };

        /// <summary>ボタン表示種別</summary>
        public enum BTNVISITYPE : ushort
        {
             ///<summary>No.00 なし</summary>
            NONE = 0,
            ///<summary>No.01 コマンドボタン表示</summary>
            CMDBTN_VISIBLE,
            ///<summary>No.02 コマンドボタン非表示</summary>
            CMDBTN_UNVISIBLE,
            ///<summary>No.03 コマンドボタン＋画面出力ボタン表示</summary>
            CMDGAMENBTN_VISIBLE,
            ///<summary>No.04 コマンドボタン＋画面出力ボタン非表示</summary>
            CMDGAMENBTN_UNVISIBLE,
            ///<summary>No.05 画面出力ボタン表示</summary>
            GAMENBTN_VISIBLE,
            ///<summary>No.06 画面出力ボタン非表示</summary>
            GAMENBTN_UNVISIBLE,
            ///<summary>No.07 (総数)</summary>
            BTNVISITYPECOUNT
        };

        /// <summary>段階No制御種別</summary>
        public enum DKCTRLTYPE : ushort
        {
            ///<summary>No.00 制御なし</summary>
            NOCTRL = 0,
            ///<summary>No.01 制御あり</summary>
            CTRL,
            ///<summary>No.02 (総数)</summary>
            DKCTRLTYPECOUNT
        };

        #endregion

        #region <メンバ変数>

        /// <summary>反応文字列</summary>
        public static List<String> m_Hannou = new List<String>();

        #endregion

        ///*******************************************************************************
        /// MODULE NAME         : コンストラクタ
        /// MODULE ID           : CButtonCtrl
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
        public CButtonCtrl()
        {
        }

        ///*******************************************************************************
        /// MODULE NAME         : キー解析処理
        /// MODULE ID           : AnalyzeKey
        ///
        /// PARAMETER IN        : 
        /// <param name="type">(in)キー種別（詳細はenum KEYTYPEを参照）</param>
        /// <param name="areaParts">(in)エリア部品</param>
        /// <param name="dcpMode">(in)DCPモード</param>
        /// <param name="nowButtonMenuID">(in)現在表示中ボタンメニューID</param>
        /// PARAMETER OUT       : 
        /// <param name="buttonData">(out)ボタン処理情報</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>処理対象フラグ（true=対象、false=対象ではない）</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// キーを解析して処理対象を判別する。
        /// </summary>
        ///
        ///*******************************************************************************
        public static bool AnalyzeKey(UInt16 type, long areaParts, UInt16 dcpMode, UInt16 nowButtonMenuID, ref CButtonCtrlData buttonData)
        {
            CButtonFuncInfoMng  buttonFuncInfo = null;  // ボタン機能情報
            CButtonInfoMng  buttonInfo = null;          // ボタン情報
            bool    isTarget = false;                   // 処理対象フラグ（true=対象、false=対象ではない）
            UInt16  uiClickDisp = 0;                    // クリック表示フラグ（0=無し、1=履歴、2=最新警報、3=最新提案）
            CButtonCtrlData.EGAMENCHGKIND   enGamenChgKind = CButtonCtrlData.EGAMENCHGKIND.NONE;
                                                        // 画面切替種別
            CButtonCtrlData.EGAMENKIND  enGamenKind = CButtonCtrlData.EGAMENKIND.NONE;
                                                        // 画面種別指定
            UInt16  eqpActMode = 0;                     // 装置動作モード
            UInt16  areaID = 0;                         // エリアＩＤ
            UInt16  screenNo = 0;                       // スクリーンNo.
            UInt16  partsType = 0;                      // 部品種別
            UInt16  partsNo = 0;                        // 部品番号
            Int16   ret = 0;                            // 戻り値取得用
            UInt16  buttonNo = 0;                       // ボタンNo
            UInt16  buttonID = 0;                       // ボタンＩＤ
            Int32   funcnameID = -1;                    // 機能名称ＩＤ
            Int32   movementID = -1;                    // 動作名称ＩＤ
            UInt16  seniGamenNo = 0;                    // 遷移先画面No.
            UInt16  nextMenuID = 0;                     // 次メニューＩＤになしを設定
            UInt16  afterNextMenuID1 = 0;               // 処理後次メニューＩＤ１になしを設定
            UInt16  afterNextMenuID2 = 0;               // 処理後次メニューＩＤ２になしを設定
            String  strFuncName = String.Empty;         // 機能名称
            String  strMovement = String.Empty;         // 動作名称
            UInt16  buttonGuide = 0;                    // ボタンガイダンス
            String  strAppLog = String.Empty;           // ログメッセージ
            UInt16 readValue1 = 0;                      // 読込みデータ１
            UInt16 readValue2 = 0;                      // 読込みデータ２
            String syncErrorMsg = String.Empty;         // 同期エラーメッセージ
            CDCPSETTEIiniMng dcpset = null;             // ＤＣＰ設定情報
            ushort changeCrno = 0;                      // 画面遷移数

            //------------------------------------------------------------
            // 初期化
            //------------------------------------------------------------

            // 処理対象フラグに対象ではないを設定
            isTarget = false;
            // クリック表示フラグに無しを設定
            uiClickDisp = 0;
            // 機能名称ＩＤになしを設定
            funcnameID = -1;
            // 動作名称ＩＤになしを設定
            movementID = -1;
            // ボタンNoとボタンＩＤを初期化
            buttonNo = 0;
            buttonID = 0;
            // 遷移先画面No.になしを設定
            seniGamenNo = 0;
            // 次メニューＩＤになしを設定
            nextMenuID = 0;
            // 処理後次メニューＩＤ１になしを設定
            afterNextMenuID1 = 0;
            // 処理後次メニューＩＤ２になしを設定
            afterNextMenuID2 = 0;
            // 画面切替種別になしを設定
            enGamenChgKind = CButtonCtrlData.EGAMENCHGKIND.NONE;
            // 画面種別指定になしを設定
            enGamenKind = CButtonCtrlData.EGAMENKIND.NONE;
            // ボタンガイダンスになしを設定
            buttonGuide = 0;

            changeCrno = CAppDat.CHANGE_CRNO;           // 画面遷移数を設定

            try
            {
                // 装置動作モードを取得
                eqpActMode = CCommon.GetEqpActMode(dcpMode);

                // ボタンメニュー設定定数ファイルから、ボタン機能情報を取得
                buttonFuncInfo = new CButtonFuncInfoMng();
                buttonInfo = new CButtonInfoMng();

                // 部品詳細情報を取得
                if ((type == (UInt16)CButtonCtrl.KEYTYPE.SETTINGMENU) ||
                    (type == (UInt16)CButtonCtrl.KEYTYPE.SCREENMENU))
                {
                    CAreaUserControl.GetPartsDtl(areaParts, ref readValue1, ref readValue2);
                }
                else
                {
                    CAreaUserControl.GetPartsDtl(areaParts, ref areaID, ref screenNo, ref partsType, ref partsNo);
                }

                //------------------------------------------------------------
                // キーを解析する
                //------------------------------------------------------------

                //==========  キー種別がエリア部品（ラベル／ボタン）押下の場合 ==========
                if ((UInt16)KEYTYPE.AREAPARTS == type)
                {
                    //------------------------------------------------------------
                    // 該当エリアの押下された部品により、ボタン機能情報を取得する
                    //------------------------------------------------------------
                    ret = 0;

                    // 提案表示エリアの場合
                    if (CAreaUserControl.AREAID_TEIAN == areaID)
                    {
                        if (CAreaUserControl.PARTSTYPE_LABEL == partsType)
                        {
                            // 提案メッセージの場合
                            if ((UInt16)CAreaMng.TEIANLABELNO.MESSAGE == partsNo)
                            {
                                // ＤＣＰ動作設定定数ファイルの提案表示情報のエリアクリック表示が、エリアクリックでの履歴画面表示可能の場合
                                if (1 == CAppDat.TypeF.TeianDispInfo.AreaClickDisp)
                                {
                                    uiClickDisp = 1;    // クリック表示フラグに履歴を設定
                                    isTarget = true;    // 処理対象フラグに対象を設定
                                }
                                // ＤＣＰ動作設定定数ファイルの提案表示情報のエリアクリック表示が、エリアクリックでの最新警報画面表示可能の場合
                                else if (2 == CAppDat.TypeF.TeianDispInfo.AreaClickDisp)
                                {
                                    uiClickDisp = 3;    // クリック表示フラグに最新提案を設定
                                    isTarget = true;    // 処理対象フラグに対象を設定
                                }
                                // ＤＣＰ動作設定定数ファイルの提案表示情報のエリアクリック表示が、エリアクリックでの履歴画面表示不可の場合
                                else
                                {
                                    // 処理なし
                                }
                            }
                            // 警報メッセージ以外の場合
                            else
                            {
                                // 処理なし
                            }
                        }
                        else if (CAreaUserControl.PARTSTYPE_BUTTON == partsType)
                        {
                            // 承認／拒否ボタンの場合
                            if (((UInt16)CAreaMng.TEIANBUTTONNO.SYOUNIN == partsNo) || ((UInt16)CAreaMng.TEIANBUTTONNO.KYOHI == partsNo))
                            {
                                // 機能名称ＩＤを設定する
                                funcnameID = (Int32)CAppDat.FUNCID.TEIAN;

                                // 動作名称ＩＤを設定する
                                if ((UInt16)CAreaMng.TEIANBUTTONNO.SYOUNIN == partsNo)
                                {
                                    movementID = (Int32)CAppDat.MOVEID.SETTEI;
                                }
                                else
                                {
                                    movementID = (Int32)CAppDat.MOVEID.RELEASE;
                                }

                                isTarget = true;        // 処理対象フラグに対象を設定
                            }
                            // 画面ボタンの場合
                            else if ((UInt16)CAreaMng.TEIANBUTTONNO.GAMEN == partsNo)
                            {
                                UInt16 screenno = 0xFFFF;
                                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.TEIANKANRI].WaitOne();
                                try
                                {
                                    if (0 < CAppDat.TeianKanriT.Teian.Count)
                                    {
                                        UInt16 ekno = CAppDat.TeianKanriT.Teian[0].TouEkiNo;
                                        foreach (CEkiConvMng ekiData in CAppDat.EkiConvF.Values)
                                        {
                                            if (ekno == ekiData.EkiNoCon.Dcpeki)
                                            {
                                                screenno = (UInt16)ekiData.ScreenNo;
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
                                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.TEIANKANRI].ReleaseMutex();
                                }
                                // 画面切替要求
                                if (CAppDat.AnsWaitFlag == false)        // アンサ待ち中ではない
                                {
                                    IntPtr wParam = new IntPtr((UInt16)CAppDat.UPDATEREQUEST.RAILWAYCHANGE_TEIAN);
                                    IntPtr lParam = new IntPtr((UInt16)screenno);
                                    CCommon.LocalPostMessage(true, CAppDat.WM_USER_MAINFORMUPDATE, wParam, lParam);
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
                        else
                        {
                            // 処理なし
                        }
                    }
                    // 警報表示エリアの場合
                    else if (CAreaUserControl.AREAID_KEIHOU == areaID)
                    {
                        if (CAreaUserControl.PARTSTYPE_LABEL == partsType)
                        {
                            // 警報メッセージの場合
                            if ((UInt16)CAreaMng.KEIHOULABELNO.MESSAGE == partsNo)
                            {
                                // ＤＣＰ動作設定定数ファイルの警報表示情報のエリアクリック表示が、エリアクリックでの履歴画面表示可能の場合
                                if (1 == CAppDat.TypeF.KeihouDispInfo.AreaClickDisp)
                                {
                                    uiClickDisp = 1;    // クリック表示フラグに履歴を設定
                                    isTarget = true;    // 処理対象フラグに対象を設定
                                }
                                // ＤＣＰ動作設定定数ファイルの警報表示情報のエリアクリック表示が、エリアクリックでの最新警報画面表示可能の場合
                                else if (2 == CAppDat.TypeF.KeihouDispInfo.AreaClickDisp)
                                {
                                    uiClickDisp = 2;    // クリック表示フラグに最新警報を設定
                                    isTarget = true;    // 処理対象フラグに対象を設定
                                }
                                // ＤＣＰ動作設定定数ファイルの警報表示情報のエリアクリック表示が、エリアクリックでの履歴画面表示不可の場合
                                else
                                {
                                    // 処理なし
                                }
                            }
                            // 警報メッセージ以外の場合
                            else
                            {
                                // 処理なし
                            }
                        }
                        else if (CAreaUserControl.PARTSTYPE_BUTTON == partsType)
                        {
                            // 確認ボタンの場合
                            if ((UInt16)CAreaMng.KEIHOUBUTTONNO.KAKUNIN == partsNo)
                            {
                                // 機能名称ＩＤ、動作名称ＩＤをセット
                                funcnameID = (Int32)CAppDat.FUNCID.KEIHOU_KAKUNIN;
                                movementID = (Int32)CAppDat.MOVEID.NONE;
                                isTarget = true;        // 処理対象フラグに対象を設定
                            }
                            // 鳴止ボタンの場合
                            else if ((UInt16)CAreaMng.KEIHOUBUTTONNO.NARIDOME == partsNo)
                            {
                                // 機能名称ＩＤ、動作名称ＩＤをセット
                                funcnameID = (Int32)CAppDat.FUNCID.MEIDOU_TEISHI;
                                movementID = (Int32)CAppDat.MOVEID.NONE;
                                isTarget = true;        // 処理対象フラグに対象を設定
                            }
                            // 画面ボタンの場合
                            else if ((UInt16)CAreaMng.KEIHOUBUTTONNO.GAMEN == partsNo)
                            {
                                UInt16 screenno = 0xFFFF;
                                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.KEIHOUKANRI].WaitOne();
                                try
                                {
                                    if (0 < CAppDat.KeihouMsgT.Keihou.Count)
                                    {
                                        UInt16 ekno = CAppDat.KeihouMsgT.Keihou[0].TouEkiNo;
                                        foreach (CEkiConvMng ekiData in CAppDat.EkiConvF.Values)
                                        {
                                            if (ekno == ekiData.EkiNoCon.Dcpeki)
                                            {
                                                screenno = (UInt16)ekiData.ScreenNo;
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
                                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.KEIHOUKANRI].ReleaseMutex();
                                }
                                // 画面切替要求
                                // アンサ待ち中ではない、かつ、
                                if ((CAppDat.AnsWaitFlag == false) && 
                                // 段階Noが初期状態である、もしくは、段階Noは初期状態ではないが、扱い中画面遷移が可能な場合
                                    ((CAppDat.IopcoT.DkNo == 0) || ((CAppDat.TypeF.AtsukaiGamenSeni == (UInt16)CAppDat.ENFLAG.FLAG_ON) && (CAppDat.IopcoT.DkNo != 0))))
                                {
                                    CAppDat.FormMain.EventStatus_Initialize();

                                    IntPtr wParam = new IntPtr((UInt16)CAppDat.UPDATEREQUEST.RAILWAYCHANGE_KEIHOU);
                                    IntPtr lParam = new IntPtr((UInt16)screenno);
                                    CCommon.LocalPostMessage(true, CAppDat.WM_USER_MAINFORMUPDATE, wParam, lParam);
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
                        else
                        {
                            // 処理なし
                        }
                    }
                    // 扱い警報表示エリアの場合
                    else if (CAreaUserControl.AREAID_ATSUKAI == areaID)
                    {
                        if (CAreaUserControl.PARTSTYPE_LABEL == partsType)
                        {
                            // 扱い警報メッセージの場合
                            if ((UInt16)CAreaMng.ATSUKAIKEIHOULABELNO.MESSAGE == partsNo)
                            {
                                // ＤＣＰ動作設定定数ファイルの扱い警報表示情報のエリアクリック表示が、エリアクリックでの履歴画面表示可能の場合
                                if (1 == CAppDat.TypeF.AtsukaiKeihouDispInfo.AreaClickDisp)
                                {
                                    uiClickDisp = 1;    // クリック表示フラグに履歴を設定
                                    isTarget = true;    // 処理対象フラグに対象を設定
                                }
                                // ＤＣＰ動作設定定数ファイルの扱い警報表示情報のエリアクリック表示が、エリアクリックでの最新扱い警報画面表示可能の場合
                                else if (2 == CAppDat.TypeF.AtsukaiKeihouDispInfo.AreaClickDisp)
                                {
                                    uiClickDisp = 4;    // クリック表示フラグに最新扱い警報を設定
                                    isTarget = true;    // 処理対象フラグに対象を設定
                                }
                                // ＤＣＰ動作設定定数ファイルの扱い警報表示情報のエリアクリック表示が、エリアクリックでの履歴画面表示不可の場合
                                else
                                {
                                    // 処理なし
                                }
                            }
                            // 警報メッセージ以外の場合
                            else
                            {
                                // 処理なし
                            }
                        }
                        else if (CAreaUserControl.PARTSTYPE_BUTTON == partsType)
                        {
                            // 確認ボタンの場合
                            if ((UInt16)CAreaMng.ATSUKAIKEIHOUBUTTONNO.KAKUNIN == partsNo)
                            {
                                // 機能名称ＩＤ、動作名称ＩＤをセット
                                funcnameID = (Int32)CAppDat.FUNCID.ATSUKAIKEIHOU_KAKUNIN;
                                movementID = (Int32)CAppDat.MOVEID.NONE;
                                isTarget = true;        // 処理対象フラグに対象を設定
                            }
                            // 鳴止ボタンの場合
                            else if ((UInt16)CAreaMng.ATSUKAIKEIHOUBUTTONNO.NARIDOME == partsNo)
                            {
                                // 機能名称ＩＤ、動作名称ＩＤをセット
                                funcnameID = (Int32)CAppDat.FUNCID.MEIDOU_TEISHI;
                                movementID = (Int32)CAppDat.MOVEID.NONE;
                                isTarget = true;        // 処理対象フラグに対象を設定
                            }
                        }
                        else
                        {
                            // 処理なし
                        }
                    }
                    // メニューボタン表示エリアまたは操作ボタン表示エリアの場合
                    else if (CAreaUserControl.AREAID_MENU == areaID || CAreaUserControl.AREAID_CONTROL == areaID)
                    {
                        // ボタンの場合
                        if (CAreaUserControl.PARTSTYPE_BUTTON == partsType)
                        {
                            // ボタンメニューＩＤ存在チェック
                            if (true == CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo.ContainsKey(nowButtonMenuID))
                            {   // ボタンメニューＩＤがある場合

                                // エリアＩＤ存在チェック
                                if (areaID == CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo[nowButtonMenuID].AreaId)
                                {   // エリアＩＤが同一の場合

                                    // ボタンＮＯ存在フラグ
                                    bool bButtonNOExists = false;

                                    // ボタンガイダンスID
                                    ushort guideNum = 0;

                                    // サブメニュー情報取得
                                    foreach (CButtonSubMenuMng subMenuData in CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo[nowButtonMenuID].SubMenuInfo.Values)
                                    {
                                        guideNum = subMenuData.GuideMsgId;

                                        // ボタン情報リストの取得
                                        foreach (CButtonInfoMng bid in subMenuData.ButtonInfo)
                                        {
                                            // ボタンＮＯが一致した場合
                                            if (bid.ButtonNo == partsNo)
                                            {
                                                bButtonNOExists = true;
                                                break;
                                            }
                                        }

                                        // ボタンＮＯが一致した場合
                                        if (true == bButtonNOExists)
                                        {
                                            break;
                                        }
                                    }

                                    if (true == bButtonNOExists)
                                    {   // 該当ボタンＮＯがあった場合

                                        // エリアポインタ
                                        List<CAreaMng> area = null;

                                        // ボタン変更フラグ：変更なし
                                        bool bChangeButton = false;

                                        // ボタン再選択フラグ
                                        CAppDat.RePressButton = false;

                                        if (CAreaUserControl.AREAID_MENU == areaID)
                                        {   // メニューボタン表示エリアの場合

                                            // メニューボタン表示エリアポインタを取得する
                                            area = CAppDat.AreaKanriT.AreaData[CAreaUserControl.AREANAME_MENU];
                                        }
                                        else if (CAreaUserControl.AREAID_CONTROL == areaID)
                                        {   // 操作ボタン表示エリアの場合

                                            // 操作ボタン表示エリアポインタを取得する
                                            area = CAppDat.AreaKanriT.AreaData[CAreaUserControl.AREANAME_CONTOROL];

                                            // ボタン変更フラグ：変更あり
                                            bChangeButton = true;
                                        }

                                        // ボタン機能情報取得処理
                                        ret = GetButtonFuncInfo(screenNo, partsNo, nowButtonMenuID, ref buttonID, ref buttonFuncInfo, ref buttonInfo);

                                        // 機能名称が画面出力ではない場合
                                        if (buttonFuncInfo.FuncName != CAppDat.FuncName[(UInt16)CAppDat.FUNCID.GAMEN_OUTPUT])
                                        {
                                            // アンサ待ち状態の場合、操作無効
                                            if (CAppDat.AnsWaitFlag == true)
                                            {
                                                return false;
                                            }
                                        }

                                        // ボタン変更フラグ：変更ありの場合
                                        if (true == bChangeButton)
                                        {
                                            // ボタンが押下された状態の場合、ボタンの状態を元に戻して処理を終了
                                            if (true == area[(UInt16)(screenNo - 1)].AreaPointer.GetButtonPressed(buttonInfo.ButtonNo))
                                            {
                                                // ボタン再選択で取消可の場合
                                                if(CAppDat.TypeF.ReSelectClear == (UInt16) CAppDat.ENFLAG.FLAG_ON)
                                                {
                                                    String strForeColor = String.Empty;     // 表示色
                                                    String strBackColor = String.Empty;     // 背景色

                                                    // 選択済みボタン情報の削除
                                                    for (int i = CAppDat.IopcoT.SelectedBtnList.Count - 1 ; i >= 0 ; i--)
                                                    {
                                                        // 再選択ボタンの場合
                                                        if ((CAppDat.IopcoT.SelectedBtnList[i].BtnID == buttonID) && (CAppDat.IopcoT.SelectedBtnList[i].BtnNO == partsNo))
                                                        {
                                                            CAppDat.RePressButton = true;
                                                        }

                                                        // 色設定
                                                        strForeColor = CAppDat.ButtonMenuF.ButtonFuncDef.ButtonFuncInfo[CAppDat.IopcoT.SelectedBtnList[i].BtnID].ForeColor;     // 表示色：通常時と同じ
                                                        strBackColor = CAppDat.ButtonMenuF.ButtonFuncDef.ButtonFuncInfo[CAppDat.IopcoT.SelectedBtnList[i].BtnID].BackColor;     // 背景色：通常時と同じ

                                                        // ボタン色設定
                                                        area[(UInt16)(screenNo - 1)].AreaPointer.SetButtonColor(CAppDat.IopcoT.SelectedBtnList[i].BtnNO, 1, strForeColor);
                                                        area[(UInt16)(screenNo - 1)].AreaPointer.SetButtonColor(CAppDat.IopcoT.SelectedBtnList[i].BtnNO, 2, strBackColor);

                                                        // リストから削除
                                                        CAppDat.IopcoT.SelectedBtnList.RemoveAt(i);

                                                        // 再選択ボタンの場合
                                                        if (CAppDat.RePressButton == true)
                                                        {
                                                            break;
                                                        }
                                                    }
                                                }

                                                // ボタン状態変更フラグ：変更なしに設定
                                                bChangeButton = false;
                                            }
                                        }

                                        // 該当ボタン機能情報あり、且つ、押下状態ではない場合
                                        if (0 == ret && false == area[(UInt16)(screenNo - 1)].AreaPointer.GetButtonPressed(buttonInfo.ButtonNo))
                                        {
                                            // 通常
                                            if (CAppDat.RePressButton == false)
                                            {
                                                // ボタンNoを設定
                                                buttonNo = partsNo;

                                                // ボタン機能情報から、機能名称と動作名称と遷移先画面No.を取得する
                                                strFuncName = buttonFuncInfo.FuncName;
                                                strMovement = buttonFuncInfo.Movement;
                                                seniGamenNo = buttonFuncInfo.SeniGamenNo;

                                                // ボタンメニュー情報から次メニューのＩＤを取得する
                                                nextMenuID = buttonInfo.NextMenu;

                                                // ボタンメニュー情報から処理後次メニューのＩＤを取得する
                                                afterNextMenuID1 = buttonInfo.AfterNextMenu1;
                                                afterNextMenuID2 = buttonInfo.AfterNextMenu2;

                                                // ボタンガイダンスを取得する
                                                buttonGuide = buttonInfo.ButtonGuide;

                                                // 機能名称ＩＤを取得する
                                                funcnameID = CCommon.GetFuncId(strFuncName);
                                                // 動作名称ＩＤを取得する
                                                movementID = CCommon.GetMoveId(strMovement);
                                            }
                                            // 操作が戻る場合
                                            else
                                            {
                                                // ボタンNoを設定
                                                buttonNo = partsNo;

                                                seniGamenNo = 0;                // 遷移先画面No.
                                                afterNextMenuID1 = 0;           // 処理後次メニューＩＤ
                                                afterNextMenuID2 = 0;

                                                // 初期メニューに戻る、ではない場合
                                                if (CAppDat.IopcoT.SelectedBtnList.Count != 0)
                                                {
                                                    // 選択済みボタン情報の取得
                                                    CSelectedBtn beforBtn = CAppDat.IopcoT.SelectedBtnList[CAppDat.IopcoT.SelectedBtnList.Count - 1];

                                                    buttonNo = beforBtn.BtnNO;          // ボタンNo
                                                    nextMenuID = beforBtn.MenuID;       // 次メニューＩＤ
                                                    buttonGuide = beforBtn.GuideID;     // ボタンガイダンス

                                                    // ボタン機能情報から、機能名称を取得する
                                                    strFuncName = buttonFuncInfo.FuncName;
                                                    // 動作名称ＩＤに「初期メニュー」を設定
                                                    strMovement = CAppDat.FuncName[(Int32)CAppDat.MOVEID.INITIAL];

                                                    // 機能名称ＩＤを取得する
                                                    funcnameID = CCommon.GetFuncId(strFuncName);
                                                    // 動作名称ＩＤに「初期メニュー」を設定
                                                    movementID = (Int32)CAppDat.MOVEID.INITIAL;
                                                }
                                                // 初期メニューに戻る場合
                                                else
                                                {
                                                    buttonNo = partsNo;     // ボタンNo
                                                    nextMenuID = CAppDat.ButtonMenuTreeT[0].ButtonMenuIDList[CAreaUserControl.AREAID_CONTROL];     // 次メニューＩＤ
                                                    buttonGuide = 0;        // ボタンガイダンス

                                                    // 機能名称ＩＤに「取消」を設定
                                                    funcnameID = (Int32)CAppDat.FUNCID.TORIKESHI;
                                                    // 動作名称ＩＤに「初期メニュー」を設定
                                                    movementID = (Int32)CAppDat.MOVEID.INITIAL;
                                                }
                                            }

                                            if ((-1 == funcnameID) || (-1 == movementID))
                                            {   // 機能名称ＩＤまたは動作名称ＩＤが取得できなかった場合

                                                strAppLog = String.Format("機能名称ＩＤ／動作名称ＩＤの取得失敗：strFuncName={0} strMovement={1}", strFuncName, strMovement);
                                                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                                            }
                                            else
                                            {   // 機能名称ＩＤと動作名称ＩＤが取得できた場合

                                                isTarget = true;    // 処理対象フラグに対象を設定

                                                // ボタン変更フラグ：変更ありの場合
                                                if (true == bChangeButton)
                                                {
                                                    // ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
                                                    // 特情機能定義による処理分岐
                                                    // ※他のコマンドボタンクリックし、てこラベル表示中に誘導信号機(メニュー機能No81)、列番修正(メニュー機能No82)の
                                                    // コマンドボタンをクリックした際、他のてこラベルが表示されたままになる障害対応。
                                                    // 誘導信号機と列番修正のコマンドボタンは、次に表示される場内誘導、列番書込等をクリックしないとてこラベルがリセットされない為。
                                                    // 別の線区で同じ障害が出た場合、条件式にメニュー機能Noを追加して対応する。

                                                    bool blGoNextMenu = true;           // 次メニュー遷移する
                                                    if (CAppDat.KOTODEN)                // 琴電特情処理
                                                    {
                                                        // ボタンメニュー遷移なし、もしくは次ボタンメニューに押下したボタンと同じID、かつNOのボタンが存在する場合
                                                        if ((buttonInfo.NextMenu == 0) || (buttonFuncInfo.MenuFuncNo == 81) || (buttonFuncInfo.MenuFuncNo == 82) ||
                                                            (true == CCommon.CheckButtonMenuContainIdNo(buttonInfo.NextMenu, CAreaUserControl.AREAID_CONTROL, buttonInfo.ButtonId, buttonInfo.ButtonNo)))
                                                        {
                                                            blGoNextMenu = false;       // 次メニュー遷移しない
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // ボタンメニュー遷移なし、もしくは次ボタンメニューに押下したボタンと同じID、かつNOのボタンが存在する場合
                                                        if ((buttonInfo.NextMenu == 0) ||
                                                            (true == CCommon.CheckButtonMenuContainIdNo(buttonInfo.NextMenu, CAreaUserControl.AREAID_CONTROL, buttonInfo.ButtonId, buttonInfo.ButtonNo)))
                                                        {
                                                            blGoNextMenu = false;       // 次メニュー遷移しない
                                                        }
                                                    }

                                                    if (false == blGoNextMenu)          // 次メニュー遷移しない場合
                                                    {
                                                        // 色設定
                                                        String strForeColor = String.Empty;      // 表示色
                                                        String strBackColor = String.Empty;      // 背景色

                                                        // ボタン間の切替が可能の場合
                                                        if (CAppDat.TypeF.DiffButtonChange == (UInt16)CAppDat.ENFLAG.FLAG_ON)
                                                        {
                                                            for (int i = 0; i < CAppDat.IopcoT.SelectedBtnList.Count; i++)
                                                            {
                                                                // 段階NOが同列以上の処理があるならフラグを立てる
                                                                if ((false == CAppDat.ExcuteClearIniFunc) && (CAppDat.IopcoT.SelectedBtnList[i].DkNO == buttonFuncInfo.DkNo))
                                                                {
                                                                    CAppDat.ExcuteClearIniFunc = true;
                                                                }

                                                                if (true == CAppDat.ExcuteClearIniFunc)
                                                                {
                                                                    // 色設定
                                                                    strForeColor = CAppDat.ButtonMenuF.ButtonFuncDef.ButtonFuncInfo[CAppDat.IopcoT.SelectedBtnList[i].BtnID].ForeColor;     // 表示色：通常時と同じ
                                                                    strBackColor = CAppDat.ButtonMenuF.ButtonFuncDef.ButtonFuncInfo[CAppDat.IopcoT.SelectedBtnList[i].BtnID].BackColor;     // 背景色：通常時と同じ

                                                                    // ボタン色設定
                                                                    area[(UInt16)(screenNo - 1)].AreaPointer.SetButtonColor(CAppDat.IopcoT.SelectedBtnList[i].BtnNO, 1, strForeColor);
                                                                    area[(UInt16)(screenNo - 1)].AreaPointer.SetButtonColor(CAppDat.IopcoT.SelectedBtnList[i].BtnNO, 2, strBackColor);

                                                                    CAppDat.IopcoT.SelectedBtnList.RemoveAt(i--);
                                                                }
                                                            }
                                                        }

                                                        // ボタン間の切替が可能の場合、もしくは、ボタン間の切替が不可かつ選択済みリストがない、かつ段階NOが0の場合
                                                        if ((CAppDat.TypeF.DiffButtonChange == (UInt16)CAppDat.ENFLAG.FLAG_ON) ||
                                                            ((CAppDat.TypeF.DiffButtonChange == (UInt16)CAppDat.ENFLAG.FLAG_OFF) &&
                                                                                     (0 == CAppDat.IopcoT.SelectedBtnList.Count) && (CAppDat.IopcoT.DkNo == 0)))
                                                        {
                                                            strForeColor = buttonFuncInfo.PressedForeColor;      // 押下時表示色を指定
                                                            strBackColor = buttonFuncInfo.PressedBackColor;      // 押下時背景色を指定

                                                            // ボタン色設定（押下状態にする）
                                                            area[(UInt16)(screenNo - 1)].AreaPointer.SetButtonColor(buttonInfo.ButtonNo, 3, strForeColor);
                                                            area[(UInt16)(screenNo - 1)].AreaPointer.SetButtonColor(buttonInfo.ButtonNo, 4, strBackColor);

                                                            // ボタンガイダンスの設定
                                                            if (buttonInfo.ButtonGuide != 0)
                                                            {
                                                                // ボタンガイダンスIDの取得
                                                                guideNum = buttonInfo.ButtonGuide;
                                                            }

                                                            // 選択済みボタン情報の登録
                                                            CAppDat.IopcoT.SelectedBtnList.Add(new CSelectedBtn(buttonFuncInfo.DkNo, buttonInfo.ButtonId, buttonInfo.ButtonNo, buttonInfo.NextMenu, guideNum));
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {   // 該当ボタン機能情報なしの場合
                                            strAppLog = String.Format("該当ボタン機能情報なし：{0:D}, {1:D}, {2:D}", ret, screenNo, buttonInfo.ButtonNo);
                                            CCommon.WriteDebugLog(MethodBase.GetCurrentMethod(), strAppLog);
                                        }
                                    }
                                    else
                                    {   // 該当ボタンＮＯなしの場合
                                        // 処理なし
                                    }
                                }
                                else
                                {   // エリアＩＤが異なっていた場合
                                    // 処理なし
                                }
                            }
                            else
                            {   // 該当ボタンメニューＩＤなしの場合
                                // 処理なし
                            }
                        }
                        else
                        {
                            // ボタンではない場合
                        }
                    }
                    // 装置状態表示エリアの場合
                    else if (CAreaUserControl.AREAID_SOUTI == areaID)
                    {
                        if (CAreaUserControl.PARTSTYPE_LABEL == partsType)
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
                                dcpset = new CDCPSETTEIiniMng();
                            }
                            finally
                            {
                                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DCPSET].ReleaseMutex();
                            }
                            if ((UInt16)CAppDat.ENFLAG.FLAG_ON == CAppDat.TypeF.YokLabelEnable)
                            {
                                if ((UInt16)CAreaMng.EQPLABELNO.ALARMSTOP == partsNo)
                                {
                                    // 警報抑止表示エリア
                                    funcnameID = (UInt16)CAppDat.FUNCID.KEIHOU_YOKUSHI;
                                    if ((Byte)CDCPSETTEIiniMng.ALARM.ALARM == dcpset.AlarmStop)
                                    {
                                        movementID = (UInt16)CAppDat.MOVEID.SETTEI;
                                    }
                                    else
                                    {
                                        movementID = (UInt16)CAppDat.MOVEID.RELEASE;
                                    }
                                    isTarget = true;                    // 処理対象フラグに対象を設定
                                }
                                else if ((UInt16)CAreaMng.EQPLABELNO.SEKKINSTOP == partsNo)
                                {
                                    // 接近抑止表示エリア
                                    funcnameID = (UInt16)CAppDat.FUNCID.SEKKIN_YOKUSHI;
                                    if ((Byte)CDCPSETTEIiniMng.YOKUSHI.NON == dcpset.SekkinStop)
                                    {
                                        movementID = (UInt16)CAppDat.MOVEID.SETTEI;
                                    }
                                    else
                                    {
                                        movementID = (UInt16)CAppDat.MOVEID.RELEASE;
                                    }
                                    isTarget = true;                    // 処理対象フラグに対象を設定
                                }
                                else if ((UInt16)CAreaMng.EQPLABELNO.TEIANSTOP == partsNo)
                                {
                                    // 提案抑止表示エリア
                                    funcnameID = (UInt16)CAppDat.FUNCID.TEIAN_YOKUSHI;
                                    if ((Byte)CDCPSETTEIiniMng.YOKUSHI.NON == dcpset.TeianStop)
                                    {
                                        movementID = (UInt16)CAppDat.MOVEID.SETTEI;
                                    }
                                    else
                                    {
                                        movementID = (UInt16)CAppDat.MOVEID.RELEASE;
                                    }
                                    isTarget = true;                    // 処理対象フラグに対象を設定
                                }
                                else if ((UInt16)CAreaMng.EQPLABELNO.GENBAYOUSEISTOP == partsNo)
                                {
                                    // 現場要請抑止表示エリア
                                    funcnameID = (UInt16)CAppDat.FUNCID.GENBAYOUSEI_YOKUSHI;
                                    if ((Byte)CDCPSETTEIiniMng.YOKUSHI.NON == dcpset.GenbayouseiStop)
                                    {
                                        movementID = (UInt16)CAppDat.MOVEID.SETTEI;
                                    }
                                    else
                                    {
                                        movementID = (UInt16)CAppDat.MOVEID.RELEASE;
                                    }
                                    isTarget = true;                    // 処理対象フラグに対象を設定
                                }
                                else if ((UInt16)CAreaMng.EQPLABELNO.SOUNDSTOP == partsNo)
                                {
                                    // 鳴動抑止表示エリア
                                    funcnameID = (UInt16)CAppDat.FUNCID.SOUND_YOKUSHI;
                                    if ((Byte)CDCPSETTEIiniMng.YOKUSHI.NON == dcpset.SoundStop)
                                    {
                                        movementID = (UInt16)CAppDat.MOVEID.SETTEI;
                                    }
                                    else
                                    {
                                        movementID = (UInt16)CAppDat.MOVEID.RELEASE;
                                    }
                                    isTarget = true;                    // 処理対象フラグに対象を設定
                                }
                                else
                                {
                                    // 処理なし
                                }
                            }
                        }
                    }

                    // クリック表示フラグが履歴の場合
                    if (1 == uiClickDisp)
                    {
                        // 機能名称ＩＤ（警報履歴）を設定する
                        funcnameID = (Int32)CAppDat.FUNCID.KEIHOU_RIREKI;

                        isTarget = true;                // 処理対象フラグに対象を設定
                    }
                    // クリック表示フラグが最新警報の場合
                    else if (2 == uiClickDisp)
                    {
                        // 機能名称ＩＤ（最新警報）を設定する
                        funcnameID = (Int32)CAppDat.FUNCID.NEWALARMDISP;

                        isTarget = true;                // 処理対象フラグに対象を設定
                    }
                    // クリック表示フラグが最新提案の場合
                    else if (3 == uiClickDisp)
                    {
                        // 機能名称ＩＤ（最新提案）を設定する
                        funcnameID = (Int32)CAppDat.FUNCID.NEWTEIANDISP;

                        isTarget = true;                // 処理対象フラグに対象を設定
                    }
                    // クリック表示フラグが最新扱い警報の場合
                    else if (4 == uiClickDisp)
                    {
                        // 機能名称ＩＤ（最新扱い警報）を設定する
                        funcnameID = (Int32)CAppDat.FUNCID.NEWATSUKAIALARMDISP;

                        isTarget = true;                // 処理対象フラグに対象を設定
                    }
                    // クリック表示フラグが無しの場合
                    else
                    {
                        // 処理なし
                    }
                }
                //========== キー種別がスペースキー押下の場合 ==========
                else if ((UInt16)KEYTYPE.SPACE_KEY == type)
                {
                    // 機能名称ＩＤ（警報確認）を設定する
                    funcnameID = (Int32)CAppDat.FUNCID.KEIHOU_KAKUNIN;

                    isTarget = true;                    // 処理対象フラグに対象を設定
                }
                //========== キー種別がマウス左ボタン押下の場合 ==========
                else if ((UInt16)KEYTYPE.MOUSE_LEFT == type)
                {
                    // マウス画面切替有無が有りの場合
                    if (CAppDat.FLAG_ARI == CAppDat.TypeF.MouseGamenChange)
                    {
                        // 機能名称ＩＤ（前画面移動）を設定する
                        funcnameID = (Int32)CAppDat.FUNCID.PREV_GAMEN;
                        // 画面切替種別にマウスを設定
                        enGamenChgKind = CButtonCtrlData.EGAMENCHGKIND.MOUSE;
                        // 画面種別指定に同種別指定を設定
                        enGamenKind = CButtonCtrlData.EGAMENKIND.SAMEGAMEN;

                        isTarget = true;                // 処理対象フラグに対象を設定
                    }
                    // 上記以外の場合
                    else
                    {
                        // 処理なし
                    }
                }
                //========== キー種別がマウス右ボタン押下の場合 ==========
                else if ((UInt16)KEYTYPE.MOUSE_RIGHT == type)
                {
                    // マウス画面切替有無が有りの場合
                    if (CAppDat.FLAG_ARI == CAppDat.TypeF.MouseGamenChange)
                    {
                        // 機能名称ＩＤ（次画面移動）を設定する
                        funcnameID = (Int32)CAppDat.FUNCID.NEXT_GAMEN;
                        // 画面切替種別にマウスを設定
                        enGamenChgKind = CButtonCtrlData.EGAMENCHGKIND.MOUSE;
                        // 画面種別指定に同種別指定を設定
                        enGamenKind = CButtonCtrlData.EGAMENKIND.SAMEGAMEN;

                        isTarget = true;                    // 処理対象フラグに対象を設定
                    }
                    // 右クリックで付箋処理を呼び出す仕様、かつ、付箋機能が利用可能な場合
                    else if (((UInt16)CAppDat.RIGHTCLICKACTION.CALL_STICKYNOTES == CAppDat.TypeF.RightClickAction) &&
                             ((UInt16)CAppDat.ENFLAG.FLAG_ON == CAppDat.TypeF.StickyNotes))
                    {
                        funcnameID = (Int32)CAppDat.FUNCID.STICKYNOTE_MENU;
                        movementID = (Int32)CAppDat.MOVEID.NONE;
                        isTarget = true;                    // 処理対象フラグに対象を設定
                    }
                    // 上記以外の場合
                    else
                    {
                        // 処理なし
                    }
                }
                //========== キー種別がＣＴＲＬ＋Ｓキー押下の場合 ==========
                else if ((UInt16)KEYTYPE.CTRL_S == type)
                {
                    // 機能名称ＩＤ（画面１移動）を設定する
                    funcnameID = (Int32)CAppDat.FUNCID.GAMEN_1;
                    // 画面切替種別にキーボードを設定
                    enGamenChgKind = CButtonCtrlData.EGAMENCHGKIND.KEYBOARD;
                    // 画面種別指定に個別画面を設定
                    enGamenKind = CButtonCtrlData.EGAMENKIND.KOBETU;

                    isTarget = true;                    // 処理対象フラグに対象を設定
                }
                //========== キー種別がＣＴＲＬ＋Ｗキー押下の場合 ==========
                else if ((UInt16)KEYTYPE.CTRL_W == type)
                {
                    // 機能名称ＩＤ（画面１移動）を設定する
                    funcnameID = (Int32)CAppDat.FUNCID.GAMEN_1;
                    // 画面切替種別にキーボードを設定
                    enGamenChgKind = CButtonCtrlData.EGAMENCHGKIND.KEYBOARD;
                    // 画面種別指定に全体画面を設定
                    enGamenKind = CButtonCtrlData.EGAMENKIND.ZENTAI;

                    isTarget = true;                    // 処理対象フラグに対象を設定
                }
                //========== キー種別がＣＴＲＬ＋Ｂキー押下の場合 ==========
                else if ((UInt16)KEYTYPE.CTRL_B == type)
                {
                    // 機能名称ＩＤ（画面１移動）を設定する
                    funcnameID = (Int32)CAppDat.FUNCID.GAMEN_1;
                    // 画面切替種別にキーボードを設定
                    enGamenChgKind = CButtonCtrlData.EGAMENCHGKIND.KEYBOARD;
                    // 画面種別指定に分割画面１を設定
                    enGamenKind = CButtonCtrlData.EGAMENKIND.BUNKATSU1;

                    isTarget = true;                    // 処理対象フラグに対象を設定
                }
                //========== キー種別がＣＴＲＬ＋Ｄキー押下の場合 ==========
                else if ((UInt16)KEYTYPE.CTRL_D == type)
                {
                    // 装置動作モードが表示卓（SDP）の場合
                    if ((UInt16)CDCPiniMng.MODE.SDP == eqpActMode)
                    {
                        // 機能名称ＩＤ（画面１移動）を設定する
                        funcnameID = (Int32)CAppDat.FUNCID.GAMEN_1;
                        // 画面切替種別にキーボードを設定
                        enGamenChgKind = CButtonCtrlData.EGAMENCHGKIND.KEYBOARD;
                        // 画面種別指定に分割画面２を設定
                        enGamenKind = CButtonCtrlData.EGAMENKIND.BUNKATSU2;

                        isTarget = true;                // 処理対象フラグに対象を設定
                    }
                    // 上記以外の場合
                    else
                    {
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.INFORMATION, MethodBase.GetCurrentMethod(), @"DCPモードのためデュアル画面遷移なし");
                    }
                }
                //========== キー種別がＣＴＲＬ＋←キー押下の場合 ==========
                else if ((UInt16)KEYTYPE.CTRL_LEFT == type)
                {
                    // 機能名称ＩＤ（前画面移動）を設定する
                    funcnameID = (Int32)CAppDat.FUNCID.PREV_GAMEN;
                    // 画面切替種別にキーボードを設定
                    enGamenChgKind = CButtonCtrlData.EGAMENCHGKIND.KEYBOARD;
                    // 画面種別指定に同種別指定を設定
                    enGamenKind = CButtonCtrlData.EGAMENKIND.SAMEGAMEN;

                    isTarget = true;                    // 処理対象フラグに対象を設定
                }
                //========== キー種別がＣＴＲＬ＋→キー押下の場合 ==========
                else if ((UInt16)KEYTYPE.CTRL_RIGHT == type)
                {
                    // 機能名称ＩＤ（次画面移動）を設定する
                    funcnameID = (Int32)CAppDat.FUNCID.NEXT_GAMEN;
                    // 画面切替種別にキーボードを設定
                    enGamenChgKind = CButtonCtrlData.EGAMENCHGKIND.KEYBOARD;
                    // 画面種別指定に同種別指定を設定
                    enGamenKind = CButtonCtrlData.EGAMENKIND.SAMEGAMEN;

                    isTarget = true;                    // 処理対象フラグに対象を設定
                }
                //========== キー種別が（画面選択）メニュー項目押下の場合 ==========
                else if ((UInt16)KEYTYPE.SCREENMENU == type)
                {
                    //funcnameID = (Int32)CAppDat.FUNCID.GAMEN_KIRIKAE;
                    funcnameID = readValue1;
                    seniGamenNo = (UInt16)readValue2;
                    enGamenChgKind = CButtonCtrlData.EGAMENCHGKIND.NONE;
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.GAMENINFO].WaitOne();
                    try
                    {
                        // 画面種別として取得
                        enGamenKind = (CButtonCtrlData.EGAMENKIND)CAppDat.GamenInfoT.DispKind;
                    }
                    catch (Exception ex)
                    {
                        // ミューテックス取得中に発生した例外の捕捉
                        syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                    }
                    finally
                    {
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.GAMENINFO].ReleaseMutex();
                    }
                    isTarget = true;                    // 処理対象フラグに対象を設定
                }
                //========== キー種別が（設定項目）ほかメニュー項目押下の場合 ==========
                else if ((UInt16)KEYTYPE.SETTINGMENU == type)
                {
                    funcnameID = readValue1;
                    movementID = readValue2;
                    isTarget = true;                    // 処理対象フラグに対象を設定
                }
                //========== キー種別がマウス右ボタン押下（ＤＣＰモード）の場合 ==========
                else if ((UInt16)KEYTYPE.MOUSE_RIGHT_DCP == type)
                {
                    // 右クリックで一つ前の操作を取り消す仕様の場合
                    if ((UInt16)CAppDat.RIGHTCLICKACTION.BACK_STATUS == CAppDat.TypeF.RightClickAction)
                    {
                        funcnameID = (Int32)CAppDat.FUNCID.TORIKESHI;
                        movementID = (Int32)CAppDat.MOVEID.BACK;
                        isTarget = true;                    // 処理対象フラグに対象を設定
                    }
                    // 右クリックで初期状態に戻す仕様の場合
                    else if ((UInt16)CAppDat.RIGHTCLICKACTION.CLEAR_STATUS == CAppDat.TypeF.RightClickAction)
                    {
                        funcnameID = (Int32)CAppDat.FUNCID.TORIKESHI;
                        movementID = (Int32)CAppDat.MOVEID.INITIAL;
                        isTarget = true;                    // 処理対象フラグに対象を設定
                    }
                    // 右クリックで付箋処理を呼び出す仕様、かつ、付箋機能が利用可能な場合
                    else if (((UInt16)CAppDat.RIGHTCLICKACTION.CALL_STICKYNOTES == CAppDat.TypeF.RightClickAction) &&
                             ((UInt16)CAppDat.ENFLAG.FLAG_ON == CAppDat.TypeF.StickyNotes))
                    {
                        funcnameID = (Int32)CAppDat.FUNCID.STICKYNOTE_MENU;
                        movementID = (Int32)CAppDat.MOVEID.NONE;
                        isTarget = true;                    // 処理対象フラグに対象を設定
                    }
                    // 上記以外の場合
                    else
                    {
                        // 処理なし
                    }
                }
                //========== キー種別がＣＴＲＬ＋↑キー押下の場合 ==========
                else if ((UInt16)KEYTYPE.CTRL_UP == type)
                {
                    // SDPモードで、画面遷移数の指定がある場合
                    if (((UInt16)CDCPiniMng.MODE.SDP == CAppDat.DCPSET.Mode) && (changeCrno != 0))
                    {
                        // ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
                        // 特情機能定義による処理分岐

                        // 初期画面Noから「+（定数）の倍数」をした画面Noを表示、それは直近（上）の画面とする。

                        int crno = (ushort)(CAppDat.DCPSET.DispCrno - (int)(CAppDat.DrawSectionF.Count / changeCrno) * changeCrno);  // 遷移先画面Noを格納
                        ushort nowno = CAppDat.GamenInfoT.Crno[0];      // 現在表示画面Noを取得

                        // 遷移先画面No取得するまで処理を続ける
                        while (true)
                        {
                            // 現在表示画面Noから、直近（上）の初期画面表示No「+（定数）の倍数」である場合
                            if ((crno - changeCrno <= nowno) && (nowno < crno))
                            {
                                // 最大画面Noより大きい場合
                                if (CAppDat.DrawSectionF[1].Crno[0] + CAppDat.DrawSectionF.Count <= crno)
                                {
                                    crno -= changeCrno;
                                }
                                break;
                            }
                            else
                            {
                                // 画面数は（定数）分の画面刻みとする
                                crno += changeCrno;
                            }
                        }

                        // 初期画面に遷移
                        CAppDat.FormMain.MoveGamenFromElseForm((UInt16)CChgGamenCtrl.SENISIGN.SITEI,
                                                    (UInt16)CButtonCtrlData.EGAMENCHGKIND.NONE, 0, (ushort)crno);
                        // ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

                        isTarget = false;                   // 処理対象フラグに非対象を設定
                    }
                    else
                    {
                        // 処理なし
                    }
                }
                //========== キー種別がＣＴＲＬ＋↓キー押下の場合 ==========
                else if ((UInt16)KEYTYPE.CTRL_DOWN == type)
                {
                    // SDPモードで、画面遷移数の指定がある場合
                    if (((UInt16)CDCPiniMng.MODE.SDP == CAppDat.DCPSET.Mode) && (changeCrno != 0))
                    {
                        // ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
                        // 特情機能定義による処理分岐

                        // 初期画面Noから「+（定数）の倍数」をした画面Noを表示、それは直近（下）の画面とする。

                        int crno = (ushort)(CAppDat.DCPSET.DispCrno + (int)(CAppDat.DrawSectionF.Count / changeCrno) * changeCrno);  // 遷移先画面Noを格納
                        ushort nowno = CAppDat.GamenInfoT.Crno[0];      // 現在表示画面Noを取得

                        // 遷移先画面NO取得するまで処理を続ける
                        while (true)
                        {
                            // 現在表示画面Noから、直近（下）の初期画面表示No「-（定数）の倍数」である場合
                            if ((crno < nowno) && (nowno <= crno + changeCrno))
                            {
                                // 最低画面Noより小さい場合
                                if (crno < CAppDat.DCPSET.DispCrno - (int)((CAppDat.DCPSET.DispCrno - CAppDat.DrawSectionF[1].Crno[0]) / changeCrno) * changeCrno)
                                {
                                    crno += changeCrno;
                                }

                                // 根室線では描画区間ID:1の画面には飛ばないものとする ※根室線特情機能 注意箇所！！
                                if (crno == CAppDat.DrawSectionF[1].Crno[0])
                                {
                                    crno += changeCrno;
                                }
                                break;
                            }
                            else
                            {
                                // 画面数は（定数）分の画面刻みとする
                                crno -= changeCrno;
                            }
                        }

                        // 初期画面に遷移
                        CAppDat.FormMain.MoveGamenFromElseForm((UInt16)CChgGamenCtrl.SENISIGN.SITEI,
                                                    (UInt16)CButtonCtrlData.EGAMENCHGKIND.NONE, 0, (ushort)crno);
                        // ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

                        isTarget = false;                   // 処理対象フラグに非対象を設定
                    }
                    else
                    {
                        // 処理なし
                    }
                }
                //========== キー種別がHOMEキー押下の場合 ==========
                else if ((UInt16)KEYTYPE.HOME_KEY == type)
                {
                    // SDPモードの場合
                    if ((UInt16)CDCPiniMng.MODE.SDP == CAppDat.DCPSET.Mode)
                    {
                        int crno = CAppDat.DCPSET.DispCrno;      // 遷移先画面Noを格納

                        // 初期画面に遷移
                        CAppDat.FormMain.MoveGamenFromElseForm((UInt16)CChgGamenCtrl.SENISIGN.SITEI,
                                                    (UInt16)CButtonCtrlData.EGAMENCHGKIND.NONE, 0, (ushort)crno);
                        isTarget = false;                   // 処理対象フラグに非対象を設定
                    }
                    else
                    {
                        // 処理なし
                    }
                }
                //========== キー種別がHOMEキー押下の場合 ==========
                else if ((UInt16)KEYTYPE.BACK_KEY == type)
                {
                    int crno = CAppDat.GamenInfoT.Crno[0];      // 遷移先画面No（現在表示画面No）を格納

                    // 初期画面に遷移
                    CAppDat.FormMain.MoveGamenFromElseForm((UInt16)CChgGamenCtrl.SENISIGN.SITEI,
                                                (UInt16)CButtonCtrlData.EGAMENCHGKIND.NONE, 0, (ushort)crno);

                    isTarget = false;                   // 処理対象フラグに非対象を設定
                }
                //========== 上記以外の場合 ==========
                else
                {
                    strAppLog = String.Format("キー種別不正：{0:D}", type);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                }


                //------------------------------------------------------------
                // ボタン処理情報を設定する
                //------------------------------------------------------------
                if (true == isTarget)                   // 処理対象フラグが対象の場合
                {
                    // ボタン処理情報エリア未設定？
                    if (null == buttonData)
                    {
                        // エリアを確保する
                        buttonData = new CButtonCtrlData();
                    }
                    // ボタン処理情報エリア設定済み？
                    else
                    {
                        // ボタン情報の初期化
                        buttonData.Clear();
                    }

                    buttonData.FuncnameID   = funcnameID;               // 機能名称ＩＤ
                    buttonData.MovementID   = movementID;               // 動作名称ＩＤ
                    buttonData.ButtonNo     = buttonNo;                 // ボタンNo
                    buttonData.ButtonID     = buttonID;                 // ボタンID
                    buttonData.NextMenuID   = nextMenuID;               // 次メニューＩＤ
                    buttonData.AfterNextMenuID1 = afterNextMenuID1;     // 処理後次メニューＩＤ１
                    buttonData.AfterNextMenuID2 = afterNextMenuID2;     // 処理後次メニューＩＤ２
                    buttonData.SeniGamenNo  = seniGamenNo;              // 遷移先画面No.
                    buttonData.GamenChgKind = (UInt16)enGamenChgKind;   // 画面切替種別
                    buttonData.GamenKind    = (UInt16)enGamenKind;      // 画面種別指定
                    // ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
                    // 特情機能定義による処理分岐
                    if (CAppDat.KOTODEN)                                   // 琴電特情処理
                    {
                        if (CAppDat.TypeF.DiffButtonChange == (UInt16)CAppDat.ENFLAG.FLAG_ON)
                        {
                            buttonData.GuideID = buttonGuide;              // ガイダンスID
                        }
                        else if (CAppDat.IopcoT.DkNo == 0)
                        {
                            buttonData.GuideID = buttonGuide;              // ガイダンスID
                        }
                    }
                    else
                    {
                        buttonData.GuideID = buttonGuide;              // ガイダンスID
                    }
                    // ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

                    // キー種別がスペースキー押下 または クリック表示の場合
                    if (((UInt16)KEYTYPE.SPACE_KEY == type) || (0 != uiClickDisp))
                    {
                        buttonData.KeyOn = true;        // キー起動
                    }
                    // 上記以外の場合
                    else
                    {
                        // 処理なし
                    }
                }
                else                                    // 処理対象フラグが対象ではない場合
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
                // 解放
                buttonFuncInfo = null;
                buttonInfo = null;
                dcpset = null;
            }

            return isTarget;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 段階No制御種別取得処理
        /// MODULE ID           : GetDkCtrl
        ///
        /// PARAMETER IN        : 
        /// <param>(in)なし</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>段階No制御種別</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 段階No制御種別を取得する。
        /// </summary>
        ///
        ///*******************************************************************************
        public static UInt16 GetDkCtrl()
        {
            UInt16  dkCtrl = 0;                         // 段階No制御種別
            UInt16  count = 0;                          // テーブルの個数
            UInt16  buttonMenuID2 = 0;                  // ボタンメニューID
            UInt16  buttonMenuID  = 0;                  // ボタンメニューID
            UInt16  buttonNo = 0;                       // ボタンNo
            UInt16  buttonID = 0;                       // ボタンID
            Int16   ret = 0;                            // 戻り値取得用
            String  strNowFuncName = String.Empty;      // 現在の機能名称
            String  syncErrorMsg = String.Empty;        // 同期エラーメッセージ

            try
            {
                dkCtrl = (UInt16)CButtonCtrl.DKCTRLTYPE.CTRL;

                strNowFuncName = String.Empty;
                CButtonFuncInfoMng buttonFuncInfo = null;
                CButtonInfoMng     buttonInfo = null;

                // ボタンメニューツリーテーブル排他制御開始
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BTNMENUTREE].WaitOne();

                try
                {
                    count = (UInt16)CAppDat.ButtonMenuTreeT.Count;

                    if (0 < (count - 1))
                    {
                        buttonMenuID2 = CAppDat.ButtonMenuTreeT[count - 1].ButtonMenuIDList[CAreaUserControl.AREAID_CONTROL];
                        buttonMenuID = CAppDat.ButtonMenuTreeT[count - 2].ButtonMenuIDList[CAreaUserControl.AREAID_CONTROL];
                        buttonNo = CAppDat.ButtonMenuTreeT[count - 2].ButtonNo;
                        buttonID = 0;

                        buttonFuncInfo = new CButtonFuncInfoMng();
                        buttonInfo = new CButtonInfoMng();

                        // ボタン機能情報取得処理
                        ret = CButtonCtrl.GetButtonFuncInfo(1, buttonNo, buttonMenuID, ref buttonID, ref buttonFuncInfo, ref buttonInfo);

                        // 該当ボタン機能情報ありの場合
                        if (0 == ret)
                        {
                            // 現在のボタンメニューIDが、１つ前のボタンメニューの処理後次メニューと同じ
                            if (buttonMenuID2 == buttonInfo.NextMenu)
                            {
                                strNowFuncName = buttonFuncInfo.FuncName;
                                                        // 現在表示中のボタンメニューの機能名称を取得する
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
                    // ボタンメニューツリーテーブル排他制御終了
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BTNMENUTREE].ReleaseMutex();

                    buttonFuncInfo = null;
                    buttonInfo = null;
                }

                // 操作内容テーブル排他制御開始
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].WaitOne();
                try
                {
                    if ((strNowFuncName == CAppDat.FuncName[(UInt16)CAppDat.FUNCID.GAMEN_OUTPUT]) &&
                        (strNowFuncName == CAppDat.IopcoT.IntName))
                    {
                        // 現在の機能が画面出力で、割り込み機能の場合
                        dkCtrl = (UInt16)CButtonCtrl.DKCTRLTYPE.NOCTRL;
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
                    // 操作内容テーブル排他制御終了
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                dkCtrl = (UInt16)CButtonCtrl.DKCTRLTYPE.CTRL;
            }

            return dkCtrl;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 簡易１つ前に戻る処理
        /// MODULE ID           : StepBackEasy
        ///
        /// PARAMETER IN        : 
        /// <param name="isBtnMenu">(in)ボタンメニューツリーテーブル更新(true=有り、false=無し)</param>
        /// <param name="isHannou">(in)反応文字列更新(true=有り、false=無し)</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// ボタンメニューツリーテーブルと反応文字列を１つ前に戻す（最後の要素を削除する）。
        ///  通常の１つ前に戻る処理ではないので、使用する際には気をつけること。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void StepBackEasy(bool isBtnMenu, bool isHannou)
        {
            UInt16  count = 0;                          // テーブルの個数
            String  syncErrorMsg = String.Empty;        // 同期エラーメッセージ

            try
            {
                // ボタンメニューツリーテーブル排他制御開始
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BTNMENUTREE].WaitOne();

                try
                {
                    // ボタンメニューツリーテーブルを更新する
                    if (true == isBtnMenu)              // ボタンメニューツリーテーブル更新有りの場合
                    {
                        // ボタンメニューツリーテーブルから現在のボタンメニューを削除する
                        count = (UInt16)CAppDat.ButtonMenuTreeT.Count;
                        if (0 < count)
                        {
                            // ボタンメニューツリーテーブルのLASTを削除する
                            CAppDat.ButtonMenuTreeT[count - 1] = null;
                            CAppDat.ButtonMenuTreeT.RemoveAt(count - 1);
                        }
                        else
                        {
                            // 処理なし
                        }

                        // ボタンNo.に初期値を設定する
                        count = (UInt16)CAppDat.ButtonMenuTreeT.Count;
                        if (0 < count)
                        {
                            CAppDat.ButtonMenuTreeT[count - 1].ButtonNo = 0;
                        }
                        else
                        {
                            // 処理なし
                        }
                    }
                    else                                // ボタンメニューツリーテーブル更新無しの場合
                    {
                        // 処理なし
                    }

                    // 反応文字列を更新する
                    if (true == isHannou)               // 反応文字列更新有りの場合
                    {
                        count = (UInt16)CButtonCtrl.m_Hannou.Count;
                        if (0 < count)
                        {
                            CButtonCtrl.m_Hannou.RemoveAt(count - 1);
                        }
                        else
                        {
                            // 処理なし
                        }
                    }
                    else                                // 反応文字列更新無しの場合
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
                    // ボタンメニューツリーテーブル排他制御終了
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BTNMENUTREE].ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 次メニュー遷移処理
        /// MODULE ID           : ShiftNextMenuKai
        ///
        /// PARAMETER IN        : 
        /// <param name="dkCtrl">(in)段階No制御種別(NOCTRL=制御なし、CTRL=制御する)</param>
        /// <param name="nextType">(in)次遷移種別</param>
        /// <param name="nextMenuID">(in)次メニューＩＤ</param>
        /// <param name="buttonNo">(in)ボタン番号</param>
        /// <param name="chgBottonMenuID">(in)画面切替後のボタンメニューID</param>
        /// <param name="isVisible">(in)表示指定（true=表示、false=非表示）</param>
        /// <param name="addString">(in)付加文字列（任意、ガイダンス下段メッセージに付加する文字列を指定）</param>
        /// <param name="isForceSubMsg">(in)サブメッセージ強制出力指定（true=強制出力、false=強制出力ではない）</param>
        /// <param name="btnGuideID">(in)ボタンガイダンスID</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 次メニューへ遷移する為に各種テーブルの更新、及び、各種ガイダンスの表示更新を行う。
        /// addString はガイダンスの下段に表示するメッセージにてこ名称などの文字列を付加して表示させたい場合に使用する。
        /// 初期値は空文字が設定されており省略可能とする。指定なしの場合は付加しない。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void ShiftNextMenuKai(UInt16 dkCtrl, UInt16 nextType, UInt16 nextMenuID, UInt16 buttonNo, UInt16 chgBottonMenuID, bool isVisible, String addString, bool isForceSubMsg, UInt16 btnGuideID = 0)
        {
            bool    isFind = false;                     // 該当情報有無フラグ
            Int32   backCount = 0;                      // 戻り個数
            UInt16[]  backIdList = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
                                                        // 戻りメニューIDリスト
            UInt16  count = 0;                          // テーブルの個数
            String  syncErrorMsg = String.Empty;        // 同期エラーメッセージ

            try
            {
                // ボタンメニューツリーテーブル排他制御開始
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BTNMENUTREE].WaitOne();

                try
                {
                    // 次メニュー遷移処理を行う
                    isFind = false;                     // 該当情報有無フラグに該当無しを設定
                    backCount = 0;

                    // 次遷移種別が有り（通常遷移）の場合
                    if (((UInt16)CButtonCtrl.SENIKIND.NEXTSENNI == nextType) ||
                        ((UInt16)CButtonCtrl.SENIKIND.SEIGYONEXTSENNI == nextType))
                    {
                        count = (UInt16)CAppDat.ButtonMenuTreeT.Count;
                        if (0 < count)
                        {
                            for (Int32 iPosi = (count - 1); iPosi > 0; iPosi--)
                            {
                                if (nextMenuID == CAppDat.ButtonMenuTreeT[iPosi].ButtonMenuIDList[CAreaUserControl.AREAID_CONTROL])
                                {
                                    isFind = true;      // 該当情報有無フラグに該当有りを設定
                                    break;
                                }
                                else
                                {
                                    // 処理なし
                                }
                                // メニューIDを保存する
                                backIdList[backCount] = CAppDat.ButtonMenuTreeT[iPosi].ButtonMenuIDList[CAreaUserControl.AREAID_CONTROL];
                                backCount++;
                            }
                        }
                        else
                        {
                            // 処理なし
                        }
                    }
                    // 次遷移種別が有り以外（通常遷移以外）の場合
                    else
                    {
                        // 処理なし
                    }

                    if (true == isFind)                 // 該当情報有りの場合
                    {
                        // メニュー遷移が戻るケースの場合、「１つ前に戻る」処理を複数回実行する
                        UInt16 dNextType = 0;
                        UInt16 dNextMenuID = 0;
                        if ((UInt16)CButtonCtrl.SENIKIND.NEXTSENNI == nextType)
                        {
                            dNextType = (UInt16)CButtonCtrl.SENIKIND.STEPBACK;
                        }
                        else
                        {
                            dNextType = (UInt16)CButtonCtrl.SENIKIND.SEIGYOSTEPBACK;
                        }

                        for (Int32 i = 0; i < backCount; i++)
                        {
                            dNextMenuID = backIdList[i];
                            CButtonCtrl.ShiftNextMenuNoHi(dkCtrl, dNextType, dNextMenuID, buttonNo, chgBottonMenuID, isVisible, addString, isForceSubMsg, btnGuideID);
                        }
                    }
                    else                            // 該当情報無しの場合
                    {
                        CButtonCtrl.ShiftNextMenuNoHi(dkCtrl, nextType, nextMenuID, buttonNo, chgBottonMenuID, isVisible, addString, isForceSubMsg, btnGuideID);
                                                        // 次メニュー遷移処理
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
                    // ボタンメニューツリーテーブル排他制御終了
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BTNMENUTREE].ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 次メニュー遷移処理
        /// MODULE ID           : ShiftNextMenu
        ///
        /// PARAMETER IN        : 
        /// <param name="nextType">(in)次遷移種別</param>
        /// <param name="nextMenuID">(in)次メニューＩＤ</param>
        /// <param name="buttonNo">(in)ボタン番号</param>
        /// <param name="chgBottonMenuID">(in)画面切替後のボタンメニューID</param>
        /// <param name="isVisible">(in)表示指定（true=表示、false=非表示）</param>
        /// <param name="addString">(in)付加文字列（任意、ガイダンス下段メッセージに付加する文字列を指定）</param>
        /// <param name="isForceSubMsg">(in)サブメッセージ強制出力指定（true=強制出力、false=強制出力ではない）</param>
        /// <param name="tekoGuideID">(in)てこガイダンスID</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 次メニューへ遷移する為に各種テーブルの更新、及び、各種ガイダンスの表示更新を行う。
        /// addString はガイダンスの下段に表示するメッセージにてこ名称などの文字列を付加して表示させたい場合に使用する。
        /// 初期値は空文字が設定されており省略可能とする。指定なしの場合は付加しない。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void ShiftNextMenu(UInt16 nextType, UInt16 nextMenuID, UInt16 buttonNo, UInt16 chgBottonMenuID, bool isVisible, String addString = "", bool isForceSubMsg = false, UInt16 tekoGuideID = 0)
        {
            String syncErrorMsg = String.Empty;

            try
            {
                // ボタンメニューツリーテーブル排他制御開始
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BTNMENUTREE].WaitOne();
                try
                {
                    // 次メニュー遷移処理を行う
                    ShiftNextMenuNoHi((UInt16)CButtonCtrl.DKCTRLTYPE.CTRL, nextType, nextMenuID, buttonNo, chgBottonMenuID, isVisible, addString, isForceSubMsg, tekoGuideID);
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    // ボタンメニューツリーテーブル排他制御終了
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BTNMENUTREE].ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 次メニュー遷移処理（排他制御なし）
        /// MODULE ID           : ShiftNextMenuNoHi
        ///
        /// PARAMETER IN        : 
        /// <param name="dkCtrl">(in)段階No制御種別(NOCTRL=制御なし、CTRL=制御する)</param>
        /// <param name="nextType">(in)次遷移種別</param>
        /// <param name="nextMenuID">(in)次メニューＩＤ</param>
        /// <param name="buttonNo">(in)ボタン番号</param>
        /// <param name="chgBottonMenuID">(in)画面切替後のボタンメニューID</param>
        /// <param name="isVisible">(in)表示指定（true=表示、false=非表示）</param>
        /// <param name="addString">(in)付加文字列（任意、ガイダンス下段メッセージに付加する文字列を指定）</param>
        /// <param name="isForceSubMsg">(in)サブメッセージ強制出力指定（true=強制出力、false=強制出力ではない）</param>
        /// <param name="btnGuideID">(in)ボタンガイダンスID</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 次メニューへ遷移する為に各種テーブルの更新、及び、各種ガイダンスの表示更新を行う。
        /// addString はガイダンスの下段に表示するメッセージにてこ名称などの文字列を付加して表示させたい場合に使用する。
        /// 初期値は空文字が設定されており省略可能とする。指定なしの場合は付加しない。
        /// ［注意事項］
        ///   ・本関数では、ボタンメニューツリーテーブル排他制御は行わないので、呼び出し側で排他制御を行うこと。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void ShiftNextMenuNoHi(UInt16 dkCtrl, UInt16 nextType, UInt16 nextMenuID, UInt16 buttonNo, UInt16 chgBottonMenuID, bool isVisible, String addString, bool isForceSubMsg, UInt16 btnGuideID = 0)
        {
            UInt16  cnt = 0;                            // ループカウンタ
            UInt16  count = 0;                          // テーブルの個数
            UInt16  reqCnt = 0;                         // 更新要求数
            Dictionary<UInt16, UInt16> iniButtonMenuID = new Dictionary<ushort, ushort>();  // 更新後の初期メニューのボタンメニューIDリスト
            Dictionary<UInt16, UInt16> newBottonMenuID = new Dictionary<ushort, ushort>();  // 更新後の初期メニューのボタンメニューIDリスト
            String  strGamenName = String.Empty;        // 画面名称
            String  strSubFuncName1 = String.Empty;     // 機能ガイド名称１
            String  strSubFuncName2 = String.Empty;     // 機能ガイド名称２
            String  strTmp1 = String.Empty;             // 文字列編集用
            String  strTmp2 = String.Empty;             // 文字列編集用
            String  strTmp3 = String.Empty;             // 文字列編集用
            String  strAppLog = String.Empty;           // ログメッセージ
            bool IsMenuNoCahnged = false;               // メニュー切替有無
            String  syncErrorMsg = String.Empty;        // 同期エラーメッセージ
            UInt16 MenuTreeCount = 0;                   // ボタンメニュー情報数
            bool IsHannouFlg = false;                   // 反応文字列の補正有無フラグ

            try
            {
                //------------------------------------------------------------
                // ボタンメニューツリーテーブルの更新を行う
                //------------------------------------------------------------
                // 次遷移種別が有りの場合
                if (((UInt16)SENIKIND.NEXTSENNI == nextType) ||
                    ((UInt16)SENIKIND.SEIGYONEXTSENNI == nextType))
                {
                    // ボタンメニューツリーテーブル排他制御開始
                    //[rem] CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BTNMENUTREE].WaitOne();

                    // ボタンメニューツリーテーブルにボタン番号を設定する
                    count = (UInt16)CAppDat.ButtonMenuTreeT.Count;
                    if (0 < count)
                    {
                        if ((0 < CAppDat.ButtonMenuTreeT[count - 1].ButtonNo) && (0 == buttonNo))
                        {
                            // 処理なし
                        }
                        else
                        {
                            CAppDat.ButtonMenuTreeT[count - 1].ButtonNo = buttonNo;
                        }
                        MenuTreeCount = count;
                    }
                    else
                    {
                        // 処理なし
                    }

                    if (0 != nextMenuID)                // 次メニューのIDが設定されている場合
                    {
                        // 次メニューIDが初期メニューと同じ場合
                        if ((0 < count) && (CAppDat.ButtonMenuTreeT[0].ButtonMenuIDList[CAreaUserControl.AREAID_CONTROL] == nextMenuID) &&
                                           (CAppDat.ButtonMenuTreeT[0].ButtonGuide == btnGuideID))
                        {
                            if ((UInt16)SENIKIND.NEXTSENNI == nextType)
                            {
                                nextType = (UInt16)SENIKIND.TOPMENU;
                                                        // 次遷移種別に初期メニューを設定
                            }
                            else
                            {
                                nextType = (UInt16)SENIKIND.SEIGYOTOPMENU;
                                                        // 次遷移種別に制御初期メニューを設定
                            }
                        }
                        // 次メニューIDが初期メニューと違う場合
                        else
                        {
                            if (0 < count)
                            {
                                // メニュー変化無しフラグに遷移有りを設定
                                CAppDat.ButtonMenuTreeT[count - 1].NoChange = 0;
                            }
                            else
                            {
                                // 処理なし
                            }
                            
                            CMenuTreeMng  menuTreeMng = new CMenuTreeMng();
                            menuTreeMng.ButtonMenuIDList[CAreaUserControl.AREAID_CONTROL] = nextMenuID;

                            if (0 < count)
                            {
                                menuTreeMng.ButtonMenuIDList[CAreaUserControl.AREAID_MENU] = CAppDat.ButtonMenuTreeT[count - 1].ButtonMenuIDList[CAreaUserControl.AREAID_MENU];
                            }

                            if (btnGuideID != 0)
                            {
                                menuTreeMng.ButtonGuide = btnGuideID;
                            }
                            else
                            {
                                if (true == CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo.ContainsKey(nextMenuID))
                                {
                                    foreach (CButtonSubMenuMng mng in CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo[nextMenuID].SubMenuInfo.Values)
                                    {
                                        if (mng.GuideMsgId != 0)
                                        {
                                            menuTreeMng.ButtonGuide = mng.GuideMsgId;
                                            break;
                                        }
                                    }
                                }
                            }

                            // ボタンメニューツリーテーブルに次遷移後のボタンメニューＩＤを設定する
                            CAppDat.ButtonMenuTreeT.Add(menuTreeMng);

                            // 機能操作表示更新要求セット
                            //[del] CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.FUNCTIONOPERATEDISP].Set();
                        }
                    }
                    // 次メニューIDがないが、ボタンガイダンスIDはある場合
                    else if (btnGuideID != 0)
                    {
                        // メニューがある場合
                        if (1 <= count)
                        {
                            if (CAppDat.ButtonMenuTreeT[count - 1].ButtonGuide != btnGuideID)
                            {
                                // メニュー変化無しフラグに遷移有りを設定
                                CAppDat.ButtonMenuTreeT[count - 1].NoChange = 0;

                                CMenuTreeMng menuTreeMng = new CMenuTreeMng();
                                foreach (UInt16 key in CAppDat.ButtonMenuTreeT[count - 1].ButtonMenuIDList.Keys)
                                {
                                    if (true == menuTreeMng.ButtonMenuIDList.ContainsKey(key))
                                    {
                                        menuTreeMng.ButtonMenuIDList[key] = CAppDat.ButtonMenuTreeT[count - 1].ButtonMenuIDList[key];
                                    }
                                    else
                                    {
                                        menuTreeMng.ButtonMenuIDList.Add(key, CAppDat.ButtonMenuTreeT[count - 1].ButtonMenuIDList[key]);
                                    }
                                }
                                menuTreeMng.ButtonGuide = btnGuideID;

                                // ボタンメニューツリーテーブルに次遷移後のボタンメニューＩＤを設定する
                                CAppDat.ButtonMenuTreeT.Add(menuTreeMng);
                            }
                            else
                            {
                                // 処理なし
                            }
                        }
                        // メニューがない場合
                        else
                        {
                            // 次遷移種別に初期メニューを設定
                            nextType = (UInt16)SENIKIND.SEIGYOTOPMENU;
                            // 次遷移種別に制御初期メニューを設定
                        }
                    }
                    else                                // 次メニューのIDが設定されていない場合
                    {
                        count = (UInt16)CAppDat.ButtonMenuTreeT.Count;
                        if (0 < count)
                        {
                            // メニュー変化無しフラグに遷移無しを設定
                            CAppDat.ButtonMenuTreeT[count - 1].NoChange = 1;
                            IsMenuNoCahnged = true;
                        }
                        else
                        {
                            // 処理なし
                        }
                    }

                    // ボタンメニューツリーテーブル排他制御終了
                    //[rem] CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BTNMENUTREE].ReleaseMutex();
                }
                // 次遷移種別が有りではない場合
                else
                {
                    // 処理なし
                }


                // 次遷移種別が初期メニューまたは画面切替の場合
                if (((UInt16)SENIKIND.TOPMENU == nextType) ||
                     ((UInt16)SENIKIND.SEIGYOTOPMENU == nextType) ||
                     ((UInt16)SENIKIND.CHGGAMEN == nextType))
                {
                    // 操作内容テーブル排他制御開始
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].WaitOne();
                    try
                    {
                        // 操作内容テーブルの割込フラグに割込機能動作中ではないを設定し割込機能名称と割込動作名称をクリアする
                        CAppDat.IopcoT.IntFlg = 0;
                        CAppDat.IopcoT.IntName = String.Empty;
                        CAppDat.IopcoT.IntMovement = String.Empty;

                        // 初期化後機能ボタン選択処理フラグが立っている場合
                        if (CAppDat.ExcuteClearIniFunc != true)
                        {
                            // 選択済みボタン情報リストを初期化
                            CAppDat.IopcoT.SelectedBtnList.Clear();
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

                    // ボタンメニューツリーテーブル排他制御開始
                    //[rem] CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BTNMENUTREE].WaitOne();

                    // ボタンメニューツリーテーブルの要素をTOPだけにする
                    count = (UInt16)CAppDat.ButtonMenuTreeT.Count;
                    if (0 == count)                     // ボタンメニューツリーテーブル未設定（空の状態）
                    {
                        // ボタンメニューツリーテーブルに１個設定
                        CAppDat.ButtonMenuTreeT.Add(new CMenuTreeMng());
                    }
                    else                                // ボタンメニューツリーテーブル設定済み（中身がある）
                    {
                        // ボタンメニューツリーテーブルのTOPを残して他はクリアする
                        for (cnt = (UInt16)(count - 1); cnt > 0 ; cnt--)
                        {
                            CAppDat.ButtonMenuTreeT[cnt] = null;
                            CAppDat.ButtonMenuTreeT.RemoveAt(cnt);
                        }
                    }

                    // ボタンNo.に初期値を設定する
                    CAppDat.ButtonMenuTreeT[0].ButtonNo = 0;
                    // メニュー変化無しフラグに遷移有りを設定
                    CAppDat.ButtonMenuTreeT[0].NoChange = 0;
                    // 画面表示フラグに表示なしを設定
                    CAppDat.ButtonMenuTreeT[0].DlgShowFlg = 0;
                    // ボタンガイダンスIDを設定
                    CAppDat.ButtonMenuTreeT[0].ButtonGuide = btnGuideID;

                    // ボタンメニューＩＤリストに画面切替後のボタンメニューＩＤリストを設定する
                    if ((0 < chgBottonMenuID) && (true == CAppDat.DrawSectionF.ContainsKey(chgBottonMenuID)))
                    {
                        CAppDat.ButtonMenuTreeT[0].ButtonMenuIDList.Clear();
                        foreach (UInt16 key in CAppDat.DrawSectionF[chgBottonMenuID].MenuInfo.Keys)
                        {
                            if (true == CAppDat.ButtonMenuTreeT[0].ButtonMenuIDList.ContainsKey(key))
                            {
                                CAppDat.ButtonMenuTreeT[0].ButtonMenuIDList[key] = CAppDat.DrawSectionF[chgBottonMenuID].MenuInfo[key].ButtonMenuId;
                            }
                            else
                            {
                                CAppDat.ButtonMenuTreeT[0].ButtonMenuIDList.Add(key, CAppDat.DrawSectionF[chgBottonMenuID].MenuInfo[key].ButtonMenuId);
                            }
                        }
                    }
                    else                                // 切替後ボタンメニューＩＤがない場合
                    {
                        // 処理なし
                    }

                    // ボタンメニューツリーテーブル排他制御終了
                    //[rem] CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BTNMENUTREE].ReleaseMutex();
                }
                // 次遷移種別が１つ前に戻る場合
                else if (((UInt16)SENIKIND.STEPBACK == nextType) ||
                         ((UInt16)SENIKIND.SEIGYOSTEPBACK == nextType))
                {
                    // ボタンメニューツリーテーブル排他制御開始
                    //[rem] CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BTNMENUTREE].WaitOne();

                    // ボタンメニューツリーテーブルから現在のボタンメニューを削除する
                    count = (UInt16)CAppDat.ButtonMenuTreeT.Count;
                    if (0 < count)
                    {
                        // 残すボタンメニュー数
                        ushort menuCount = 1;

                        // TOPのみ残す
                        if(nextMenuID >= count)
                        {
                            menuCount = 1;
                        }
                        // 残すボタンメニュー数の取得
                        else
                        {
                            menuCount = (ushort)(CAppDat.ButtonMenuTreeT.Count - nextMenuID);
                        }

                        // ボタンメニューの削除
                        for (int i = CAppDat.ButtonMenuTreeT.Count - 1; i >= menuCount; i--)
                        {
                            // ボタンメニューツリーテーブルの削除
                            CAppDat.ButtonMenuTreeT[i] = null;
                            CAppDat.ButtonMenuTreeT.RemoveAt(i);
                        }

                        // ボタンガイダンスIDの設定
                        btnGuideID = CAppDat.ButtonMenuTreeT[CAppDat.ButtonMenuTreeT.Count - 1].ButtonGuide;
                    }
                    else
                    {
                        // 処理なし
                    }

                    // ボタンNo.に初期値を設定する
                    count = (UInt16)CAppDat.ButtonMenuTreeT.Count;
                    if (0 < count)
                    {
                        CAppDat.ButtonMenuTreeT[count - 1].ButtonNo = 0;
                    }
                    else
                    {
                        // 処理なし
                    }

                    // ボタンメニューツリーテーブル排他制御終了
                    //[rem] CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BTNMENUTREE].ReleaseMutex();
                }
                // 上記以外の場合
                else
                {
                    // ボタンメニューツリーテーブル排他制御開始
                    //[rem] CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BTNMENUTREE].WaitOne();

                    // ボタンメニューツリーテーブルにボタン番号を設定する
                    count = (UInt16)CAppDat.ButtonMenuTreeT.Count;
                    if (0 < count)
                    {
                        if ((0 < buttonNo) & (0xFFFF != buttonNo))
                        {
                            CAppDat.ButtonMenuTreeT[count - 1].ButtonNo = buttonNo;
                            // ボタン番号がある場合、ボタン番号を設定する
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

                    // ボタンメニューツリーテーブル排他制御終了
                    //[rem] CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BTNMENUTREE].ReleaseMutex();
                }


                //------------------------------------------------------------
                // 現在表示中画面の画面名称を取得する
                //------------------------------------------------------------

                // 表示画面情報テーブル排他制御開始
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.GAMENINFO].WaitOne();
                try
                {
                    // 画面名称を取得する
                    strGamenName = CAppDat.GamenInfoT.Name;
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    // 表示画面情報テーブル排他制御終了
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.GAMENINFO].ReleaseMutex();
                }

                //------------------------------------------------------------
                // 各種ガイドメッセージの表示更新を行う
                //------------------------------------------------------------

                // 次遷移種別が有る（次のメニューに遷移する）場合
                if (((UInt16)SENIKIND.NEXTSENNI <= nextType) && (nextType < (UInt16)SENIKIND.SENIKINDCOUNT))
                {
                    // 次遷移種別が、制御次遷移あり／制御初期メニュー／制御１つ前に戻るの場合
                    if (((UInt16)SENIKIND.SEIGYONEXTSENNI == nextType) ||
                        ((UInt16)SENIKIND.SEIGYOTOPMENU == nextType)   ||
                        ((UInt16)SENIKIND.SEIGYOSTEPBACK == nextType))
                    {
                        // 処理なし
                    }
                    // 制御以外の場合
                    else
                    {
                        // 操作内容テーブル排他制御開始
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].WaitOne();
                        try
                        {
                            // 操作内容テーブルの割込フラグに割込機能動作中ではないを設定し、
                            // 機能名称と機能動作名称、割込機能名称、割込動作名称をクリアする
#if CHILD_BTNVISICTRL   // 子画面表示時にボタン表示／非表示制御を行う場合
                            CAppDat.IopcoT.IntFlg = 0;
                            CAppDat.IopcoT.FuncName = String.Empty;
                            CAppDat.IopcoT.FuncMovement = String.Empty;

                            CAppDat.IopcoT.IntName = String.Empty;
                            CAppDat.IopcoT.IntMovement = String.Empty;
#endif

                            // 次遷移種別が１つ前に戻る場合
                            if ((UInt16)SENIKIND.STEPBACK == nextType)
                            {
                                if (0 < CAppDat.IopcoT.DkNo)
                                {
                                    if (dkCtrl == (UInt16)DKCTRLTYPE.CTRL)  // 段階No制御種別が制御ありの場合
                                    {
                                        CAppDat.IopcoT.DkNo--;
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
                            // 次遷移種別が１つ前に戻る以外の場合
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

                    // ボタンメニューツリーテーブル排他制御開始
                    //[rem] CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BTNMENUTREE].WaitOne();

                    // ボタンメニューツリーテーブルから現在のボタンメニューＩＤと初期メニューのボタンメニューＩＤを取得する
                    count = (UInt16)CAppDat.ButtonMenuTreeT.Count;
                    if (0 < count)
                    {
                        iniButtonMenuID = CAppDat.ButtonMenuTreeT[0].ButtonMenuIDList;
                        newBottonMenuID = CAppDat.ButtonMenuTreeT[count - 1].ButtonMenuIDList;
                    }
                    else
                    {
                        // 処理なし
                    }

                    // ボタンメニューツリーテーブル排他制御終了
                    //[rem] CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BTNMENUTREE].ReleaseMutex();


                    // 指定ボタンメニューIDが、ボタンメニュー定義情報のボタンメニュー情報の中に含まれる場合
                    bool bMenuExists = false;

                    Dictionary<UInt16, UInt16> btnMenuList = new Dictionary<UInt16, UInt16>();
                    foreach (UInt16 key in newBottonMenuID.Keys)
                    {
                        if (true == CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo.ContainsKey(newBottonMenuID[key]))
                        {
                            btnMenuList.Add(key, newBottonMenuID[key]);
                            bMenuExists = true;
                        }
                        else
                        {
                            // 処理なし
                        }
                    }
                    newBottonMenuID = null;
                    newBottonMenuID = btnMenuList;
                    if (true == bMenuExists)
                    {
                        reqCnt = 0;                     // ボタンメニュー更新要求数を初期化

                        // ボタンガイド表示更新管理テーブル排他制御開始
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BUTTONGUIDEMSG].WaitOne();

                        try
                        {
                            foreach (UInt16 key in newBottonMenuID.Keys)
                            {
                                // ボタンメニュー設定定数ファイルのボタンメニュー定義情報のボタンメニュー情報内のサブメニュー情報の個数を取得する
                                UInt16 SubMenuCount = 0;
                                foreach (CButtonSubMenuMng subMenuData in CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo[newBottonMenuID[key]].SubMenuInfo.Values)
                                {
                                    if (key == CAreaUserControl.AREAID_CONTROL)
                                    {
                                        // 機能ガイド名称１と機能ガイド名称２を取得
                                        strSubFuncName1 = subMenuData.SubFuncName1;
                                        strSubFuncName2 = subMenuData.SubFuncName2;

                                        // ボタンメニューＩＤが初期メニューのボタンメニューＩＤの場合
                                        if (iniButtonMenuID[key] == newBottonMenuID[key])
                                        {
                                            strTmp1 = strGamenName;
                                        }
                                        // ボタンメニューＩＤが初期メニュー以外のボタンメニューＩＤの場合
                                        else
                                        {
                                            strTmp1 = strSubFuncName1;
                                        }

                                        UInt16 areacount = (UInt16)m_Hannou.Count;
                                        if (0 == areacount)
                                        {
                                            strTmp2 = String.Empty;                 // 反応表示文字列を取得
                                        }
                                        else
                                        {
                                            //----------------------------------------------------------------------------------------------------
                                            // 次遷移種別が制御次遷移ありの場合のみ以下の補正処理を行う
                                            // ボタンメニューの情報数が反応文字列の情報数以下の場合は制御完了シーケンスでメニューが遷移している
                                            // この場合、反応文字列のリストから戻り分を補正削除し情報数を再取得し、
                                            // 反応文字列の更新有無フラグを有効にする
                                            if ((UInt16)SENIKIND.SEIGYONEXTSENNI == nextType)
                                            {
                                                if (areacount <= MenuTreeCount)
                                                {
                                                    strTmp2 = m_Hannou[areacount - 1];              // 反応表示文字列を取得
                                                    IsHannouFlg = false;

                                                    if (true == isForceSubMsg)                      // サブメッセージ強制出力指定が強制出力の場合
                                                    {
                                                        strTmp2 = String.Empty;                     // 反応表示文字列をクリアする
                                                    }
                                                    else                                            // サブメッセージ強制出力指定が強制出力ではない場合
                                                    {
                                                        // 処理なし
                                                    }
                                                }
                                                else
                                                {
                                                    if (0 >= MenuTreeCount)
                                                    {
                                                        strTmp2 = String.Empty;                     // 反応表示文字列を取得
                                                        m_Hannou.Clear();
                                                    }
                                                    else
                                                    {
                                                        strTmp2 = m_Hannou[MenuTreeCount - 1];      // 反応表示文字列を取得
                                                        m_Hannou.RemoveRange(MenuTreeCount, areacount - MenuTreeCount);
                                                        IsMenuNoCahnged = true;
                                                    }
                                                    areacount = (UInt16)m_Hannou.Count;
                                                    IsHannouFlg = true;
                                                }
                                            }
                                            // 次遷移種別が制御次遷移あり以外の場合は通常処理を行う
                                            else
                                            {
                                                strTmp2 = m_Hannou[areacount - 1];                  // 反応表示文字列を取得
                                            }
                                            //----------------------------------------------------------------------------------------------------
                                        }

                                        // 次遷移種別が１つ前に戻る場合
                                        if (((UInt16)SENIKIND.STEPBACK == nextType) || ((UInt16)SENIKIND.SEIGYOSTEPBACK == nextType))
                                        {
                                            if (0 == areacount)
                                            {
                                                strTmp2 = String.Empty;
                                            }
                                            else
                                            {
                                                m_Hannou.RemoveAt(areacount - 1);
                                                areacount = (UInt16)m_Hannou.Count;
                                                if (0 == areacount)
                                                {
                                                    strTmp2 = String.Empty;
                                                }
                                                else
                                                {
                                                    strTmp2 = m_Hannou[areacount - 1];  // 反応表示文字列を取得
                                                }
                                            }
                                        }
                                        // 次遷移種別が初期メニューまたは画面切替の場合
                                        else if (((UInt16)SENIKIND.TOPMENU == nextType) ||
                                            ((UInt16)SENIKIND.SEIGYOTOPMENU == nextType) || ((UInt16)SENIKIND.CHGGAMEN == nextType))
                                        {
                                            m_Hannou.Clear();
                                            strTmp2 = String.Empty;
                                        }
                                        // 次遷移種別が上記以外の場合
                                        else
                                        {
                                            if (String.Empty == strTmp2)
                                            {
                                                if (CHandBase.STRRETUBANNASI == addString)
                                                {
                                                    strTmp2 = strSubFuncName2;
                                                }
                                                else
                                                {
                                                    strTmp2 = strSubFuncName2 + addString;
                                                }
                                            }
                                            else
                                            {
                                                if ((String.Empty != strSubFuncName2) || (addString != String.Empty))
                                                {
                                                    if (true == IsMenuNoCahnged)
                                                    {
                                                        // メニュー階層に変化なしは処理なし
                                                    }
                                                    else
                                                    {
                                                        String strSubAddString = String.Empty;
                                                        if (CHandBase.STRRETUBANNASI == addString)
                                                        {
                                                            if (String.Empty == strSubFuncName2.Trim())
                                                            {
                                                                // 処理なし
                                                            }
                                                            else
                                                            {
                                                                strSubAddString = "　" + strSubFuncName2;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            strSubAddString = "　" + strSubFuncName2 + addString;
                                                        }

                                                        strTmp2 = strTmp2 + strSubAddString;
                                                    }
                                                }
                                                else
                                                {
                                                    if (true == IsMenuNoCahnged)
                                                    {
                                                        // メニュー階層に変化なしは処理なし
                                                    }
                                                    else
                                                    {
                                                        strTmp2 = String.Empty;
                                                    }
                                                }
                                            }
                                            if (IsHannouFlg)
                                            {
                                                // 処理なし
                                            }
                                            else
                                            {
                                                m_Hannou.Add(strTmp2);
                                            }
                                        }
                                        strTmp3 = strTmp2;

                                        CButtonGuideMsgMng buttonGuideMng = new CButtonGuideMsgMng();
                                        if (btnGuideID == 0)
                                        {
                                            btnGuideID = subMenuData.GuideMsgId;
                                        }

                                        buttonGuideMng.GuidanceMsgId = btnGuideID;

                                        buttonGuideMng.SubFuncName1 = strTmp1;
                                        buttonGuideMng.SubFuncName2 = strTmp3;

                                        // ボタンガイド表示更新管理テーブルに更新要求を設定する
                                        if (true == isVisible)      // 表示指定
                                        {
                                            buttonGuideMng.Kind = (UInt16)(CButtonGuideMsgMng.EKIND.BIT_GUIDANCE | CButtonGuideMsgMng.EKIND.BIT_VISIBUTTON);
                                            buttonGuideMng.SubKind = (UInt16)CButtonGuideMsgMng.ESUBKIND.NONE;
                                        }
                                        else                        // 非表示指定
                                        {
                                            buttonGuideMng.Kind = (UInt16)(CButtonGuideMsgMng.EKIND.BIT_GUIDANCE | CButtonGuideMsgMng.EKIND.BIT_UNVISIBUTTON);
                                            buttonGuideMng.SubKind = (UInt16)CButtonGuideMsgMng.ESUBKIND.CMDGAMENBUTTON;
                                        }
                                        buttonGuideMng.ScreenNo = (UInt16)(SubMenuCount + 1);
                                        buttonGuideMng.ButtonMenuId = newBottonMenuID[key];

                                        CAppDat.ButtonGuideMsgT.Add(buttonGuideMng);

                                        SubMenuCount++;             // サブメニューの情報数を更新
                                        reqCnt++;                   // ボタンメニュー更新要求数をカウントＵＰ
                                    }
                                    else
                                    {
                                        CTeiKeiDisp.ReDispButton((UInt16)(SubMenuCount + 1), newBottonMenuID[key], 0xFFFF, String.Empty, String.Empty);
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
                            // ボタンガイド表示更新管理テーブル排他制御終了
                            CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BUTTONGUIDEMSG].ReleaseMutex();
                        }

                        if (0 < reqCnt)                 // 更新要求ありの場合
                        {
                            // ガイダンス表示更新要求を行う
                            //[未使用] CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.GUIDANCEDISP].Set();
                            IntPtr wParam = new IntPtr((UInt16)CAppDat.UPDATEREQUEST.GUIDANCEUPDATE);
                            CCommon.LocalPostMessage(true, CAppDat.WM_USER_MAINFORMUPDATE, wParam, IntPtr.Zero);
                        }
                        else
                        {
                            // 処理なし
                        }
                    }
                    // 指定ボタンメニューIDが、ボタンメニュー定義情報のボタンメニュー情報の中に含まれていない場合
                    else
                    {
                        // 装置動作モードを取得
                        UInt16 eqpActMode = CCommon.GetEqpActMode(CAppDat.DCPSET.Mode);

                        // 装置動作モードが表示卓（SDP）でボタンメニューIDが「0」の場合
                        if (((UInt16)CDCPiniMng.MODE.SDP == eqpActMode) && (0 == newBottonMenuID.Count))
                        {
                            // 処理なし
                        }
                        // 装置動作モードが表示卓（SDP）以外でボタンメニューIDが「0」の場合
                        else if (0 == newBottonMenuID.Count)
                        {
                            strAppLog = String.Format("該当メニュー情報なし：newBottonMenuID={0:D}", newBottonMenuID);
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.DEBUG, MethodBase.GetCurrentMethod(), strAppLog);
                        }
                        // 上記以外の場合
                        else
                        {
                            strAppLog = String.Format("該当メニュー情報なし：newBottonMenuID={0:D}", newBottonMenuID);
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                        }
                    }
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

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 画面移動パラメータ取得処理
        /// MODULE ID           : GetMoveGamenPara
        ///
        /// PARAMETER IN        : 
        /// <param name="funcNameID">(in)機能名称</param>
        /// <param name="signSeni">(in)遷移先（CChgGamenCtrlのSENISIGNを参照）</param>
        /// <param name="bcGamenChgKind">(in)画面切替種別（CButtonCtrlDataのEGAMENCHGKINDを参照）</param>
        /// <param name="bcGamenKind">(in)画面種別指定（CButtonCtrlDataのEGAMENKINDを参照）</param>
        /// PARAMETER OUT       : 
        /// <param name="gamenKind">(out)画面種別</param>
        /// <param name="seniSign">(out)遷移先</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>結果（true=成功、false=失敗）</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 画面移動（画面切替）用のパラメータを取得する。
        /// </summary>
        ///
        ///*******************************************************************************
        public static bool GetMoveGamenPara(UInt16 funcNameID, UInt16 signSeni, UInt16 bcGamenChgKind, UInt16 bcGamenKind, ref UInt16 gamenKind, ref UInt16 seniSign)
        {
            bool    blRtn = false;                      // 結果
            bool    isError = false;                    // エラー有無フラグ
            bool    isSameKind = false;                 // 同一画面種別フラグ
            String  strAppLog = String.Empty;           // ログメッセージ
            String  syncErrorMsg = String.Empty;        // 同期エラーメッセージ

            // 初期化
            isError = false;                            // エラー有無フラグにエラーなしを設定
            gamenKind = 0;                              // 画面種別
            seniSign = 0;                               // 遷移先

            try
            {
                isSameKind = false;                     // 同一画面種別フラグに同一ではないを設定

                //--------------------------------------------------
                // 画面切替用のパラメータを作成する（画面種別）
                //--------------------------------------------------

                // 個別画面指定の場合
                if ((UInt16)CButtonCtrlData.EGAMENKIND.KOBETU == bcGamenKind)
                {
                    gamenKind = (UInt16)CDrawKukanMng.EDISPKIND.KOBETUGAMEN;
                }
                // 全体画面指定の場合
                else if ((UInt16)CButtonCtrlData.EGAMENKIND.ZENTAI == bcGamenKind)
                {
                    gamenKind = (UInt16)CDrawKukanMng.EDISPKIND.ZENTAIGAMEN;
                }
                // 分割画面１指定の場合
                else if ((UInt16)CButtonCtrlData.EGAMENKIND.BUNKATSU1 == bcGamenKind)
                {
                    gamenKind = (UInt16)CDrawKukanMng.EDISPKIND.BUNKATSUGAMEN1;
                }
                // 分割画面２指定の場合
                else if ((UInt16)CButtonCtrlData.EGAMENKIND.BUNKATSU2 == bcGamenKind)
                {
                    gamenKind = (UInt16)CDrawKukanMng.EDISPKIND.BUNKATSUGAMEN2;
                }
                // 上記以外の場合
                else
                {
                    isSameKind = true;                  // 同一画面種別フラグに同一を設定
                }

                // 同一画面種別にする場合
                if (true == isSameKind)
                {
                    // 表示画面情報テーブル排他制御開始
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.GAMENINFO].WaitOne();
                    try
                    {
                        // 現在表示中画面の画面種別を取得する
                        gamenKind = CAppDat.GamenInfoT.DispKind;
                    }
                    catch (Exception ex)
                    {
                        // ミューテックス取得中に発生した例外の捕捉
                        syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                    }
                    finally
                    {
                        // 表示画面情報テーブル排他制御終了
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.GAMENINFO].ReleaseMutex();
                    }
                }
                // 同一画面種別にしない場合
                else
                {
                    // 処理なし
                }


                //--------------------------------------------------
                // 画面切替用のパラメータを作成する（遷移先）
                //--------------------------------------------------

                // 遷移先が指定画面移動の場合
                if ((UInt16)CChgGamenCtrl.SENISIGN.SITEI == signSeni)
                {
                    seniSign = (UInt16)CChgGamenCtrl.SENISIGN.SITEI;
                                                        // 遷移先指定に指定画面移動を設定

                    // 機能名称ＩＤが画面１で画面切替種別がキーボードの場合
                    if (((UInt16)CAppDat.FUNCID.GAMEN_1 == funcNameID) && ((UInt16)CButtonCtrlData.EGAMENCHGKIND.KEYBOARD == bcGamenChgKind))
                    {
                        if ((UInt16)CButtonCtrlData.EGAMENKIND.KOBETU == bcGamenKind)
                        {
                            seniSign = (UInt16)CChgGamenCtrl.SENISIGN.KOBETU1;
                                                        // 遷移先指定に個別画面１を設定
                        }
                        else if ((UInt16)CButtonCtrlData.EGAMENKIND.ZENTAI == bcGamenKind)
                        {
                            seniSign = (UInt16)CChgGamenCtrl.SENISIGN.ZENTAI1;
                                                        // 遷移先指定に全体画面１を設定
                        }
                    }
                }
                // 遷移先が前／次画面移動の場合
                else if (((UInt16)CChgGamenCtrl.SENISIGN.PREV == signSeni) ||
                         ((UInt16)CChgGamenCtrl.SENISIGN.NEXT == signSeni))
                {
                    seniSign = signSeni;                // 遷移先指定に前／次画面移動を設定
                }
                // 上記以外の場合
                else
                {
                    isError = true;                     // エラー有無フラグにエラーありを設定

                    strAppLog = String.Format("引数不正：funcNameID={0:D} signSeni={1:D} bcGamenChgKind={2:D} bcGamenKind={3:D}",
                                              funcNameID, signSeni, bcGamenChgKind, bcGamenKind);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                isError = true;                         // エラー有無フラグにエラーありを設定
            }

            if (true == isError)                        // エラーありの場合
            {
                blRtn = false;                          // 結果に失敗を設定
            }
            else
            {
                blRtn = true;                           // 結果に成功を設定
            }

            return blRtn;
        }

        ///*******************************************************************************
        /// MODULE NAME         : ボタン表示／非表示要求処理
        /// MODULE ID           : VisibleButton
        ///
        /// PARAMETER IN        : 
        /// <param name="type">(in)種別（CMDBTN_VISIBLE=コマンドボタン表示、CMDBTN_UNVISIBLE=コマンドボタン非表示、CMDGAMENBTN_VISIBLE=コマンドボタン＋画面出力ボタン表示、CMDGAMENBTN_UNVISIBLE=コマンドボタン＋画面出力ボタン非表示、GAMENBTN_VISIBLE=画面出力ボタン表示、GAMENBTN_UNVISIBLE=画面出力ボタン非表示）</param>
        /// <param name="buttonMenuID">(in)ボタンメニューＩＤ</param>
        /// <param name="guidanceMsgID">(in)ガイダンスメッセージＩＤ</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// ボタン表示／非表示の更新要求を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void VisibleButton(UInt16 type, UInt16 buttonMenuID, UInt16 guidanceMsgID)
        {
            UInt16  cnt = 0;                            // ループカウンタ
            UInt16  count = 0;                          // テーブルの個数
            UInt16  kind = 0;                           // 種別
            UInt16  subKind = 0;                        // 種別
            UInt16  reqCnt = 0;                         // 更新要求数
            String  syncErrorMsg = String.Empty;        // 同期エラーメッセージ
            String  logMsg = String.Empty;              // ログ出力メッセージ

            try
            {
                // コマンドボタン表示の場合
                if ((UInt16)BTNVISITYPE.CMDBTN_VISIBLE == type)
                {
                    kind = (UInt16)CButtonGuideMsgMng.EKIND.BIT_BTN_VISIBLE;
                    subKind = (UInt16)CButtonGuideMsgMng.ESUBKIND.CMDBUTTON;
                }
                // コマンドボタン非表示の場合
                else if ((UInt16)BTNVISITYPE.CMDBTN_UNVISIBLE == type)
                {
                    kind = (UInt16)CButtonGuideMsgMng.EKIND.BIT_BTN_UNVISIBLE;
                    subKind = (UInt16)CButtonGuideMsgMng.ESUBKIND.CMDBUTTON;
                }
                // コマンドボタン＋画面出力ボタン表示の場合
                else if ((UInt16)BTNVISITYPE.CMDGAMENBTN_VISIBLE == type)
                {
                    kind = (UInt16)CButtonGuideMsgMng.EKIND.BIT_BTN_VISIBLE;
                    subKind = (UInt16)CButtonGuideMsgMng.ESUBKIND.CMDGAMENBUTTON;
                }
                // コマンドボタン＋画面出力ボタン非表示の場合
                else if ((UInt16)BTNVISITYPE.CMDGAMENBTN_UNVISIBLE == type)
                {
                    kind = (UInt16)CButtonGuideMsgMng.EKIND.BIT_BTN_UNVISIBLE;
                    subKind = (UInt16)CButtonGuideMsgMng.ESUBKIND.CMDGAMENBUTTON;
                }
                // 画面出力ボタン表示の場合
                else if ((UInt16)BTNVISITYPE.GAMENBTN_VISIBLE == type)
                {
                    kind = (UInt16)CButtonGuideMsgMng.EKIND.BIT_BTN_VISIBLE;
                    subKind = (UInt16)CButtonGuideMsgMng.ESUBKIND.GAMENBUTTON;
                }
                // 画面出力ボタン非表示の場合
                else if ((UInt16)BTNVISITYPE.GAMENBTN_UNVISIBLE == type)
                {
                    kind = (UInt16)CButtonGuideMsgMng.EKIND.BIT_BTN_UNVISIBLE;
                    subKind = (UInt16)CButtonGuideMsgMng.ESUBKIND.GAMENBUTTON;
                }
                // 上記以外の場合
                else
                {
                    logMsg = String.Format("種別不正：{0:D}", type);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                }


                reqCnt = 0;                             // ボタンメニュー更新要求数を初期化

                // ボタンメニューＩＤの指定が範囲内の場合
                bool bMenuExists = CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo.ContainsKey(buttonMenuID);
                if (true == bMenuExists)
                {
                    // ボタンガイド表示更新管理テーブル排他制御開始
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BUTTONGUIDEMSG].WaitOne();
                    try
                    {
                        // ボタンメニュー設定定数ファイルのボタンメニュー定義情報のボタンメニュー情報内のサブメニュー情報の個数を取得する
                        count = (UInt16)CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo[buttonMenuID].SubMenuInfo.Count;
                        for (cnt = 0; cnt < count; cnt++)
                        {
                            // ボタンガイド表示更新管理テーブルに更新要求を設定する
                            CButtonGuideMsgMng buttonGuideMng = new CButtonGuideMsgMng();
                            buttonGuideMng.Kind = kind;
                            buttonGuideMng.SubKind = subKind;
                            buttonGuideMng.ScreenNo = (UInt16)(cnt + 1);
                            buttonGuideMng.ButtonMenuId = buttonMenuID;
                            buttonGuideMng.GuidanceMsgId = guidanceMsgID;
                            buttonGuideMng.SubFuncName1 = String.Empty;
                            buttonGuideMng.SubFuncName2 = String.Empty;
                            buttonGuideMng.FuncName = String.Empty;
                            buttonGuideMng.ForeColor = String.Empty;
                            buttonGuideMng.BackColor = String.Empty;

                            CAppDat.ButtonGuideMsgT.Add(buttonGuideMng);

                            reqCnt++;                       // ボタンメニュー更新要求数をカウントＵＰ
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
                        // ボタンガイド表示更新管理テーブル排他制御終了
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BUTTONGUIDEMSG].ReleaseMutex();
                    }
                }
                // ボタンメニューＩＤが0の場合（遷移しない場合）
                else if (buttonMenuID == 0)
                {
                    // 処理なし
                }
                // ボタンメニューＩＤの指定が範囲外の場合
                else
                {
                    logMsg = String.Format("ボタンメニューＩＤ範囲外：buttonMenuID={0:D}", buttonMenuID);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                }

                if (0 < reqCnt)                         // 更新要求ありの場合
                {
                    // ガイダンス表示更新要求を行う
                    //[未使用] CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.GUIDANCEDISP].Set();
                    IntPtr wParam = new IntPtr((UInt16)CAppDat.UPDATEREQUEST.GUIDANCEUPDATE);
                    CCommon.LocalPostMessage(true, CAppDat.WM_USER_MAINFORMUPDATE, wParam, IntPtr.Zero);
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

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : ボタン色変更要求処理
        /// MODULE ID           : ChangeButtonColor
        ///
        /// PARAMETER IN        : 
        /// <param name="type">(in)種別（1=表示色、2=背景色、3=表示色＋背景色）</param>
        /// <param name="buttonMenuID">(in)ボタンメニューＩＤ</param>
        /// <param name="strFuncName">(in)ボタン機能名称</param>
        /// <param name="strForeColor">(in)表示色</param>
        /// <param name="strBackColor">(in)背景色</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// ボタンの色変更要求を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void ChangeButtonColor(UInt16 type, UInt16 buttonMenuID, String strFuncName, String strForeColor, String strBackColor)
        {
            UInt16  cnt = 0;                            // ループカウンタ
            UInt16  count = 0;                          // テーブルの個数
            UInt16  kind = 0;                           // 種別
            UInt16  subKind = 0;                        // 種別
            UInt16  reqCnt = 0;                         // 更新要求数
            String  syncErrorMsg = String.Empty;        // 同期エラーメッセージ
            String  logMsg = String.Empty;              // ログ出力メッセージ

            try
            {
                subKind = (UInt16)CButtonGuideMsgMng.ESUBKIND.NONE;

                // 「表示色＋背景色」の場合
                if (3 == type)
                {
                    kind = (UInt16)((UInt16)CButtonGuideMsgMng.EKIND.BIT_BTN_FORECOLORCHG | (UInt16)CButtonGuideMsgMng.EKIND.BIT_BTN_BACKCOLORCHG);
                }
                // 「背景色」の場合
                else if (2 == type)
                {
                    kind = (UInt16)CButtonGuideMsgMng.EKIND.BIT_BTN_BACKCOLORCHG;
                }
                // 「表示色」の場合
                else if (1 == type)
                {
                    kind = (UInt16)CButtonGuideMsgMng.EKIND.BIT_BTN_FORECOLORCHG;
                }
                // 上記以外の場合
                else
                {
                    logMsg = String.Format("種別不正：{0:D}", type);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                }


                reqCnt = 0;                             // ボタンメニュー更新要求数を初期化

                // ボタンメニューＩＤの指定が範囲内の場合
                bool bMenuExists = CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo.ContainsKey(buttonMenuID);
                if (true == bMenuExists)
                {
                    // ボタンガイド表示更新管理テーブル排他制御開始
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BUTTONGUIDEMSG].WaitOne();
                    try
                    {
                        // ボタンメニュー設定定数ファイルのボタンメニュー定義情報のボタンメニュー情報内のサブメニュー情報の個数を取得する
                        count = (UInt16)CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo[buttonMenuID].SubMenuInfo.Count;
                        for (cnt = 0; cnt < count; cnt++)
                        {
                            bool isEntryReq = false;                // 登録要求フラグに要求なしを設定
                            Int32 btnGuideKosu = CAppDat.ButtonGuideMsgT.Count;
                                                        // ボタンガイド表示更新管理テーブルの登録数を取得する
                            if (0 < btnGuideKosu)       // ボタンガイド表示更新管理テーブルに登録がある場合
                            {
                                Int32 idx = 0;
                                for (idx = (btnGuideKosu - 1); idx >= 0; idx--)
                                {
                                    // ボタン機能名称とボタンメニューＩＤが同じ場合（同一ボタンに対する要求が既に存在する場合）
                                    if ((CAppDat.ButtonGuideMsgT[idx].ButtonMenuId == buttonMenuID) &&
                                        (CAppDat.ButtonGuideMsgT[idx].FuncName == strFuncName))
                                    {
                                        // 種別、表示色、背景色が同じ場合
                                        if ((CAppDat.ButtonGuideMsgT[idx].Kind == kind) &&
                                            (CAppDat.ButtonGuideMsgT[idx].ForeColor == strForeColor) &&
                                            (CAppDat.ButtonGuideMsgT[idx].BackColor == strBackColor))
                                        {
                                            // 処理なし
                                        }
                                        // 種別、表示色、背景色が同じではない場合
                                        else
                                        {
                                            isEntryReq = true;  // 登録要求フラグに要求ありを設定
                                        }
                                        break;
                                    }
                                    // ボタン機能名称とボタンメニューＩＤが同じではない場合
                                    else
                                    {
                                        // 処理なし
                                    }
                                }

                                if (0 > idx)            // ボタン機能名称とボタンメニューＩＤが同じものが見つからなかった場合
                                {
                                    isEntryReq = true;  // 登録要求フラグに要求ありを設定
                                }
                                else                    // ボタン機能名称とボタンメニューＩＤが同じものが見つかった場合
                                {
                                    // 処理なし
                                }
                            }
                            else                        // ボタンガイド表示更新管理テーブルに登録がない場合
                            {
                                isEntryReq = true;      // 登録要求フラグに要求ありを設定
                            }

                            if (true == isEntryReq)     // 登録要求フラグが要求ありの場合
                            {
                                 // ボタンガイド表示更新管理テーブルに更新要求を設定する
                                 CButtonGuideMsgMng buttonGuideMng = new CButtonGuideMsgMng();
                                 buttonGuideMng.Kind = kind;
                                 buttonGuideMng.SubKind = subKind;
                                 buttonGuideMng.ScreenNo = (UInt16)(cnt + 1);
                                 buttonGuideMng.ButtonMenuId = buttonMenuID;
                                 buttonGuideMng.GuidanceMsgId = 0;
                                 buttonGuideMng.SubFuncName1 = String.Empty;
                                 buttonGuideMng.SubFuncName2 = String.Empty;
                                 buttonGuideMng.FuncName = strFuncName;
                                 buttonGuideMng.ForeColor = strForeColor;
                                 buttonGuideMng.BackColor = strBackColor;

                                 CAppDat.ButtonGuideMsgT.Add(buttonGuideMng);

                                 reqCnt++;              // ボタンメニュー更新要求数をカウントＵＰ
                            }
                            else                        // 登録要求フラグが要求なしの場合
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
                        // ボタンガイド表示更新管理テーブル排他制御終了
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.BUTTONGUIDEMSG].ReleaseMutex();
                    }
                }
                // ボタンメニューＩＤの指定が範囲外の場合
                else
                {
                    logMsg = String.Format("ボタンメニューＩＤ範囲外：buttonMenuID={0:D}", buttonMenuID);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), logMsg);
                }

                if (0 < reqCnt)                         // 更新要求ありの場合
                {
                    // ガイダンス表示更新要求を行う
                    //[未使用] CAppDat.HandleCtrl[(UInt32)CAppDat.WAITID.GUIDANCEDISP].Set();
                    IntPtr wParam = new IntPtr((UInt16)CAppDat.UPDATEREQUEST.GUIDANCEUPDATE);
                    CCommon.LocalPostMessage(true, CAppDat.WM_USER_MAINFORMUPDATE, wParam, IntPtr.Zero);
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

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 割込機能設定処理
        /// MODULE ID           : SetInterFunc
        ///
        /// PARAMETER IN        : 
        /// <param name="funcNameID">(in)機能名称ID</param>
        /// <param name="movementID">(in)動作名称ID</param>
        /// PARAMETER OUT       : 
        /// <param name="rirekiType">(out)履歴種別（0=履歴なし、1=割込履歴、2=通常履歴）</param>
        /// <param name="isNext">(out)次遷移有無（true=有り、false=無し）</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 操作内容テーブルの割込機能名称と割込動作名称に、機能名称と動作名称を設定する。
        /// 本関数では出力情報の履歴種別に通常履歴を設定することはない。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void SetInterFunc(UInt16 funcNameID, UInt16 movementID, ref UInt16 rirekiType, ref bool isNext)
        {
            String  strFuncName = String.Empty;         // 機能名称
            String  strMovement = String.Empty;         // 動作名称
            String  syncErrorMsg = String.Empty;        // 同期エラーメッセージ

            // 出力情報を初期化
            rirekiType = 0;                             // 履歴種別をクリア（履歴なし）
            isNext = false;                             // 次遷移有無に無しを設定

            try
            {
                // 機能名称ID，動作名称IDから名称文字列を取得
                strFuncName = CCommon.GetFuncNameFromID(funcNameID);
                strMovement = CCommon.GetMovementFromID(movementID);

                // 操作内容テーブル排他制御開始
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].WaitOne();
                try
                {
                    // 操作内容テーブルの割込フラグに割込機能動作中を設定し、割込機能名称と割込動作名称に、機能名称と動作名称を設定する
                    CAppDat.IopcoT.IntFlg = 1;
                    CAppDat.IopcoT.IntName = strFuncName;
                    CAppDat.IopcoT.IntMovement = strMovement;
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

                rirekiType = 1;                         // 履歴種別に割込機能を設定
                isNext = false;                         // 次遷移有無に無しを設定
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 機能名称設定処理
        /// MODULE ID           : SetFunc
        ///
        /// PARAMETER IN        : 
        /// <param name="funcNameID">(in)機能名称ID</param>
        /// <param name="movementID">(in)動作名称ID</param>
        /// PARAMETER OUT       : 
        /// <param name="rirekiType">(out)履歴種別（0=履歴なし、1=割込履歴、2=通常履歴）</param>
        /// <param name="isNext">(out)次遷移有無（true=有り、false=無し）</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 操作内容テーブルの機能名称と機能動作名称に、機能名称と動作名称を設定する。
        /// 本関数では出力情報の履歴種別に割込履歴を設定することはない。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void SetFunc(UInt16 funcNameID, UInt16 movementID, ref UInt16 rirekiType, ref bool isNext)
        {
            String  strFuncName = String.Empty;         // 機能名称
            String  strMovement = String.Empty;         // 動作名称
            String  syncErrorMsg = String.Empty;        // 同期エラーメッセージ

            // 出力情報を初期化
            rirekiType = 0;                             // 履歴種別をクリア（履歴なし）
            isNext = false;                             // 次遷移有無に無しを設定

            try
            {
                // 機能名称ID，動作名称IDから名称文字列を取得
                strFuncName = CCommon.GetFuncNameFromID(funcNameID);
                strMovement = CCommon.GetMovementFromID(movementID);

                // 操作内容テーブル排他制御開始
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].WaitOne();
                try
                {
                    // 操作内容テーブルの機能名称と機能動作名称に、機能名称と動作名称を設定する
                    CAppDat.IopcoT.FuncName = strFuncName;
                    CAppDat.IopcoT.FuncMovement = strMovement;
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

                rirekiType = 2;                         // 履歴種別に通常機能を設定
                isNext = true;                          // 次遷移有無に有りを設定
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 割込機能設定処理
        /// MODULE ID           : SetInterFunc2
        ///
        /// PARAMETER IN        : 
        /// <param name="strFuncName">(in)機能名称</param>
        /// <param name="strMovement">(in)動作名称</param>
        /// <param name="intFlg">(in)割込みフラグ</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 操作内容テーブルの割込機能名称と割込動作名称と割込みフラグを設定する。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void SetInterFunc2(String strFuncName, String strMovement, Int16 intFlg)
        {
            String  syncErrorMsg = String.Empty;        // 同期エラーメッセージ

            try
            {
                // 操作内容テーブル排他制御開始
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].WaitOne();
                try
                {
                    // 操作内容テーブルの割込フラグに割込機能動作中を設定し、割込機能名称と割込動作名称に、機能名称と動作名称を設定する
                    CAppDat.IopcoT.IntName = strFuncName;
                    CAppDat.IopcoT.IntMovement = strMovement;
                    CAppDat.IopcoT.IntFlg = intFlg;
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
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 割込機能名称取得処理
        /// MODULE ID           : GetInterFunc
        ///
        /// PARAMETER IN        : 
        /// <param>(in)なし</param>
        /// PARAMETER OUT       : 
        /// <param name="strFuncName">(out)機能名称</param>
        /// <param name="strMovement">(out)動作名称</param>
        /// <param name="intFlg">(out)割込みフラグ</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 操作内容テーブルの割込機能名称と割込動作名称と割込みフラグを取得する。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void GetInterFunc(ref String strFuncName, ref String strMovement, ref Int16 intFlg)
        {
            String  syncErrorMsg = String.Empty;        // 同期エラーメッセージ

            try
            {
                // 出力情報を初期化
                strFuncName = String.Empty;
                strMovement = String.Empty;
                intFlg = 0;

                // 操作内容テーブル排他制御開始
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].WaitOne();
                try
                {
                    // 操作内容テーブルの割込機能名称，割込動作名称，割込みフラグを取得する
                    strFuncName = CAppDat.IopcoT.IntName;
                    strMovement = CAppDat.IopcoT.IntMovement;
                    intFlg = CAppDat.IopcoT.IntFlg;
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
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);

                strFuncName = String.Empty;
                strMovement = String.Empty;
                intFlg = 0;
            }

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 機能名称取得処理
        /// MODULE ID           : GetFunc
        ///
        /// PARAMETER IN        : 
        /// <param>(in)なし</param>
        /// PARAMETER OUT       : 
        /// <param name="strFuncName">(out)機能名称</param>
        /// <param name="strMovement">(out)動作名称</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 操作内容テーブルの機能名称と動作名称を取得する。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void GetFunc(ref String strFuncName, ref String strMovement)
        {
            String  syncErrorMsg = String.Empty;        // 同期エラーメッセージ

            try
            {
                // 出力情報を初期化
                strFuncName = String.Empty;
                strMovement = String.Empty;

                // 操作内容テーブル排他制御開始
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].WaitOne();
                try
                {
                    // 操作内容テーブルの機能名称，動作名称を取得する
                    strFuncName = CAppDat.IopcoT.FuncName;
                    strMovement = CAppDat.IopcoT.FuncMovement;
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
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                strFuncName = String.Empty;
                strMovement = String.Empty;
            }

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : ボタン機能情報取得処理
        /// MODULE ID           : GetButtonFuncInfo
        ///
        /// PARAMETER IN        : 
        /// <param name="screenNo">(in)スクリーンNo.</param>
        /// <param name="buttonNo">(in)ボタンＮｏ</param>
        /// <param name="buttonMenuID">(in)ボタンメニューＩＤ</param>
        /// PARAMETER OUT       : 
        /// <param name="buttonID">(out)ボタンＩＤ</param>
        /// <param name="buttonFuncInfo">(out)ボタン機能情報</param>
        /// <param name="buttonInfo">(out)ボタン情報</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>
        /// 結果（0=正常、-1=例外、1=該当メニュー情報なし、2=該当サブメニュー情報なし、3=該当ボタン情報なし、4=該当ボタン機能情報なし）
        /// </returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// スクリーンＮｏ，ボタンＮｏ，ボタンメニューＩＤを指定して、ボタンメニュー設定定数
        /// ファイルの該当ボタンのボタン機能情報とボタン情報を取得する。
        /// </summary>
        ///
        ///*******************************************************************************
        public static Int16 GetButtonFuncInfo(UInt16 screenNo, UInt16 buttonNo, UInt16 buttonMenuID, ref UInt16 buttonID, ref CButtonFuncInfoMng buttonFuncInfo, ref CButtonInfoMng buttonInfo)
        {
            Int16   rtn = -1;                           // 結果
            Int16   ret = 0;                            // 戻り値取得用
            UInt16  index = 0;                          // 該当ボタン情報のINDEX
            String  strAppLog = String.Empty;           // ログメッセージ

            buttonID = 0;                               // ボタンＩＤ

            try
            {
                // ボタンメニュー設定定数ファイルのボタンメニュー定義情報のボタンメニュー情報の中の該当ボタン情報のインデックスを取得する
                // （ボタンメニューＩＤ，スクリーンＮｏ，ボタンＮｏを指定して該当ボタン情報のINDEXを取得する）
                ret = GetButtonInfoIdx(screenNo, buttonNo, buttonMenuID, ref index);

                if (0 == ret)                           // 取得成功の場合
                {
                    // ボタン情報からボタンＩＤを取得する
                    buttonID = CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo[buttonMenuID].SubMenuInfo[screenNo].ButtonInfo[index].ButtonId;

                    // ボタンＩＤが、ボタン機能定義情報のボタンＩＤ情報の中に含まれる場合
                    bool bFuncExists = CAppDat.ButtonMenuF.ButtonFuncDef.ButtonFuncInfo.ContainsKey(buttonID);
                    if (true == bFuncExists)
                    {
                        // 当該ボタンＩＤのボタン機能情報とボタン情報を返す
                        buttonFuncInfo = (CButtonFuncInfoMng)CAppDat.ButtonMenuF.ButtonFuncDef.ButtonFuncInfo[buttonID].Clone();
                        buttonInfo = (CButtonInfoMng)CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo[buttonMenuID].SubMenuInfo[screenNo].ButtonInfo[index].Clone();
                        rtn = 0;                        // 結果に正常を設定
                    }
                    // ボタンＩＤが、ボタン機能定義情報のボタンＩＤ情報の中に含まれていない場合
                    else
                    {
                        strAppLog = String.Format("該当ボタン機能情報なし：buttonID={0:D}", buttonID);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.ERROR, MethodBase.GetCurrentMethod(), strAppLog);
                        rtn = 4;                        // 結果に該当ボタン機能情報なしを設定
                    }
                }
                else                                    // 取得失敗の場合
                {
                    strAppLog = String.Format("該当ボタン情報なし：buttonMenuID={0:D} screenNo={1:D} buttonNo={2:D} ret={3:D}", buttonMenuID, screenNo, buttonNo, ret);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.INFORMATION, MethodBase.GetCurrentMethod(), strAppLog);
                    rtn = ret;                          // 結果を設定
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                rtn = -1;                               // 結果に例外を設定
            }

            return rtn;
        }

        ///*******************************************************************************
        /// MODULE NAME         : ボタン情報インデックス取得処理
        /// MODULE ID           : GetButtonInfoIdx
        ///
        /// PARAMETER IN        : 
        /// <param name="screenNo">(in)スクリーンNo.</param>
        /// <param name="buttonNo">(in)ボタンＮｏ</param>
        /// <param name="buttonMenuID">(in)ボタンメニューＩＤ</param>
        /// PARAMETER OUT       : 
        /// <param name="index">(out)ボタンメニュー設定定数ファイルのボタンメニュー定義情報の中の該当ボタン情報のインデックス（0-N）</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>
        /// 結果（0=正常、-1=例外、1=該当メニュー情報なし、2=該当サブメニュー情報なし、3=該当ボタン情報なし）
        /// </returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// スクリーンＮｏ，ボタンＮｏ，ボタンメニューＩＤを指定して、ボタンメニュー設定定数
        /// ファイルのボタンメニュー定義情報の中の該当ボタン情報のインデックスを取得する。
        /// </summary>
        ///
        ///*******************************************************************************
        public static Int16 GetButtonInfoIdx(UInt16 screenNo, UInt16 buttonNo, UInt16 buttonMenuID, ref UInt16 index)
        {
            Int16   rtn = -1;                           // 結果
            Int32   btnIdx = -1;                        // 該当ボタン情報のINDEX
            UInt16  count = 0;                          // テーブルの個数
            UInt16  cnt = 0;                            // ループカウンタ
            String  strAppLog = String.Empty;           // ログメッセージ

            // 出力情報を初期化
            index = 0;                                  // ボタン情報のINDEXに０を設定
            rtn = 0;                                    // 結果に正常を設定

            try
            {
                btnIdx = -1;                            // 該当ボタン情報のINDEXに該当なしを設定

                // 指定ボタンメニューIDが、ボタンメニュー定義情報のボタンメニュー情報の中に含まれる場合
                bool bMenuExists = CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo.ContainsKey(buttonMenuID);
                if (true == bMenuExists)
                {
                    // ボタンメニュー設定定数ファイルのボタンメニュー定義情報のボタンメニュー情報内のサブメニュー情報の個数を取得する
                    count = (UInt16)CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo[buttonMenuID].SubMenuInfo.Count;

                    // 指定Screen No.が、ボタンメニュー情報のサブメニュー情報の中に含まれる場合
                    if ((1 <= screenNo) && (screenNo <= count))
                    {
                        // 当該ボタンメニュー情報内の当該機能操作エリアのサブメニュー情報の中のボタン個数を取得する
                        count = (UInt16)CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo[buttonMenuID].SubMenuInfo[screenNo].ButtonInfo.Count;

                        // サブメニュー情報内のボタン情報の個数分ループ
                        for (cnt = 0; cnt < count; cnt++)
                        {
                            // ボタン情報のボタンNoが指定されたボタンNoと同じ場合
                            if (buttonNo == CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo[buttonMenuID].SubMenuInfo[screenNo].ButtonInfo[cnt].ButtonNo)
                            {
                                btnIdx = (Int32)cnt;    // 該当ボタン情報のINDEXに該当位置を設定
                                break;
                            }
                            // ボタン情報のボタンNoが指定されたボタンNoと同じではない場合
                            else
                            {
                                // 処理なし
                            }
                        }
                    }
                    // 指定Screen No.が、ボタンメニュー情報のサブメニュー情報の中に含まれていない場合
                    else
                    {
                        strAppLog = String.Format("該当サブメニュー情報なし：count={0:D} screenNo={1:D}", count, screenNo);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.ERROR, MethodBase.GetCurrentMethod(), strAppLog);
                        rtn = 2;                        // 結果に該当サブメニュー情報なしを設定
                    }
                }
                // 指定ボタンメニューIDが、ボタンメニュー定義情報のボタンメニュー情報の中に含まれていない場合
                else
                {
                    strAppLog = String.Format("該当メニュー情報なし：buttonMenuID={0:D}", buttonMenuID);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.ERROR, MethodBase.GetCurrentMethod(), strAppLog);
                    rtn = 1;                            // 結果に該当メニュー情報なしを設定
                }


                // 該当ボタン情報のINDEXが該当なしの場合
                if (btnIdx < 0)
                {
                    strAppLog = String.Format("該当ボタン情報なし：buttonMenuID={0:D} screenNo={1:D} buttonNo={2:D}", buttonMenuID, screenNo, buttonNo);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.ERROR, MethodBase.GetCurrentMethod(), strAppLog);
                    rtn = 3;                            // 結果に該当ボタン情報なしを設定
                }
                // 該当ボタン情報のINDEXが該当ありの場合
                else
                {
                    index = (UInt16)btnIdx;             // ボタン情報のINDEXに該当位置を設定
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                rtn = -1;                               // 結果に例外を設定
            }

            return rtn;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 印刷保存処理
        /// MODULE ID           : PrinSave
        ///
        /// PARAMETER IN        : 
        /// <param name="areaParts">(in)エリア部品</param>
        /// <param name="owner">(in)親Form</param>
        /// <param name="nowButtonMenuID">(in)現在表示中のボタンメニューＩＤ</param>
        /// <param name="funcNameID">(in)機能名称ID</param>
        /// <param name="movementID">(in)動作名称ID</param>
        /// PARAMETER OUT       : 
        /// <param name="rirekiType">(out)履歴種別（0=履歴なし、1=割込履歴）</param>
        /// <param name="isNext">(out)次遷移有無（true=有り、false=無し）</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 印刷保存処理を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void PrinSave(long areaParts, Form owner, UInt16 nowButtonMenuID, UInt16 funcNameID, UInt16 movementID, ref UInt16 rirekiType, ref bool isNext)
        {
            UInt16  formType = 0;                       // フォーム種別
            UInt16  kind = 0;                           // 種別（詳細はCPrintReqMngのenum PRNTYPEを参照）
            bool    isExec = false;                     // 実行取消フラグ（true=実行、false=取消）
            bool    isConfirm = false;                  // 確認画面フラグ（true=確認画面あり、false=確認画面なし）
            bool    isTokushu = false;                  // 特殊条件フラグ（true=成立、false=不成立）
            String  strFuncName = String.Empty;         // 機能名称
            String  strMovement = String.Empty;         // 動作名称
            String  strAppLog = String.Empty;           // ログメッセージ
            String  strDate = String.Empty;             // 日時
            String  syncErrorMsg = String.Empty;        // 同期エラーメッセージ

            // 出力情報を初期化
            rirekiType = 0;                             // 履歴種別をクリア（履歴なし）
            isNext = false;                             // 次遷移有無に無しを設定

            try
            {
                ///////////////////////////////////////////////////////////////////
                // 初期化
                isExec = false;                         // 実行取消フラグ
                isConfirm = false;                      // 確認画面フラグ

                ///////////////////////////////////////////////////////////////////
                // 日時を取得
                strDate = String.Format("{0:yyyyMMdd}_{1:HHmmss}", DateTime.Today, DateTime.Now);

                ///////////////////////////////////////////////////////////////////
                // 機能名称ID，動作名称IDから名称文字列を取得
                strFuncName = CCommon.GetFuncNameFromID(funcNameID);
                strMovement = CCommon.GetMovementFromID(movementID);

                ///////////////////////////////////////////////////////////////////
                // 動作に関連するフラグを設定する
                if ((UInt16)CAppDat.MOVEID.EXEC == movementID)  // 実行
                {
                    isExec = true;
                }
                else if ((UInt16)CAppDat.MOVEID.BACK == movementID)  // 取消
                {
                    isExec = false;
                }
                else if ((UInt16)CAppDat.MOVEID.INITIAL == movementID)  // 初期メニュー
                {
                    // 動作名称が初期メニューの場合は、取消と同じ扱いにする
                    isExec = false;
                }
                else if ((UInt16)CAppDat.MOVEID.CONFIRM == movementID)  // 確認
                {
                    isConfirm = true;
                }
                else if ((UInt16)CAppDat.MOVEID.SETTEI == movementID)  // 設定
                {
                    // 処理なし
                }
                else                                    // 上記以外の場合
                {
                    strAppLog = String.Format("画面印刷保存時の動作名称不正：funcNameID={0:D} movementID={1:D}", funcNameID, movementID);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                }

                ///////////////////////////////////////////////////////////////////
                // フォーム種別と機能指定初期状態種別の初期値を設定する
                formType = (UInt16)CFormPrintKakunin.FORMTYPE.NORMAL;
                                                        // フォーム種別に、通常タイプを設定
                kind = (UInt16)CPrintReqMng.PRNTYPE.PRINSAVE;
                                                        // 機能指定初期状態種別に、「印刷＋保存」を設定

                ///////////////////////////////////////////////////////////////////
                // フォーム種別と機能指定初期状態種別を設定する
                if ((UInt16)CAppDat.FUNCID.GAMEN_PRINT == funcNameID)  // 画面印刷
                {
                    kind = (UInt16)CPrintReqMng.PRNTYPE.PRINT;
                }
                else if ((UInt16)CAppDat.FUNCID.GAMEN_SAVE == funcNameID)  // 画面保存
                {
                    kind = (UInt16)CPrintReqMng.PRNTYPE.SAVE;
                }
                else if ((UInt16)CAppDat.FUNCID.GAMEN_OUTPUT == funcNameID)  // 画面出力
                {
                    //[rem] kind = (UInt16)CPrintReqMng.PRNTYPE.PRINSAVE;
                    if (true == isConfirm)              // 確認画面ありの場合（画面実行タイプ）
                    {
                        formType = (UInt16)CFormPrintKakunin.FORMTYPE.FUNCSELECT;
                                                        // フォーム種別に、機能選択タイプを設定
                        kind = CAppDat.TypeF.GamenInitial;  // 画面出力フォームの印刷／保存機能指定の初期状態を取得
                    }
                    else                                // 確認画面なしの場合
                    {
                        // 処理なし
                    }
                }
                else
                {
                    strAppLog = String.Format("画面印刷保存時の機能名称不正：funcNameID={0:D} movementID={1:D}", funcNameID, movementID);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                }

                ///////////////////////////////////////////////////////////////////
                // 印刷／保存処理を行う
                if (true == isConfirm)                  // 確認画面ありの場合（画面実行タイプ）
                {
                    // 操作内容テーブルの割込機能名称と割込動作名称に、機能名称と動作名称を設定する
                    SetInterFunc(funcNameID, movementID, ref rirekiType, ref isNext);

                    #if CHILD_BTNVISICTRL   // 子画面表示時にボタン表示／非表示制御を行う場合
                    // 子画面を表示する場合は、ボタンを非表示にする（コマンドボタン＋画面出力ボタン非表示）
                    VisibleButton((UInt16)BTNVISITYPE.CMDGAMENBTN_UNVISIBLE, nowButtonMenuID, (UInt16)0xFFFF);
                    #endif

                    // 印刷確認画面を表示し、画面出力処理を行う
                    CPrinSaveCtrl.GamenOutput(areaParts, formType, kind, strFuncName, strMovement, strDate, owner);

                    rirekiType = 0;                     // 履歴種別をクリア（履歴なし）
                    isNext = false;                     // 次遷移有無に無しを設定
                }
                else                                    // 確認画面なしの場合（ボタン実行タイプ）
                {
                    // 操作内容テーブル排他制御開始
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].WaitOne();
                    try
                    {
                        if (String.Empty == strMovement)    // 動作名称が未設定の場合
                        {
                            // 操作内容テーブルの割込フラグに割込機能動作中を設定し、割込機能名称と割込動作名称に、機能名称と動作名称を設定する
                            CAppDat.IopcoT.IntFlg = 1;
                            CAppDat.IopcoT.IntName = strFuncName;
                            CAppDat.IopcoT.IntMovement = strMovement;

                            // 印刷／保存処理を行う
                            CPrinSaveCtrl.ButtonPrinSave(strFuncName, strMovement, strDate);
                            isNext = true;                  // 次遷移有無に有りを設定
                        }
                        else                                // 動作名称がある場合（実行／取消の場合）
                        {
                            isTokushu = false;              // 特殊条件フラグに不成立を設定

                            // 操作内容テーブルの機能名称が「画面出力」の場合
                            if (CAppDat.FuncName[(UInt16)CAppDat.FUNCID.GAMEN_OUTPUT] == CAppDat.IopcoT.IntName)
                            {
                                // 機能指定が「画面印刷」「画面保存」の場合
                                if (((UInt16)CAppDat.FUNCID.GAMEN_PRINT == funcNameID) ||
                                    ((UInt16)CAppDat.FUNCID.GAMEN_SAVE == funcNameID))
                                {
                                    isTokushu = true;       // 特殊条件フラグに成立を設定
                                }
                                // 機能指定が「画面印刷」「画面保存」ではない場合
                                else
                                {
                                    // 処理なし
                                }
                            }
                            // 操作内容テーブルの機能名称が「画面出力」ではない場合
                            else
                            {
                                // 処理なし
                            }
                           
                            // 特殊条件フラグが成立の場合
                            if (true == isTokushu)
                            {
                                // 操作内容テーブルの割込フラグに割込機能動作中を設定し、割込機能名称と割込動作名称に、機能名称と動作名称を設定する
                                CAppDat.IopcoT.IntFlg = 1;
                                CAppDat.IopcoT.IntName = strFuncName;
                                CAppDat.IopcoT.IntMovement = strMovement;
                            }
                            // 操作内容テーブルの機能名称に当該機能の機能名称が設定されていない場合
                            else if (strFuncName != CAppDat.IopcoT.IntName)
                            {
                                // 操作内容テーブルの割込フラグに割込機能動作中を設定し、割込機能名称と割込動作名称に、機能名称と動作名称を設定する
                                CAppDat.IopcoT.IntFlg = 1;
                                CAppDat.IopcoT.IntName = strFuncName;
                                CAppDat.IopcoT.IntMovement = strMovement;

                                // 印刷／保存処理を行う
                                CPrinSaveCtrl.ButtonPrinSave(strFuncName, strMovement, strDate);
                                isNext = true;              // 次遷移有無に有りを設定
                            }
                            // 操作内容テーブルの機能名称に当該機能の機能名称が設定されていて、動作名称が設定の場合
                            else if ((strFuncName == CAppDat.IopcoT.IntName) && ((UInt16)CAppDat.MOVEID.SETTEI == movementID))
                            {
                                // 操作内容テーブルの割込フラグに割込機能動作中を設定し、割込機能名称と割込動作名称に、機能名称と動作名称を設定する
                                CAppDat.IopcoT.IntFlg = 1;
                                CAppDat.IopcoT.IntName = strFuncName;
                                CAppDat.IopcoT.IntMovement = strMovement;

                                // 印刷／保存処理を行う
                                CPrinSaveCtrl.ButtonPrinSave(strFuncName, strMovement, strDate);
                                isNext = true;              // 次遷移有無に有りを設定
                            }
                            // 操作内容テーブルの機能名称に当該機能の機能名称が設定されている場合
                            else
                            {
                                // 処理なし
                            }

                            if ((UInt16)CAppDat.MOVEID.SETTEI != movementID)  // 設定ではない
                            {
                                // 印刷保存実行／取消処理を行う
                                CPrinSaveCtrl.ButtonExecOrCancel(isExec, kind);
                                isNext = true;              // 次遷移有無に有りを設定

                                if (true == isExec)         // 実行の場合
                                {
                                    CAppDat.IopcoT.IntFlg = 0;
                                    CAppDat.IopcoT.IntName = String.Empty;
                                    CAppDat.IopcoT.IntMovement = String.Empty;
                                }
                                else
                                {
                                    // 処理なし
                                }
                            }
                            else                            // 上記以外の場合
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
                        // 操作内容テーブル排他制御終了
                        CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.OPCOINFO].ReleaseMutex();
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
        /// MODULE NAME         : 鳴動停止処理
        /// MODULE ID           : MeidouStop
        ///
        /// PARAMETER IN        : 
        /// <param name="funcNameID">(in)機能名称ＩＤ</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 鳴動停止（鳴動停止／警報音停止／接近音停止／現場要請音停止／提案音停止／ブザー音停止）を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void MeidouStop(UInt16 funcNameID)
        {
            UInt16  actKind = 0;                        // 動作種別
            UInt16  groupKind = 0;                      // 鳴動種別
            String  strMeidouID = String.Empty;         // 鳴動要求
            UInt16  timerNo = 0;                        // タイマＮｏ
            UInt16  id = 0;                             // ＩＤ
            UInt16  soundKind = 0;                      // 鳴動対象種別
            UInt16  meidouTeishiTarget = 0;             // 鳴動停止対象
            String  strFuncName = String.Empty;         // 機能名称
            String  strAppLog = String.Empty;           // ログメッセージ

            try
            {
                strFuncName = CCommon.GetFuncNameFromID(funcNameID);
                                                        // 機能名称を取得する

                actKind = (UInt16)CMeidouReqMng.ACTKIND.NONE;
                                                        // 動作種別になしを設定

                soundKind = (UInt16)CMeidouReqMng.SOUNDKIND.NONE;
                                                        // 鳴動対象種別に「なし」を設定
                meidouTeishiTarget = CAppDat.TypeF.Keihou.MeidouTeishiTarget;
                                                        // 鳴動停止対象を取得

                ///////////////////////////////////////////////////////////////////
                // 鳴動要求の準備を行う

                // 鳴動停止の場合
                if ((UInt16)CAppDat.FUNCID.MEIDOU_TEISHI == funcNameID)
                {
                    actKind = (UInt16)CMeidouReqMng.ACTKIND.MEIDOUTEISI;
                                                        // 動作種別に鳴動停止を設定

                    soundKind = meidouTeishiTarget;     // 鳴動対象種別に鳴動停止対象を設定
                }
                // 警報音停止の場合
                else if ((UInt16)CAppDat.FUNCID.STOPSOUND_KEIHOU == funcNameID)
                {
                    actKind = (UInt16)CMeidouReqMng.ACTKIND.KEIHOUTEISI;
                                                        // 動作種別に警報音停止を設定
                    strMeidouID = CRumblingManage.MEID_STOPSOUND_KEIHOU;
                                                        // 鳴動要求に「鳴動中警報音全停止」を設定
                }
                // 接近音停止の場合
                else if ((UInt16)CAppDat.FUNCID.STOPSOUND_SEKKIN == funcNameID)
                {
                    actKind = (UInt16)CMeidouReqMng.ACTKIND.SEKKINTEISI;
                                                        // 動作種別に接近音停止を設定
                    strMeidouID = CRumblingManage.MEID_STOPSOUND_SEKKIN;
                                                        // 鳴動要求に「鳴動中接近音全停止」を設定
                }
                // 現場要請音停止の場合
                else if ((UInt16)CAppDat.FUNCID.STOPSOUND_GENBAYOUSEI == funcNameID)
                {
                    actKind = (UInt16)CMeidouReqMng.ACTKIND.GENBAYOUSEITEISI;
                                                        // 動作種別に現場要請音停止を設定
                    strMeidouID = CRumblingManage.MEID_STOPSOUND_GENBAYOUSEI;
                                                        // 鳴動要求に「鳴動中現場要請音全停止」を設定
                }
                // 提案音停止の場合
                else if ((UInt16)CAppDat.FUNCID.STOPSOUND_TEIAN == funcNameID)
                {
                    actKind = (UInt16)CMeidouReqMng.ACTKIND.TEIANTEISI;
                                                        // 動作種別に提案停止を設定
                    strMeidouID = CRumblingManage.MEID_STOPSOUND_TEIAN;
                                                        // 鳴動要求に「鳴動中提案音全停止」を設定
                }
                // ブザー音停止の場合
                else if ((UInt16)CAppDat.FUNCID.STOPSOUND_BUZZER == funcNameID)
                {
                    // 接近音停止のパラメータを設定する
                    actKind = (UInt16)CMeidouReqMng.ACTKIND.SEKKINTEISI;
                                                        // 動作種別に接近音停止を設定
                    strMeidouID = CRumblingManage.MEID_STOPSOUND_SEKKIN;
                                                        // 鳴動要求に「鳴動中接近音全停止」を設定
                    // 鳴動要求（接近音停止）を行う
                    CKeihouCom.OutputMeidou((Byte)actKind,
                                            (Byte)groupKind,
                                            strMeidouID,
                                            timerNo,
                                            id,
                                            true);

                    // 現場要請音停止のパラメータを設定する
                    actKind = (UInt16)CMeidouReqMng.ACTKIND.GENBAYOUSEITEISI;
                                                        // 動作種別に現場要請音停止を設定
                    strMeidouID = CRumblingManage.MEID_STOPSOUND_GENBAYOUSEI;
                                                        // 鳴動要求に「鳴動中現場要請音全停止」を設定
                    // 鳴動要求（現場要請音停止）を行う
                    CKeihouCom.OutputMeidou((Byte)actKind,
                                            (Byte)groupKind,
                                            strMeidouID,
                                            timerNo,
                                            id,
                                            true);

                    // 提案音停止のパラメータを設定する
                    actKind = (UInt16)CMeidouReqMng.ACTKIND.TEIANTEISI;
                                                        // 動作種別に提案停止を設定
                    strMeidouID = CRumblingManage.MEID_STOPSOUND_TEIAN;
                                                        // 鳴動要求に「鳴動中提案音全停止」を設定
                }
                // 上記以外の場合
                else
                {
                    strAppLog = String.Format("機能名称ＩＤが不正：{0}（{1:D}）", strFuncName, funcNameID);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                }


                ///////////////////////////////////////////////////////////////////
                // 鳴動要求（鳴動停止要求）を行う

                if ((UInt16)CMeidouReqMng.ACTKIND.NONE == actKind)
                {
                    // 処理なし
                }
                else
                {
                    // 鳴動要求を行う
                    CKeihouCom.OutputMeidouKind((Byte)actKind,
                                                (Byte)groupKind,
                                                strMeidouID,
                                                timerNo,
                                                id,
                                                soundKind,
                                                true);
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 警報履歴画面表示処理
        /// MODULE ID           : DispAlarmRireki
        ///
        /// PARAMETER IN        : 
        /// <param name="areaParts">(in)エリア部品</param>
        /// <param name="owner">(in)親フォーム</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 警報履歴画面を表示する。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void DispAlarmRireki(long areaParts, Form owner)
        {
            Byte    byRet = 0;                          // 戻り値取得用
            Int32   posiX = -1;                         // Ｘ座標
            Int32   posiY = -1;                         // Ｙ座標
            Byte    postType = 0;                       // フォーム表示位置種別
            String  strFormName = String.Empty;         // フォーム名
            String  strAppLog = String.Empty;           // ログメッセージ
            String  syncErrorMsg = String.Empty;        // 同期エラーメッセージ

            try
            {
                strFormName = CFormAlarmRireki.FormName;// フォーム名を取得

                byRet = CCommon.GetFormPosition(strFormName, ref postType, ref posiX, ref posiY);
                                                        // フォーム初期位置取得（画面表示位置取得）
                if (CCommon.RTN_OK == byRet)            // フォーム初期位置取得正常の場合
                {
                    // 処理なし
                }
                else                                    // フォーム初期位置取得失敗の場合
                {
                    strAppLog = String.Format("フォーム初期位置取得失敗：strFormName={0} byRet={1:D}", strFormName, byRet);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                }

                // 履歴フォームポインタ更新排他制御開始
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.RIREKIPOINTER].WaitOne();
                try
                {
                    // 履歴フォームを表示する
                    if (null == CAppDat.FormAlarmRireki)
                    {
                        CAppDat.FormAlarmRireki = new CFormAlarmRireki(posiX, posiY, owner);
                        CAppDat.FormAlarmRireki.SetAreaParts(areaParts);
                        CAppDat.FormAlarmRireki.Show();
                    }
                    else
                    {
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), "履歴フォーム表示済み");
                        CAppDat.FormAlarmRireki.SetAreaParts(areaParts);
                        // 呼び出した画面はアクティブにしておく
                        CAppDat.FormAlarmRireki.Activate();
                    }
                    CCommon.SetButtonMenuTreeTDlgShowFlg((UInt32)CAppDat.KOGAMENID.KEIHOU_RIREKI);
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
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : バージョン情報表示処理
        /// MODULE ID           : DispVersion
        ///
        /// PARAMETER IN        : 
        /// <param name="areaParts">(in)エリア部品</param>
        /// <param name="owner">(in)親フォーム</param>
        /// <param name="strFuncName">(in)機能名称</param>
        /// <param name="strFuncMovement">(in)機能動作名称</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// バージョン情報画面を表示し、バージョン情報の表示を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void DispVersion(long areaParts, Form owner, String strFuncName, String strFuncMovement)
        {
            Byte    byRet = 0;                          // 戻り値取得用
            Int32   posiX = -1;                         // Ｘ座標
            Int32   posiY = -1;                         // Ｙ座標
            Byte    postType = 0;                       // フォーム表示位置種別
            String  strEqpName = String.Empty;          // 装置名
            String  strProgVersion = String.Empty;      // プログラムバージョン
            String  strDatVersion = String.Empty;       // 定数バージョン
            String  strFormName = String.Empty;         // フォーム名
            String  strAppLog = String.Empty;           // ログメッセージ

            try
            {
                strEqpName = CCommon.GetEqpName();  // 装置名を取得

                strProgVersion = CAppDat.ProgVersion;       // プログラムバージョン
                strDatVersion  = CAppDat.TypeF.DatVersion;  // 定数バージョン


                strFormName = CFormVersionInfo.FormName;// フォーム名を取得

                byRet = CCommon.GetFormPosition(strFormName, ref postType, ref posiX, ref posiY);
                                                        // フォーム初期位置取得（画面表示位置取得）
                if (CCommon.RTN_OK == byRet)            // フォーム初期位置取得正常の場合
                {
                    // 処理なし
                }
                else                                    // フォーム初期位置取得失敗の場合
                {
                    strAppLog = String.Format("フォーム初期位置取得失敗：strFormName={0} byRet={1:D}", strFormName, byRet);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                }

                // バージョン情報表示フォームを表示する
                if (null == CAppDat.FormVersionInfo)
                {
                    CAppDat.FormVersionInfo = new CFormVersionInfo(posiX, posiY, owner);
                    CAppDat.FormVersionInfo.SetAreaParts(areaParts);
                    CAppDat.FormVersionInfo.SetVersion(strEqpName, strProgVersion, strDatVersion);
                    CAppDat.FormVersionInfo.Show();

                    if (String.Empty == strFuncName)
                    {
                        // 処理なし
                    }
                    else
                    {
                        // 操作履歴出力要求を行う
                        CKeihouCom.SetSousaRireki(strFuncName, strFuncMovement);
                    }
                }
                else
                {
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), "バージョン情報フォーム表示済み");
                    CAppDat.FormVersionInfo.SetAreaParts(areaParts);
                    // 呼び出した画面はアクティブにしておく
                    CAppDat.FormVersionInfo.Activate();
                }
                CCommon.SetButtonMenuTreeTDlgShowFlg((UInt32)CAppDat.KOGAMENID.VERSIONINFO);
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 接続解除提案設定画面表示処理
        /// MODULE ID           : DispTeianConCancel
        ///
        /// PARAMETER IN        : 
        /// <param name="teiKanIndex">(in)提案管理テーブルの提案情報INDEX(0-N)</param>
        /// <param name="timerVal">(in)提案アンサ監視タイマ値（単位:100mS）</param>
        /// <param name="areaParts">(in)エリア部品</param>
        /// <param name="owner">(in)親フォーム</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 接続解除提案設定画面を表示する。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void DispTeianConCancel(Int32 teiKanIndex, UInt16 timerVal, long areaParts, Form owner)
        {
            Byte    byRet = 0;                          // 戻り値取得用
            Int32   posiX = -1;                         // Ｘ座標
            Int32   posiY = -1;                         // Ｙ座標
            Byte    postType = 0;                       // フォーム表示位置種別
            String  strFormName = String.Empty;         // フォーム名
            String  strAppLog = String.Empty;           // ログメッセージ

            try
            {
                strFormName = CFormTeianConCancel.FormName;// フォーム名を取得

                byRet = CCommon.GetFormPosition(strFormName, ref postType, ref posiX, ref posiY);
                                                        // フォーム初期位置取得（画面表示位置取得）
                if (CCommon.RTN_OK == byRet)            // フォーム初期位置取得正常の場合
                {
                    // 処理なし
                }
                else                                    // フォーム初期位置取得失敗の場合
                {
                    strAppLog = String.Format("フォーム初期位置取得失敗：strFormName={0} byRet={1:D}", strFormName, byRet);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                }

                // 接続解除提案設定フォームポインタ更新排他制御開始
                // （20140514:コメント化開始） モードレス→モーダルに変更
                //[rem] CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.TEIANCONCANCELPOINTER].WaitOne();
                // （20140514:コメント化終了） モードレス→モーダルに変更

                try
                {
                    // 接続解除提案設定フォームを表示する
                    if (null == CAppDat.FormTeianConCancel)
                    {
                        CAppDat.FormTeianConCancel = new CFormTeianConCancel(posiX, posiY, owner, teiKanIndex, timerVal);
                        CAppDat.FormTeianConCancel.SetAreaParts(areaParts);
                        // （20140514:変更開始） モードレス→モーダルに変更
                        //[mod] CAppDat.FormTeianConCancel.Show();
                        // 2014/11/25 モーダル→モードレスに変更開始
                        //[mod] CAppDat.FormTeianConCancel.ShowDialog();
                        //[mod] CAppDat.FormTeianConCancel.Dispose();
                        CAppDat.FormTeianConCancel.Show();
                        // 2014/11/25 モーダル→モードレスに変更終了
                        // （20140514:変更終了） モードレス→モーダルに変更
                    }
                    else
                    {
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), "接続解除提案設定フォーム表示済み");
                        var msg = new CMyMessageBox();
                        msg.ButtonText.OK = "確　認";
                        msg.Show("提案出力画面が表示済みです。\n提案出力画面を終了してから操作を行って下さい。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 始発予告窓表示処理
        /// MODULE ID           : ShihatsuHyouji
        ///
        /// PARAMETER IN        : 
        /// <param name="isDisp">(in)始発予告窓表示指定（true=表示する／false=表示しない）</param>
        /// <param name="isEki">(in)駅単位指定有無（true=有り／false=無し）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// ＤＣＰ設定定義ファイルの予告窓表示設定を更新し、窓表示処理の起動要求を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void ShihatsuHyouji(bool isDisp, bool isEki = false)
        {
            Byte   byDisp = 0;                          // 表示する／しない／駅単位表示
            String syncErrorMsg = String.Empty;         // 同期エラーメッセージ
            CDrawKukanMng drawDataT = null;             // 画面描画テーブルワークエリア

            try
            {
                // ＤＣＰ設定定義ファイルの予告窓表示設定を更新する
                if (true == isDisp)                     // 表示する場合
                {
                    if (true == isEki)                     // 駅単位表示する場合
                    {
                        byDisp = (Byte)CDCPSETTEIiniMng.DISP.DISP_EKI;      // 駅単位表示状態
                    }
                    else
                    {
                        byDisp = (Byte)CDCPSETTEIiniMng.DISP.DISP;          // 表示する
                    }
                }
                else                                    // 表示しない場合
                {
                    byDisp = (Byte)CDCPSETTEIiniMng.DISP.NON;          // 表示しない                                                        
                }

                // 排他制御を行い、ＤＣＰ設定定義ファイルの予告窓表示設定を更新する
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DCPSET].WaitOne();
                try
                {
                    CAppDat.DCPSETTEI.YokokuMado = byDisp;
                    // iniファイルに予告窓表示設定を設定
                    CKeihouCom.SetFileDCPSETTEIIni(CKeihouCom.DCPSETTEIKIND.YOKOKU_MADO, 0, (UInt16)byDisp);
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


                // 表示画面情報テーブルの情報を取得する
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.GAMENINFO].WaitOne();
                try
                {
                    drawDataT = (CDrawKukanMng)CAppDat.GamenInfoT.Clone();
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.GAMENINFO].ReleaseMutex();
                }

                // 画面Ｎｏの数分ループして窓状態テーブルの変化フラグを設定する
                foreach (UInt16 gIndex in drawDataT.Crno)
                {
                    SetMadoChangeFlag(gIndex, true);    // 窓状態テーブルの変化フラグ設定処理
                }


                // 窓表示処理起動要求を行う
                CAppDat.HandleCtrl[(UInt16)CAppDat.WAITID.MADODISP].Set();
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
            finally
            {
                drawDataT = null;
            }
            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 現発送信窓表示処理
        /// MODULE ID           : GenpatuSousinMadoHyouji
        ///
        /// PARAMETER IN        : 
        /// <param name="isDisp">(in)現発送信窓表示指定（true=表示する／false=表示しない）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// ＤＣＰ設定定義ファイルの現発送信窓表示設定を更新し、窓表示処理の起動要求を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void GenpatuSousinMadoHyouji(bool isDisp)
        {
            Byte   byDisp = 0;                          // 表示する／しない／駅単位表示
            String syncErrorMsg = String.Empty;         // 同期エラーメッセージ
            CDrawKukanMng drawDataT = null;             // 画面描画テーブルワークエリア

            try
            {
                // ＤＣＰ設定定義ファイルの現発送信窓表示設定を更新する
                if (true == isDisp)                     // 表示する場合
                {
                    byDisp = (Byte)CDCPSETTEIiniMng.DISP.DISP;  // 表示する
                }
                else                                    // 表示しない場合
                {
                    byDisp = (Byte)CDCPSETTEIiniMng.DISP.NON;   // 表示しない                                                        
                }

                // 排他制御を行い、ＤＣＰ設定定義ファイルの現発送信窓表示設定を更新する
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DCPSET].WaitOne();
                try
                {
                    CAppDat.DCPSETTEI.GenpatuSousinMado = byDisp;
                    // iniファイルに現発送信窓表示設定を設定
                    CKeihouCom.SetFileDCPSETTEIIni(CKeihouCom.DCPSETTEIKIND.GENPATUSOUSIN_MADO, 0, (UInt16)byDisp);
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


                // 表示画面情報テーブルの情報を取得する
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.GAMENINFO].WaitOne();
                try
                {
                    drawDataT = (CDrawKukanMng)CAppDat.GamenInfoT.Clone();
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.GAMENINFO].ReleaseMutex();
                }

                // 画面Ｎｏの数分ループして窓状態テーブルの変化フラグを設定する
                foreach (UInt16 gIndex in drawDataT.Crno)
                {
                    SetMadoChangeFlag(gIndex, true);    // 窓状態テーブルの変化フラグ設定処理
                }


                // 窓表示処理起動要求を行う
                CAppDat.HandleCtrl[(UInt16)CAppDat.WAITID.MADODISP].Set();
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
            finally
            {
                drawDataT = null;
            }
            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 窓状態テーブル変化フラグ設定処理
        /// MODULE ID           : SetMadoChangeFlag
        ///
        /// PARAMETER IN        : 
        /// <param name="gIndex">(in)対象の画面番号</param>
        /// <param name="isChange">(in)変化フラグ設定値</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 窓状態テーブルの変化フラグの設定を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void SetMadoChangeFlag(UInt16 gIndex, bool isChange)
        {
            String syncErrorMsg = String.Empty;

            try
            {
                // 排他制御を行い、窓状態テーブルの変化フラグを変化ありにする
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.MADOSTATUS].WaitOne();
                try
                {
                    // 窓状態テーブルの情報数分、処理を繰り返す
                    foreach (String KeyID in CAppDat.MadoStatusT.Condition[gIndex].Keys)
                    {
                        CAppDat.MadoStatusT.Condition[gIndex][KeyID].IsOperateChangeable = isChange;
                                                        // 窓状態テーブルの変化フラグを設定する
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
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 表示てこラベル表示処理
        /// MODULE ID           : DispTekoLabelHyouji
        ///
        /// PARAMETER IN        : 
        /// <param name="isDisp">(in)表示てこラベル表示指定（true=表示する／false=表示しない）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// ＤＣＰ設定定義ファイルの表示てこラベル表示設定を更新し、表示てこラベル表示処理の起動要求を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void DispTekoLabelHyouji(bool isDisp)
        {
            Byte byDisp = 0;                            // 表示する／しない／駅単位表示
            String syncErrorMsg = String.Empty;         // 同期エラーメッセージ
            CDrawKukanMng drawDataT = null;             // 画面描画テーブルワークエリア

            try
            {
                // ＤＣＰ設定定義ファイルの表示てこラベル表示設定を更新する
                if (true == isDisp)                     　 // 表示する場合
                {
                    byDisp = (Byte)CDCPSETTEIiniMng.DISP.DISP;          // 表示する
                }
                else                                    　 // 表示しない場合
                {
                    byDisp = (Byte)CDCPSETTEIiniMng.DISP.NON;          // 表示しない                                                        
                }

                // 排他制御を行い、ＤＣＰ設定定義ファイルの表示てこラベル表示設定を更新する
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DCPSET].WaitOne();
                try
                {
                    CAppDat.DCPSETTEI.Disptekolabel = byDisp;
                    // iniファイルに表示てこラベル表示設定を設定
                    CKeihouCom.SetFileDCPSETTEIIni(CKeihouCom.DCPSETTEIKIND.DISPTEKO_LABEL, 0, (UInt16)byDisp);
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


                // 表示画面情報テーブルの情報を取得する
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.GAMENINFO].WaitOne();
                try
                {
                    drawDataT = (CDrawKukanMng)CAppDat.GamenInfoT.Clone();
                }
                catch (Exception ex)
                {
                    // ミューテックス取得中に発生した例外の捕捉
                    syncErrorMsg = "〇 同期中エラー発生 " + ex.Message;
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), syncErrorMsg);
                }
                finally
                {
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.GAMENINFO].ReleaseMutex();
                }

                // 画面Ｎｏの数分ループして表示てこラベル状態テーブルの変化フラグを設定する
                foreach (UInt16 gIndex in drawDataT.Crno)
                {
                    SetDispTekoLabelChangeFlag(gIndex, true);    // 表示てこラベル状態テーブルの変化フラグ設定処理
                }


                // 表示てこラベル表示処理起動要求を行う
                CAppDat.HandleCtrl[(UInt16)CAppDat.WAITID.DISPTEKOLABELDISP].Set();
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
            finally
            {
                drawDataT = null;
            }
            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 表示てこラベル状態テーブル変化フラグ設定処理
        /// MODULE ID           : SetDispTekoLabelChangeFlag
        ///
        /// PARAMETER IN        : 
        /// <param name="gIndex">(in)対象の画面番号</param>
        /// <param name="isChange">(in)変化フラグ設定値</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 表示てこラベル状態テーブルの変化フラグの設定を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void SetDispTekoLabelChangeFlag(UInt16 gIndex, bool isChange)
        {
            String syncErrorMsg = String.Empty;

            try
            {
                // 排他制御を行い、表示てこラベル状態テーブルの変化フラグを変化ありにする
                CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.DISPTEKOLABELSTATUS].WaitOne();
                try
                {
                    // 表示てこラベル状態テーブルの情報数分、処理を繰り返す
                    foreach (String KeyID in CAppDat.DispTekoLabelStatusT.Condition[gIndex].Keys)
                    {
                        CAppDat.DispTekoLabelStatusT.Condition[gIndex][KeyID].IsOperateChangeable = isChange;
                        // 表示てこラベル状態テーブルの変化フラグを設定する
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
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 装置切替処理
        /// MODULE ID           : ChangeEQP
        ///
        /// PARAMETER IN        : 
        /// <param name="eqpMode">(in)装置モード</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// アプリ再起動型の装置切替要求処理を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void ChangeEQP(UInt16 eqpMode)
        {
            String  strAppLog = String.Empty;           // ログメッセージ

            try
            {
                if ((UInt16)CDCPiniMng.MODE.SDP == eqpMode)   // 表示卓切替の場合
                {
                    CAppDat.EqpEndState = 2;
                }
                else                                          // 制御卓切替の場合
                {
                    CAppDat.EqpEndState = 1;
                }

                strAppLog = String.Format("装置切替実行：{0:D}", CAppDat.EqpEndState);
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.INFORMATION, MethodBase.GetCurrentMethod(), strAppLog);

                // アプリケーションを終了する
                Application.Exit();
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return;
        }
    }
}
