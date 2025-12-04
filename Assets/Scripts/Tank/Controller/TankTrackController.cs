// ==============================================================================
// TankTrackController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-04
// 更新日時 : 2025-12-04
// 概要     : 戦車の左右キャタピラ入力を元に、前進後退量と旋回量を算出するロジッククラス
// ==============================================================================

using UnityEngine;

namespace TankSystem.Controller
{
    /// <summary>
    /// 左右キャタピラ入力から前後移動量と旋回量を計算して返す純粋ロジッククラス
    /// Transform を直接操作せず、TankManager に制御を委ねる
    /// </summary>
    public class TankTrackController
    {
        // ==============================================================================
        // 定数
        // ==============================================================================

        /// <summary>基準前後移動速度定数</summary>
        private const float MOVE_SPEED = 1f;

        /// <summary>基準前進量の指数補正値</summary>
        private const float FORWARD_EXPONENT = 1.5f;

        /// <summary>基準旋回速度定数</summary>
        private const float TURN_SPEED = 10f;

        /// <summary>基準旋回量の指数補正値</summary>
        private const float TURN_EXPONENT = 1.5f;

        // ==============================================================================
        // パブリックメソッド
        // ==============================================================================

        /// <summary>
        /// 入力値から前進/後退量と旋回量を計算して返す
        /// </summary>
        public void UpdateTrack(
            float leftInput,
            float rightInput,
            out float forwardAmount,
            out float turnAmount
        )
        {
            // 入力を四捨五入しスナップする
            float processedLeft = RoundValue(leftInput);
            float processedRight = RoundValue(rightInput);

            // 計算
            forwardAmount = CalculateForwardAmount(processedLeft, processedRight);
            turnAmount = CalculateTurnAmount(processedLeft, processedRight);

            Debug.Log($"L:{processedLeft}, R:{processedRight} | Forward: {forwardAmount:F3}, Turn: {turnAmount:F3}");
        }

        // ==============================================================================
        // プライベートメソッド
        // ==============================================================================

        /// <summary>
        /// 入力値を小数第1位に四捨五入する処理を行う
        /// </summary>
        private float RoundValue(float value)
        {
            // 小数第1位に四捨五入
            return Mathf.Round(value * 10f) * 0.1f;
        }
        
        // --------------------------------------------------
        // 前進 / 後退
        // --------------------------------------------------
        /// <summary>
        /// 左右スティック入力の平均値から前進／後退の移動量を算出する
        /// </summary>
        private float CalculateForwardAmount(float leftInput, float rightInput)
        {
            // 左右スティックの平均値を算出
            float average = (leftInput + rightInput) * 0.5f;

            // 入力がほぼ 0 の場合は移動なし
            if (Mathf.Approximately(average, 0f))
            {
                return 0f;
            }

            // 絶対値を取得
            float abs = Mathf.Abs(average);

            // 指数補正
            float curved = Mathf.Pow(abs, FORWARD_EXPONENT);

            // 元の符号を反映
            float curvedSigned = curved * Mathf.Sign(average);

            // 最終的な速度へ変換
            float amount = curvedSigned * MOVE_SPEED;

            return amount;
        }

        // --------------------------------------------------
        // 旋回
        // --------------------------------------------------
        /// <summary>
        /// 左右の差から旋回量を計算
        /// </summary>
        private float CalculateTurnAmount(float leftInput, float rightInput)
        {
            // 差分を取得
            float diff = leftInput - rightInput;

            // 入力がほぼゼロなら旋回なし
            if (Mathf.Approximately(diff, 0f))
            {
                return 0f;
            }

            // 差分の絶対値を取得
            float absDiff = Mathf.Abs(diff);

            // 指数補正
            float curved = Mathf.Pow(absDiff, 1f / TURN_EXPONENT);

            // 符号を付け直す
            float curvedSigned = curved * Mathf.Sign(diff);

            // 最終旋回量へ変換
            float amount = curvedSigned * TURN_SPEED;

            return amount;
        }
    }
}