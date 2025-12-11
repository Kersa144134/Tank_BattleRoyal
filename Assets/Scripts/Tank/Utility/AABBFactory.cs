// ======================================================
// OBBFactory.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-11
// 更新日時 : 2025-12-11
// 概要     : Transform の回転を考慮した OBB を生成するユーティリティクラス
// ======================================================

using UnityEngine;
using TankSystem.Data;

namespace TankSystem.Utility
{
    /// <summary>
    /// OBB を生成するファクトリー
    /// </summary>
    public class OBBFactory
    {
        // ======================================================
        // OBB生成メソッド
        // ======================================================
        /// <summary>
        /// Transform とローカルサイズ情報から OBB を生成
        /// </summary>
        /// <param name="targetTransform">対象 Transform</param>
        /// <param name="localCenter">ローカル座標での中心</param>
        /// <param name="localSize">ローカル座標でのサイズ</param>
        public OBBData CreateOBB(in Transform targetTransform, in Vector3 localCenter, in Vector3 localSize)
        {
            // ワールド座標での中心
            Vector3 worldCenter = targetTransform.TransformPoint(localCenter);

            // 半サイズにスケールを反映
            Vector3 halfSize = Vector3.Scale(localSize * 0.5f, targetTransform.lossyScale);

            // 回転をそのまま取得（全軸対応）
            Quaternion rotation = targetTransform.rotation;

            return new OBBData(worldCenter, halfSize, rotation);
        }

        // ======================================================
        // デバッグ用描画
        // ======================================================
        /// <summary>
        /// Gizmos にワイヤーフレームで OBB を描画
        /// </summary>
        /// <param name="targetTransform">対象 Transform</param>
        /// <param name="localCenter">ローカル中心</param>
        /// <param name="localSize">ローカルサイズ</param>
        /// <param name="color">描画色</param>
        public void DrawDebugOBB(Transform targetTransform, Vector3 localCenter, Vector3 localSize, Color color)
        {
            // OBB生成
            OBBData obb = CreateOBB(targetTransform, localCenter, localSize);

            // Gizmos色設定
            Gizmos.color = color;

            // 現在のGizmos行列を保存
            Matrix4x4 oldMatrix = Gizmos.matrix;

            // OBBの回転と位置を行列に反映
            Gizmos.matrix = Matrix4x4.TRS(obb.Center, obb.Rotation, Vector3.one);

            // ワイヤーフレーム描画（ローカルサイズにスケールを反映）
            Vector3 size = obb.HalfSize * 2f;
            Gizmos.DrawWireCube(Vector3.zero, size);

            // 行列を元に戻す
            Gizmos.matrix = oldMatrix;
        }
    }
}