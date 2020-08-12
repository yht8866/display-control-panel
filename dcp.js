"use strict";
// グルーバル変数宣言 ----------------------------------------------------------
var delaydBlinkTargets = [];

// -----------------------------------------------------------------------------
// window で load したときのイベント
// -----------------------------------------------------------------------------
window.onload = function () {
    var e = document.getElementById('timeKeeper');
    e.addEventListener('animationiteration', eventListener);
    // 以下はスタンドアローン用。アプリに組み込むときはコメントアウトすること。
    //doChangeState("00_窓_8,none,4,1,,,");
}

// -----------------------------------------------------------------------------
// timeKeeper オブジェクトが animationiteration を発行したときのイベント
// -----------------------------------------------------------------------------
function eventListener(e) {
    UpdateTargets();
}

// -----------------------------------------------------------------------------
// リストに保管済みのデータを表示部品に反映するメソッド
// -----------------------------------------------------------------------------
function UpdateTargets() {
    for (var i = 0, len = delaydBlinkTargets.length; i < len; i++) {
        doChangeState(delaydBlinkTargets[i], "TRUE");
    }
    delaydBlinkTargets.length = 0;
}

// -----------------------------------------------------------------------------
// 変化状態をリストに保管しておくメソッド
// param=args："キーID,表示有無,区分番号,状態番号,付加情報１,付加情報２,"形式の文字列
// -----------------------------------------------------------------------------
function storeChangeState(arg) {
    delaydBlinkTargets.push(arg);
}

// -----------------------------------------------------------------------------
// 変化状態を解析して状態更新を行うメソッド
// param=args："キーID,表示有無,区分番号,状態番号,付加情報１,付加情報２,"形式の文字列
// -----------------------------------------------------------------------------
function doChangeState(args1, args2) {
    //alert(args);
    if (null == args2) {
        args2 = "FALSE";
    }

    var doc = document;
    var params = args1.split(",");

    var fst_p;    // firstChildを入れる用
    var now_p;    // 最背面に持っていく要素を入れる用

    for (var i = 0, paramslen = params.length - 1; i < paramslen ; i += 6) {
        var elem = doc.getElementById(params[i]);
        if (elem == null) {
            continue;
        }
        
        if (params[i + 1] == "inline") {
            elem.setAttribute("style", "display:none");
        }
        var kind = elem.getAttribute("kind");
        var label;
        switch (kind) {
            case "窓":
            case "進路":
            case "転てつ器":
            case "軌道回路":
                label = "subunit2";
                break;
            default:
                label = "subunit1";
                break;
        }

/*
    部品名称     |区分1(付1,付2)     |区分2(付1,付2)     |区分3(付1,付2)     |区分4(付1,付2)   |区分11(付1,付2)    |区分12(付1,付2)    |区分14(付1,付2)
    -------------+-------------------+-------------------+-------------------+-----------------+-------------------+-------------------+-------------------
    表示ﾗﾍﾞﾙ     |文字列,なし        |***                |***                |***              |***                |***                |***                
    駅名ﾗﾍﾞﾙ     |文字列,なし        |***                |***                |***              |***                |***                |***                
    てこﾗﾍﾞﾙ     |文字列,なし        |文字列,なし        |文字列,なし        |***              |***                |***                |***                
    てこｸﾞﾗﾌｨｯｸ  |なし,なし          |***                |***                |***              |***                |***                |***                
    表示ｸﾞﾗﾌｨｯｸ  |なし,なし          |***                |***                |***              |***                |***                |***                
    進路         |なし,なし          |なし,なし          |***                |***              |***                |***                |***                
    軌道回路     |ﾊﾟﾀｰﾝ番号,前面有無 |ﾊﾟﾀｰﾝ番号,なし     |***                |***              |***                |***                |***                
    転てつ器     |ﾊﾟﾀｰﾝ番号,前面有無 |***                |***                |***              |***                |***                |***                
    窓           |文字列,なし        |なし,なし          |文字列,なし        |枠表示有無,なし  |***                |***                |***                
    発着てこﾗﾍﾞﾙ |文字列,区分番号    |文字列,区分番号    |文字列,区分番号    |***              |文字列,区分番号    |***                |文字列,区分番号    
    表示てこﾗﾍﾞﾙ |文字列,操作状態    |文字列,操作状態    |***                |***              |文字列,操作状態    |文字列,操作状態    |***                
*/

        // クラス名: <パーツ種別>_<区分>_<状態>_<情報種別>_<fill|stroke>
        var selector = 'x' + kind + '_' + params[i + 2] + "_" + params[i + 3] + "_";
        var gs = elem.getElementsByTagName("g");

        // gエレメントを総ざらい
        var kubun;
        for (var j = 0, gslen = gs.length; j < gslen; j++) {
            var target = gs[j];

            var child = target.firstElementChild;
            var attr = target.getAttribute("gkind");
            var prevClassName = child.className.baseVal;

            // パーツの最初の要素を取得する
            if(kind == "軌道回路"){
                // 最背面となる場所を保持る
                fst_p = elem.firstElementChild.firstElementChild;
            }

            if (kind == "表示グラフィック") {
                var tekographkind = target.getAttribute("kind");
                if ("1" == tekographkind) {
                    // 線部分のみでなく、パーツ全体を表示⇔非表示させる
                        if (params[i + 1] == "inline") {
                            if (params[i + 3] < 1) {
                                params[i + 1] = "none";
                            }
                        }
                }
            }

            if (kind == "てこグラフィック") {
                var tekographkind = target.getAttribute("kind");
                if ("1" == tekographkind) {
                    // 線部分のみでなく、パーツ全体を表示⇔非表示させる
                        if (params[i + 1] == "inline") {
                            if (params[i + 3] <= 1) {
                                params[i + 1] = "none";
                            }
                        }
                }
            }

            if (attr == label) {
                if (kind == "窓") {
                    var infoKind = target.getAttribute("kind");
                    if (infoKind == "遅延時分") {
                        if (params[i + 1] == "inline") {
                            if (params[i + 2] == "3") {
                                var dispKind = "none";
                                if (params[i + 3] == 0) {
                                    dispKind = "none";
                                }
                                else {
                                    dispKind = "inline";
                                }
                                target.setAttribute("style", "display:" + dispKind);
                            }
                        }
                    }
                }
            }

            if (attr == "unit") {
                // 「情報種別」を取得
                kubun = target.getAttribute("kind");
                selector += kubun;
            }
            if (attr == label) {
                var infoKind = target.getAttribute("kind");
                switch (infoKind) {
                    // 塗りつぶし
                    case "背景":
                    case "塗りつぶし部分":
                        child.className.baseVal = selector + "_fill";
                        if ("TRUE" == args2) {
                           child.setAttribute("style", "animation-play-state:running");
                        }
                        else {
                           child.setAttribute("style", "animation-play-state:paused");
                        }
                        break;
                    case "現示部分":
                        if (params[i + 2] != 1) {
                            continue;
                        }
                        child.className.baseVal = selector + "_fill";
                        if ("TRUE" == args2) {
                           child.setAttribute("style", "animation-play-state:running");
                        }
                        else {
                           child.setAttribute("style", "animation-play-state:paused");
                        }
                        break;
                    case "列番背景":
                    case "遅延背景":
                        if (kind == "窓") {
                            if ((infoKind == "列番背景" && params[i + 2] != 2) || (infoKind == "遅延背景" && params[i + 2] != 3)) {
                                continue;
                            }
                        }
                        child.className.baseVal = selector + "_fill";
                        if ("TRUE" == args2) {
                           child.setAttribute("style", "animation-play-state:running");
                        }
                        else {
                           child.setAttribute("style", "animation-play-state:paused");
                        }
                        break;
                    // 塗りつぶし+文字列設定
                    case "文字列":
                    case "列番文字":
                    case "遅延時分":
                        if (kind == "窓") {
                            if ((infoKind == "列番文字" && params[i + 2] != 1) || (infoKind == "遅延時分" && params[i + 2] != 3)) {
                                continue;
                            }
                        }
                        if (kind == "発着てこラベル") {
                            if ((infoKind == "文字列" && params[i + 2] == 1 && params[i + 5] > 30 && params[i + 5] <= 60) ||
                                (infoKind == "文字列" && params[i + 2] == 11 && params[i + 5] > 30 && params[i + 5] <= 60)) {
                                child.className.baseVal = child.className.baseVal;
                                child.textContent = params[i + 4];
                                if ("TRUE" == args2) {
                                    child.setAttribute("style", "animation-play-state:running");
                                }
                                else {
                                    child.setAttribute("style", "animation-play-state:paused");
                                }
                                continue;
                            }
                        }
                        child.className.baseVal = selector + "_fill";
                        if ("TRUE" == args2) {
                            child.setAttribute("style", "animation-play-state:running");
                        }
                        else {
                            child.setAttribute("style", "animation-play-state:paused");
                        }

                        if (kind == "コマンドボタンラベル") {
                            if (infoKind == "文字列") {
                                continue;
                            }
                        }
                        if (kind == "表示てこラベル") {
                            if ((infoKind == "文字列" && params[i + 2] == 11 && params[i + 5] != 0) ||
                                (infoKind == "文字列" && params[i + 2] == 12 && params[i + 5] != 0)) {
                                continue;
                            }
                        }
                        child.textContent = params[i + 4];
                        break;
                    //線の色
                    case "選択枠":
                        if (kind == "窓" && params[i + 2] != 4) {
                            continue;
                        }
                        child.setAttribute("display", params[i + 4]);
                        child.className.baseVal = selector + "_stroke";
                        if ("TRUE" == args2) {
                            child.setAttribute("style", "animation-play-state:running");
                        }
                        else {
                            child.setAttribute("style", "animation-play-state:paused");
                        }
                        break;
                    case "枠":
                    case "線部分":
                        child.className.baseVal = selector + "_stroke";
                        if ("TRUE" == args2) {
                            child.setAttribute("style", "animation-play-state:running");
                        }
                        else {
                            child.setAttribute("style", "animation-play-state:paused");
                        }
                        break;
                    case "軌道部分":
                    case "線路閉鎖部分":
                        //line-ptn が一致するか
                        if (child.firstElementChild.textContent != params[i + 4]) {
                            continue;
                        }
                        // 軌道回路と転てつ器パーツで区分1,2(軌道部分or線閉部分)以外であるか？
                        if ((infoKind == "線路閉鎖部分" && params[i + 2] != 2) || (infoKind == "軌道部分" && params[i + 2] != 1)) {
                            continue;
                        }
                        child.className.baseVal = selector + "_stroke";
                        if ("TRUE" == args2) {
                            child.setAttribute("style", "animation-play-state:running");
                        }
                        else {
                            child.setAttribute("style", "animation-play-state:paused");
                        }

                        // まず、軌道回路で振るいわける
                        if(infoKind == "軌道部分") {
                            // 次に最前面指定であれば、表示順を上位へ
                            if(params[i + 5] == 1) {
                                var p = child.parentNode;
                                p.parentNode.appendChild(p);
                            }
                            // 次に最背面設定ならば、表示順を下位へ
                            if(params[i+5] == 10){
                                now_p = child.parentNode;                      // 現在の場所をとって
                                fst_p.insertBefore(now_p,fst_p.firstChild);    // 最初にとったパーツの最初の部分の前に入れる
                            }
                        } //end_ infoKind 
                        break;
                    case "接近ボタン動作防止枠":
                        if (params[i + 3] == 0) {
                            target.setAttribute("style", "display:inline");
                        }
                        else {
                            target.setAttribute("style", "display:none");
                        }
                        break;
                    default:
                        break;
                } //
            } //end_attr==label
        } //end_for(j) 
        elem.setAttribute("style", "display:" + params[i + 1]);
    } //end_for(i) 
}//end_doChange()

// -----------------------------------------------------------------------------
// 表示切換更新を行うメソッド
// param=args："キーID,表示有無,"形式の文字列
// -----------------------------------------------------------------------------
function doChangeEnabled(args) {
    //alert(args);
    var doc = document;
    var params = args.split(",");
    for (var i = 0, paramslen = params.length - 1; i < paramslen ; i += 2) {
        var elem = doc.getElementById(params[i]);
        if (elem == null) {
            continue;
        }
        
        if (params[i + 1] == "inline") {
            elem.setAttribute("style", "display:inline");
        }
        else {
            elem.setAttribute("style", "display:none");
        }
    }
}

// -----------------------------------------------------------------------------
// カーソル待機切換メソッド
// param=args："TRUE/FALSE"形式の文字列
// -----------------------------------------------------------------------------
function changeCursors(args) {
    //alert(args);
    var doc = document;
    if (args == "TRUE") {
        doc.body.style.cursor = 'wait';
    }
    else {
        doc.body.style.cursor = 'default';
    }
}
