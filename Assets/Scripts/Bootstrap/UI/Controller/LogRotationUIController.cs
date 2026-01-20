// ======================================================
// LogRotationUIController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-01-20
// 更新日時 : 2026-01-20
// 概要     : ログ表示をスロット方式で制御する UI コントローラー
//            ターゲット座標に基づく線形補間移動と
//            TextMeshPro の循環再利用を行う
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace UISystem.Controller
{
    public sealed class LogRotationUIController
    {
        // ======================================================
        // 列挙体
        // ======================================================

        /// <summary>ログの縦方向表示方向</summary>
        public enum VerticalDirection
        {
            Positive,
            Negative
        }

        /// <summary>ログの挿入方向</summary>
        public enum InsertDirection
        {
            Left,
            Right
        }

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>表示可能行数</summary>
        private const int VISIBLE_LINE_COUNT = 5;

        /// <summary>ログ1行分の縦方向間隔</summary>
        private const float LINE_SPACING_Y = 100.0f;

        /// <summary>ログ表示の基準X座標</summary>
        private const float LOG_BASE_POSITION_X = -10.0f;

        /// <summary>ログ表示1行目の基準Y座標</summary>
        private const float LOG_BASE_FIRST_POSITION_Y = -50.0f;

        /// <summary>ログ挿入時の初期X移動距離</summary>
        private const float INSERT_OFFSET_X = 1000.0f;

        /// <summary>ログの移動速度</summary>
        private const float MOVE_SPEED = 2000.0f;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>ログ表示に使用する TextMeshPro 配列（循環利用）</summary>
        private readonly TextMeshProUGUI[] _logTexts;

        /// <summary>縦方向の表示方向</summary>
        private readonly VerticalDirection _verticalDirection;

        /// <summary>ログ挿入方向</summary>
        private readonly InsertDirection _insertDirection;

        /// <summary>ログ表示用ターゲット座標配列（0 は非表示行）</summary>
        private Vector2[] _targetPositions;

        /// <summary>表示中ログの RectTransform キュー</summary>
        private readonly Queue<RectTransform> _logQueue;

        /// <summary>次に使用する TextMeshPro の循環インデックス</summary>
        private int _currentTextIndex;

        /// <summary>ログ通し番号（仮表示用）</summary>
        private int _logSerialNumber;

        // ======================================================
        // コンストラクタ
        // ======================================================

        public LogRotationUIController(
            in TextMeshProUGUI[] logTexts,
            in VerticalDirection verticalDirection,
            in InsertDirection insertDirection)
        {
            _logTexts = logTexts;
            _verticalDirection = verticalDirection;
            _insertDirection = insertDirection;

            _logQueue = new Queue<RectTransform>();

            _currentTextIndex = 0;
            _logSerialNumber = 0;

            // ターゲット座標を初期化
            InitializeTargetPositions();

            // すべてのログテキストを非表示行へ初期配置
            InitializeLogTextPositions();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 新しいログを追加する
        /// </summary>
        public void AddLog()
        {
            // 循環利用する TextMeshPro を取得
            TextMeshProUGUI logText = GetNextLogText();

            // ログ番号を更新
            _logSerialNumber++;

            // 仮表示としてログ番号を設定
            logText.text = $"Log {_logSerialNumber}";

            // RectTransform を取得
            RectTransform rect = logText.rectTransform;

            // --------------------------------------------------
            // 挿入方向に応じた初期 X 座標を算出
            // --------------------------------------------------

            float startX =
                (_insertDirection == InsertDirection.Left)
                    ? LOG_BASE_POSITION_X - INSERT_OFFSET_X
                    : LOG_BASE_POSITION_X + INSERT_OFFSET_X;

            // --------------------------------------------------
            // 追加後に配置される表示行インデックスを算出
            // --------------------------------------------------

            int targetLineIndex = _logQueue.Count + 1;

            // 念のため最大表示行数でクランプ
            if (targetLineIndex > VISIBLE_LINE_COUNT)
            {
                targetLineIndex = VISIBLE_LINE_COUNT;
            }

            // 対応する目標 Y 座標を取得
            float startY = _targetPositions[targetLineIndex].y;

            // X は画面外、Y は目標行へ瞬間移動
            rect.anchoredPosition = new Vector2(startX, startY);

            // ログキューに追加
            _logQueue.Enqueue(rect);

            // --------------------------------------------------
            // 表示可能行数を超えた場合の排出処理
            // --------------------------------------------------

            while (_logQueue.Count > VISIBLE_LINE_COUNT)
            {
                RectTransform removed = _logQueue.Dequeue();

                // 現在の X 座標を保持
                float currentX = removed.anchoredPosition.x;

                // Y のみ非表示行へ瞬間移動
                removed.anchoredPosition =
                    new Vector2(currentX, _targetPositions[0].y);
            }
        }

        /// <summary>
        /// 毎フレーム呼び出してログを線形補間移動させる
        /// </summary>
        public void Update(in float deltaTime)
        {
            float moveDelta = MOVE_SPEED * deltaTime;

            int index = 1;

            foreach (RectTransform rect in _logQueue)
            {
                // 対応するターゲット座標を取得
                Vector2 target = _targetPositions[index];

                // 現在位置を取得
                Vector2 current = rect.anchoredPosition;

                // X方向は固定位置へ線形補間
                float nextX = Mathf.MoveTowards(current.x, LOG_BASE_POSITION_X, moveDelta);

                // Y方向はターゲット行へ線形補間
                float nextY = Mathf.MoveTowards(current.y, target.y, moveDelta);

                // 座標を更新
                rect.anchoredPosition = new Vector2(nextX, nextY);

                index++;
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// ターゲット座標配列を初期化する
        /// 0 番は非表示行
        /// </summary>
        private void InitializeTargetPositions()
        {
            // 非表示行 + 表示行数分の要素数を算出
            int totalLineCount = VISIBLE_LINE_COUNT + 1;

            // ターゲット座標配列を生成
            _targetPositions = new Vector2[totalLineCount];

            // --------------------------------------------------
            // 非表示行（退場用）の Y 座標を決定
            // 表示方向と逆側に 1 行分ずらす
            // --------------------------------------------------

            float hiddenLineY =
                (_verticalDirection == VerticalDirection.Negative)
                    ? LOG_BASE_FIRST_POSITION_Y + LINE_SPACING_Y
                    : LOG_BASE_FIRST_POSITION_Y - LINE_SPACING_Y;

            // 非表示行（index 0）
            _targetPositions[0] = new Vector2(
                LOG_BASE_POSITION_X,
                hiddenLineY);

            // --------------------------------------------------
            // 表示行（index 1 ～）
            // --------------------------------------------------

            for (int i = 1; i < totalLineCount; i++)
            {
                // 表示1行目からのオフセット量を算出
                float offset = LINE_SPACING_Y * (i - 1);

                // 表示方向に応じて Y 座標を決定
                float y =
                    (_verticalDirection == VerticalDirection.Negative)
                        ? LOG_BASE_FIRST_POSITION_Y - offset
                        : LOG_BASE_FIRST_POSITION_Y + offset;

                // ターゲット座標を設定
                _targetPositions[i] = new Vector2(
                    LOG_BASE_POSITION_X,
                    y);
            }
        }

        /// <summary>
        /// すべてのログテキストを非表示行へ初期配置する
        /// </summary>
        private void InitializeLogTextPositions()
        {
            Vector2 hiddenPosition = _targetPositions[0];

            for (int i = 0; i < _logTexts.Length; i++)
            {
                // RectTransform を取得
                RectTransform rect = _logTexts[i].rectTransform;

                // X / Y ともに非表示行へ瞬間移動
                rect.anchoredPosition = hiddenPosition;
            }
        }

        /// <summary>
        /// 次に使用する TextMeshPro を循環取得する
        /// </summary>
        private TextMeshProUGUI GetNextLogText()
        {
            TextMeshProUGUI text = _logTexts[_currentTextIndex];

            _currentTextIndex++;

            if (_currentTextIndex >= _logTexts.Length)
            {
                _currentTextIndex = 0;
            }

            return text;
        }
    }
}