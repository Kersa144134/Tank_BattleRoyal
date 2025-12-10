// ======================================================
// AABBFactory.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-10
// 更新日時 : 2025-12-10
// 概要     : Transform の回転を考慮した AABB を生成するユーティリティクラス
// ======================================================

using UnityEngine;
using TankSystem.Data;

namespace TankSystem.Utility
{
    /// <summary>
    /// OBB から AABBを計算して生成するためのファクトリークラス
    /// </summary>
    public class AABBFactory
    {
        /// <summary>
        /// Transform とローカルサイズ情報から回転込みの AABB を計算して返す
        /// </summary>
        /// <param name="targetTransform">対象の Transform</param>
        /// <param name="localCenter">ローカル座標での中心点</param>
        /// <param name="localSize">ローカル座標でのサイズ</param>
        public AABBData CreateAABB(Transform targetTransform, Vector3 localCenter, Vector3 localSize)
        {
            // AABB の中心をワールド座標に変換
            Vector3 worldCenter = targetTransform.TransformPoint(localCenter);

            // ローカル半サイズを計算
            Vector3 half = localSize * 0.5f;

            // 回転情報を取得
            Quaternion rotation = targetTransform.rotation;

            // 右・上・前方向の回転済みベクトルを取得
            Vector3 right = rotation * Vector3.right;
            Vector3 up = rotation * Vector3.up;
            Vector3 forward = rotation * Vector3.forward;

            // 各ローカル軸をワールド軸に投影して絶対値を合算
            Vector3 worldHalf = new Vector3(
                Mathf.Abs(Vector3.Dot(right, Vector3.right)) * half.x +
                Mathf.Abs(Vector3.Dot(up, Vector3.right)) * half.y +
                Mathf.Abs(Vector3.Dot(forward, Vector3.right)) * half.z,

                Mathf.Abs(Vector3.Dot(right, Vector3.up)) * half.x +
                Mathf.Abs(Vector3.Dot(up, Vector3.up)) * half.y +
                Mathf.Abs(Vector3.Dot(forward, Vector3.up)) * half.z,

                Mathf.Abs(Vector3.Dot(right, Vector3.forward)) * half.x +
                Mathf.Abs(Vector3.Dot(up, Vector3.forward)) * half.y +
                Mathf.Abs(Vector3.Dot(forward, Vector3.forward)) * half.z
            );

            // AABB データを生成して返す
            return new AABBData(worldCenter, worldHalf);
        }
    }
}