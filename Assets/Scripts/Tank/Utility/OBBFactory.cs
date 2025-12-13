// ======================================================
// OBBFactory.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-11
// 更新日時 : 2025-12-13
// 概要     : ローカルヒットボックス定義に基づき、
//            Transform の回転を反映した OBB を生成する
// ======================================================

using UnityEngine;
using TankSystem.Data;

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
            // ローカル座標を Transform の回転・位置を考慮してワールド空間へ変換する
            Vector3 worldCenter = targetTransform.TransformPoint(localCenter);

            // ローカルサイズをそのまま半サイズとして使用する
            Vector3 halfSize = localSize * 0.5f;

            // Transform の回転をそのまま OBB の回転として使用する
            Quaternion rotation = targetTransform.rotation;

            return new OBBData(
                worldCenter,
                halfSize,
                rotation
            );
        }
    }
}