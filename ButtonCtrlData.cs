//********************************************************************************
//
//  SYSTEM          : DCP
//  UNIT            : コマンドボタン処理データクラス
//
//********************************************************************************
using System;
using System.Reflection;
using System.Collections.Generic;

namespace DCP
{
    ///*******************************************************************************
    ///
    /// CLASS NAME      : コマンドボタン処理データクラス
    /// CLASS ID        : CButtonCtrlData
    ///
    /// FUNCTION        : 
    /// <summary>
    /// コマンドボタン処理で使用する情報を格納するデータクラスとする。
    /// </summary>
    /// 
    ///*******************************************************************************
    class CButtonCtrlData
    {
        #region <定数>

        /// <summary>画面種別指定</summary>
        public enum EGAMENKIND : ushort
        {
            ///<summary>No.00 なし</summary>
            NONE = 0,
            ///<summary>No.01 同種別指定</summary>
            SAMEGAMEN,
            ///<summary>No.02 個別画面</summary>
            KOBETU,
            ///<summary>No.03 全体画面</summary>
            ZENTAI,
            ///<summary>No.04 分割画面１</summary>
            BUNKATSU1,
            ///<summary>No.05 分割画面２</summary>
            BUNKATSU2,
            ///<summary>No.06 (総数)</summary>
            EGAMENKINDCOUNT
        };

        /// <summary>画面切替種別</summary>
        public enum EGAMENCHGKIND : ushort
        {
            ///<summary>No.00 なし</summary>
            NONE = 0,
            ///<summary>No.01 順送り</summary>
            JYUNOKURI,
            ///<summary>No.02 駅名選択</summary>
            EyangEISELECT,
            ///<summary>No.03 ボタン</summary>
            BUTTON,
            ///<summary>No.04 キーボード</summary>
            KEYBOARD,
            ///<summary>No.05 マウス</summary>
            MOUSE,
            ///<summary>No.06 ダイアログ</summary>
            DIALOG,
            ///<summary>No.07 (総数)</summary>
            EGAMENCHGKINDCOUNT
        };

        #endregion

        #region <プロパティ>

        /// <summary>機能名称ＩＤ</summary>
        public Int32 FuncnameID { get; set; }
        /// <summary>動作名称ＩＤ</summary>
        public Int32 MovementID { get; set; }
        /// <summary>遷移先画面No.</summary>
        public Int32 SeniGamenNo { get; set; }
        /// <summary>ボタンNo.</summary>
        public UInt16 ButtonNo { get; set; }
        /// <summary>ボタンID</summary>
        public UInt16 ButtonID { get; set; }
        /// <summary>次メニューＩＤ</summary>
        public UInt16 NextMenuID { get; set; }
        /// <summary>処理後次メニューＩＤ１</summary>
        public UInt16 AfterNextMenuID1 { get; set; }
        /// <summary>処理後次メニューＩＤ２</summary>
        public UInt16 AfterNextMenuID2 { get; set; }
        /// <summary>画面切替種別</summary>
        public UInt16 GamenChgKind { get; set; }
        /// <summary>画面種別指定</summary>
        public UInt16 GamenKind { get; set; }
        /// <summary>キー起動</summary>
        public bool KeyOn { get; set; }
        /// <summary>ガイダンスID</summary>
        public UInt16 GuideID { get; set; }

        #endregion

        ///*******************************************************************************
        /// MODULE NAME         : コンストラクタ
        /// MODULE ID           : CButtonCtrlData
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
        public CButtonCtrlData()
        {
        }

        ///*******************************************************************************
        /// MODULE NAME         : 初期化処理
        /// MODULE ID           : Clear
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
        /// クラス変数の初期化処理を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        public void Clear()
        {
            try
            {
                this.FuncnameID   = -1;                 // 機能名称ＩＤ
                this.MovementID   = -1;                 // 動作名称ＩＤ
                this.SeniGamenNo  = -1;                 // 遷移先画面No.
                this.ButtonNo     = 0;                  // ボタンNo.
                this.ButtonID     = 0;                  // ボタンID
                this.NextMenuID   = 0;                  // 次メニューＩＤ
                this.AfterNextMenuID1 = 0;              // 処理後次メニューＩＤ１
                this.AfterNextMenuID2 = 0;              // 処理後次メニューＩＤ２
                this.GamenChgKind = 0;                  // 画面切替種別
                this.GamenKind    = 0;                  // 画面種別指定
                this.KeyOn = false;                     // キー起動
                this.GuideID = 0;                       // ガイダンスID
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }
    }
}
