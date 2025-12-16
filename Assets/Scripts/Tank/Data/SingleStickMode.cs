// ==============================================================================
// SingleStickMode.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-16
// 概要     : 左スティックのみで移動と旋回を行う入力モード
// ==============================================================================

using UnityEngine;
using TankSystem.Controller;
using TankSystem.Interface;

namespace TankSystem.Data
{
    /// <summary>
    /// 左スティック単独操作用入力モード
    /// </summary>
    internal sealed class SingleStickMode : ITrackInputMode
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>計算ロジック参照</summary>
        private readonly TankTrackController calculator;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// 直前の前進／後退方向（前進:+1 / 停止:0 / 後退:-1）
        /// </summary>
        private float _lastForwardSign = 0f;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>
        /// 直前の前進方向として記録するための最小入力閾値
        /// </summary>
        private const float LAST_FORWARD_SIGN_UPDATE_DEADZONE = 0.5f;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// シングルスティック入力方式の計算ロジックを生成する
        /// </summary>
        /// <param name="calculator">移動量および旋回量計算を担当するコントローラ</param>
        internal SingleStickMode(TankTrackController calculator)
        {
            this.calculator = calculator;
        }

        // ======================================================
        // ITrackInputMode イベント
        // ======================================================

        /// <summary>
        /// 左スティック入力を移動量および旋回量に変換する
        /// </summary>
        /// <param name="leftInput">左スティックの入力値</param>
        /// <param name="rightInput">右スティックの入力値（本モードでは未使用）</param>
        /// <param name="forwardAmount">算出された前進／後退量</param>
        /// <param name="turnAmount">算出された旋回量</param>
        public void Calculate(
            in Vector2 leftInput,
            in Vector2 rightInput,
            out float forwardAmount,
            out float turnAmount
        )
        {
            // 横入力を補正
            float x = calculator.RoundValue(leftInput.x);

            // 縦入力を補正
            float y = calculator.RoundValue(leftInput.y);

            // 上下入力がある場合は前進・後退を優先
            if (!Mathf.Approximately(y, 0f))
            {
                // 前進量を算出（入力の大小に関わらず実行）
                forwardAmount = calculator.CalculateForwardFromSingleAxis(y);

                // 入力履歴更新に使う有効入力かを判定
                bool canUpdateLastSign =
                    Mathf.Abs(y) >= LAST_FORWARD_SIGN_UPDATE_DEADZONE;

                // 履歴更新判定用の入力絶対値を取得
                float absY = Mathf.Abs(y);

                // 現在の入力符号を取得（0 の場合は 0）
                float currentSign = Mathf.Sign(y);

                // 逆符号入力かどうかを判定
                bool isReverseInput =
                    _lastForwardSign != 0f &&
                    currentSign != 0f &&
                    currentSign != _lastForwardSign;

                // --------------------------------------------------
                // 直前方向の更新ロジック
                // --------------------------------------------------
                if (isReverseInput)
                {
                    // 逆符号かつ十分な入力がある場合は即座に反転
                    if (absY >= LAST_FORWARD_SIGN_UPDATE_DEADZONE)
                    {
                        _lastForwardSign = currentSign;
                    }
                    // 逆符号だが微小入力の場合はいったん無効化
                    else
                    {
                        _lastForwardSign = 0f;
                    }
                }
                else
                {
                    // 同符号、または未設定状態の場合のみ通常更新
                    if (_lastForwardSign == 0f && absY >= LAST_FORWARD_SIGN_UPDATE_DEADZONE)
                    {
                        _lastForwardSign = currentSign;
                    }
                }

                // 左右入力がある場合のみ旋回
                turnAmount = Mathf.Approximately(x, 0f)
                    ? 0f
                    : calculator.CalculateTurnFromSingleAxis(x) * Mathf.Sign(y);

                return;
            }

            // 前進量はゼロ
            forwardAmount = 0f;

            // 直前が後退だった場合のみ旋回方向を反転
            float turnSign = _lastForwardSign < 0f ? -1f : 1f;

            // 旋回量を算出
            turnAmount = calculator.CalculateTurnFromSingleAxis(x) * turnSign;
        }
    }
}