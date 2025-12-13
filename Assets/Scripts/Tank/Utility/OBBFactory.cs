// ======================================================
// OBBFactory.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-11
// 更新日時 : 2025-12-13
// 概要     : ローカルヒットボックス定義に基づき、
//            Transform の回転を反映した OBB を生成する
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
        /// Transform とローカルヒットボックス定義から OBB を生成する
        /// </summary>
        /// <param name="targetTransform">対象 Transform</param>
        /// <param name="localCenter">ローカル空間でのヒットボックス座標</param>
        /// <param name="localSize">ローカル空間でのヒットボックスサイズ</param>
        public OBBData CreateOBB(
            in Transform targetTransform,
            in Vector3 localCenter,
            in Vector3 localSize
        )
        {
            // ローカル中心をワールド空間へ変換
            Vector3 worldCenter = targetTransform.TransformPoint(localCenter);

            // ローカルサイズに Transform のスケールを反映
            Vector3 scaledSize = Vector3.Scale(localSize, targetTransform.lossyScale);

            // 半サイズに変換
            Vector3 halfSize = scaledSize * 0.5f;

            // 回転は Transform のワールド回転を使用
            Quaternion rotation = targetTransform.rotation;

            return new OBBData(
                worldCenter,
                halfSize,
                rotation
            );
        }
    }
}