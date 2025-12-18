// ======================================================
// OBBFactory.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-11
// 更新日時 : 2025-12-16
// 概要     : ローカルヒットボックス定義に基づき、
//            静的 OBB と動的 OBB を生成するファクトリー
// ======================================================

using UnityEngine;
using CollisionSystem.Data;

namespace TankSystem.Utility
{
    /// <summary>
    /// ローカルヒットボックス定義から OBB を生成するファクトリー
    /// </summary>
    public sealed class OBBFactory
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 静的 OBBData を生成する
        /// </summary>
        /// <param name="targetPosition">対象のワールド座標</param>
        /// <param name="targetPosition">対象のワールド回転</param>
        /// <param name="localCenter">ローカル空間での中心位置</param>
        /// <param name="localSize">ローカル空間でのサイズ</param>
        /// <returns>静的 OBBData</returns>
        public StaticOBBData CreateStaticOBB(
            in Vector3 targetPosition,
            in Quaternion targetRotation,
            in Vector3 localCenter,
            in Vector3 localSize
        )
        {
            // ローカル中心をワールド座標に変換
            Vector3 worldCenter = targetPosition + targetRotation * localCenter;

            // 半サイズに変換
            Vector3 halfSize = localSize * 0.5f;

            // StaticOBBData を返却
            return new StaticOBBData(worldCenter, halfSize, targetRotation);
        }

        /// <summary>
        /// 動的 OBBData を生成する
        /// </summary>
        /// <param name="localCenter">ローカル空間での中心位置</param>
        /// <param name="localSize">ローカル空間でのサイズ</param>
        /// <returns>動的 OBBData</returns>
        public DynamicOBBData CreateDynamicOBB(
            in Vector3 localCenter,
            in Vector3 localSize
        )
        {
            // 半サイズに変換
            Vector3 halfSize = localSize * 0.5f;

            // DynamicOBBData を生成して返す
            return new DynamicOBBData(localCenter, halfSize);
        }
    }
}