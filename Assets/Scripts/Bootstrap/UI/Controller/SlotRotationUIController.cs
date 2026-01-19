// ======================================================
// SlotRotationUIController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-01-19
// 更新日時 : 2026-01-19
// 概要     : UI スロットを循環表示する汎用コントローラー
//            起動時に ±SLOT_SPACING * 要素数 / 2 の範囲でパターンを決定
//            回転方向に応じて最低/最大目標値への移動のみ瞬間移動
// ======================================================

using UnityEngine;
using UnityEngine.UI;

namespace UISystem.Controller
{
    public sealed class SlotRotationUIController
    {
        // ======================================================
        // 列挙体
        // ======================================================

        /// <summary>
        /// UI スロットの配置方向
        /// </summary>
        public enum LayoutDirection
        {
            Horizontal,
            Vertical
        }

        /// <summary>
        /// スロット回転方向
        /// </summary>
        public enum RotationSign
        {
            Positive,
            Negative
        }

        // ======================================================  
        // 定数  
        // ======================================================  

        /// <summary>スロット間の表示間隔(px)</summary>
        private const float SLOT_SPACING = 200.0f;

        /// <summary>スロットの移動速度(px/s)</summary>
        private const float MOVE_SPEED = 1000.0f;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>制御対象の UI スロット Image 配列</summary>
        private readonly Image[] _slotImages;

        /// <summary>UI スロットの配置方向</summary>
        private readonly LayoutDirection _layoutDirection;

        /// <summary>スロット回転方向</summary>
        private readonly RotationSign _rotationSign;

        /// <summary>各スロットの目標座標パターン</summary>
        private Vector2[] _targetPositions;

        /// <summary>スロットごとの参照インデックス</summary>
        private int[] _slotTargetIndex;     

        /// <summary>現在のターゲット番号</summary>
        private int _currentTargetIndex;

        /// <summary>最低目標座標</summary>
        private float _minTargetCoord;

        /// <summary>最大目標座標</summary>
        private float _maxTargetCoord;

        // ======================================================
        // コンストラクタ
        // ======================================================
        public SlotRotationUIController(Image[] slotImages, LayoutDirection layoutDirection, RotationSign rotationSign)
        {
            _slotImages = slotImages;
            _layoutDirection = layoutDirection;
            _rotationSign = rotationSign;

            int slotCount = _slotImages.Length;
            _targetPositions = new Vector2[slotCount];
            _slotTargetIndex = new int[slotCount];
            _currentTargetIndex = 0;

            // 初期目標座標パターン計算
            InitializeTargetPositions();

            // 初期参照インデックスを設定
            for (int i = 0; i < slotCount; i++)
            {
                _slotTargetIndex[i] = i;
                _slotImages[i].rectTransform.anchoredPosition = _targetPositions[i];
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ターゲット番号を1つ進め、線形補完／瞬間移動を適用
        /// </summary>
        public void Rotate()
        {
            int slotCount = _slotImages.Length;

            // 現在のターゲット番号を循環更新
            _currentTargetIndex = (_currentTargetIndex + 1) % slotCount;

            for (int i = 0; i < slotCount; i++)
            {
                RectTransform rect = _slotImages[i].rectTransform;

                // 参照インデックスを回転方向に応じて循環シフト
                // スロットを表示上の順番で回転させるには参照インデックスを座標配列の逆方向に更新する必要があるため、負方向にシフトする
                _slotTargetIndex[i] = (_slotTargetIndex[i] - 1 + slotCount) % slotCount;
                
                // 次の目標座標を取得
                Vector2 nextTarget = _targetPositions[_slotTargetIndex[i]];

                // 瞬間移動判定
                float coord = (_layoutDirection == LayoutDirection.Horizontal) ? nextTarget.x : nextTarget.y;
                bool isInstant = (_rotationSign == RotationSign.Positive && Mathf.Approximately(coord, _minTargetCoord)) ||
                                 (_rotationSign == RotationSign.Negative && Mathf.Approximately(coord, _maxTargetCoord));

                if (isInstant)
                {
                    // 瞬間移動適用
                    rect.anchoredPosition = nextTarget;
                }
                else
                {
                    // 線形補完用目標座標に設定（Update() で補完）
                    _slotImages[i].rectTransform.anchoredPosition = Vector2.MoveTowards(rect.anchoredPosition, nextTarget, 0f);
                }
            }
        }

        /// <summary>
        /// 毎フレーム呼び出してスロットを線形補完移動
        /// </summary>
        public void Update(float deltaTime)
        {
            for (int i = 0; i < _slotImages.Length; i++)
            {
                RectTransform rect = _slotImages[i].rectTransform;
                Vector2 target = _targetPositions[_slotTargetIndex[i]];
                rect.anchoredPosition = Vector2.MoveTowards(rect.anchoredPosition, target, MOVE_SPEED * deltaTime);
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 起動時に初期目標座標パターンを計算
        /// 要素0は必ず0に配置
        /// 配置上限を超えた場合は ±SLOT_SPACING*要素数 でループ補正
        /// </summary>
        private void InitializeTargetPositions()
        {
            int slotCount = _slotImages.Length;

            // 配列の初期化
            _targetPositions = new Vector2[slotCount];

            float halfRange = SLOT_SPACING * (slotCount - 1) / 2f;

            // 初期座標を計算
            for (int i = 0; i < slotCount; i++)
            {
                float coord;

                // 回転方向に応じて初期座標を決定
                if (_rotationSign == RotationSign.Positive)
                {
                    coord = -SLOT_SPACING * i;
                }
                else
                {
                    coord = SLOT_SPACING * i;
                }

                // 配置上限を超えた場合はループ補正
                if (coord > halfRange)
                {
                    coord -= SLOT_SPACING * slotCount;
                }
                if (coord < -halfRange)
                {
                    coord += SLOT_SPACING * slotCount;
                }

                // 配置方向に応じて Vector2 に変換
                if (_layoutDirection == LayoutDirection.Horizontal)
                {
                    _targetPositions[i] = new Vector2(coord, 0f);
                }
                else
                {
                    _targetPositions[i] = new Vector2(0f, coord);
                }
            }

            // 最小／最大目標座標を配列から決定
            _minTargetCoord = float.MaxValue;
            _maxTargetCoord = float.MinValue;

            for (int i = 0; i < slotCount; i++)
            {
                float value = (_layoutDirection == LayoutDirection.Horizontal) ? _targetPositions[i].x : _targetPositions[i].y;
                if (value < _minTargetCoord)
                {
                    _minTargetCoord = value;
                }
                if (value > _maxTargetCoord)
                {
                    _maxTargetCoord = value;
                }
            }
        }
    }
}