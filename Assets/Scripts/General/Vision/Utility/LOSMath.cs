// ======================================================
// LOSMath.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-22
// 更新日時 : 2025-12-22
// 概要     : 線分と OBB の交差判定を担当するユーティリティクラス
// ======================================================

using UnityEngine;
using CollisionSystem.Interface;

namespace VisionSystem.Utility
{
    /// <summary>
    /// Line of Sight 計算用ユーティリティ
    /// 線分が OBB に交差するか判定
    /// </summary>
    public sealed class LOSMath
    {
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
            in IOBBData obb
        )
        {
            // --------------------------------------------------
            // OBB のローカル座標系に線分を変換
            // --------------------------------------------------
            // OBB 逆回転
            Quaternion invRot = Quaternion.Inverse(obb.Rotation);

            // 線分開始を OBB ローカルに
            Vector3 localStart = invRot * (start - obb.Center);

            // 線分終了を OBB ローカルに
            Vector3 localEnd = invRot * (end - obb.Center);

            // 線分ベクトル
            Vector3 lineDir = localEnd - localStart;

            // 線分開始パラメータ
            float tMin = 0f;

            // 線分終了パラメータ
            float tMax = 1f;

            // --------------------------------------------------
            // slab 法による各軸交差判定
            // --------------------------------------------------
            for (int i = 0; i < 3; i++)
            {
                // 線分が軸に平行か判定
                if (Mathf.Abs(lineDir[i]) < Mathf.Epsilon)
                {
                    // 平行で軸外にある場合は交差なし
                    if (localStart[i] < -obb.HalfSize[i] || localStart[i] > obb.HalfSize[i])
                    {
                        return false;
                    }
                }
                else
                {
                    // 線分の t パラメータ範囲を計算
                    float ood = 1.0f / lineDir[i];
                    float t1 = (-obb.HalfSize[i] - localStart[i]) * ood;
                    float t2 = (obb.HalfSize[i] - localStart[i]) * ood;

                    // t1 < t2 に補正
                    if (t1 > t2) { float tmp = t1; t1 = t2; t2 = tmp; }

                    // 線分の交差範囲を更新
                    tMin = Mathf.Max(tMin, t1);
                    tMax = Mathf.Min(tMax, t2);

                    // tMin > tMax なら交差なしと判定
                    if (tMin > tMax)
                    {
                        return false;
                    }
                }
            }

            // 全軸で交差範囲が存在すれば線分は OBB に交差したと判定
            return true;
        }
    }
}