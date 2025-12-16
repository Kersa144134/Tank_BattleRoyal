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
    public class OBBFactory
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 静的 OBBData を生成する
        /// </summary>
        /// <param name="targetTransform">対象 Transform</param>
        /// <param name="localCenter">ローカル空間での中心位置</param>
        /// <param name="localSize">ローカル空間でのサイズ</param>
        /// <returns>静的 OBBData</returns>
        public StaticOBBData CreateStaticOBB(
            in Transform targetTransform,
            in Vector3 localCenter,
            in Vector3 localSize
        )
        {
            // ローカル中心をワールド空間に変換
            Vector3 worldCenter = targetTransform.TransformPoint(localCenter);

            // Transform のスケールを反映
            Vector3 scaledSize = Vector3.Scale(localSize, targetTransform.lossyScale);

            // 半サイズに変換
            Vector3 halfSize = scaledSize * 0.5f;

            // 回転は Transform のワールド回転を使用
            Quaternion rotation = targetTransform.rotation;

            // StructOBBData を返す
            return new StaticOBBData(worldCenter, halfSize, rotation);
        }

        /// <summary>
        /// 動的 OBBData（DynamicOBBData）を生成する
        /// </summary>
        /// <param name="targetTransform">追従対象 Transform</param>
        /// <param name="localCenter">ローカル空間での中心位置</param>
        /// <param name="localSize">ローカル空間でのサイズ</param>
        /// <returns>動的 OBBData</returns>
        public DynamicOBBData CreateDynamicOBB(
            in Transform targetTransform,
            in Vector3 localCenter,
            in Vector3 localSize
        )
        {
            // 半サイズに変換
            Vector3 halfSize = localSize * 0.5f;

            // DynamicOBBData を生成して返す
            return new DynamicOBBData(targetTransform, localCenter, halfSize);
        }
    }
}