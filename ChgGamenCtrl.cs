//********************************************************************************
//
//  SYSTEM          : DCP
//  UNIT            : 画面切替処理
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
    /// CLASS NAME      : 画面切替処理クラス
    /// CLASS ID        : CChgGamenCtrl
    ///
    /// FUNCTION        : 
    /// <summary>
    /// 画面の切り替えを行う。
    /// </summary>
    /// 
    ///*******************************************************************************
    class CChgGamenCtrl
    {
        #region <定数>

        /// <summary>遷移先指定</summary>
        public enum SENISIGN : ushort
        {
            ///<summary>No.00 なし</summary>
            NONE = 0,
            ///<summary>No.01 個別画面１</summary>
            KOBETU1,
            ///<summary>No.02 全体画面１</summary>
            ZENTAI1,
            ///<summary>No.03 指定画面移動</summary>
            SITEI,
            ///<summary>No.04 前画面移動</summary>
            PREV,
            ///<summary>No.05 次画面移動</summary>
            NEXT,
            ///<summary>No.06 (総数)</summary>
            SENISIGNCOUNT
        };

        #endregion

        #region <メンバ変数>

        #endregion

        ///*******************************************************************************
        /// MODULE NAME         : コンストラクタ
        /// MODULE ID           : CChgGamenCtrl
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
        public CChgGamenCtrl()
        {
        }

        ///*******************************************************************************
        /// MODULE NAME         : 遷移先画面Ｎｏ取得処理
        /// MODULE ID           : GetSenisakiGamenNo
        ///
        /// PARAMETER IN        : 
        /// <param name="seniSign">(in)遷移先（enum SENISIGNを参照）</param>
        /// <param name="gamenKind">(in)画面種別（詳細はCDrawKukanMngのenum EDISPKINDを参照）</param>
        /// <param name="strSeniGamen">(in)遷移先画面（画面Ｎｏ／基本ファイル名）</param>
        /// <param name="gamenSeni">(in)画面遷移設定ファイルの個別画面／全体画面／分割画面１／分割画面２の画面遷移情報</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>遷移先画面Ｎｏ（取得不能時は-1を返す）</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 指定された遷移先，画面種別，遷移先画面、及び、画面遷移設定ファイルの個別画面／全体画面／分割画面１／分割画面２の画面遷移情報
        /// を参照して次遷移先の画面を特定し、特定した遷移先画面Ｎｏを返す。
        /// </summary>
        ///
        ///*******************************************************************************
        public static Int32 GetSenisakiGamenNo(UInt16 seniSign, UInt16 gamenKind, String strSeniGamen, Dictionary<UInt16, CGamenSeniMng> gamenSeni)
        {
            bool    isFind = false;                     // 該当情報有無フラグ
            Int32   senisakiGamenNo = -1;               // 遷移先画面Ｎｏ
            Int32   seniGamenNo = -1;                   // 遷移画面Ｎｏ
            Int32   iCrNoKosu = 0;                      // 画面遷移情報の前画面Ｎｏ／次画面Ｎｏの個数
            UInt16  kosu = 0;                           // 画面遷移情報の個数
            UInt16  cnt = 0;                            // ループカウンタ
            List<UInt16>  crNoList = null;              // 前画面／次画面Ｎｏリスト
            String  strAppLog = String.Empty;           // ログメッセージ
            senisakiGamenNo = -1;                       // 遷移先画面Ｎｏになしを設定

            try
            {
                seniGamenNo = GetSeniGamenNo(seniSign, gamenKind, strSeniGamen);
                                                        // 遷移画面Ｎｏ取得処理
                if (seniGamenNo < 0)                    // 遷移画面Ｎｏが取得できなかった場合
                {
                    strAppLog = String.Format("遷移先指定不正１：seniSign={0:D} gamenKind={1:D} strSeniGamen={2} seniGamenNo={3:D}", seniSign, gamenKind, strSeniGamen, seniGamenNo);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                }
                else                                    // 遷移画面Ｎｏが取得できた場合
                {
                    // 画面遷移設定ファイルの個別画面／全体画面／分割画面１／分割画面２の画面遷移情報の個数を取得
                    kosu = (UInt16)gamenSeni.Count;

                    isFind = false;                     // 該当情報有無フラグに無しを設定
                    for (cnt = 1; cnt <= kosu; cnt++)
                    {
                        // 画面遷移情報の画面Ｎｏと指定された画面Ｎｏが一致した場合
                        if (seniGamenNo == gamenSeni[cnt].Crno)
                        {
                            isFind = true;              // 該当情報有無フラグに有りを設定
                            break;
                        }
                        // 画面遷移情報の画面Ｎｏと指定された画面Ｎｏが一致しなかった場合
                        else
                        {
                            // 処理なし
                        }
                    }

                    if (false == isFind)                // 該当情報なしの場合
                    {
                        strAppLog = String.Format("遷移先指定不正２：seniSign={0:D} gamenKind={1:D} strSeniGamen={2} seniGamenNo={3:D}", seniSign, gamenKind, strSeniGamen, seniGamenNo);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    }
                    else                                // 該当情報ありの場合
                    {
                        // 前画面／次画面Ｎｏリストを構築
                        crNoList = new List<UInt16>();

                        // 画面遷移情報から画面Ｎｏを取得して遷移先画面Ｎｏとする
                        senisakiGamenNo = gamenSeni[cnt].Crno;

                        // 遷移先として前画面／次画面移動を指定した場合
                        if (((UInt16)SENISIGN.PREV == seniSign) ||
                            ((UInt16)SENISIGN.NEXT == seniSign))
                        {
                            // 遷移先として前画面移動を指定した場合
                            if ((UInt16)SENISIGN.PREV == seniSign)
                            {
                                // 前画面Ｎｏのリストをコピーする
                                foreach(UInt16 crno in gamenSeni[cnt].PrevCrno)
                                {
                                    crNoList.Add(crno);
                                }
                            }
                            // 遷移先として次画面移動を指定した場合
                            else
                            {
                                // 次画面Ｎｏのリストをコピーする
                                foreach(UInt16 crno in gamenSeni[cnt].NextCrno)
                                {
                                    crNoList.Add(crno);
                                }
                            }

                            // 画面遷移情報の前／次画面Ｎｏの個数を取得する
                            iCrNoKosu = crNoList.Count;

                            if (0 == iCrNoKosu)         // 前／次画面Ｎｏの個数が０の場合
                            {
                                strAppLog = String.Format("遷移先なし：seniSign={0:D} gamenKind={1:D} strSeniGamen={2} seniGamenNo={3:D}", seniSign, gamenKind, strSeniGamen, seniGamenNo);
                                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.INFORMATION, MethodBase.GetCurrentMethod(), strAppLog);
                            }
                            else if (2 <= iCrNoKosu)    // 前／次画面Ｎｏの個数が複数の場合
                            {
                                strAppLog = String.Format("遷移先複数今期未サポート：seniSign={0:D} gamenKind={1:D} strSeniGamen={2} seniGamenNo={3:D}", seniSign, gamenKind, strSeniGamen, seniGamenNo);
                                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.INFORMATION, MethodBase.GetCurrentMethod(), strAppLog);
                            }
                            else                        // 前／次画面Ｎｏの個数が１の場合
                            {
                                // 画面遷移情報の前／次画面Ｎｏを取得して遷移先画面Ｎｏとする
                                senisakiGamenNo = crNoList[0];
                            }
                        }
                        // 遷移先として前画面／次画面移動以外を指定された場合
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
                // 解放
                if (null != crNoList)
                {
                    crNoList.Clear();
                    crNoList = null;
                }
                else
                {
                    // 処理なし
                }
            }

            return senisakiGamenNo;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 遷移先画面Ｎｏ取得処理
        /// MODULE ID           : GetSenisakiGamenNo
        ///
        /// PARAMETER IN        : 
        /// <param name="seniSign">(in)遷移先（enum SENISIGNを参照）</param>
        /// <param name="gamenKind">(in)画面種別（詳細はCDrawKukanMngのenum EDISPKINDを参照）</param>
        /// <param name="strSeniGamen">(in)遷移先画面（画面Ｎｏ／基本ファイル名）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>遷移先画面Ｎｏ（取得不能時は-1を返す）</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 指定された遷移先，画面種別，遷移先画面を参照して次遷移先の画面を特定し、該当する線形表示エリアを表示にする。
        /// 画面遷移設定ファイルの個別画面／全体画面／分割画面１／分割画面２の画面遷移情報は指定しない。
        /// （指定すると、全体画面から個別画面へと遷移する場合などに活用できないため）
        /// </summary>
        ///
        ///*******************************************************************************
        public static Int32 GetSenisakiGamenNo(UInt16 seniSign, UInt16 gamenKind, String strSeniGamen)
        {
            bool isFind = false;                            // 該当情報有無フラグ
            Int32 senisakiGamenNo = -1;                     // 遷移先画面Ｎｏ
            Int32 seniGamenNo = -1;                         // 遷移画面Ｎｏ
            Int32 iCrNoKosu = 0;                            // 画面遷移情報の前画面Ｎｏ／次画面Ｎｏの個数
            UInt16 kosu = 0;                                // 画面遷移情報の個数
            UInt16 cnt = 0;                                 // ループカウンタ
            List<UInt16> crNoList = null;                   // 前画面／次画面Ｎｏリスト
            String strAppLog = String.Empty;                // ログメッセージ
            senisakiGamenNo = -1;                           // 遷移先画面Ｎｏになしを設定
            Dictionary<UInt16, CGamenSeniMng> gamenSeni;    // 画面遷移情報

            try
            {
                // 画面指定の場合は遷移先である画面Ｎｏを取得する
                // 「＜」、「＞」の場合は現在表示している画面IDを取得する
                seniGamenNo = GetSeniGamenNo(seniSign, gamenKind, strSeniGamen);
                                                        // 遷移画面Ｎｏ取得処理
                if (seniGamenNo < 0)                    // 遷移画面Ｎｏが取得できなかった場合
                {
                    strAppLog = String.Format("遷移先指定不正３：seniSign={0:D} gamenKind={1:D} strSeniGamen={2} seniGamenNo={3:D}", seniSign, gamenKind, strSeniGamen, seniGamenNo);
                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                }
                else                                    // 遷移画面Ｎｏが取得できた場合
                {
                    // 画面指定の場合
                    if ((UInt16)SENISIGN.SITEI == seniSign)
                    {
                        // 指定された画面があるかどうか
                        bool isBe = false;
                        foreach (UInt16 keyId in CAppDat.DrawSectionF.Keys)
                        {
                            if (true == CAppDat.DrawSectionF[keyId].Crno.Contains((UInt16)seniGamenNo))
                            {
                                isBe = true;
                                break;
                            }
                        }

                        // 指定された画面がある場合
                        if (true == isBe)
                        {
                            // 遷移先画面Ｎｏをそのまま利用
                            senisakiGamenNo = seniGamenNo;
                        }
                        else
                        {
                            strAppLog = String.Format("遷移先指定不正４：seniSign={0:D} gamenKind={1:D} strSeniGamen={2} seniGamenNo={3:D}", seniSign, gamenKind, strSeniGamen, seniGamenNo);
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                        }
                    }
                    // 次画面、もしくは前画面の場合
                    else if (((UInt16)SENISIGN.PREV == seniSign) || ((UInt16)SENISIGN.NEXT == seniSign))
                    {
                        // 現在の表示画面Ｎｏを取得
                        ushort nowCrno = CAppDat.GamenInfoT.Crno[0];

                        // 現在の画面種別から、それぞれの遷移情報を取得
                        switch (CAppDat.GamenInfoT.DispKind)
                        {
                            case (UInt16)CDrawKukanMng.EDISPKIND.KOBETUGAMEN:       // 個別画面
                                gamenSeni = CAppDat.GamenSeniF.KobetuGamenSeni;
                                break;

                            case (UInt16)CDrawKukanMng.EDISPKIND.ZENTAIGAMEN:       // 全体画面
                                gamenSeni = CAppDat.GamenSeniF.ZentaiGamenSeni;
                                break;

                            case (UInt16)CDrawKukanMng.EDISPKIND.BUNKATSUGAMEN1:    // 分割画面１
                                gamenSeni = CAppDat.GamenSeniF.BunGamen1Seni;
                                break;

                            case (UInt16)CDrawKukanMng.EDISPKIND.BUNKATSUGAMEN2:    // 分割画面２
                                gamenSeni = CAppDat.GamenSeniF.BunGamen2Seni;
                                break;

                            default:
                                gamenSeni = new Dictionary<UInt16, CGamenSeniMng>();
                                break;
                        }

                        // 画面遷移設定ファイルの個別画面／全体画面／分割画面１／分割画面２の画面遷移情報の個数を取得
                        kosu = (UInt16)gamenSeni.Count;

                        isFind = false;                     // 該当情報有無フラグに無しを設定
                        for (cnt = 1; cnt <= kosu; cnt++)
                        {
                            // 画面遷移情報の画面Ｎｏと現在の画面Ｎｏが一致した場合
                            if (nowCrno == gamenSeni[cnt].Crno)
                            {
                                isFind = true;              // 該当情報有無フラグに有りを設定
                                break;
                            }
                            // 画面遷移情報の画面Ｎｏと現在の画面Ｎｏが一致しなかった場合
                            else
                            {
                                // 処理なし
                            }
                        }

                        if (false == isFind)                // 該当情報なしの場合
                        {
                            strAppLog = String.Format("遷移先指定不正５：seniSign={0:D} gamenKind={1:D} strSeniGamen={2} seniGamenNo={3:D}", seniSign, gamenKind, strSeniGamen, seniGamenNo);
                            CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                        }
                        else                                // 該当情報ありの場合
                        {
                            // 前画面／次画面Ｎｏリストを構築
                            crNoList = new List<UInt16>();

                            // 指定された画面Ｎｏが現在の画面Ｎｏを同じ場合
                            if ((((UInt16)SENISIGN.PREV == seniSign) || ((UInt16)SENISIGN.NEXT == seniSign)) && (nowCrno == seniGamenNo))
                            {
                                // 遷移先として前画面移動を指定した場合
                                if ((UInt16)SENISIGN.PREV == seniSign)
                                {
                                    // 前画面Ｎｏのリストをコピーする
                                    foreach (UInt16 crno in gamenSeni[cnt].PrevCrno)
                                    {
                                        crNoList.Add(crno);
                                    }
                                }
                                // 遷移先として次画面移動を指定した場合
                                else
                                {
                                    // 次画面Ｎｏのリストをコピーする
                                    foreach (UInt16 crno in gamenSeni[cnt].NextCrno)
                                    {
                                        crNoList.Add(crno);
                                    }
                                }

                                // 画面遷移情報の前／次画面Ｎｏの個数を取得する
                                iCrNoKosu = crNoList.Count;

                                if (0 == iCrNoKosu)         // 前／次画面Ｎｏの個数が０の場合
                                {
                                    strAppLog = String.Format("遷移先なし：seniSign={0:D} gamenKind={1:D} strSeniGamen={2} seniGamenNo={3:D}", seniSign, gamenKind, strSeniGamen, seniGamenNo);
                                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.INFORMATION, MethodBase.GetCurrentMethod(), strAppLog);
                                }
                                else if (2 <= iCrNoKosu)    // 前／次画面Ｎｏの個数が複数の場合
                                {
                                    strAppLog = String.Format("遷移先複数今期未サポート：seniSign={0:D} gamenKind={1:D} strSeniGamen={2} seniGamenNo={3:D}", seniSign, gamenKind, strSeniGamen, seniGamenNo);
                                    CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.INFORMATION, MethodBase.GetCurrentMethod(), strAppLog);
                                }
                                else                        // 前／次画面Ｎｏの個数が１の場合
                                {
                                    // 画面遷移情報の前／次画面Ｎｏを取得して遷移先画面Ｎｏとする
                                    senisakiGamenNo = crNoList[0];
                                }
                            }
                            // それ以外
                            else
                            {
                                // 処理なし
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
                // 解放
                if (null != crNoList)
                {
                    crNoList.Clear();
                    crNoList = null;
                }
                else
                {
                    // 処理なし
                }
                gamenSeni = null;
            }

            return senisakiGamenNo;
        }

        ///*******************************************************************************
        /// MODULE NAME         : 遷移画面Ｎｏ取得処理
        /// MODULE ID           : GetSeniGamenNo
        ///
        /// PARAMETER IN        : 
        /// <param name="seniSign">(in)遷移先（enum SENISIGNを参照）</param>
        /// <param name="gamenKind">(in)画面種別（詳細はCDrawKukanMngのenum EDISPKINDを参照）</param>
        /// <param name="strSeniGamen">(in)遷移先画面（画面Ｎｏ／基本ファイル名）</param>
        /// PARAMETER OUT       : 
        /// <param>(out)なし</param>
        ///
        /// RETURN VALUE        : 
        /// <returns>遷移先画面Ｎｏ（取得不能時は-1を返す）</returns>
        ///
        /// FUNCTION            :
        /// <summary>
        /// 指定された遷移先，画面種別，遷移先画面に該当する遷移先画面Ｎｏを取得して返す。
        /// </summary>
        ///
        ///*******************************************************************************
        public static Int32 GetSeniGamenNo(UInt16 seniSign, UInt16 gamenKind, String strSeniGamen)
        {
            bool    blRet = false;                      // 戻り値取得用
            bool    isFind = false;                     // 該当情報有無フラグ
            Int32   senisakiGamenNo = -1;               // 遷移先画面Ｎｏ
            Int32   kobetuGameNo1 = -1;                 // 個別画面１の画面Ｎｏ
            Int32   zentaiGameNo1 = -1;                 // 全体画面１の画面Ｎｏ
            Int32   value = 0;                          // 計算値
            Int32   signGamenNo = -1;                   // 遷移先指定画面Ｎｏ
            UInt16  kind = 0;                           // 基本ファイル名
            String  strBaseFile = String.Empty;         // 基本ファイル名
            String  strAppLog = String.Empty;           // ログメッセージ
            String syncErrorMsg = String.Empty;         // 同期エラーメッセージ

            senisakiGamenNo = -1;                       // 遷移先画面Ｎｏになしを設定
            kobetuGameNo1 = -1;                         // 個別画面１の画面Ｎｏに保存済みではないを設定
            zentaiGameNo1 = -1;                         // 全体画面１の画面Ｎｏに保存済みではないを設定

            try
            {
                // 遷移先として個別画面１または全体画面１を指定した場合
                if (((UInt16)SENISIGN.KOBETU1 == seniSign) ||
                    ((UInt16)SENISIGN.ZENTAI1 == seniSign))
                {
                    foreach (UInt16 drawkeys in CAppDat.DrawSectionF.Keys)
                    {
                        // 描画区間情報の画面種別が個別画面の場合
                        if ((UInt16)CDrawKukanMng.EDISPKIND.KOBETUGAMEN == CAppDat.DrawSectionF[drawkeys].DispKind)
                        {
                            if (kobetuGameNo1 < 0)      // 個別画面の画面Ｎｏが保存済みではない場合
                            {
                                kobetuGameNo1 = (Int32)CAppDat.DrawSectionF[drawkeys].Crno[0];
                                                        // 描画区間情報の画面Ｎｏ（画面Ｎｏリスト内の先頭の画面Ｎｏ）を保存する
                            }
                            else                        // 画面Ｎｏが保存済みの場合
                            {
                                // 処理なし
                            }
                        }
                        // 描画区間情報の画面種別が全体画面の場合
                        else if ((UInt16)CDrawKukanMng.EDISPKIND.ZENTAIGAMEN == CAppDat.DrawSectionF[drawkeys].DispKind)
                        {
                            if (zentaiGameNo1 < 0)      // 全体画面の画面Ｎｏが保存済みではない場合
                            {
                                zentaiGameNo1 = (Int32)CAppDat.DrawSectionF[drawkeys].Crno[0];
                                                        // 描画区間情報の画面Ｎｏ（画面Ｎｏリスト内の先頭の画面Ｎｏ）を保存する
                            }
                            else                        // 画面Ｎｏが保存済みの場合
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


                    // 画面種別として個別画面を指定した場合
                    if ((UInt16)CDrawKukanMng.EDISPKIND.KOBETUGAMEN == gamenKind)
                    {
                        if (0 <= kobetuGameNo1)         // 個別画面の画面Ｎｏが保存済み（保存できた）の場合
                        {
                            senisakiGamenNo = kobetuGameNo1;
                                                        // 個別画面１の画面Ｎｏを遷移先画面Ｎｏとする
                        }
                        else                            // 個別画面の画面Ｎｏが保存済みではない場合
                        {
                            // 処理なし
                        }
                    }
                    // 画面種別として全体画面を指定した場合
                    else if ((UInt16)CDrawKukanMng.EDISPKIND.ZENTAIGAMEN == gamenKind)
                    {
                        if (0 <= zentaiGameNo1)         // 全体画面の画面Ｎｏが保存済み（保存できた）の場合
                        {
                            senisakiGamenNo = zentaiGameNo1;
                                                        // 全体画面１の画面Ｎｏを遷移先画面Ｎｏとする
                        }
                        else                            // 全体画面の画面Ｎｏが保存済みではない場合
                        {
                            // 処理なし
                        }
                    }
                    // 画面種別として分割画面１または分割画面２を指定した場合
                    else if (((UInt16)CDrawKukanMng.EDISPKIND.BUNKATSUGAMEN1 == gamenKind) ||
                             ((UInt16)CDrawKukanMng.EDISPKIND.BUNKATSUGAMEN2 == gamenKind))
                    {
                        if (0 <= kobetuGameNo1)         // 個別画面の画面Ｎｏが保存済み（保存できた）の場合
                        {
                            foreach (UInt16 drawkeys in CAppDat.DrawSectionF.Keys)
                            {
                                // 描画区間情報の画面種別と指定された画面種別が一致した場合
                                if (gamenKind == CAppDat.DrawSectionF[drawkeys].DispKind)
                                {
                                    for (UInt16 iCnt = 0; iCnt < CAppDat.DrawSectionF[drawkeys].Crno.Count; iCnt++)
                                    {
                                        // 描画区間情報内の画面Ｎｏと個別画面１の画面Ｎｏが一致した場合
                                        if (kobetuGameNo1 == CAppDat.DrawSectionF[drawkeys].Crno[iCnt])
                                        {
                                            senisakiGamenNo = CAppDat.DrawSectionF[drawkeys].Crno[0];
                                                                // 描画区間情報の画面Ｎｏ（画面Ｎｏリスト内の先頭の画面Ｎｏ）を遷移先画面Ｎｏとする
                                            break;
                                        }
                                        // 描画区間情報内の画面Ｎｏと個別画面１の画面Ｎｏが一致しない場合
                                        else
                                        {
                                            // 処理なし
                                        }
                                    }

                                    // 遷移先画面Ｎｏが保存できた場合
                                    if (0 <= senisakiGamenNo)
                                    {
                                        break;
                                    }
                                    // 遷移先画面Ｎｏが保存されていない場合
                                    else
                                    {
                                        // 処理なし
                                    }
                                }
                                // 描画区間情報の画面種別と指定された画面種別が一致しない場合
                                else
                                {
                                    // 処理なし
                                }
                            }
                        }
                        else                            // 個別画面の画面Ｎｏが保存済みではない場合
                        {
                            // 処理なし
                        }
                    }
                    // 上記以外
                    else
                    {
                        // 処理なし
                    }
                }
                // 遷移先として指定画面移動を指定した場合
                else if ((UInt16)SENISIGN.SITEI == seniSign)
                {
                    signGamenNo = -1;                   // 遷移先指定画面Ｎｏになしを設定

                    try
                    {
                        blRet = CCommon.IsNumeric(strSeniGamen);
                                                        // 数値かどうかをチェック
                        if (true == blRet)              // 数値の場合
                        {
                            value = Convert.ToInt32(strSeniGamen, 10);
                                                        // 遷移先画面指定を数値に変換する
                            signGamenNo = value;        // 遷移先指定画面Ｎｏを保存する
                        }
                        else                            // 数値ではない場合
                        {
                            // 処理なし
                        }
                    }
                    catch (Exception ex)
                    {
                        // デバッグ用出力
                        CCommon.WriteDebugLog(MethodBase.GetCurrentMethod(), ex.Message);

                        // 変換できなかった場合は、画面の基本ファイル名なので後で処理する
                    }

                    if (0 <= signGamenNo)               // 遷移先画面指定が画面Ｎｏの場合
                    {
                        senisakiGamenNo = signGamenNo;  // 指定された画面Ｎｏを遷移先画面Ｎｏとする
                    }
                    else                                // 遷移先画面指定が画面Ｎｏではない場合
                    {
                        // 画面種別として全体画面を指定した場合
                        if ((UInt16)CDrawKukanMng.EDISPKIND.ZENTAIGAMEN == gamenKind)
                        {
                            kind = 1;                   // 種別に全体画面を設定
                        }
                        // 画面種別として全体画面以外を指定した場合
                        else
                        {
                            kind = 0;                   // 種別に個別画面を設定
                        }

                        foreach (UInt16 drawkeys in CAppDat.DrawSectionF.Keys)
                        {
                            for (UInt16 iCnt = 0; iCnt < CAppDat.DrawSectionF[drawkeys].Crno.Count; iCnt++)
                            {
                                isFind = CCommon.GetBaseFileName(kind, CAppDat.DrawSectionF[drawkeys].Crno[iCnt], ref strBaseFile);
                                                        // 描画区間情報の画面Ｎｏに該当する基本ファイル名を取得する

                                // 基本ファイル名と遷移先画面指定が一致する場合
                                if ((true == isFind) && (strSeniGamen == strBaseFile))
                                {
                                    senisakiGamenNo = CAppDat.DrawSectionF[drawkeys].Crno[0];
                                                        // 描画区間情報の画面Ｎｏ（画面Ｎｏリスト内の先頭の画面Ｎｏ）を遷移先画面Ｎｏとする
                                    break;
                                }
                                // 基本ファイル名と遷移先画面指定が一致しない場合
                                else
                                {
                                    // 処理なし
                                }
                            }

                            // 遷移先画面Ｎｏが保存できた場合
                            if (0 <= senisakiGamenNo)
                            {
                                break;
                            }
                            // 遷移先画面Ｎｏが保存されていない場合
                            else
                            {
                                // 処理なし
                            }
                        }
                    }
                }
                // 上記以外の場合
                else
                {
                    // 遷移先として前画面／次画面移動を指定した場合
                    if (((UInt16)SENISIGN.PREV == seniSign) ||
                        ((UInt16)SENISIGN.NEXT == seniSign))
                    {
                        // 処理なし
                    }
                    // 上記以外の場合
                    else
                    {
                        strAppLog = String.Format("同一画面遷移：seniSign={0:D} gamenKind={1:D} strSeniGamen={2}", seniSign, gamenKind, strSeniGamen);
                        CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.WARNING, MethodBase.GetCurrentMethod(), strAppLog);
                    }

                    // 表示画面情報テーブル排他制御開始
                    CAppDat.MutexCtrl[(ushort)CAppDat.MUTEXID.GAMENINFO].WaitOne();
                    try
                    {
                        // 表示画面情報テーブルの画面Ｎｏ（現在表示中の画面Ｎｏリスト内の先頭の画面Ｎｏ）を遷移先画面Ｎｏとする
                        senisakiGamenNo = CAppDat.GamenInfoT.Crno[0];
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
            }
            catch (Exception ex)
            {
                CAppDat.AppLog.Output(CLogCtrl.LOGTYPE.FATALERROR, MethodBase.GetCurrentMethod(), ex.Message);
            }

            return senisakiGamenNo;
        }
    }
}
