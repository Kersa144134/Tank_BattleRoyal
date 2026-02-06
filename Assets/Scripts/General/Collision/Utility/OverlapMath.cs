// ======================================================
// OverlapMath.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-13
// 更新日時 : 2026-01-31
// 概要     : 投影・距離・半径に基づく重なり量計算を提供する数学ユーティリティ
// ======================================================

using UnityEngine;
using CollisionSystem.Interface;

namespace CollisionSystem.Utility
{
    /// <summary>
    /// 汎用的な重なり量計算を提供する数学ユーティリティ
    /// </summary>
    public sealed class OverlapMath
    {
        // ======================================================
        // 依存コンポーネント
        // ======================================================

        /// <summary>OBB の射影半径計算に使用する数学ユーティリティ</summary>
        private readonly OBBMath _obbMath;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 必要な数学ユーティリティを注入して初期化する
        /// </summary>
        /// <param name="obbMath">OBB 射影半径算出に使用する数学ユーティリティ</param>
        public OverlapMath(in OBBMath obbMath)
        {
            // OBB 用重なり計算で使用するため保持する
            _obbMath = obbMath;
        }

        // ======================================================
        // 半径ベース重なり計算
        // ======================================================

        /// <summary>
        /// 半径と距離から重なり量を算出する
        /// </summary>
        /// <param name="radiusA">形状 A の半径</param>
        /// <param name="radiusB">形状 B の半径</param>
        /// <param name="distance">中心間距離</param>
        /// <returns>正の値なら重なり量、負の値なら分離量</returns>
        public float CalculateOverlapFromRadius(
            float radiusA,
            float radiusB,
            float distance
        )
        {
            // 半径合計から中心距離を引いて重なり量を算出する
            return radiusA + radiusB - distance;
        }

        /// <summary>
        /// 半径合計と距離から重なり有無を判定する
        /// </summary>
        /// <param name="radiusSum">半径合計</param>
        /// <param name="distance">中心間距離</param>
        /// <returns>重なっている場合 true</returns>
        public bool IsOverlappingFromRadius(
            float radiusSum,
            float distance
        )
        {
            // 距離が半径合計以下なら重なりと判定する
            return distance <= radiusSum;
        }

        // ======================================================
        // 投影ベース重なり計算
        // ======================================================

        /// <summary>
        /// 投影半径と投影距離から重なり量を算出する
        /// </summary>
        /// <param name="projectionA">形状 A の投影半径</param>
        /// <param name="projectionB">形状 B の投影半径</param>
        /// <param name="projectedDistance">軸方向投影距離</param>
        /// <returns>正の値なら重なり量</returns>
        public float CalculateProjectedOverlap(
            float projectionA,
            float projectionB,
            float projectedDistance
        )
        {
            // 投影半径合計から投影距離を引いて重なり量を算出する
            return projectionA + projectionB - projectedDistance;
        }

        /// <summary>
        /// 投影半径と投影距離から重なり有無を判定する
        /// </summary>
        /// <param name="projectionA">形状 A の投影半径</param>
        /// <param name="projectionB">形状 B の投影半径</param>
        /// <param name="projectedDistance">軸方向投影距離</param>
        /// <returns>重なっている場合 true</returns>
        public bool IsOverlappingOnAxis(
            float projectionA,
            float projectionB,
            float projectedDistance
        )
        {
            // 投影距離が投影半径合計以下なら重なりと判定する
            return projectedDistance <= projectionA + projectionB;
        }

        // ======================================================
        // OBB 専用重なり計算
        // ======================================================

        /// <summary>
        /// 指定軸上での OBB 同士の重なり量を算出する
        /// </summary>
        /// <param name="a">OBB A</param>
        /// <param name="b">OBB B</param>
        /// <param name="axis">正規化済み判定軸</param>
        /// <returns>正の値なら重なり量、負の値なら分離量</returns>
        public float CalculateOBBOverlapOnAxis(
            in IOBBData a,
            in IOBBData b,
            in Vector3 axis
        )
        {
            // OBB A の指定軸方向投影半径を算出する
            float projectionA = _obbMath.CalculateProjectionRadius(a, axis);

            // OBB B の指定軸方向投影半径を算出する
            float projectionB = _obbMath.CalculateProjectionRadius(b, axis);

            // OBB 中心差分を指定軸方向に射影した距離を算出する
            float projectedDistance =
                Mathf.Abs(Vector3.Dot(b.Center - a.Center, axis));

            // 投影半径合計から投影距離を引いて重なり量を算出する
            return projectionA + projectionB - projectedDistance;
        }
    }
}