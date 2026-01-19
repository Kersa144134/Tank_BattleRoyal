// ======================================================
// SlotRotationUIController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-01-19
// 更新日時 : 2026-01-19
// 概要     : UI スロットを循環表示する汎用コントローラー
//            マスク対応・符号反転ループ・線形補間移動
//            Rotate呼び出しでターゲット番号を更新
// ======================================================

using UnityEngine;
using UnityEngine.UI;

namespace UISystem.Controller
{
    /// <summary>
    /// UI スロットを一定方向に並べ、
    /// スロットの循環表示を行う汎用 UI コントローラー
    /// </summary>
    public sealed class SlotRotationUIController
    {
        // ======================================================
        // 列挙体
        // ======================================================

        public enum LayoutDirection
        {
            Horizontal,
            Vertical
        }

        public enum RotationSign
        {
            Positive,
            Negative
        }

        // ======================================================
        // 定数
        // ======================================================

        private const float SLOT_SPACING = 200.0f;  // スロット間隔(px)
        private const float MOVE_SPEED = 1000.0f;   // 移動速度(px/s)

        // ======================================================
        // フィールド
        // ======================================================

        private readonly Image[] _slotImages;              // 制御対象スロット
        private readonly LayoutDirection _layoutDirection; // 配置方向
        private readonly RotationSign _rotationSign;       // 初期符号方向

        private int _positionSign;      // 現在の符号(1 or -1)
        private int _currentTargetIndex; // Rotate呼び出しで更新されるターゲット番号

        private Vector2[] _targetPositions; // 各スロットの目標位置

        // ======================================================
        // コンストラクタ
        // ======================================================

        public SlotRotationUIController(
            Image[] slotImages,
            LayoutDirection layoutDirection,
            RotationSign rotationSign)
        {
            _slotImages = slotImages;
            _layoutDirection = layoutDirection;
            _rotationSign = rotationSign;

            _positionSign = (_rotationSign == RotationSign.Positive) ? 1 : -1;
            _currentTargetIndex = 0;

            _targetPositions = new Vector2[_slotImages.Length];

            // 初期レイアウトを設定
            SetupLayout();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// Rotate呼び出しでターゲット番号を1つ進める（符号反転ループ）
        /// </summary>
        public void Rotate()
        {
            _currentTargetIndex++;

            if (_currentTargetIndex >= _slotImages.Length)
            {
                _currentTargetIndex = 0;
                _positionSign *= -1; // 符号反転
            }

            // ターゲット番号に基づいて各スロットの目標位置を計算
            for (int i = 0; i < _slotImages.Length; i++)
            {
                _targetPositions[i] = CalculateTargetPosition(i);
            }
        }

        /// <summary>
        /// 毎フレーム呼び出してスロットを線形補間で移動
        /// </summary>
        public void Update(float deltaTime)
        {
            for (int i = 0; i < _slotImages.Length; i++)
            {
                RectTransform rect = _slotImages[i].rectTransform;
                rect.anchoredPosition = Vector2.MoveTowards(
                    rect.anchoredPosition,
                    _targetPositions[i],
                    MOVE_SPEED * deltaTime
                );
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 初期レイアウトを設定（ターゲット位置も初期化）
        /// </summary>
        private void SetupLayout()
        {
            for (int i = 0; i < _slotImages.Length; i++)
            {
                Vector2 pos = CalculateTargetPosition(i);
                _slotImages[i].rectTransform.anchoredPosition = pos;
                _targetPositions[i] = pos;
            }
        }

        /// <summary>
        /// スロットindexに対する現在のターゲット位置を計算
        /// _currentTargetIndexに基づき配置
        /// </summary>
        private Vector2 CalculateTargetPosition(int index)
        {
            Vector2 basePosition = Vector2.zero;

            // ターゲット番号との差分でオフセットを計算
            int offsetIndex = index - _currentTargetIndex;

            if (_layoutDirection == LayoutDirection.Horizontal)
            {
                return new Vector2(basePosition.x + SLOT_SPACING * offsetIndex * _positionSign, basePosition.y);
            }
            else
            {
                return new Vector2(basePosition.x, basePosition.y - SLOT_SPACING * offsetIndex * _positionSign);
            }
        }
    }
}