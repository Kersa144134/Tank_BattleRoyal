// ==============================================================================
// TankTrackController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-04
// 更新日時 : 2025-12-16
// 概要     : キャタピラ入力モードに応じて、前進後退量と旋回量を算出するロジッククラス
//            ・左右キャタピラ操作モード
//            ・左スティック単独操作モード
// ==============================================================================

using UnityEngine;
using TankSystem.Manager;

namespace TankSystem.Controller
{
    /// <summary>
    /// キャタピラ入力方式に応じた移動・旋回量を計算するロジッククラス
    /// </summary>
    public class TankTrackController
    {
        // ======================================================================
        // 定数
        // ======================================================================

        /// <summary>基準前後移動速度定数</summary>
        private const float MOVE_SPEED = 1f;

        /// <summary>基準前進量の指数補正値</summary>
        private const float FORWARD_EXPONENT = 1.5f;

        /// <summary>基準旋回速度定数</summary>
        private const float TURN_SPEED = 7.5f;

        /// <summary>基準旋回量の指数補正値</summary>
        private const float TURN_EXPONENT = 1.5f;

        // ======================================================================
        // パブリックメソッド
        // ======================================================================

        /// <summary>
        /// 入力モードに応じて前進量と旋回量を計算して返す
        /// </summary>
        public void UpdateTrack(
            in BaseTankRootManager.TrackInputMode inputMode,
            in Vector2 leftInput,
            in Vector2 rightInput,
            out float forwardAmount,
            out float turnAmount
        )
        {
            // モードに応じて処理を分岐
            switch (inputMode)
            {
                // 左右キャタピラ操作モード
                case BaseTankRootManager.TrackInputMode.Dual:
                    UpdateDualTrack(
                        leftInput.y,
                        rightInput.y,
                        out forwardAmount,
                        out turnAmount
                    );
                    break;

                // 左スティック単独操作モード
                case BaseTankRootManager.TrackInputMode.Single:
                    UpdateLeftStickOnly(
                        leftInput.x,
                        leftInput.y,
                        out forwardAmount,
                        out turnAmount
                    );
                    break;

                // 想定外モード対策
                default:
                    forwardAmount = 0f;
                    turnAmount = 0f;
                    break;
            }
        }

        // ======================================================================
        // プライベートメソッド
        // ======================================================================

        // --------------------------------------------------
        // 左右キャタピラ操作モード
        // --------------------------------------------------

        /// <summary>
        /// 左右キャタピラ入力による従来方式の移動・旋回計算
        /// </summary>
        private void UpdateDualTrack(
            in float leftInput,
            in float rightInput,
            out float forwardAmount,
            out float turnAmount
        )
        {
            // 入力値を丸めて微小誤差を抑制
            float processedLeft = RoundValue(leftInput);
            float processedRight = RoundValue(rightInput);

            // 前進量を算出
            forwardAmount = CalculateForwardAmount(
                processedLeft,
                processedRight
            );

            // 旋回量を算出
            turnAmount = CalculateTurnAmount(
                processedLeft,
                processedRight
            );
        }

        // --------------------------------------------------
        // 左スティック単独操作モード
        // --------------------------------------------------

        /// <summary>
        /// 左スティック入力のみで移動・旋回を行う処理
        /// </summary>
        private void UpdateLeftStickOnly(
            in float stickX,
            in float stickY,
            out float forwardAmount,
            out float turnAmount
        )
        {
            // 入力値を丸めて微小誤差を抑制
            float processedX = RoundValue(stickX);
            float processedY = RoundValue(stickY);

            // 上下入力が存在する場合は前進・後退を優先
            if (!Mathf.Approximately(processedY, 0f))
            {
                // 前進量は上下入力のみで算出
                forwardAmount = CalculateForwardFromSingleAxis(processedY);

                // 左右入力が無い場合は旋回なし
                turnAmount = Mathf.Approximately(processedX, 0f)
                    ? 0f
                    : CalculateTurnFromSingleAxis(processedX);

                return;
            }

            // 上下入力が無い場合はその場旋回
            forwardAmount = 0f;
            turnAmount = CalculateTurnFromSingleAxis(processedX);
        }

        // --------------------------------------------------
        // 入力値補正
        // --------------------------------------------------

        /// <summary>
        /// 入力値を小数第1位に丸める
        /// </summary>
        private float RoundValue(in float value)
        {
            return Mathf.Round(value * 10f) * 0.1f;
        }

        // --------------------------------------------------
        // 前進 / 後退（共通）
        // --------------------------------------------------

        /// <summary>
        /// 左右入力平均から前進／後退量を算出
        /// </summary>
        private float CalculateForwardAmount(in float leftInput, in float rightInput)
        {
            // 平均値を算出
            float average = (leftInput + rightInput) * 0.5f;

            // 絶対値を取得
            float abs = Mathf.Abs(average);

            // 指数補正を適用
            float curved = Mathf.Pow(abs, FORWARD_EXPONENT);

            // 元の符号を反映
            float curvedSigned = curved * Mathf.Sign(average);

            // 移動速度へ変換
            return curvedSigned * MOVE_SPEED;
        }

        /// <summary>
        /// 単一軸入力から前進／後退量を算出
        /// </summary>
        private float CalculateForwardFromSingleAxis(in float input)
        {
            // 絶対値を取得
            float abs = Mathf.Abs(input);

            // 指数補正を適用
            float curved = Mathf.Pow(abs, FORWARD_EXPONENT);

            // 元の符号を反映
            float curvedSigned = curved * Mathf.Sign(input);

            // 移動速度へ変換
            return curvedSigned * MOVE_SPEED;
        }

        // --------------------------------------------------
        // 旋回（共通）
        // --------------------------------------------------

        /// <summary>
        /// 左右差分から旋回量を算出
        /// </summary>
        private float CalculateTurnAmount(in float leftInput, in float rightInput)
        {
            // 差分を取得
            float diff = leftInput - rightInput;

            // 絶対値を取得
            float abs = Mathf.Abs(diff);

            // 指数補正を適用
            float curved = Mathf.Pow(abs, 1f / TURN_EXPONENT);

            // 元の符号を反映
            float curvedSigned = curved * Mathf.Sign(diff);

            // 旋回速度へ変換
            return curvedSigned * TURN_SPEED;
        }

        /// <summary>
        /// 単一軸入力から旋回量を算出
        /// </summary>
        private float CalculateTurnFromSingleAxis(in float input)
        {
            // 絶対値を取得
            float abs = Mathf.Abs(input);

            // 指数補正を適用
            float curved = Mathf.Pow(abs, 1f / TURN_EXPONENT);

            // 元の符号を反映
            float curvedSigned = curved * Mathf.Sign(input);

            // 旋回速度へ変換
            return curvedSigned * TURN_SPEED;
        }
    }
}