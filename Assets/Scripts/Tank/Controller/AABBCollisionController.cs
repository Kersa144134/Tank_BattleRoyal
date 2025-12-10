// ======================================================
// AABBCollisionController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-10
// 更新日時 : 2025-12-10
// 概要     : 距離計算による衝突判定のみを担当するコントローラクラス
// ======================================================

using UnityEngine;
using TankSystem.Data;

namespace TankSystem.Controller
{
    /// <summary>
    /// AABBでの衝突判定を行うクラス
    /// </summary>
    public class AABBCollisionController
    {
        // ======================================================
        // IsColliding
        // ======================================================

        /// <summary>
        /// 2 つの AABB が衝突しているかを判定する
        /// </summary>
        /// <param name="a">比較対象 A の AABB 情報</param>
        /// <param name="b">比較対象 B の AABB 情報</param>
        /// <returns>衝突していれば true、していなければ false を返す</returns>
        public bool IsColliding(in AABBData a, in AABBData b)
        {
            // X 方向
            bool hitX = Mathf.Abs(a.Center.x - b.Center.x) <= (a.HalfSize.x + b.HalfSize.x);

            // Y 方向
            bool hitY = Mathf.Abs(a.Center.y - b.Center.y) <= (a.HalfSize.y + b.HalfSize.y);

            // Z 方向
            bool hitZ = Mathf.Abs(a.Center.z - b.Center.z) <= (a.HalfSize.z + b.HalfSize.z);

            // 全軸で重なりが発生していれば AABB が交差していると判定する
            return hitX && hitY && hitZ;
        }
    }
}