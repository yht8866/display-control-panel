//********************************************************************************
//
//  SYSTEM          : DCP
//  UNIT            : エリア処理
//
//********************************************************************************
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DCP.MngData.File;
using DCP.MngData.Table;

namespace DCP
{
    ///*******************************************************************************
    ///
    /// CLASS NAME      : ユーザー定義ラベルクラス
    /// CLASS ID        : VisiblityLabel
    ///
    /// FUNCTION        : 
    /// <summary>
    /// 実際に見えているラベルのクラス定義を行う。
    /// </summary>
    /// 
    ///*******************************************************************************
    public class VisiblityLabel : System.Windows.Forms.Label
    {
        #region <プロパティ>

        /// <summary>ラベルの使用有無</summary>
        public bool IsVisiblity { get; set; }

        #endregion

        ///*******************************************************************************
        /// MODULE NAME         : コンストラクタ
        /// MODULE ID           : VisiblityLabel
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
        public VisiblityLabel()
        {
            this.IsVisiblity = false;
        }
    }

    ///*******************************************************************************
    ///
    /// CLASS NAME      : ユーザー定義ボタンクラス
    /// CLASS ID        : NotSelectableButton
    ///
    /// FUNCTION        : 
    /// <summary>
    /// 選択状態にならないボタンのクラス定義を行う。
    /// </summary>
    /// 
    ///*******************************************************************************
    public class NotSelectableButton : System.Windows.Forms.Button
    {
        #region <プロパティ>

        /// <summary>ボタンの使用有無</summary>
        public bool IsVisiblity { get; set; }

        /// <summary>ボタンの押下状態</summary>
        public bool IsPressed { get; set; }

        #endregion

        ///*******************************************************************************
        /// MODULE NAME         : コンストラクタ
        /// MODULE ID           : NotSelectableButton
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
        public NotSelectableButton()
        {
            // コントロールはフォーカスを受け取らない
            this.SetStyle(ControlStyles.Selectable, false);
            this.IsVisiblity = false;
            this.IsPressed = false;
        }
    }

    ///*******************************************************************************
    ///
    /// CLASS NAME      : エリアクラス
    /// CLASS ID        : CAreaUserControl
    ///
    /// FUNCTION        : 
    /// <summary>
    /// エリアの表示を行う。
    /// </summary>
    /// 
    ///*******************************************************************************
    public partial class CAreaUserControl : UserControl
    {
        #region <定数>

        /// <summary>ラベル登録最大数</summary>
        public const short LABEL_MAX  = 20;
        /// <summary>ボタン登録最大数</summary>
        public const short BUTTON_MAX = 30;

        /// <summary>表示部品ラベル</summary>
        public const UInt16 PARTSTYPE_LABEL = 1;
        /// <summary>表示部品ボタン</summary>
        public const UInt16 PARTSTYPE_BUTTON = 2;

        /// <summary>表示指定種別</summary>
        public enum VISIBLEKIND : ushort
        {
            ///<summary>No.00 非表示</summary>
            NONVISIBLE = 0,
            ///<summary>No.01 不活性</summary>
            DISABLE,
            ///<summary>No.02 表示</summary>
            VISIBLE,
            ///<summary>No.03 (総数)</summary>
            VISIBLEKINDCOUNT
        };

        /// <summary>子画面部品種別</summary>
        public enum CHILDKIND : ushort
        {
            ///<summary>No.00 なし</summary>
            NONE = 0,
            ///<summary>No.01 実行</summary>
            EXEC,
            ///<summary>No.02 取消</summary>
            CANCEL,
            ///<summary>No.03 設定</summary>
            SETTEI,
            ///<summary>No.04 解除</summary>
            RELEASE,
            ///<summary>No.05 (総数)</summary>
            CHILDKINDCOUNT
        };

        /// <summary>装置状態表示エリア</summary>
        public const UInt16 AREAID_SOUTI = 1;
        /// <summary>線形表示エリア</summary>
        public const UInt16 AREAID_SENKEI = 2;
        /// <summary>提案表示エリア</summary>
        public const UInt16 AREAID_TEIAN = 3;
        /// <summary>警報表示エリア</summary>
        public const UInt16 AREAID_KEIHOU = 4;
        /// <summary>ガイダンス表示エリア</summary>
        public const UInt16 AREAID_GUIDANCE = 5;
        // /// <summary>機能操作表示エリア</summary>
        // public const UInt16 AREAID_SOUSA = 6;
        /// <summary>メニューボタン表示エリア</summary>
        public const UInt16 AREAID_MENU = 6;
        /// <summary>扱い警報表示エリア</summary>
        public const UInt16 AREAID_ATSUKAI = 7;
        /// <summary>操作ボタン表示エリア</summary>
        public const UInt16 AREAID_CONTROL = 8;

        /// <summary>装置状態表示エリア比較文字列</summary>
        public const String AREANAME_SOUTI = @"装置状態表示エリア";
        /// <summary>線形表示エリア比較文字列</summary>
        public const String AREANAME_SENKEI = @"線形表示エリア";
        /// <summary>提案表示エリア比較文字列</summary>
        public const String AREANAME_TEIAN = @"提案表示エリア";
        /// <summary>警報表示エリア比較文字列</summary>
        public const String AREANAME_KEIHOU = @"警報表示エリア";
        /// <summary>ガイダンス表示エリア比較文字列</summary>
        public const String AREANAME_GUIDANCE = @"ガイダンス表示エリア";
        // /// <summary>機能操作表示エリア比較文字列</summary>
        // public const String AREANAME_SOUSA = @"機能操作表示エリア";
        /// <summary>メニューボタン表示エリア比較文字列</summary>
        public const String AREANAME_MENU = @"メニューボタン表示エリア";
        /// <summary>扱い警報表示エリア比較文字列</summary>
        public const String AREANAME_ATSUKAI = @"扱い警報表示エリア";
        /// <summary>操作ボタン表示エリア比較文字列</summary>
        public const String AREANAME_CONTOROL = @"操作ボタン表示エリア";

        #endregion

        #region <メンバ変数>

        /// <summary>ラベル</summary>
        //private System.Windows.Forms.Label[] m_LabelList = null;
        private VisiblityLabel[] m_LabelList = null;
        /// <summary>ボタン</summary>
        //private System.Windows.Forms.Button[] m_ButtonList = null;
        private NotSelectableButton[] m_ButtonList = null;

        /// <summary>エリアID</summary>
        private UInt16 m_AreaID = 0;
        /// <summary>Screen No.</summary>
        private UInt16 m_ScreenNo = 0;

        /// <summary>表示切替データ格納エリア（表示切替を行うと枠もブリンクするためテキストを切替える）</summary>
        private String m_VisibleAnsCodeText = String.Empty;

        #endregion

        #region <イベント>

        /// <summary>イベントデリゲートの宣言</summary>
        public event CNoticeEventHandler AreaPartsClick;

        #endregion

        ///*******************************************************************************
        /// MODULE NAME         : コンストラクタ
        /// MODULE ID           : CAreaUserControl
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
        public CAreaUserControl()
        {
            try
            {
                InitializeComponent();

                CreateParts();
                this.TimerBlink.Start();
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 最終処理
        /// MODULE ID           : Dispose
        ///
        /// PARAMETER IN        : 
        /// <param name="disposing">(in)リソース解放（true=する、false=しない）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 終了時の後処理を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);

                DeleteParts();
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 部品詳細情報取得処理
        /// MODULE ID           : GetPartsDtl
        ///
        /// PARAMETER IN        : 
        /// <param name="areaParts">(in)エリア部品</param>
        /// PARAMETER OUT       : 
        /// <param name="areaID">(out)エリアＩＤ</param>
        /// <param name="screenNo">(out)スクリーンNo.</param>
        /// <param name="partsType">(out)部品種別</param>
        /// <param name="partsNo">(out)部品番号</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// エリア部品情報から、部品詳細情報（エリアＩＤ、スクリーンNo、部品種別、部品番号）を取得する。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void GetPartsDtl(long areaParts, ref UInt16 areaID, ref UInt16 screenNo, ref UInt16 partsType, ref UInt16 partsNo)
        {
            // 出力情報を初期化
            areaID = 0;                                 // エリアＩＤ
            screenNo = 0;                               // スクリーンNo.
            partsType = 0;                              // 部品種別
            partsNo = 0;                                // 部品番号

            try
            {
                areaID    = (UInt16)((areaParts & 0xFF000000) >> 24);
                screenNo  = (UInt16)((areaParts & 0x00FF0000) >> 16);
                partsType = (UInt16)((areaParts & 0x0000FF00) >> 8);
                partsNo   = (UInt16)(areaParts & 0x000000FF);
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 部品詳細情報取得処理
        /// MODULE ID           : GetPartsDtl
        ///
        /// PARAMETER IN        : 
        /// <param name="areaParts">(in)エリア部品</param>
        /// PARAMETER OUT       : 
        /// <param name="funcID">(out)機能ＩＤ</param>
        /// <param name="moveID">(out)動作名称ＩＤ</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// エリア部品情報から、機能番号と動作番号を取得する。
        /// 小規模線区向け表示制御卓、且つメニュー項目押下の場合に使用する。
        /// </summary>
        ///
        ///*******************************************************************************
        public static void GetPartsDtl(long areaParts, ref UInt16 funcID, ref UInt16 moveID)
        {
            // 出力情報を初期化
            funcID = 0;                 // 機能番号を初期化
            moveID = 0;                 // 動作番号を初期化

            try
            {
                funcID = (UInt16)((areaParts & 0xFFFF0000) >> 16);
                moveID = (UInt16)(areaParts & 0x0000FFFF);
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : エリア部品変換処理
        /// MODULE ID           : ConvAreaParts
        ///
        /// PARAMETER IN        : 
        /// <param name="areaID">(in)エリアＩＤ</param>
        /// <param name="screenNo">(in)スクリーンNo.</param>
        /// <param name="partsType">(in)部品種別</param>
        /// <param name="partsNo">(in)部品番号</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>エリア部品</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// エリアＩＤ，スクリーンNo，部品種別，部品番号を、エリア部品情報に変換する。
        /// </summary>
        ///
        ///*******************************************************************************
        public static long ConvAreaParts(UInt16 areaID, UInt16 screenNo, UInt16 partsType, UInt16 partsNo)
        {
            long    areaParts = 0;                      // エリア部品

            try
            {
                // エリアIDから部品番号までを long型にまとめる
                areaParts = (long)((areaID << 24) | (screenNo << 16) | (partsType << 8) | partsNo);
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return areaParts;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 部品クリックイベント発生処理
        /// MODULE ID           : SetClickEvent
        ///
        /// PARAMETER IN        : 
        /// <param name="areaID">(in)エリアＩＤ</param>
        /// <param name="screenNo">(in)スクリーンNo.</param>
        /// <param name="partsType">(in)部品種別</param>
        /// <param name="partsNo">(in)部品番号</param>
        /// <param name="childKind">(in)子画面部品種別</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 本エリアの部品がクリックされたことをMainFormに通知する。
        /// </summary>
        ///
        ///*******************************************************************************
        public void SetClickEvent(UInt16 areaID, UInt16 screenNo, UInt16 partsType, UInt16 partsNo, UInt16 childKind)
        {
            long    areaParts = 0;                      // エリア部品

            try
            {
                // エリアＩＤ～部品番号を、エリア部品情報へ変換する
                areaParts = ConvAreaParts(areaID, screenNo, partsType, partsNo);

                // 部品クリックイベントを発生
                SetClickEvent(areaParts, childKind);
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 部品クリックイベント発生処理
        /// MODULE ID           : SetClickEvent
        ///
        /// PARAMETER IN        : 
        /// <param name="areaParts">(in)エリア部品</param>
        /// <param name="childKind">(in)子画面部品種別</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 本エリアの部品がクリックされたことをMainFormに通知する。
        /// </summary>
        ///
        ///*******************************************************************************
        public void SetClickEvent(long areaParts, UInt16 childKind)
        {
            try
            {
                // イベント通知データを作成
                CNoticeEventArgs evargs = new CNoticeEventArgs();
                evargs.data1 = areaParts;
                evargs.data2 = (long)childKind;
                evargs.data3 = (long)0;
                evargs.data4 = (long)0;
                evargs.data5 = (long)0;

                // ボタンクリックイベントの発生
                AreaPartsClick(this, evargs);
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : ラベル設定処理
        /// MODULE ID           : SetPartsLabel
        ///
        /// PARAMETER IN        : 
        /// <param name="partsList">(in)ラベル部品設定情報</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 本エリアのラベル部品に対して情報設定を行う（座標、サイズ、表示等の情報設定を行う）。
        /// 本関数はShow()関数の前に呼び出すこと。一度部品設定した情報の再設定は不可とする。
        /// </summary>
        ///
        ///*******************************************************************************
        public void SetPartsLabel(List<CPartsInfo> partsList)
        {
            Int32   no = 0;                             // 部品番号（1-N）
            Int32   iMax = 0;                           // 部品最大数
            Int32   iKosu = 0;                          // 個数
            Int32   iCnt = 0;                           // ループカウンタ
            Cursor  cursol = Cursors.Default;           // カーソルタイプ
            String  strAppLog = String.Empty;           // ログメッセージ

            try
            {
                iKosu = partsList.Count;                // ラベル部品設定情報の個数を取得する
                iMax = m_LabelList.Length;              // 部品数を取得する

                for (iCnt = 0; iCnt < iKosu; iCnt++)
                {
                    no = (Int32)partsList[iCnt].No;     // 部品番号を取得

                    if ((1 <= no) && (no <= iMax))      // 部品番号が範囲内の場合
                    {
                        if ((Int16)CPartsInfo.CURSOLTYPE.HAND == partsList[iCnt].Cursol)
                        {
                            cursol = Cursors.Hand;      // カーソルタイプに手を設定
                        }
                        else
                        {
                            cursol = Cursors.Default;   // カーソルタイプにデフォルトを設定
                        }

                        m_LabelList[no - 1].Location = new System.Drawing.Point(partsList[iCnt].PointX, partsList[iCnt].PointY);
                        m_LabelList[no - 1].Size = new System.Drawing.Size(partsList[iCnt].Width, partsList[iCnt].Height);
                        if ((0 == partsList[iCnt].Width) && (0 == partsList[iCnt].Height))
                        {
                            // 処理なし
                        }
                        else
                        {
                            m_LabelList[no - 1].Text = partsList[iCnt].Text;
                            m_LabelList[no - 1].IsVisiblity = true;
                        }
                        m_LabelList[no - 1].Visible = partsList[iCnt].Visible;
                        m_LabelList[no - 1].Cursor = cursol;
                        if (0 == partsList[iCnt].FontSize)
                        {
                            // フォントスタイルが太字の場合
                            if (partsList[iCnt].FontStyle == (UInt16)CPartsInfo.FONTSTYLE.BOLD)
                            {
                                m_LabelList[no - 1].Font = new Font(m_LabelList[no - 1].Font, FontStyle.Bold);
                            }
                        }
                        else
                        {
                            // フォントスタイルが太字の場合
                            if (partsList[iCnt].FontStyle == (UInt16)CPartsInfo.FONTSTYLE.BOLD)
                            {
                                m_LabelList[no - 1].Font = new Font("ＭＳ ゴシック", partsList[iCnt].FontSize, FontStyle.Bold, GraphicsUnit.Pixel);
                            }
                            // 標準の場合
                            else
                            {
                                m_LabelList[no - 1].Font = new Font("ＭＳ ゴシック", partsList[iCnt].FontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                            }
                        }

                        SetLabelColor(m_LabelList[no - 1], 1, partsList[iCnt].ForeColor);
                                                        // 文字色を設定
                        SetLabelColor(m_LabelList[no - 1], 2, partsList[iCnt].BackColor);
                                                        // 背景色を設定

                        // ラベルに線を描く
                        DrawLabelLine(m_LabelList[no - 1], (Int16)partsList[iCnt].LineKind, partsList[iCnt].LineColor);
                    }
                    else                                // 部品番号が範囲外の場合
                    {
                        strAppLog = String.Format("ラベル部品番号範囲外：index={0:D} no={1:D}", iCnt, no);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    }
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : ボタン設定処理
        /// MODULE ID           : SetPartsButton
        ///
        /// PARAMETER IN        : 
        /// <param name="partsList">(in)ボタン部品設定情報</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 本エリアのボタン部品に対して情報設定を行う（座標、サイズ、表示等の情報設定を行う）。
        /// 本関数はShow()関数の前に呼び出すこと。一度部品設定した情報の再設定は不可とする。
        /// </summary>
        ///
        ///*******************************************************************************
        public void SetPartsButton(List<CPartsInfo> partsList)
        {
            Int32   no = 0;                             // 部品番号（1-N）
            Int32   iMax = 0;                           // 部品最大数
            Int32   iKosu = 0;                          // 個数
            Int32   iCnt = 0;                           // ループカウンタ
            Cursor  cursol = Cursors.Default;           // カーソルタイプ
            String  strAppLog = String.Empty;           // ログメッセージ

            try
            {
                iKosu = partsList.Count;                // ボタン部品設定情報の個数を取得する
                iMax = m_ButtonList.Length;             // 部品数を取得する

                for (iCnt = 0; iCnt < iKosu; iCnt++)
                {
                    no = (Int32)partsList[iCnt].No;     // 部品番号を取得

                    if ((1 <= no) && (no <= iMax))      // 部品番号が範囲内の場合
                    {
                        if ((Int16)CPartsInfo.CURSOLTYPE.HAND == partsList[iCnt].Cursol)
                        {
                            cursol = Cursors.Hand;      // カーソルタイプに手を設定
                        }
                        else
                        {
                            cursol = Cursors.Default;   // カーソルタイプにデフォルトを設定
                        }

                        m_ButtonList[no - 1].Location = new System.Drawing.Point(partsList[iCnt].PointX, partsList[iCnt].PointY);
                        m_ButtonList[no - 1].Size = new System.Drawing.Size(partsList[iCnt].Width, partsList[iCnt].Height);
                        if ((0 == partsList[iCnt].Width) && (0 == partsList[iCnt].Height))
                        {
                            // 処理なし
                        }
                        else
                        {
                            m_ButtonList[no - 1].Text = partsList[iCnt].Text;
                            m_ButtonList[no - 1].IsVisiblity = true;
                        }
                        m_ButtonList[no - 1].Visible = partsList[iCnt].Visible;
                        m_ButtonList[no - 1].Cursor = cursol;
                        m_ButtonList[no - 1].FlatStyle = FlatStyle.Standard;
                        if (0 == partsList[iCnt].FontSize)
                        {
                            // フォントスタイルが太字の場合
                            if (partsList[iCnt].FontStyle == (UInt16)CPartsInfo.FONTSTYLE.BOLD)
                            {
                                m_ButtonList[no - 1].Font = new Font(m_ButtonList[no - 1].Font, FontStyle.Bold);
                            }
                        }
                        else
                        {
                            // フォントスタイルが太字の場合
                            if (partsList[iCnt].FontStyle == (UInt16)CPartsInfo.FONTSTYLE.BOLD)
                            {
                                m_ButtonList[no - 1].Font = new Font("ＭＳ ゴシック", partsList[iCnt].FontSize, FontStyle.Bold, GraphicsUnit.Pixel);
                            }
                            // 標準の場合
                            else
                            {
                                m_ButtonList[no - 1].Font = new Font("ＭＳ ゴシック", partsList[iCnt].FontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                            }
                        }
                        m_ButtonList[no - 1].BringToFront();

                        SetButtonColor(m_ButtonList[no - 1], 1, partsList[iCnt].ForeColor);
                                                        // 文字色を設定
                        SetButtonColor(m_ButtonList[no - 1], 2, partsList[iCnt].BackColor);
                                                        // 背景色を設定

                        // ボタンに枠線を描く
                        DrawButtonlLine(m_ButtonList[no - 1], (Int16)partsList[iCnt].LineKind, partsList[iCnt].LineColor);
                    }
                    else                                // 部品番号が範囲外の場合
                    {
                        strAppLog = String.Format("ボタン部品番号範囲外：index={0:D} no={1:D}", iCnt, no);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    }
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        delegate void SetLabelTextCallback(Int32 no, String text);
        ///*******************************************************************************
        /// MODULE NAME         : ラベルテキスト設定処理
        /// MODULE ID           : SetLabelText
        ///
        /// PARAMETER IN        : 
        /// <param name="no">(in)ラベル部品番号(1-N)</param>
        /// <param name="text">(in)テキスト</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 本エリアの指定ラベル部品に対してテキスト設定を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        public void SetLabelText(Int32 no, String text)
        {
            Int32   iMax = 0;                           // 部品最大数
            String  strAppLog = String.Empty;           // ログメッセージ
            if (no == 0)
            {
                return;
            }
            try
            {
                // 呼び出し元が別スレッドか？
                // 別スレッドの場合は、デリゲートを使用してアクセスする
                if (this.m_LabelList[no - 1].InvokeRequired)
                {
                    SetLabelTextCallback delegateMethod = new SetLabelTextCallback(this.SetLabelText);
                    // コントロールの親のInvoke()メソッドを呼び出すことで、
                    // 呼び出し元のコントロールのスレッドでこのメソッドを実行する
                    this.Invoke(delegateMethod, new Object[2] { no, text });
                    delegateMethod = null;
                }
                // 呼び出し元がコントロールの作成されたスレッドと同じ？
                else
                {
                    iMax = m_LabelList.Length;              // 部品数を取得する
                    if ((1 <= no) && (no <= iMax))          // 部品番号が範囲内の場合
                    {
                        m_LabelList[no - 1].Text = text;
                        // ガイダンス表示エリア、アンサ表示エリアの場合はテキストを保管
                        if ((m_AreaID == AREAID_GUIDANCE) && (no == (Int32)CAreaMng.GUIDELABELNO.ANSWER))
                        {
                            m_VisibleAnsCodeText = m_LabelList[no - 1].Text;
                        }
                        else
                        {
                            // 処理なし
                        }
                    }
                    else                                    // 部品番号が範囲外の場合
                    {
                        strAppLog = String.Format("ラベル部品番号範囲外：no={0:D}", no);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    }
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        delegate void SetLabelColorCallback(Int32 no, UInt16 kind, String strColor);
        ///*******************************************************************************
        /// MODULE NAME         : ラベル色設定処理
        /// MODULE ID           : SetLabelColor
        ///
        /// PARAMETER IN        : 
        /// <param name="no">(in)ラベル部品番号(1-N)</param>
        /// <param name="kind">(in)表示色背景色種別（1=表示色、2=背景色）</param>
        /// <param name="strColor">(in)色指定（例：#000000）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 本エリアの指定ラベル部品に対して色の設定を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        public void SetLabelColor(Int32 no, UInt16 kind, String strColor)
        {
            bool    blRet = false;                      // 戻り値取得用
            Int32   iMax = 0;                           // 部品最大数
            short   rVal = 0;                           // R値
            short   gVal = 0;                           // G値
            short   bVal = 0;                           // B値
            String  strAppLog = String.Empty;           // ログメッセージ

            try
            {
                // 呼び出し元が別スレッドか？
                // 別スレッドの場合は、デリゲートを使用してアクセスする
                if (this.m_LabelList[no - 1].InvokeRequired)
                {
                    SetLabelColorCallback delegateMethod = new SetLabelColorCallback(this.SetLabelColor);
                    // コントロールの親のInvoke()メソッドを呼び出すことで、
                    // 呼び出し元のコントロールのスレッドでこのメソッドを実行する
                    this.Invoke(delegateMethod, new Object[3] { no, kind, strColor });
                    delegateMethod = null;
                }
                // 呼び出し元がコントロールの作成されたスレッドと同じ？
                else
                {
                    iMax = m_LabelList.Length;              // 部品数を取得する
                    if ((1 <= no) && (no <= iMax))          // 部品番号が範囲内の場合
                    {
                        blRet = CCommon.GetRGB(strColor, ref rVal, ref gVal, ref bVal);
                        // 文字色をString→Int変換
                        if (false == blRet)                 // 変換失敗の場合
                        {
                            strAppLog = String.Format("文字色不正={0}", strColor);
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.ERROR, MethodBase.GetCurrentMethod(), strAppLog);
                        }
                        else if (1 == kind)                 // 表示色指定の場合
                        {
                            m_LabelList[no - 1].ForeColor = Color.FromArgb(rVal, gVal, bVal);
                        }
                        else                                // 背景色指定の場合
                        {
                            m_LabelList[no - 1].BackColor = Color.FromArgb(rVal, gVal, bVal);
                        }
                    }
                    else                                    // 部品番号が範囲外の場合
                    {
                        strAppLog = String.Format("ラベル部品番号範囲外：no={0:D}", no);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    }
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        delegate void SetLabelVisibleCallback(Int32 no, UInt16 uiVisible);
        ///*******************************************************************************
        /// MODULE NAME         : ラベル表示／非表示設定処理
        /// MODULE ID           : SetLabelVisible
        ///
        /// PARAMETER IN        : 
        /// <param name="no">(in)ラベル部品番号(1-N)</param>
        /// <param name="uiVisible">(in)表示指定（NONVISIBLE=非表示、DISABLE=不活性、VISIBLE=表示）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 本エリアの指定ラベル部品に対して表示／非表示の設定を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        public void SetLabelVisible(Int32 no, UInt16 uiVisible)
        {
            Int32   iMax = 0;                           // 部品最大数
            bool    isVisible = false;                  // 表示／非表示
            bool    isEnabled  = false;                 // 活性／不活性
            String  strAppLog = String.Empty;           // ログメッセージ

            try
            {
                // 呼び出し元が別スレッドか？
                // 別スレッドの場合は、デリゲートを使用してアクセスする
                if (this.m_LabelList[no - 1].InvokeRequired)
                {
                    SetLabelVisibleCallback delegateMethod = new SetLabelVisibleCallback(this.SetLabelVisible);
                    // コントロールの親のInvoke()メソッドを呼び出すことで、
                    // 呼び出し元のコントロールのスレッドでこのメソッドを実行する
                    this.Invoke(delegateMethod, new Object[2] { no, uiVisible });
                    delegateMethod = null;
                }
                // 呼び出し元がコントロールの作成されたスレッドと同じ？
                else
                {
                    iMax = m_LabelList.Length;              // 部品数を取得する
                    if ((1 <= no) && (no <= iMax))          // 部品番号が範囲内の場合
                    {
                        isVisible = m_LabelList[no - 1].Visible;
                        isEnabled = m_LabelList[no - 1].Enabled;

                        // 表示指定が不活性の場合
                        if ((UInt16)VISIBLEKIND.DISABLE == uiVisible)
                        {
                            isVisible = true;
                            isEnabled = false;
                        }
                        // 表示指定が非表示の場合
                        else if ((UInt16)VISIBLEKIND.NONVISIBLE == uiVisible)
                        {
                            isVisible = false;
                            isEnabled = true;
                        }
                        // 表示指定が表示の場合
                        else if ((UInt16)VISIBLEKIND.VISIBLE == uiVisible)
                        {
                            isVisible = true;
                            isEnabled = true;
                        }
                        // 上記以外の場合
                        else
                        {
                            strAppLog = String.Format("表示指定範囲外：no={0:D} uiVisible={1:D}", no, uiVisible);
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                        }
                        m_LabelList[no - 1].Visible = isVisible;
                        m_LabelList[no - 1].Enabled = isEnabled;
                    }
                    else                                    // 部品番号が範囲外の場合
                    {
                        strAppLog = String.Format("ラベル部品番号範囲外：no={0:D}", no);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    }
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        delegate void SetButtonTextCallback(Int32 no, String text);
        ///*******************************************************************************
        /// MODULE NAME         : ボタンテキスト設定処理
        /// MODULE ID           : SetButtonText
        ///
        /// PARAMETER IN        : 
        /// <param name="no">(in)ボタン部品番号(1-N)</param>
        /// <param name="text">(in)テキスト</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 本エリアの指定ボタン部品に対してテキスト設定を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        public void SetButtonText(Int32 no, String text)
        {
            Int32   iMax = 0;                           // 部品最大数
            String  strAppLog = String.Empty;           // ログメッセージ

            try
            {
                // 呼び出し元が別スレッドか？
                // 別スレッドの場合は、デリゲートを使用してアクセスする
                if (this.m_ButtonList[no - 1].InvokeRequired)
                {
                    SetButtonTextCallback delegateMethod = new SetButtonTextCallback(this.SetButtonText);
                    // コントロールの親のInvoke()メソッドを呼び出すことで、
                    // 呼び出し元のコントロールのスレッドでこのメソッドを実行する
                    this.Invoke(delegateMethod, new Object[2] { no, text });
                    delegateMethod = null;
                }
                // 呼び出し元がコントロールの作成されたスレッドと同じ？
                else
                {
                    iMax = m_ButtonList.Length;             // 部品数を取得する
                    if ((1 <= no) && (no <= iMax))          // 部品番号が範囲内の場合
                    {
                        m_ButtonList[no - 1].Text = text;
                    }
                    else                                    // 部品番号が範囲外の場合
                    {
                        strAppLog = String.Format("ボタン部品番号範囲外：no={0:D}", no);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    }
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        delegate void SetButtonColorCallback(Int32 no, UInt16 kind, String strColor);
        ///*******************************************************************************
        /// MODULE NAME         : ボタン色設定処理
        /// MODULE ID           : SetButtonColor
        ///
        /// PARAMETER IN        : 
        /// <param name="no">(in)ボタン部品番号(1-N)</param>
        /// <param name="kind">(in)表示色背景色種別（1=表示色、2=背景色、3=押下時表示色、4=押下時背景色）</param>
        /// <param name="strColor">(in)色指定（例：#000000）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 本エリアの指定ボタン部品に対して色の設定を行う。
        /// 現状では色の設定は通常時と押下時で同じ処理だが、ボタンのFlatStayleおよび押下状態を設定している。
        /// 通常時のFlatStyle：標準、押下時のFlatStyle：平面
        /// 押下された状態 IsPressed：ture、押下されていない状態 IsPressed：false
        /// </summary>
        ///
        ///*******************************************************************************
        public void SetButtonColor(Int32 no, UInt16 kind, String strColor)
        {
            bool    blRet = false;                      // 戻り値取得用
            Int32   iMax = 0;                           // 部品最大数
            short   rVal = 0;                           // R値
            short   gVal = 0;                           // G値
            short   bVal = 0;                           // B値
            String  strAppLog = String.Empty;           // ログメッセージ

            try
            {
                // 呼び出し元が別スレッドか？
                // 別スレッドの場合は、デリゲートを使用してアクセスする
                if (this.m_ButtonList[no - 1].InvokeRequired)
                {
                    SetButtonColorCallback delegateMethod = new SetButtonColorCallback(this.SetButtonColor);
                    // コントロールの親のInvoke()メソッドを呼び出すことで、
                    // 呼び出し元のコントロールのスレッドでこのメソッドを実行する
                    this.Invoke(delegateMethod, new Object[3] { no, kind, strColor });
                    delegateMethod = null;
                }
                // 呼び出し元がコントロールの作成されたスレッドと同じ？
                else
                {
                    iMax = m_ButtonList.Length;             // 部品数を取得する
                    if ((1 <= no) && (no <= iMax))          // 部品番号が範囲内の場合
                    {
                        blRet = CCommon.GetRGB(strColor, ref rVal, ref gVal, ref bVal);
                        // 文字色をString→Int変換
                        if (false == blRet)                 // 変換失敗の場合
                        {
                            strAppLog = String.Format("文字色不正={0}", strColor);
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.ERROR, MethodBase.GetCurrentMethod(), strAppLog);
                        }
                        else if (1 == kind)                 // 表示色指定の場合
                        {
                            m_ButtonList[no - 1].ForeColor = Color.FromArgb(rVal, gVal, bVal);
                            m_ButtonList[no - 1].FlatStyle = FlatStyle.Standard;
                            m_ButtonList[no - 1].IsPressed = false;
                        }
                        else if (2 == kind)                 // 背景色指定の場合
                        {
                            m_ButtonList[no - 1].BackColor = Color.FromArgb(rVal, gVal, bVal);
                            m_ButtonList[no - 1].FlatStyle = FlatStyle.Standard;
                            m_ButtonList[no - 1].IsPressed = false;
                        }
                        else if (3 == kind)                 // 押下時表示色指定の場合
                        {
                            m_ButtonList[no - 1].ForeColor = Color.FromArgb(rVal, gVal, bVal);
                            m_ButtonList[no - 1].FlatStyle = FlatStyle.Flat;
                            m_ButtonList[no - 1].IsPressed = true;
                        }
                        else if (4 == kind)                 // 押下時背景色指定の場合
                        {
                            m_ButtonList[no - 1].BackColor = Color.FromArgb(rVal, gVal, bVal);
                            m_ButtonList[no - 1].FlatStyle = FlatStyle.Flat;
                            m_ButtonList[no - 1].IsPressed = true;
                        }
                    }
                    else                                    // 部品番号が範囲外の場合
                    {
                        strAppLog = String.Format("ボタン部品番号範囲外：no={0:D}", no);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    }
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : ボタン押下状態取得処理
        /// MODULE ID           : GetButtonPressed
        ///
        /// PARAMETER IN        : 
        /// <param name="no">(in)ボタン部品番号(1-N)</param>
        /// PARAMETER OUT       : 
        /// <param>(out)ボタン押下状態</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 本エリアの指定ボタン部品に対してボタン押下状態を取得する。
        /// 押下された状態：ture、押下されていない状態：false
        /// </summary>
        ///
        ///*******************************************************************************
        public bool GetButtonPressed(Int32 no)
        {
            bool blRet = false;                             // 戻り値取得用
            Int32 iMax = 0;                                 // 部品最大数
            String strAppLog = String.Empty;                // ログメッセージ

            try
            {
                iMax = m_ButtonList.Length;                 // 部品数を取得する
                if ((1 <= no) && (no <= iMax))              // 部品番号が範囲内の場合
                {
                    blRet = m_ButtonList[no - 1].IsPressed; // ボタン押下状態取得
                }
                else                                        // 部品番号が範囲外の場合
                {
                    strAppLog = String.Format("ボタン部品番号範囲外：no={0:D}", no);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                }

                return blRet;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
                return false;
            }
        }

        delegate void SetButtonVisibleCallback(Int32 no, UInt16 uiVisible);
        ///*******************************************************************************
        /// MODULE NAME         : ボタン表示／非表示設定処理
        /// MODULE ID           : SetButtonVisible
        ///
        /// PARAMETER IN        : 
        /// <param name="no">(in)ボタン部品番号(1-N)</param>
        /// <param name="uiVisible">(in)表示指定（NONVISIBLE=非表示、DISABLE=不活性、VISIBLE=表示）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 本エリアの指定ボタン部品に対して表示／非表示の設定を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        public void SetButtonVisible(Int32 no, UInt16 uiVisible)
        {
            Int32   iMax = 0;                           // 部品最大数
            bool    isVisible = false;                  // 表示／非表示
            bool    isEnabled  = false;                 // 活性／不活性
            String  strAppLog = String.Empty;           // ログメッセージ

            try
            {
                // 呼び出し元が別スレッドか？
                // 別スレッドの場合は、デリゲートを使用してアクセスする
                if (this.m_ButtonList[no - 1].InvokeRequired)
                {
                    SetButtonVisibleCallback delegateMethod = new SetButtonVisibleCallback(this.SetButtonVisible);
                    // コントロールの親のInvoke()メソッドを呼び出すことで、
                    // 呼び出し元のコントロールのスレッドでこのメソッドを実行する
                    this.Invoke(delegateMethod, new Object[2] { no, uiVisible });
                    delegateMethod = null;
                }
                // 呼び出し元がコントロールの作成されたスレッドと同じ？
                else
                {
                    iMax = m_ButtonList.Length;             // 部品数を取得する
                    if ((1 <= no) && (no <= iMax))          // 部品番号が範囲内の場合
                    {
                        isVisible = m_ButtonList[no - 1].Visible;
                        isEnabled = m_ButtonList[no - 1].Enabled;

                        // 表示指定が不活性の場合
                        if ((UInt16)VISIBLEKIND.DISABLE == uiVisible)
                        {
                            isVisible = true;
                            isEnabled = false;
                        }
                        // 表示指定が非表示の場合
                        else if ((UInt16)VISIBLEKIND.NONVISIBLE == uiVisible)
                        {
                            isVisible = false;
                            isEnabled = true;
                        }
                        // 表示指定が表示の場合
                        else if ((UInt16)VISIBLEKIND.VISIBLE == uiVisible)
                        {
                            isVisible = true;
                            isEnabled = true;
                        }
                        // 上記以外の場合
                        else
                        {
                            strAppLog = String.Format("表示指定範囲外：no={0:D} uiVisible={1:D}", no, uiVisible);
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                        }
                        m_ButtonList[no - 1].Visible = isVisible;
                        m_ButtonList[no - 1].Enabled = isEnabled;
                    }
                    else                                    // 部品番号が範囲外の場合
                    {
                        strAppLog = String.Format("ボタン部品番号範囲外：no={0:D}", no);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    }
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        delegate void SetButtonEnableCallback(Int32 no, bool isEnabled);
        ///*******************************************************************************
        /// MODULE NAME         : ボタン活性／不活性設定処理
        /// MODULE ID           : SetButtonEnable
        ///
        /// PARAMETER IN        : 
        /// <param name="no">(in)ボタン部品番号(1-N)</param>
        /// <param name="isEnabled">(in)活性指定（true=活性、false=不活性）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 本エリアの指定ボタン部品に対して活性／不活性の設定を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        public void SetButtonEnable(Int32 no, bool isEnabled)
        {
            Int32   iMax = 0;                           // 部品最大数
            String  strAppLog = String.Empty;           // ログメッセージ

            try
            {
                // 呼び出し元が別スレッドか？
                // 別スレッドの場合は、デリゲートを使用してアクセスする
                if (this.m_ButtonList[no - 1].InvokeRequired)
                {
                    SetButtonEnableCallback delegateMethod = new SetButtonEnableCallback(this.SetButtonEnable);
                    // コントロールの親のInvoke()メソッドを呼び出すことで、
                    // 呼び出し元のコントロールのスレッドでこのメソッドを実行する
                    this.Invoke(delegateMethod, new Object[2] { no, isEnabled });
                    delegateMethod = null;
                }
                // 呼び出し元がコントロールの作成されたスレッドと同じ？
                else
                {
                    iMax = m_ButtonList.Length;             // 部品数を取得する
                    if ((1 <= no) && (no <= iMax))          // 部品番号が範囲内の場合
                    {
                        m_ButtonList[no - 1].Enabled = isEnabled;

                        // 「鳴止」「画面」ボタンなどの不活性時、表示しない設定の場合
                        if (CAppDat.DCPSETTEI.EnabledDisplay == 0)
                        {
                            if (true == CAppDat.PostAreaF.DispInfo.ContainsKey(m_AreaID))
                            {
                                // 存在するボタンの場合
                                if (true == CAppDat.PostAreaF.DispInfo[m_AreaID].ButtonInfo.ContainsKey((ushort)no))
                                {
                                    m_ButtonList[no - 1].Visible = isEnabled;
                                }
                            }
                        }
                    }
                    else                                    // 部品番号が範囲外の場合
                    {
                        strAppLog = String.Format("ボタン部品番号範囲外：no={0:D}", no);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    }
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        delegate void DrawLabelLineCallback(Int32 no, Int16 lineType, String lineColor);
        ///*******************************************************************************
        /// MODULE NAME         : ラベル枠線描画処理
        /// MODULE ID           : DrawLabelLine
        ///
        /// PARAMETER IN        : 
        /// <param name="no">(in)ラベル部品番号(1-N)</param>
        /// <param name="lineType">(in)線種（CPartsInfo.LINETYPEのSOLID/DASH/DOT）</param>
        /// <param name="lineColor">(in)線色（例：#000000）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// ラベル部品に対して枠線の描画を行う。
        /// （正確には枠線を描画しているのではなく、非表示の枠の内側に四角い線を描画することで
        ///   枠線が描画されているように見せている。）
        /// </summary>
        ///
        ///*******************************************************************************
        public void DrawLabelLine(Int32 no, Int16 lineType, String lineColor)
        {
            Int32   iMax = 0;                           // 部品最大数
            String  strAppLog = String.Empty;           // ログメッセージ

            try
            {
                // 呼び出し元が別スレッドか？
                // 別スレッドの場合は、デリゲートを使用してアクセスする
                if (this.m_ButtonList[no - 1].InvokeRequired)
                {
                    DrawLabelLineCallback delegateMethod = new DrawLabelLineCallback(this.DrawLabelLine);
                    // コントロールの親のInvoke()メソッドを呼び出すことで、
                    // 呼び出し元のコントロールのスレッドでこのメソッドを実行する
                    this.Invoke(delegateMethod, new Object[3] { no, lineType, lineColor });
                    delegateMethod = null;
                }
                // 呼び出し元がコントロールの作成されたスレッドと同じ？
                else
                {
                    iMax = m_LabelList.Length;              // 部品数を取得する
                    if ((1 <= no) && (no <= iMax))          // 部品番号が範囲内の場合
                    {
                        DrawLabelLine(m_LabelList[no - 1], lineType, lineColor);
                        // ラベル枠線描画処理
                    }
                    else                                    // 部品番号が範囲外の場合
                    {
                        strAppLog = String.Format("ラベル部品番号範囲外：no={0:D}", no);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    }
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 部品生成処理
        /// MODULE ID           : CreateParts
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
        /// ラベルとボタンを最大数分生成する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void CreateParts()
        {
            Int32   iCnt = 0;                           // ループカウンタ

            try
            {
                m_LabelList = new VisiblityLabel[LABEL_MAX];
                m_ButtonList = new NotSelectableButton[BUTTON_MAX];

                for (iCnt = 0; iCnt < m_LabelList.Length; iCnt++)
                {
                    m_LabelList[iCnt] = new VisiblityLabel();
                    m_LabelList[iCnt].AutoSize = false;
                    m_LabelList[iCnt].Name = "m_labelList" + (iCnt+1).ToString();
                    m_LabelList[iCnt].TabIndex = (iCnt+1);
                    m_LabelList[iCnt].TextAlign = ContentAlignment.MiddleCenter;
                    m_LabelList[iCnt].Visible = false;
                    m_LabelList[iCnt].Click += new System.EventHandler(LabelList_Click);

                    this.Controls.Add(m_LabelList[iCnt]);
                }

                for (iCnt = 0; iCnt < m_ButtonList.Length; iCnt++)
                {
                    m_ButtonList[iCnt] = new NotSelectableButton();
                    m_ButtonList[iCnt].AutoSize = false;
                    m_ButtonList[iCnt].Name = "m_buttonList" + (iCnt+1).ToString();
                    m_ButtonList[iCnt].TabIndex = 50 + (iCnt+1);
                    m_ButtonList[iCnt].TextAlign = ContentAlignment.MiddleCenter;
                    m_ButtonList[iCnt].FlatStyle = FlatStyle.Flat;
                    m_ButtonList[iCnt].Visible = false;
                    m_ButtonList[iCnt].Click += new System.EventHandler(ButtonList_Click);

                    this.Controls.Add(m_ButtonList[iCnt]);
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 部品削除処理
        /// MODULE ID           : DeleteParts
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
        /// ラベルとボタンのインスタンスを削除する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void DeleteParts()
        {
            Int32   iCnt = 0;                           // ループカウンタ

            try
            {
                for (iCnt = 0; iCnt < m_LabelList.Length; iCnt++)
                {
                    this.Controls.Remove(m_LabelList[iCnt]);
                    m_LabelList[iCnt].Dispose();
                    m_LabelList[iCnt] = null;
                }

                for (iCnt = 0 ; iCnt < m_ButtonList.Length ; iCnt++)
                {
                    this.Controls.Remove(m_ButtonList[iCnt]);
                    m_ButtonList[iCnt].Dispose();
                    m_ButtonList[iCnt] = null;
                }

                m_LabelList  = null;
                m_ButtonList = null;
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : ラベル枠線描画処理
        /// MODULE ID           : DrawLabelLine
        ///
        /// PARAMETER IN        : 
        /// <param name="labelParts">(in)Label部品</param>
        /// <param name="lineType">(in)線種（CPartsInfo.LINETYPEのSOLID/DASH/DOT）</param>
        /// <param name="lineColor">(in)線色（例：#000000）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// ラベル部品に対して枠線の描画を行う。
        /// （正確には枠線を描画しているのではなく、非表示の枠の内側に四角い線を描画することで
        ///   枠線が描画されているように見せている。）
        /// </summary>
        ///
        ///*******************************************************************************
        private void DrawLabelLine(System.Windows.Forms.Label labelParts, Int16 lineType, String lineColor)
        {
            bool    blRet = false;                      // 戻り値取得用
            short   rVal = 0;                           // R値
            short   gVal = 0;                           // G値
            short   bVal = 0;                           // B値
            Color   setColor = Color.Black;             // 色指定
            String  strAppLog = String.Empty;           // ログメッセージ

            try
            {
                // 線種が実線／破線／点線の場合
                if (((Int16)CPartsInfo.LINETYPE.SOLID == lineType) ||
                    ((Int16)CPartsInfo.LINETYPE.DASH  == lineType) ||
                    ((Int16)CPartsInfo.LINETYPE.DOT   == lineType))
                {
                    blRet = CCommon.GetRGB(lineColor, ref rVal, ref gVal, ref bVal);
                                                        // 色指定をString→Int変換
                    if (true == blRet)
                    {
                        setColor = Color.FromArgb(rVal, gVal, bVal);
                                                        // 色を設定
                    }
                    else
                    {
                        strAppLog = String.Format("線色指定が不正：lineColor={0}", lineColor);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    }

                    // 描画先とするImageオブジェクトを作成する
                    Bitmap canvas = new Bitmap(labelParts.Width, labelParts.Height);

                    // ImageオブジェクトのGraphicsオブジェクトを作成する
                    Graphics g = Graphics.FromImage(canvas);

                    // Penオブジェクトの作成
                    Pen clrPen = new Pen(setColor, 1);

                    if ((Int16)CPartsInfo.LINETYPE.SOLID == lineType)
                    {
                        clrPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                    }
                    else if ((Int16)CPartsInfo.LINETYPE.DASH == lineType)
                    {
                        clrPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    }
                    else if ((Int16)CPartsInfo.LINETYPE.DOT == lineType)
                    {
                        clrPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    }

                    g.DrawRectangle(clrPen, 0, 0, (labelParts.Width - 1), (labelParts.Height - 1));

                    // リソースを解放する
                    g.Dispose();
                    g = null;
                    clrPen.Dispose();
                    clrPen = null;

                    // Labelに表示する
                    labelParts.Image = canvas;
                }
                // 線種が対象外の場合
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
        /// MODULE NAME         : ボタン枠線描画処理
        /// MODULE ID           : DrawButtonlLine
        ///
        /// PARAMETER IN        : 
        /// <param name="buttonParts">(in)Button部品</param>
        /// <param name="lineType">(in)線種（CPartsInfo.LINETYPEのSOLID/DASH/DOT）</param>
        /// <param name="lineColor">(in)線色（例：#000000）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// ボタン部品に対して枠線の描画を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void DrawButtonlLine(System.Windows.Forms.Button buttonParts, Int16 lineType, String lineColor)
        {
            bool blRet = false;                      // 戻り値取得用
            short rVal = 0;                           // R値
            short gVal = 0;                           // G値
            short bVal = 0;                           // B値
            Color setColor = Color.Black;             // 色指定
            String strAppLog = String.Empty;           // ログメッセージ

            try
            {
                // 線種が実線／破線／点線の場合
                if (((Int16)CPartsInfo.LINETYPE.SOLID == lineType) ||
                    ((Int16)CPartsInfo.LINETYPE.DASH == lineType) ||
                    ((Int16)CPartsInfo.LINETYPE.DOT == lineType))
                {
                    blRet = CCommon.GetRGB(lineColor, ref rVal, ref gVal, ref bVal);
                    // 色指定をString→Int変換
                    if (true == blRet)
                    {
                        setColor = Color.FromArgb(rVal, gVal, bVal);
                        // 色を設定
                    }
                    else
                    {
                        strAppLog = String.Format("線色指定が不正：lineColor={0}", lineColor);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    }

                    // 描画先とするImageオブジェクトを作成する
                    Bitmap canvas = new Bitmap(buttonParts.Width, buttonParts.Height);

                    // ImageオブジェクトのGraphicsオブジェクトを作成する
                    Graphics g = Graphics.FromImage(canvas);

                    // Penオブジェクトの作成
                    Pen clrPen = new Pen(setColor, 1);

                    if ((Int16)CPartsInfo.LINETYPE.SOLID == lineType)
                    {
                        clrPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                    }
                    else if ((Int16)CPartsInfo.LINETYPE.DASH == lineType)
                    {
                        clrPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    }
                    else if ((Int16)CPartsInfo.LINETYPE.DOT == lineType)
                    {
                        clrPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    }

                    g.DrawRectangle(clrPen, 0, 0, (buttonParts.Width - 1), (buttonParts.Height - 1));

                    // リソースを解放する
                    g.Dispose();
                    g = null;
                    clrPen.Dispose();
                    clrPen = null;

                    // Buttonに表示する
                    buttonParts.Image = canvas;
                }
                // 線種が対象外の場合
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
        /// MODULE NAME         : ラベル色設定処理
        /// MODULE ID           : SetLabelColor
        ///
        /// PARAMETER IN        : 
        /// <param name="labelParts">(in)Label部品</param>
        /// <param name="kind">(in)色種指定（1=表示色、2=背景色）</param>
        /// <param name="strColor">(in)色</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// ラベル部品に対して表示色／背景色の設定を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void SetLabelColor(System.Windows.Forms.Label labelParts, Int16 kind, String strColor)
        {
            bool    blRet = false;                      // 戻り値取得用
            short   rVal = 0;                           // R値
            short   gVal = 0;                           // G値
            short   bVal = 0;                           // B値
            Color   setColor = Color.Black;             // 色指定

            try
            {
                if (String.Empty != strColor)           // 色指定ありの場合
                {
                    blRet = CCommon.GetRGB(strColor, ref rVal, ref gVal, ref bVal);
                                                        // 色指定をString→Int変換
                    if (true == blRet)                  // 取得成功の場合
                    {
                        setColor = Color.FromArgb(rVal, gVal, bVal);
                                                        // 色を設定
                        if (1 == kind)                  // 表示色指定の場合
                        {
                            labelParts.ForeColor = setColor;
                        }
                        else if (2 == kind)             // 背景色指定の場合
                        {
                            labelParts.BackColor = setColor;
                        }
                        else                            // 上記以外の場合
                        {
                            // 処理なし
                        }
                    }
                    else                                // 取得失敗の場合
                    {
                        // 処理なし
                    }
                }
                else                                    // 色指定なしの場合
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
        /// MODULE NAME         : ボタン色設定処理
        /// MODULE ID           : SetButtonColor
        ///
        /// PARAMETER IN        : 
        /// <param name="buttonParts">(in)Button部品</param>
        /// <param name="kind">(in)色種指定（1=表示色、2=背景色）</param>
        /// <param name="strColor">(in)色</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// ラベル部品に対して表示色／背景色の設定を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void SetButtonColor(System.Windows.Forms.Button buttonParts, Int16 kind, String strColor)
        {
            bool    blRet = false;                      // 戻り値取得用
            short   rVal = 0;                           // R値
            short   gVal = 0;                           // G値
            short   bVal = 0;                           // B値
            Color   setColor = Color.Black;             // 色指定

            try
            {
                if (String.Empty != strColor)           // 色指定ありの場合
                {
                    blRet = CCommon.GetRGB(strColor, ref rVal, ref gVal, ref bVal);
                                                        // 色指定をString→Int変換
                    if (true == blRet)                  // 取得成功の場合
                    {
                        setColor = Color.FromArgb(rVal, gVal, bVal);
                                                        // 色を設定
                        if (1 == kind)                  // 表示色指定の場合
                        {
                            buttonParts.ForeColor = setColor;
                        }
                        else if (2 == kind)             // 背景色指定の場合
                        {
                            buttonParts.BackColor = setColor;
                        }
                        else                            // 上記以外の場合
                        {
                            // 処理なし
                        }
                    }
                    else                                // 取得失敗の場合
                    {
                        // 処理なし
                    }
                }
                else                                    // 色指定なしの場合
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
        /// MODULE NAME         : ラベル押下処理
        /// MODULE ID           : LabelList_Click
        ///
        /// PARAMETER IN        : 
        /// <param name="sender">(in)オブジェクト情報</param>
        /// <param name="e">(in)イベント情報</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// ラベルをマウスでクリックした時の処理。ラベル押下イベントを発行する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void LabelList_Click(object sender, EventArgs e)
        {
            Int32   iCnt = 0;                           // ループカウンタ
            Int32   x = 0;                              // Ｘ座標
            Int32   y = 0;                              // Ｙ座標
            Int32   partsX = 0;                         // ラベルのＸ座標
            Int32   partsY = 0;                         // ラベルのＹ座標
            Int32   partsNo = 0;                        // ラベルNo.（1-N）
            Int32 jCnt = 0;                             // ループカウンタ
            Int32 btnPartsX = 0;                        // ボタンのＸ座標
            Int32 btnPartsY = 0;                        // ボタンのＹ座標
            bool IsButtonExists = false;                // ボタンの表示有無

            try
            {
                MouseEventArgs moue = (MouseEventArgs)e;
                if (moue.Button != MouseButtons.Left)
                {
                    return;
                }
                // クリックした部品の座標から、該当部品を特定する
                System.Windows.Forms.Label label = (System.Windows.Forms.Label)(sender);
                x = label.Location.X;                   // クリックした部品のＸ座標を取得
                y = label.Location.Y;                   // クリックした部品のＹ座標を取得

                for (iCnt = 0 ; iCnt < m_LabelList.Length ; iCnt++)
                {
                    partsX = m_LabelList[iCnt].Location.X;
                    partsY = m_LabelList[iCnt].Location.Y;

                    if((x == partsX) && (y == partsY) && (true == m_LabelList[iCnt].IsVisiblity))
                    {
                        IsButtonExists = false;     // ボタンの表示有無の初期化
                        for (jCnt = 0; jCnt < m_ButtonList.Length; jCnt++)
                        {
                            // 2018/02/19 [共通障害]左クリック以外で抑止ラベルを反応させない del start
                            //MouseEventArgs moue = (MouseEventArgs)e;
                            // 2018/02/19 [共通障害]左クリック以外で抑止ラベルを反応させない del end
                            btnPartsX = m_ButtonList[jCnt].Location.X;      // ボタンの表示位置取得(X)
                            btnPartsY = m_ButtonList[jCnt].Location.Y;      // ボタンの表示位置取得(Y)
                            // ボタンがクリック範囲内に存在し、コントロールが有効、非表示でない？
                            if (((btnPartsX <= moue.X) && (moue.X <= btnPartsX + m_ButtonList[jCnt].Width)) &&
                                ((btnPartsY <= moue.Y) && (moue.Y <= btnPartsY + m_ButtonList[jCnt].Height)) &&
                                (true == m_ButtonList[jCnt].IsVisiblity) && (true == m_ButtonList[jCnt].Visible))
                            {
                                IsButtonExists = true;      // ボタンありをセット
                                break;
                            }
                        }

                        if (IsButtonExists)
                        {
                            partsNo = 0;
                        }
                        else
                        {
                            partsNo = (iCnt + 1);
                        }
                        break;
                    }
                }

                // 該当部品が見つかった場合、ラベルクリックイベントを通知する
                if (0 != partsNo)                       // 該当部品ありの場合
                {
                    // 部品クリックイベント発生処理
                    SetClickEvent(m_AreaID, m_ScreenNo, (UInt16)1, (UInt16)partsNo, (UInt16)CHILDKIND.NONE);
                }
                else                                    // 該当部品なしの場合
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
        /// MODULE NAME         : ボタン押下処理
        /// MODULE ID           : ButtonList_Click
        ///
        /// PARAMETER IN        : 
        /// <param name="sender">(in)オブジェクト情報</param>
        /// <param name="e">(in)イベント情報</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// ボタンをマウスでクリックした時の処理。ボタン押下イベントを発行する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void ButtonList_Click(object sender, EventArgs e)
        {
            Int32   iCnt = 0;                           // ループカウンタ
            Int32   x = 0;                              // Ｘ座標
            Int32   y = 0;                              // Ｙ座標
            Int32   partsX = 0;                         // ボタンのＸ座標
            Int32   partsY = 0;                         // ボタンのＹ座標
            Int32   partsNo = 0;                        // ボタンNo.（1-N）

            try
            {
                // クリックした部品の座標から、該当部品を特定する
                System.Windows.Forms.Button button = (System.Windows.Forms.Button)(sender);
                x = button.Location.X;                  // クリックした部品のＸ座標を取得
                y = button.Location.Y;                  // クリックした部品のＹ座標を取得

                for (iCnt = 0 ; iCnt < m_ButtonList.Length ; iCnt++)
                {
                    partsX = m_ButtonList[iCnt].Location.X;
                    partsY = m_ButtonList[iCnt].Location.Y;

                    if ((x == partsX) && (y == partsY) && (true == m_ButtonList[iCnt].IsVisiblity))
                    {
                        partsNo = (iCnt + 1);
                        break;
                    }
                }

                // 該当部品が見つかった場合、ボタンクリックイベントを通知する
                if (0 != partsNo)                       // 該当部品ありの場合
                {
                    // 部品クリックイベント発生処理
                    SetClickEvent(m_AreaID, m_ScreenNo, (UInt16)2, (UInt16)partsNo, (UInt16)CHILDKIND.NONE);
                }
                else                                    // 該当部品なしの場合
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
        /// MODULE NAME         : 表示エリアの初期表示処理
        /// MODULE ID           : SetAreaInitialize
        ///
        /// PARAMETER IN        : 
        /// <param name="areaKey">(in)エリア識別種別</param>
        /// <param name="screenNo">(in)スクリーン番号</param>
        /// <param name="areaIndex">(in)エリアインデックス</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 表示エリアの初期表示処理を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        public void SetAreaInitialize(String areaKey, UInt16 screenNo, UInt16 areaIndex)
        {
            //[未使用] CDCPiniMng dcpini = null;
            //[未使用] String syncErrorMsg = String.Empty;

            try
            {
                switch (areaKey)
                {
                    case AREANAME_SOUTI:            // 装置状態表示エリア
                        m_AreaID = AREAID_SOUTI;
                        m_ScreenNo = screenNo;
                        SetSoutiStateAreaInitialize();
                        break;

                    case AREANAME_SENKEI:           // 線形表示エリア
                        m_AreaID = AREAID_SENKEI;
                        m_ScreenNo = screenNo;
                        SetSenkeiAreaInitialize();
                        break;

                    case AREANAME_TEIAN:            // 提案表示エリア
                        m_AreaID = AREAID_TEIAN;
                        m_ScreenNo = screenNo;
                        SetTeianAreaInitialize();
                        break;

                    case AREANAME_KEIHOU:           // 警報表示エリア
                        m_AreaID = AREAID_KEIHOU;
                        m_ScreenNo = screenNo;
                        SetKeihouAreaInitialize();
                        break;

                    case AREANAME_GUIDANCE:         // ガイダンス表示エリア
                        m_AreaID = AREAID_GUIDANCE;
                        m_ScreenNo = screenNo;
                        SetGuidanceAreaInitialize();
                        break;

                    case AREANAME_MENU:       // メニューボタン表示エリア
                        m_AreaID = AREAID_MENU;
                        m_ScreenNo = screenNo;
                        SetMenuAreaInitialize();
                        break;

                    case AREANAME_ATSUKAI:          // 扱い警報表示エリア
                        m_AreaID = AREAID_ATSUKAI;
                        m_ScreenNo = screenNo;
                        SetAtsukaiAreaInitialize();
                        break;

                    case AREANAME_CONTOROL:       // 操作ボタン表示エリア
                        m_AreaID = AREAID_CONTROL;
                        m_ScreenNo = screenNo;
                        SetControlAreaInitialize();
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
            finally
            {
                //[未使用] dcpini = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 装置状態表示エリアの初期処理
        /// MODULE ID           : SetSoutiStateAreaInitialize
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
        /// 装置状態表示エリアの初期表示処理を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void SetSoutiStateAreaInitialize()
        {
            try
            {
                // 時刻表示を初期化
                m_LabelList[(UInt16)(CAreaMng.EQPLABELNO.DATETIME - 1)].Text = @"----/--/-- --:--:--";

                // 装置状態の登録数分ループ
                for (UInt16 cnt = 0; cnt < CAppDat.SoutiInfoF.Count; cnt++)
                {
                    // 装置状態種別を取得して部品番号にする
                    Int32 no = CAppDat.SoutiInfoF[cnt].SoutiKind;

                    // 文字色をセット
                    this.SetLabelColor(no, 1, CAppDat.SoutiInfoF[cnt].Back.MojiColor);
                    // 背景色をセット
                    this.SetLabelColor(no, 2, CAppDat.SoutiInfoF[cnt].Back.BackColor);
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 線形表示エリアの初期処理
        /// MODULE ID           : SetSenkeiAreaInitialize
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
        /// 線形表示エリアの初期表示処理を行う。
        /// ボタンメニューファイルの情報を使用して初期表示メニューボタンを設定する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void SetSenkeiAreaInitialize()
        {
            CButtonFuncInfoMng funcInfo = null;             // ボタン機能情報

            try
            {
                // ボタンを非表示に設定する
                for (UInt16 i = 0; i < BUTTON_MAX; i++)
                {
                    this.SetButtonVisible((i + 1), (UInt16)VISIBLEKIND.NONVISIBLE);
                }

                // ボタンメニューＩＤ検索：1～0xFFFF-1
                for (UInt16 cntID = 1; cntID < 0xFFFF; cntID++)
                {
                    // ボタンメニューＩＤ存在チェック
                    bool bButtonMenuExists = CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo.ContainsKey(cntID);

                    // ボタンメニューＩＤが存在しない場合、次へ
                    if (false == bButtonMenuExists)
                    {
                        continue;
                    }

                    // ボタンメニューのエリアＩＤが線形表示エリアＩＤでない場合、次へ
                    if (AREAID_SENKEI != CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo[cntID].AreaId)
                    {
                        continue;
                    }

                    // サブメニュー情報取得
                    foreach (CButtonSubMenuMng subMenuData in CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo[cntID].SubMenuInfo.Values)
                    {
                        // ボタン情報の登録数分ループ
                        foreach (CButtonInfoMng buttonInfo in subMenuData.ButtonInfo)
                        {
                            // ボタン機能情報を取得
                            bool bRetFunc = CAppDat.ButtonMenuF.ButtonFuncDef.ButtonFuncInfo.TryGetValue(buttonInfo.ButtonId, out funcInfo);
                            if (true == bRetFunc)
                            {
                                this.SetButtonText(buttonInfo.ButtonNo, funcInfo.ButtonName);                   // 文字列をセット
                                this.SetButtonColor(buttonInfo.ButtonNo, 1, funcInfo.ForeColor);                // 文字色をセット
                                this.SetButtonColor(buttonInfo.ButtonNo, 2, funcInfo.BackColor);                // 背景色をセット
                                this.SetButtonVisible(buttonInfo.ButtonNo, (UInt16)VISIBLEKIND.VISIBLE);        // 表示状態にセット
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
                funcInfo = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 線形表示エリア(ボタン)の再設定処理
        /// MODULE ID           : SetSenkeiAreaReset
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
        /// 線形表示エリア(ボタン)の再設定処理を行う。
        /// ボタンメニューファイルの情報を使用して初期表示メニューボタン色を設定する。
        /// </summary>
        ///
        ///*******************************************************************************
        public void SetSenkeiAreaReset()
        {
            CButtonFuncInfoMng funcInfo = null;             // ボタン機能情報

            try
            {
                // ボタンメニューＩＤ検索：1～0xFFFF-1
                for (UInt16 cntID = 1; cntID < 0xFFFF; cntID++)
                {
                    // ボタンメニューＩＤ存在チェック
                    bool bButtonMenuExists = CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo.ContainsKey(cntID);

                    // ボタンメニューＩＤが存在しない場合、次へ
                    if (false == bButtonMenuExists)
                    {
                        continue;
                    }

                    // ボタンメニューのエリアＩＤが線形表示エリアＩＤでない場合、次へ
                    if (AREAID_SENKEI != CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo[cntID].AreaId)
                    {
                        continue;
                    }

                    // サブメニュー情報取得
                    foreach (CButtonSubMenuMng subMenuData in CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo[cntID].SubMenuInfo.Values)
                    {
                        // ボタン情報の登録数分ループ
                        foreach (CButtonInfoMng buttonInfo in subMenuData.ButtonInfo)
                        {
                            // ボタン機能情報を取得
                            bool bRetFunc = CAppDat.ButtonMenuF.ButtonFuncDef.ButtonFuncInfo.TryGetValue(buttonInfo.ButtonId, out funcInfo);
                            if (true == bRetFunc)
                            {
                                this.SetButtonColor(buttonInfo.ButtonNo, 1, funcInfo.ForeColor);                // 文字色をセット
                                this.SetButtonColor(buttonInfo.ButtonNo, 2, funcInfo.BackColor);                // 背景色をセット
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
                funcInfo = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 提案表示エリアの初期処理
        /// MODULE ID           : SetTeianAreaInitialize
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
        /// 提案表示エリアの初期表示処理を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void SetTeianAreaInitialize()
        {
            try
            {
                // 提案表示エリアの初期化
                for (UInt16 cnt = 0; cnt < m_LabelList.Length; cnt++)
                {
                    m_LabelList[cnt].TextAlign = ContentAlignment.MiddleLeft;
                    m_LabelList[cnt].Text = String.Empty;

                    switch (CAppDat.TypeF.KeihouDispInfo.AreaClickType)
                    {
                        case 1:     // シングルクリックの場合

                            // ダブルクリックイベント設定を解除し、クリックイベント設定に変更
                            m_LabelList[cnt].DoubleClick -= new System.EventHandler(LabelList_Click);
                            m_LabelList[cnt].Click += new System.EventHandler(LabelList_Click);

                            break;

                        case 2:     // ダブルクリックの場合

                            // クリックイベント設定を解除し、ダブルクリックイベント設定に変更
                            m_LabelList[cnt].Click -= new System.EventHandler(LabelList_Click);
                            m_LabelList[cnt].DoubleClick += new System.EventHandler(LabelList_Click);

                            break;

                        default:    // 未定義

                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 警報表示エリアの初期処理
        /// MODULE ID           : SetKeihouAreaInitialize
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
        /// 警報表示エリアの初期表示処理を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void SetKeihouAreaInitialize()
        {
            try
            {
                // 警報表示エリアの初期化
                for (UInt16 cnt = 0; cnt < m_LabelList.Length; cnt++)
                {
                    m_LabelList[cnt].TextAlign = ContentAlignment.MiddleLeft;
                    m_LabelList[cnt].Text = String.Empty;

                    switch (CAppDat.TypeF.KeihouDispInfo.AreaClickType)
                    {
                        case 1:     // シングルクリックの場合

                            // ダブルクリックイベント設定を解除し、クリックイベント設定に変更
                            m_LabelList[cnt].DoubleClick -= new System.EventHandler(LabelList_Click);
                            m_LabelList[cnt].Click += new System.EventHandler(LabelList_Click);

                            break;

                        case 2:     // ダブルクリックの場合

                            // クリックイベント設定を解除し、ダブルクリックイベント設定に変更
                            m_LabelList[cnt].Click -= new System.EventHandler(LabelList_Click);
                            m_LabelList[cnt].DoubleClick += new System.EventHandler(LabelList_Click);

                            break;

                        default:    // 未定義

                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : ガイダンス表示エリアの初期処理
        /// MODULE ID           : SetGuidanceAreaInitialize
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
        /// ガイダンス表示エリアの初期表示処理を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void SetGuidanceAreaInitialize()
        {
            try
            {
                // ガイダンス表示エリアの初期化
                for (UInt16 cnt = 0; cnt < m_LabelList.Length; cnt++)
                {
                    m_LabelList[cnt].TextAlign = ContentAlignment.MiddleLeft;
                    m_LabelList[cnt].Text = String.Empty;
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 機能操作表示エリアの初期処理
        /// MODULE ID           : SetSousaAreaInitialize
        ///
        /// PARAMETER IN        : 
        /// <param name="crno">(in)表示画面番号</param>
        /// <param name="index">(in)エリアインデックス</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 機能操作表示エリアの初期表示処理を行う。
        /// 描画区間設定ファイルとボタンメニューファイルの情報を使用して初期表示メニューボタンを設定する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void SetSousaAreaInitialize(UInt16 crno, UInt16 index)
        {
            bool bRetMenu = false;
            bool bRetFunc = false;
            CMenuInfoMng menuInfo = null;
            bool IsHit = false;
            CButtonMenuInfoMng buttonMenuInfo = null;   // ボタンメニュー情報
            CButtonFuncInfoMng funcInfo = null;         // ボタン機能情報

            try
            {
                // ボタンを非表示に設定する
                for (UInt16 i = 0; i < BUTTON_MAX; i++)
                {
                    this.SetButtonVisible((i + 1), (UInt16)VISIBLEKIND.NONVISIBLE);
                }

                // 描画区間設定情報数分ループ
                foreach (CDrawKukanMng drow in CAppDat.DrawSectionF.Values)
                {
                    // 描画管理ファイルから初期画面番号の情報を取得
                    foreach (CConfigMng config in CAppDat.ConfigF.Values)
                    {
                        // 画面名称が一致する描画区間なら操作機能ボタンを初期化
                        if (drow.Name == config.Name)
                        {
                            menuInfo = drow.MenuInfo[index];
                            IsHit = true;
                            break;
                        }
                        else
                        {
                            // 処理なし
                        }
                    }
                }
                // 一致条件なしなら処理終了
                if (false == IsHit)
                {
                    return;
                }
                else
                {
                    // 処理なし
                }

                // メニュー情報を取得
                bRetMenu = CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo.TryGetValue(menuInfo.ButtonMenuId, out buttonMenuInfo);
                if (true == bRetMenu)
                {
                    // サブメニュー情報の登録数分ループ
                    foreach (CButtonSubMenuMng submenu in buttonMenuInfo.SubMenuInfo.Values)
                    {
                        // ボタン情報の登録数分ループ
                        foreach (CButtonInfoMng buttonInfo in submenu.ButtonInfo)
                        {
                            // ボタン機能情報を取得
                            bRetFunc = CAppDat.ButtonMenuF.ButtonFuncDef.ButtonFuncInfo.TryGetValue(buttonInfo.ButtonId, out funcInfo);
                            if (true == bRetFunc)
                            {
                                this.SetButtonText(buttonInfo.ButtonNo, funcInfo.ButtonName);                   // 文字列をセット
                                this.SetButtonColor(buttonInfo.ButtonNo, 1, funcInfo.ForeColor);                // 文字色をセット
                                this.SetButtonColor(buttonInfo.ButtonNo, 2, funcInfo.BackColor);                // 背景色をセット
                                this.SetButtonVisible(buttonInfo.ButtonNo, (UInt16)VISIBLEKIND.VISIBLE);        // 表示状態にセット
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
                    // 処理なし
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
            finally
            {
                menuInfo = null;
                buttonMenuInfo = null;
                funcInfo = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : メニューボタン表示エリアの初期処理
        /// MODULE ID           : SetMenuAreaInitialize
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
        /// メニューボタン表示エリアの初期表示処理を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void SetMenuAreaInitialize()
        {
            bool bRetFunc = false;
            bool IsHit = false;
            CButtonMenuInfoMng buttonMenuInfo = null;   // 初期ボタンメニュー情報
            CButtonFuncInfoMng funcInfo = null;         // ボタン機能情報

            try
            {
                // ボタンを非表示に設定する
                for (UInt16 i = 0; i < BUTTON_MAX; i++)
                {
                    this.SetButtonVisible((i + 1), (UInt16)VISIBLEKIND.NONVISIBLE);
                }

                // ボタンメニュー設定情報数分ループ
                foreach (CButtonMenuInfoMng menu in CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo.Values)
                {
                    if (menu.AreaId == CAreaUserControl.AREAID_MENU)
                    {
                        // メニューボタン表示エリアの場合、メニュー情報を取得
                        buttonMenuInfo = menu;
                        IsHit = true;
                        break;
                    }
                    else
                    {
                        // 処理なし
                    }
                }

                // 一致条件なしなら処理終了
                if (false == IsHit)
                {
                    return;
                }
                else
                {
                    // 処理なし
                }

                // サブメニュー情報の登録数分ループ
                foreach (CButtonSubMenuMng submenu in buttonMenuInfo.SubMenuInfo.Values)
                {
                    // ボタン情報の登録数分ループ
                    foreach (CButtonInfoMng buttonInfo in submenu.ButtonInfo)
                    {
                        // ボタン機能情報を取得
                        bRetFunc = CAppDat.ButtonMenuF.ButtonFuncDef.ButtonFuncInfo.TryGetValue(buttonInfo.ButtonId, out funcInfo);
                        if (true == bRetFunc)
                        {
                            this.SetButtonText(buttonInfo.ButtonNo, funcInfo.ButtonName);                   // 文字列をセット
                            this.SetButtonColor(buttonInfo.ButtonNo, 1, funcInfo.ForeColor);                // 文字色をセット
                            this.SetButtonColor(buttonInfo.ButtonNo, 2, funcInfo.BackColor);                // 背景色をセット
                            this.SetButtonVisible(buttonInfo.ButtonNo, (UInt16)VISIBLEKIND.VISIBLE);        // 表示状態にセット
                        }
                        else
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
            finally
            {
                buttonMenuInfo = null;
                funcInfo = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 扱い警報表示エリアの初期処理
        /// MODULE ID           : SetAtsukaiAreaInitialize
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
        /// 扱い警報表示エリアの初期表示処理を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void SetAtsukaiAreaInitialize()
        {
            try
            {
                // 扱い警報表示エリアの初期化
                for (UInt16 cnt = 0; cnt < m_LabelList.Length; cnt++)
                {
                    m_LabelList[cnt].TextAlign = ContentAlignment.MiddleLeft;
                    m_LabelList[cnt].Text = String.Empty;

                    switch (CAppDat.TypeF.KeihouDispInfo.AreaClickType)
                    {
                        case 1:     // シングルクリックの場合

                            // ダブルクリックイベント設定を解除し、クリックイベント設定に変更
                            m_LabelList[cnt].DoubleClick -= new System.EventHandler(LabelList_Click);
                            m_LabelList[cnt].Click += new System.EventHandler(LabelList_Click);

                            break;

                        case 2:     // ダブルクリックの場合

                            // クリックイベント設定を解除し、ダブルクリックイベント設定に変更
                            m_LabelList[cnt].Click -= new System.EventHandler(LabelList_Click);
                            m_LabelList[cnt].DoubleClick += new System.EventHandler(LabelList_Click);

                            break;

                        default:    // 未定義

                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : 操作ボタン表示エリアの初期処理
        /// MODULE ID           : SetControlAreaInitialize
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
        /// 操作ボタン表示エリアの初期表示処理を行う。
        /// </summary>
        ///
        ///*******************************************************************************
        private void SetControlAreaInitialize()
        {
            bool bRetFunc = false;
            bool IsHit = false;
            CButtonMenuInfoMng buttonMenuInfo = null;   // 初期ボタンメニュー情報
            CButtonFuncInfoMng funcInfo = null;         // ボタン機能情報

            try
            {
                // ボタンを非表示に設定する
                for (UInt16 i = 0; i < BUTTON_MAX; i++)
                {
                    this.SetButtonVisible((i + 1), (UInt16)VISIBLEKIND.NONVISIBLE);
                }

                // ボタンメニュー設定情報数分ループ
                foreach (CButtonMenuInfoMng menu in CAppDat.ButtonMenuF.ButtonMenuDef.ButtonMenuInfo.Values)
                {
                    if (menu.AreaId == CAreaUserControl.AREAID_CONTROL)
                    {
                        // 操作ボタン表示エリアの場合、メニュー情報を取得
                        buttonMenuInfo = menu;
                        IsHit = true;
                        break;
                    }
                    else
                    {
                        // 処理なし
                    }
                }

                // 一致条件なしの場合、処理終了
                if (false == IsHit)
                {
                    return;
                }
                else
                {
                    // 処理なし
                }

                // サブメニュー情報の登録数分ループ
                foreach (CButtonSubMenuMng submenu in buttonMenuInfo.SubMenuInfo.Values)
                {
                    // ボタン情報の登録数分ループ
                    foreach (CButtonInfoMng buttonInfo in submenu.ButtonInfo)
                    {
                        // ボタン機能情報を取得
                        bRetFunc = CAppDat.ButtonMenuF.ButtonFuncDef.ButtonFuncInfo.TryGetValue(buttonInfo.ButtonId, out funcInfo);
                        if (true == bRetFunc)
                        {
                            this.SetButtonText(buttonInfo.ButtonNo, funcInfo.ButtonName);                   // 文字列をセット
                            this.SetButtonColor(buttonInfo.ButtonNo, 1, funcInfo.ForeColor);                // 文字色をセット
                            this.SetButtonColor(buttonInfo.ButtonNo, 2, funcInfo.BackColor);                // 背景色をセット
                            this.SetButtonVisible(buttonInfo.ButtonNo, (UInt16)VISIBLEKIND.NONVISIBLE);     // 非表示状態にセット
                        }
                        else
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
            finally
            {
                buttonMenuInfo = null;
                funcInfo = null;
            }
        }

        ///*******************************************************************************
        /// MODULE NAME         : ブリンクタイマータイムアウト処理
        /// MODULE ID           : TimerBlink_Tick
        ///
        /// PARAMETER IN        : 
        /// <param name="sender">(in)オブジェクト情報</param>
        /// <param name="e">(in)イベント情報</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>なし</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 指定エリアの指定表示エリアを固定タイマーで表示反転しブリンク表示する。
        /// </summary>
        ///
        ///*******************************************************************************
        private void TimerBlink_Tick(object sender, EventArgs e)
        {
            try
            {
                // ガイダンス表示エリアの場合のみ有効とする（左記以外は無効）
                // アンサ表示エリアの表示テキストを反転しブリンク表示する
                if (m_AreaID == AREAID_GUIDANCE)
                {
                    UInt16 areano = (UInt16)CAreaMng.GUIDELABELNO.ANSWER;
                    if (String.Empty == m_LabelList[areano - 1].Text)
                    {
                        m_LabelList[areano - 1].Text = m_VisibleAnsCodeText;
                    }
                    else
                    {
                        m_LabelList[areano - 1].Text = String.Empty;
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
        }
    }
}

