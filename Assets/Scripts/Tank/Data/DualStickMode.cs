// ==============================================================================
// DualStickMode.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-16
// 概要     : 左右スティックをキャタピラとして扱う入力モード
// ==============================================================================

using UnityEngine;
using TankSystem.Controller;
using TankSystem.Interface;

namespace TankSystem.Data
{
    /// <summary>
    /// 左右キャタピラ独立操作用入力モード
    /// </summary>
    internal sealed class DualStickMode : ITrackInputMode
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>計算ロジック参照</summary>
        private readonly TankTrackController calculator;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// デュアルスティック入力方式の計算ロジックを生成する
        /// </summary>
        /// <param name="calculator">移動量および旋回量計算を担当するコントローラ</param>
        internal DualStickMode(TankTrackController calculator)
        {
            this.calculator = calculator;
        }

        // ======================================================
        // ITrackInputMode イベント
        // ======================================================

        /// <summary>
        /// 左右スティックのY軸入力をキャタピラ入力として解釈し、移動量と旋回量を算出する
        /// </summary>
        /// <param name="leftInput">左スティックの入力値</param>
        /// <param name="rightInput">右スティックの入力値</param>
        /// <param name="forwardAmount">算出された前進／後退量</param>
        /// <param name="turnAmount">算出された旋回量</param>
        public void Calculate(
            in Vector2 leftInput,
            in Vector2 rightInput,
            out float forwardAmount,
            out float turnAmount
        )
        {
            // 左キャタピラ入力を補正
            float left = calculator.RoundValue(leftInput.y);

            // 右キャタピラ入力を補正
            float right = calculator.RoundValue(rightInput.y);

            // 前進量を算出
            forwardAmount = calculator.CalculateForwardAmount(left, right);

            // 旋回量を算出
            turnAmount = calculator.CalculateTurnAmount(left, right);
        }
    }
}