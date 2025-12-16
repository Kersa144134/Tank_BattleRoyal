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
                // 前進量を算出
                forwardAmount = calculator.CalculateForwardFromSingleAxis(y);

                // 左右入力がある場合のみ旋回
                turnAmount = Mathf.Approximately(x, 0f)
                    ? 0f
                    : calculator.CalculateTurnFromSingleAxis(x);

                return;
            }

            // 上下入力が無い場合はその場旋回
            forwardAmount = 0f;
            turnAmount = calculator.CalculateTurnFromSingleAxis(x);
        }
    }
}