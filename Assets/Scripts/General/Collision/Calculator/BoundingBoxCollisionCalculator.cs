// ======================================================
// BoundingBoxCollisionCalculator.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-11
// 更新日時 : 2026-01-31
// 概要     : OBB 衝突処理のユースケースクラス
// ======================================================

using System.Collections.Generic;
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

        /// <summary>円 vs OBB 判定計算器</summary>
        private readonly CircleOBBCollisionCalculator _circleOBBCollisionCalculator;

        /// <summary>OBB の射影計算を担当する数学ユーティリティ</summary>
        private readonly OBBMath _obbMath;

        /// <summary>汎用的な重なり量計算を担当する数学ユーティリティ</summary>
        private readonly OverlapMath _overlapMath;

        /// <summary>分離判定（重なり有無）を担当する数学ユーティリティ</summary>
        private readonly SeparationMath _separationMath;

        /// <summary>侵入量（押し戻し量）算出を担当する数学ユーティリティ</summary>
        private readonly PenetrationMath _penetrationMath;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>円と OBB の重なり判定で使用する結果キャッシュリスト</summary>
        private List<IOBBData> _overlapResults = new List<IOBBData>(DEFAULT_OVERLAP_LIST_CAPACITY);

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>円重なり判定用 OBB リストの初期容量</summary>
        private const int DEFAULT_OVERLAP_LIST_CAPACITY = 16;

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
            _circleOBBCollisionCalculator = new CircleOBBCollisionCalculator(_obbMath, _overlapMath);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// OBB 同士が水平面上で重なっているかを判定する
        /// </summary>
        public bool IsCollidingHorizontal(
            in IOBBData a,
            in IOBBData b
        )
        {
            return _obbCollisionCalculator.IsCollidingHorizontal(a, b);
        }

        /// <summary>
        /// OBB 同士が重なった場合の水平押し戻し軸と距離を算出する
        /// </summary>
        public bool TryGetPushOutAxisAndDistance(
            in IOBBData a,
            in IOBBData b,
            out Vector3 resolveAxis,
            out float resolveDistance
        )
        {
            return _obbCollisionCalculator.TryGetPushOutAxisAndDistance(
                a,
                b,
                out resolveAxis,
                out resolveDistance
            );
        }

        /// <summary>
        /// 指定円と水平面上で重なっている OBB をすべて取得する
        /// </summary>
        public IOBBData[] GetOverlappingOBBsCircleHorizontal(
            in Vector3 circleCenter,
            in float circleRadius,
            in IOBBData[] obbArray
        )
        {
            _overlapResults.Clear();

            // 重なっている OBB を収集
            _circleOBBCollisionCalculator.CollectOverlappingHorizontal(
                circleCenter,
                circleRadius,
                obbArray,
                ref _overlapResults
            );

            // 配列に変換して返却
            return _overlapResults.ToArray();
        }
    }
}