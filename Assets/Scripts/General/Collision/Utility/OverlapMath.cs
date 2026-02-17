// ======================================================
// OverlapMath.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-13
// 更新日時 : 2026-02-18
// 概要     : 投影・距離ベースによる重なり計算を提供する数学ユーティリティ
// ======================================================

using UnityEngine;
using CollisionSystem.Data;

namespace CollisionSystem.Utility
{
    /// <summary>
    /// 汎用的な重なり量計算ユーティリティ
    /// </summary>
    public sealed class OverlapMath
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定軸上での OBB 同士の重なり量を算出する
        /// </summary>
        /// <param name="a">OBB A</param>
        /// <param name="b">OBB B</param>
        /// <param name="axis">正規化済み判定軸</param>
        /// <returns>正の値なら重なり量、負の値なら分離量</returns>
        public float CalculateOBBOverlapOnAxis(
            in BaseOBBData a,
            in BaseOBBData b,
            in Vector3 axis
        )
        {
            // --------------------------------------------------
            // OBB A の投影半径をキャッシュから算出
            // --------------------------------------------------
            float projectionA =
                Mathf.Abs(Vector3.Dot(a.ScaledRight, axis));

            projectionA +=
                Mathf.Abs(Vector3.Dot(a.ScaledUp, axis));

            projectionA +=
                Mathf.Abs(Vector3.Dot(a.ScaledForward, axis));

            // --------------------------------------------------
            // OBB B の投影半径をキャッシュから算出
            // --------------------------------------------------
            float projectionB =
                Mathf.Abs(Vector3.Dot(b.ScaledRight, axis));

            projectionB +=
                Mathf.Abs(Vector3.Dot(b.ScaledUp, axis));

            projectionB +=
                Mathf.Abs(Vector3.Dot(b.ScaledForward, axis));

            // --------------------------------------------------
            // 中心間距離を軸方向に射影する
            // --------------------------------------------------
            // OBB 中心差分を算出
            Vector3 centerDelta = b.Center - a.Center;

            // 差分を指定軸へ射影し絶対値を取得
            float projectedDistance =
                Mathf.Abs(Vector3.Dot(centerDelta, axis));

            // --------------------------------------------------
            // 重なり量を算出
            // --------------------------------------------------
            // 投影半径の合計から中心距離を減算する
            return projectionA + projectionB - projectedDistance;
        }

        /// <summary>
        /// 円と OBB の水平重なり量を算出する
        /// </summary>
        public float CalculateCircleOBBHorizontalOverlap(
            in Vector3 circleCenter,
            in float circleRadius,
            in BaseOBBData obb
        )
        {
            // --------------------------------------------------
            // 水平判定用に Y 座標を揃える
            // --------------------------------------------------
            // OBB 中心を取得する
            Vector3 obbCenter = obb.Center;

            // 円中心の Y に揃える
            obbCenter.y = circleCenter.y;

            // --------------------------------------------------
            // 円中心から OBB 中心への差分を取得
            // --------------------------------------------------
            Vector3 delta = circleCenter - obbCenter;

            // 水平判定のため Y を無効化
            delta.y = 0f;

            // --------------------------------------------------
            // OBB ローカル座標へ射影する
            // --------------------------------------------------
            float localX =
                Vector3.Dot(delta, obb.AxisRight);

            float localZ =
                Vector3.Dot(delta, obb.AxisForward);

            // --------------------------------------------------
            // 最近傍点を算出
            // --------------------------------------------------
            float closestX =
                Mathf.Clamp(localX, -obb.HalfSize.x, obb.HalfSize.x);

            float closestZ =
                Mathf.Clamp(localZ, -obb.HalfSize.z, obb.HalfSize.z);

            // ワールド最近傍点を復元
            Vector3 closestPoint =
                obbCenter +
                obb.AxisRight * closestX +
                obb.AxisForward * closestZ;

            // --------------------------------------------------
            // 距離から重なり量を算出
            // --------------------------------------------------
            Vector3 diff = circleCenter - closestPoint;

            diff.y = 0f;

            float distance = diff.magnitude;

            return circleRadius - distance;
        }
    }
}