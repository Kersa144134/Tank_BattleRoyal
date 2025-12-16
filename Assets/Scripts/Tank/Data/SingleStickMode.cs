// ==============================================================================
// SingleStickMode.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-16
// 更新日時 : 2025-12-16
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

        /// <summary>移動量および旋回量計算を担当するコントローラ</summary>
        private readonly TankTrackController _trackController;

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

        /// <summary>
        /// 前進優先かその場旋回かを判定するための入力差分閾値
        /// </summary>
        private const float IN_PLACE_TURN_DIFF_THRESHOLD = 0.75f;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// シングルスティック入力方式の計算ロジックを生成する
        /// </summary>
        /// <param name="calculator">移動量および旋回量計算を担当するコントローラ</param>
        internal SingleStickMode(TankTrackController trackController)
        {
            _trackController = trackController;
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
            float x = _trackController.ConvertRoundedInputToCorrectedValue(leftInput.x);

            // 縦入力を補正
            float y = _trackController.ConvertRoundedInputToCorrectedValue(leftInput.y);

            // 前進優先かどうかを判定
            bool isForwardPriority = IsForwardPriority(x, y);

            // 前進／後退優先時の処理
            if (isForwardPriority)
            {
                // 前進量を算出
                forwardAmount = _trackController.CalculateForwardFromSingleAxis(y);

                // 入力履歴を更新
                UpdateLastForwardSign(y);

                // 前進中の旋回量を算出
                turnAmount = CalculateForwardTurn(x, y);

                return;
            }

            // その場旋回時の処理
            forwardAmount = 0f;

            // その場旋回量を算出
            turnAmount = CalculateInPlaceTurn(x);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // 前進／旋回判定
        // --------------------------------------------------
        /// <summary>
        /// 前進入力が旋回入力より優勢かを判定する
        /// </summary>
        private bool IsForwardPriority(float x, float y)
        {
            // 横入力の絶対値を取得
            float absX = Mathf.Abs(x);

            // 縦入力の絶対値を取得
            float absY = Mathf.Abs(y);

            // 横入力が縦入力より一定以上大きい場合はその場旋回と判定
            return absX - absY < IN_PLACE_TURN_DIFF_THRESHOLD;
        }

        // --------------------------------------------------
        // 旋回量算出
        // --------------------------------------------------
        /// <summary>
        /// 前進中の旋回量を算出する
        /// </summary>
        private float CalculateForwardTurn(float x, float y)
        {
            // 横入力が無い場合は旋回しない
            if (Mathf.Approximately(x, 0f))
            {
                return 0f;
            }

            // 前進方向に応じて旋回方向を決定
            return _trackController.CalculateTurnFromSingleAxis(x) * Mathf.Sign(y);
        }

        /// <summary>
        /// その場旋回時の旋回量を算出する
        /// </summary>
        private float CalculateInPlaceTurn(float x)
        {
            // 直前が後退だった場合のみ旋回方向を反転
            float turnSign = _lastForwardSign < 0f ? -1f : 1f;

            // 横入力に基づいて旋回量を算出
            return _trackController.CalculateTurnFromSingleAxis(x) * turnSign;
        }

        // --------------------------------------------------
        // 入力履歴更新
        // --------------------------------------------------
        /// <summary>
        /// 前進／後退方向の入力履歴を更新する
        /// </summary>
        private void UpdateLastForwardSign(float y)
        {
            // 縦入力の絶対値を取得
            float absY = Mathf.Abs(y);

            // 現在の入力符号を取得
            float currentSign = Mathf.Sign(y);

            // 履歴更新に使える入力かを判定
            bool canUpdate =
                absY >= LAST_FORWARD_SIGN_UPDATE_DEADZONE;

            // 逆符号入力かどうかを判定
            bool isReverseInput =
                _lastForwardSign != 0f &&
                currentSign != 0f &&
                currentSign != _lastForwardSign;

            // 逆符号入力時の処理
            if (isReverseInput)
            {
                // 十分な入力がある場合は即座に反転
                if (canUpdate)
                {
                    _lastForwardSign = currentSign;
                }
                // 微小入力の場合は一旦リセット
                else
                {
                    _lastForwardSign = 0f;
                }

                return;
            }

            // 同符号または未設定時の通常更新
            if (_lastForwardSign == 0f && canUpdate)
            {
                _lastForwardSign = currentSign;
            }
        }
    }
}