// ======================================================
// BoundingBoxCollisionCalculator.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-11
// 更新日時 : 2025-12-13
// 概要     : OBB 衝突処理のユースケースクラス
// ======================================================

using UnityEngine;
using CollisionSystem.Interface;
using CollisionSystem.Utility;

namespace CollisionSystem.Calculator
{
    /// <summary>
    /// OBB 衝突処理のユースケースクラス
    /// </summary>
    public sealed class BoundingBoxCollisionCalculator
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>OBB 衝突計算器</summary>
        private readonly OBBCollisionCalculator _obbCollisionCalculator;

        /// <summary>OBB の射影計算を担当する数学ユーティリティ</summary>
        private readonly OBBMath _obbMath;

        /// <summary>汎用的な重なり量計算を担当する数学ユーティリティ</summary>
        private readonly OverlapMath _overlapMath;

        /// <summary>分離判定（重なり有無）を担当する数学ユーティリティ</summary>
        private readonly SeparationMath _separationMath;

        /// <summary>侵入量（押し戻し量）算出を担当する数学ユーティリティ</summary>
        private readonly PenetrationMath _penetrationMath;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// OBB 衝突計算コンポーネントを注入して初期化する
        /// </summary>
        public BoundingBoxCollisionCalculator()
        {
            _obbMath = new OBBMath();
            _overlapMath = new OverlapMath(_obbMath);
            _separationMath = new SeparationMath(_overlapMath);
            _penetrationMath = new PenetrationMath(_overlapMath);
            _obbCollisionCalculator = new OBBCollisionCalculator(_obbMath, _separationMath, _penetrationMath);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// Y 回転のみを考慮した OBB 衝突判定
        /// </summary>
        public bool IsCollidingHorizontal(
            in IOBBData a,
            in IOBBData b
        )
        {
            // OBB 衝突判定を委譲
            return _obbCollisionCalculator.IsCollidingHorizontal(a, b);
        }

        /// <summary>
        /// Y 回転のみを考慮した MTV を算出する
        /// </summary>
        public bool TryCalculateHorizontalMTV(
            in IOBBData a,
            in IOBBData b,
            out Vector3 resolveAxis,
            out float resolveDistance
        )
        {
            // MTV 算出処理を委譲
            return _obbCollisionCalculator.TryCalculateHorizontalMTV(
                a,
                b,
                out resolveAxis,
                out resolveDistance
            );
        }
    }
}