// ======================================================
// PenetrationMath.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-13
// 更新日時 : 2026-01-30
// 概要     : 侵入量（押し出し量）の算出ロジックを提供する数学ユーティリティ
// ======================================================

using UnityEngine;
using CollisionSystem.Interface;

namespace CollisionSystem.Utility
{
    /// <summary>
    /// 分離軸上の侵入量を提供する数学ユーティリティ
    /// </summary>
    public sealed class PenetrationMath
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
        public PenetrationMath(in OverlapMath overlapMath)
        {
            _overlapMath = overlapMath;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定軸上で侵入量を算出する
        /// </summary>
        public bool TryCalculatePenetration(
            in IOBBData a,
            in IOBBData b,
            in Vector3 axis,
            out float penetration
        )
        {
            // 指定軸上の重なり量を算出
            penetration = _overlapMath.CalculateOBBOverlapOnAxis(
                a,
                b,
                axis
            );

            // 正の値なら侵入している
            return penetration > 0f;
        }
    }
}