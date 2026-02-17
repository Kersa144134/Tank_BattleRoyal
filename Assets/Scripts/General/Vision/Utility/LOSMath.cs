// ======================================================
// LOSMath.cs
// 作成者 : 高橋一翔
// 作成日時 : 2025-12-22
// 更新日時 : 2026-02-18
// 概要 : 線分と OBB の交差判定を担当するユーティリティクラス
// ======================================================

using UnityEngine;
using CollisionSystem.Data;

namespace VisionSystem.Utility
{
    /// <summary>
    /// 視線（Line of Sight）計算用ユーティリティ
    /// 線分が OBB に交差するか判定
    /// キャッシュ済み軸・半サイズを使用して高速化
    /// </summary>
    public sealed class LOSMath
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>OBB の軸数</summary>
        private const int OBB_AXIS_COUNT = 3;
        
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 線分と回転付き OBB が交差しているかを判定
        /// </summary>
        /// <param name="start">線分開始位置（ワールド座標）</param>
        /// <param name="end">線分終了位置（ワールド座標）</param>
        /// <param name="obb">判定対象 OBBData</param>
        /// <returns>線分が OBB に交差していれば true</returns>
        public bool IsLineIntersectOBB(
            in Vector3 start,
            in Vector3 end,
            in BaseOBBData obb
        )
        {
            float tMin = 0f;
            float tMax = 1f;

            // 線分ベクトル算出
            Vector3 lineDir = end - start;

            // --------------------------------------------------
            // 各軸交差判定
            // slab 法
            // --------------------------------------------------
            Vector3 axis0 = obb.AxisRight;
            Vector3 axis1 = obb.AxisUp;
            Vector3 axis2 = obb.AxisForward;

            float half0 = obb.HalfSize.x;
            float half1 = obb.HalfSize.y;
            float half2 = obb.HalfSize.z;

            // 各軸判定
            for (int i = 0; i < OBB_AXIS_COUNT; i++)
            {
                Vector3 axis;
                float halfSize;

                // 軸・半サイズを選択
                if (i == 0) { axis = axis0; halfSize = half0; }
                else if (i == 1) { axis = axis1; halfSize = half1; }
                else { axis = axis2; halfSize = half2; }

                // 線分の開始ベクトルを OBB 軸方向に射影
                float projStart = Vector3.Dot(start - obb.Center, axis);

                // 線分の終了ベクトルを OBB 軸方向に射影
                float projEnd = Vector3.Dot(end - obb.Center, axis);

                // 線分方向を算出
                float dir = projEnd - projStart;

                // 軸に平行か判定
                if (Mathf.Abs(dir) < Mathf.Epsilon)
                {
                    // 平行で軸外にある場合は交差なし
                    if (projStart < -halfSize || projStart > halfSize)
                    {
                        return false;
                    }
                }
                else
                {
                    // t パラメータ範囲を計算
                    float ood = 1.0f / dir;
                    float t1 = (-halfSize - projStart) * ood;
                    float t2 = (halfSize - projStart) * ood;

                    // t1 < t2 に補正
                    if (t1 > t2)
                    {
                        float tmp = t1;
                        t1 = t2;
                        t2 = tmp;
                    }

                    // 線分の交差範囲を更新
                    tMin = Mathf.Max(tMin, t1);
                    tMax = Mathf.Min(tMax, t2);

                    // tMin > tMax なら交差なし
                    if (tMin > tMax)
                    {
                        return false;
                    }
                }
            }

            // 全軸で交差範囲が存在すれば OBB に交差
            return true;
        }
    }
}