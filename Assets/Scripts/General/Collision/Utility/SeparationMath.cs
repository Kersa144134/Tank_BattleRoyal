// ======================================================
// SeparationMath.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-13
// 更新日時 : 2026-01-30
// 概要     : 分離判定（重なり有無）の算出ロジックを提供する数学ユーティリティ
// ======================================================

using UnityEngine;
using CollisionSystem.Interface;

namespace CollisionSystem.Utility
{
    /// <summary>
    /// 分離軸上での重なり判定を提供する数学ユーティリティ
    /// </summary>
    public sealed class SeparationMath
    {
        // ======================================================
        // 依存コンポーネント
        // ======================================================

        /// <summary>重なり量などの純粋数値計算を担当する数学ユーティリティ</summary>
        private readonly OverlapMath _overlapMath;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 必要な数学ユーティリティを注入して初期化する
        /// </summary>
        public SeparationMath(in OverlapMath overlapMath)
        {
            _overlapMath = overlapMath;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定軸上で 2 つの OBB が重なっているか判定する
        /// </summary>
        public bool IsOverlappingOnAxis(
            in IOBBData a,
            in IOBBData b,
            in Vector3 axis
        )
        {
            // 指定軸上の重なり量を算出
            float overlap = _overlapMath.CalculateOBBOverlapOnAxis(
                a,
                b,
                axis
            );

            // 正の値なら重なっている
            return overlap > 0f;
        }
    }
}