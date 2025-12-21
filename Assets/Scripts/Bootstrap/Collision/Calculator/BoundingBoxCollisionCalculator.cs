// ======================================================
// BoundingBoxCollisionCalculator.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-11
// 更新日時 : 2025-12-13
// 概要     : OBB 衝突計算を OBBCollisionCalculator に委譲する調停クラス
// ======================================================

using UnityEngine;
using CollisionSystem.Interface;
using CollisionSystem.Utility;

namespace CollisionSystem.Calculator
{
    /// <summary>
    /// OBB 衝突処理のユースケース窓口
    /// 数学的処理はすべて OBBCollisionCalculator に委譲する
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

        /// <summary>SAT による重なり判定を担当するユーティリティ</summary>
        private readonly SATMath _satMath;

        /// <summary>MTV（侵入量）算出を担当するユーティリティ</summary>
        private readonly MTVMath _mtvMath;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// OBB 衝突計算コンポーネントを注入して初期化する
        /// </summary>
        public BoundingBoxCollisionCalculator()
        {
            _obbMath = new OBBMath();
            _satMath = new SATMath(_obbMath);
            _mtvMath = new MTVMath(_obbMath);
            _obbCollisionCalculator = new OBBCollisionCalculator(_obbMath, _satMath, _mtvMath);
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