// ==============================================================================
// InputCorrectionUtility.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-16
// 更新日時 : 2025-12-16
// 概要     : キャタピラ入力値の丸め・段階補正処理を提供するユーティリティクラス
// ==============================================================================

using UnityEngine;

namespace TankSystem.Utility
{
    /// <summary>
    /// キャタピラ入力の丸めおよび段階補正を行う静的ユーティリティクラス
    /// </summary>
    public class InputCorrectionUtility
    {
        // ======================================================================
        // 定数
        // ======================================================================

        /// <summary>小数第1位丸め用倍率</summary>
        private const float ROUND_DIGIT_MULTIPLIER = 10f;

        /// <summary>小数第1位丸め用逆数</summary>
        private const float ROUND_DIGIT_RECIPROCAL = 0.1f;

        /// <summary>補正段階レベル0（無効化）上限</summary>
        private const float CORRECTION_LEVEL_00_MAX = 0.2f;

        /// <summary>補正段階レベル01 判定用上限</summary>
        private const float CORRECTION_LEVEL_01_MAX = 0.3f;

        /// <summary>補正段階レベル03 判定用上限</summary>
        private const float CORRECTION_LEVEL_03_MAX = 0.4f;

        /// <summary>補正段階レベル05 判定用上限</summary>
        private const float CORRECTION_LEVEL_05_MAX = 0.5f;

        /// <summary>補正段階レベル07 判定用上限</summary>
        private const float CORRECTION_LEVEL_07_MAX = 0.6f;

        /// <summary>補正段階レベル09 判定用上限</summary>
        private const float CORRECTION_LEVEL_09_MAX = 0.7f;

        /// <summary>補正段階レベル01 値</summary>
        private const float CORRECTION_LEVEL_01_VALUE = 0.1f;

        /// <summary>補正段階レベル03 値</summary>
        private const float CORRECTION_LEVEL_03_VALUE = 0.3f;

        /// <summary>補正段階レベル05 値</summary>
        private const float CORRECTION_LEVEL_05_VALUE = 0.5f;

        /// <summary>補正段階レベル07 値</summary>
        private const float CORRECTION_LEVEL_07_VALUE = 0.7f;

        /// <summary>補正段階レベル09 値</summary>
        private const float CORRECTION_LEVEL_09_VALUE = 0.9f;

        /// <summary>補正段階レベル10（最大）値</summary>
        private const float CORRECTION_LEVEL_10_VALUE = 1.0f;

        // ======================================================================
        // パブリックメソッド
        // ======================================================================

        /// <summary>
        /// 入力値を小数第1位に丸めた後、段階補正テーブルに基づいて補正値へ変換する
        /// </summary>
        /// <param name="value">補正対象となる入力値</param>
        /// <returns>段階補正後の入力値</returns>
        public float ConvertRoundedInputToCorrectedValue(in float value)
        {
            // 小数第1位に丸めた値を算出
            float roundedValue = Mathf.Round(value * ROUND_DIGIT_MULTIPLIER) * ROUND_DIGIT_RECIPROCAL;

            // 入力値の絶対値を取得
            float absValue = Mathf.Abs(roundedValue);

            if (absValue <= CORRECTION_LEVEL_00_MAX)
            {
                return 0f;
            }
            if (absValue <= CORRECTION_LEVEL_01_MAX)
            {
                return Mathf.Sign(roundedValue) * CORRECTION_LEVEL_01_VALUE;
            }
            if (absValue <= CORRECTION_LEVEL_03_MAX)
            {
                return Mathf.Sign(roundedValue) * CORRECTION_LEVEL_03_VALUE;
            }
            if (absValue <= CORRECTION_LEVEL_05_MAX)
            {
                return Mathf.Sign(roundedValue) * CORRECTION_LEVEL_05_VALUE;
            }
            if (absValue <= CORRECTION_LEVEL_07_MAX)
            {
                return Mathf.Sign(roundedValue) * CORRECTION_LEVEL_07_VALUE;
            }
            if (absValue <= CORRECTION_LEVEL_09_MAX)
            {
                return Mathf.Sign(roundedValue) * CORRECTION_LEVEL_09_VALUE;
            }

            return Mathf.Sign(roundedValue) * CORRECTION_LEVEL_10_VALUE;
        }
    }
}