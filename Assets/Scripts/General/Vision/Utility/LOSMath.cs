// ======================================================
// LOSMath.cs
// 作成者 : 高橋一翔
// 作成日時 : 2025-12-22
// 更新日時 : 2026-03-10
// 概要 : 線分と OBB の交差判定ユーティリティ
//        XZ 平面のみで遮蔽物判定を行う
// ======================================================

using UnityEngine;
using CollisionSystem.Data;

namespace VisionSystem.Utility
{
    /// <summary>
    /// 視線（Line of Sight）計算ユーティリティ
    /// スラブ法を使用した線分と OBB の交差判定を実行する
    /// </summary>
    public sealed class LOSMath
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 線分と OBB の交差判定
        /// </summary>
        /// <param name="start">線分開始位置（ワールド座標）</param>
        /// <param name="end">線分終了位置（ワールド座標）</param>
        /// <param name="obb">判定対象 OBB データ</param>
        /// <returns>線分が OBB に交差している場合 true</returns>
        public bool IsLineIntersectOBB(
            in Vector3 start,
            in Vector3 end,
            in BaseOBBData obb)
        {
            // 線分パラメータ範囲
            float tMin = 0f;
            float tMax = 1f;

            // 線分方向ベクトル
            Vector3 lineDir = end - start;

            // OBB中心から線分開始点へのベクトル
            Vector3 startToCenter = start - obb.Center;

            // --------------------------------------------------
            // Axis X（OBB ローカル右方向）
            // --------------------------------------------------
            {
                // OBB のローカル X 軸
                Vector3 axis = obb.AxisRight;

                // OBB 半サイズ（X方向）
                float half = obb.HalfSize.x;

                // 線分開始点を OBB 軸へ射影
                float projStart = Vector3.Dot(startToCenter, axis);

                // 線分方向を OBB 軸へ射影
                float projDir = Vector3.Dot(lineDir, axis);

                // 線分がこの軸に平行か判定
                if (Mathf.Abs(projDir) < Mathf.Epsilon)
                {
                    // 平行かつ OBB 範囲外にある場合、交差なし
                    if (projStart < -half || projStart > half)
                    {
                        return false;
                    }
                }
                else
                {
                    // 線分とスラブ面の交差パラメータ
                    float ood = 1f / projDir;
                    float t1 = (-half - projStart) * ood;
                    float t2 = (half - projStart) * ood;

                    // 小さい値を t1 に補正
                    if (t1 > t2)
                    {
                        float temp = t1;
                        t1 = t2;
                        t2 = temp;
                    }

                    // 交差範囲更新
                    if (t1 > tMin)
                    {
                        tMin = t1;
                    }
                    if (t2 < tMax)
                    {
                        tMax = t2;
                    }

                    // 交差範囲が消失した場合、交差なし
                    if (tMin > tMax)
                    {
                        return false;
                    }
                }
            }

            // --------------------------------------------------
            // Axis Z（OBB ローカル前方向）
            // --------------------------------------------------
            {
                // OBB のローカル Z 軸
                Vector3 axis = obb.AxisForward;

                // OBB 半サイズ（Z方向）
                float half = obb.HalfSize.z;

                // 線分開始点を OBB 軸へ射影
                float projStart = Vector3.Dot(startToCenter, axis);

                // 線分方向を OBB 軸へ射影
                float projDir = Vector3.Dot(lineDir, axis);

                // 線分がこの軸に平行か判定
                if (Mathf.Abs(projDir) < Mathf.Epsilon)
                {
                    // 平行かつ OBB 範囲外にある場合、交差なし
                    if (projStart < -half || projStart > half)
                    {
                        return false;
                    }
                }
                else
                {
                    // 線分とスラブ面の交差パラメータ
                    float ood = 1f / projDir;
                    float t1 = (-half - projStart) * ood;
                    float t2 = (half - projStart) * ood;

                    // 小さい値を t1 に補正
                    if (t1 > t2)
                    {
                        float temp = t1;
                        t1 = t2;
                        t2 = temp;
                    }

                    // 交差範囲更新
                    if (t1 > tMin)
                    {
                        tMin = t1;
                    }
                    if (t2 < tMax)
                    {
                        tMax = t2;
                    }

                    // 交差範囲が消失した場合、交差なし
                    if (tMin > tMax)
                    {
                        return false;
                    }
                }
            }

            // XZ 平面で交差範囲が存在
            return true;
        }
    }
}