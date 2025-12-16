// ==============================================================================
// ITrackInputMode.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-16
// 概要     : キャタピラ入力モードの責務を定義するインターフェース
// ==============================================================================

using UnityEngine;

namespace TankSystem.Interface
{
    /// <summary>
    /// キャタピラ入力モードの計算責務を定義するインターフェース
    /// </summary>
    internal interface ITrackInputMode
    {
        // ======================================================
        // ITrackInputMode イベント
        // ======================================================

        /// <summary>
        /// 入力値から前進量および旋回量を算出する
        /// </summary>
        /// <param name="leftInput">左スティックの入力値</param>
        /// <param name="rightInput">右スティックの入力値</param>
        /// <param name="forwardAmount">算出された前進／後退量</param>
        /// <param name="turnAmount">算出された旋回量</param>
        void Calculate(
            in Vector2 leftInput,
            in Vector2 rightInput,
            out float forwardAmount,
            out float turnAmount
        );
    }
}