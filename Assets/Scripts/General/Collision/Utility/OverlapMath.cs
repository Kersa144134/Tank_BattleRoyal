// ======================================================
// OverlapMath.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-13
// 更新日時 : 2026-02-12
// 概要     : 投影・距離ベースによる重なり計算を提供する数学ユーティリティ
// ======================================================

using UnityEngine;
using CollisionSystem.Interface;

namespace CollisionSystem.Utility
{
    /// <summary>
    /// 汎用的な重なり量計算ユーティリティ
    /// </summary>
    public sealed class OverlapMath
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>OBB 射影半径計算用ユーティリティ</summary>
        private readonly OBBMath _obbMath;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 必要な数学ユーティリティを注入して初期化
        /// </summary>
        /// <param name="obbMath">OBB 射影半径算出ユーティリティ</param>
        public OverlapMath(in OBBMath obbMath)
        {
            _obbMath = obbMath;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定軸上での OBB 同士の重なり量を算出
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
            // OBB A の指定軸方向投影半径を計算
            float projectionA = _obbMath.CalculateProjectionRadius(a, axis);

            // OBB B の指定軸方向投影半径を計算
            float projectionB = _obbMath.CalculateProjectionRadius(b, axis);

            // OBB 中心差分を軸方向に射影して距離算出
            float projectedDistance = Mathf.Abs(Vector3.Dot(b.Center - a.Center, axis));

            // 投影半径の合計から中心間距離を引いた値が重なり量になる
            return projectionA + projectionB - projectedDistance;
        }

        /// <summary>
        /// 円と OBB の水平重なり量を算出
        /// </summary>
        /// <param name="circleCenter">円中心ワールド座標</param>
        /// <param name="circleRadius">円半径</param>
        /// <param name="obb">対象 OBB</param>
        /// <returns>正の値なら重なり量、負の値なら分離量</returns>
        public float CalculateCircleOBBHorizontalOverlap(
            in Vector3 circleCenter,
            in float circleRadius,
            in IOBBData obb
        )
        {
            // OBB のローカル軸をワールド空間で取得
            _obbMath.GetAxes(obb, out Vector3 rightAxis, out _, out Vector3 forwardAxis);

            // 水平判定用に OBB 中心の Y 座標を円中心に揃える
            Vector3 obbCenter = obb.Center;
            obbCenter.y = circleCenter.y;

            // 円中心から OBB 中心への差分ベクトル
            Vector3 delta = circleCenter - obbCenter;

            // 水平判定のため Y を 0 に固定
            delta.y = 0f;

            // OBB ローカル座標系での円中心位置
            float localX = Vector3.Dot(delta, rightAxis);
            float localZ = Vector3.Dot(delta, forwardAxis);

            // OBB の半サイズ
            Vector3 half = obb.HalfSize;

            // OBB 内での最近点をローカル座標で算出
            float closestX = Mathf.Clamp(localX, -half.x, half.x);
            float closestZ = Mathf.Clamp(localZ, -half.z, half.z);

            // ワールド座標に変換
            Vector3 closestPoint = obbCenter + rightAxis * closestX + forwardAxis * closestZ;

            // 円中心から最近点までの水平距離
            Vector3 diff = circleCenter - closestPoint;
            diff.y = 0f;

            // 半径から距離を引いた値が重なり量となる
            float distance = diff.magnitude;
            return circleRadius - distance;
        }
    }
}