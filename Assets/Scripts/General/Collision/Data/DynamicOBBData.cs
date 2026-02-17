// ======================================================
// DynamicOBBData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-16
// 更新日時 : 2026-02-17
// 概要     : 動的オブジェクト用 OBB データ
// ======================================================

using UnityEngine;

namespace CollisionSystem.Data
{
    /// <summary>
    /// 動的 OBB データ
    /// </summary>
    public sealed class DynamicOBBData : BaseOBBData
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// Transform 原点からのローカル中心オフセット
        /// </summary>
        private readonly Vector3 _localCenter;

        // ======================================================
        // コンストラクタ
        // ======================================================

        public DynamicOBBData(
            in Vector3 localCenter,
            in Vector3 halfSize
        )
        {
            // ローカル中心オフセットを保持する
            _localCenter = localCenter;

            // 半サイズを設定する
            HalfSize = halfSize;

            // 初期中心をゼロで初期化する
            Center = Vector3.zero;

            // 初期回転を単位回転に設定する
            Rotation = Quaternion.identity;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ワールド Transform を同期する
        /// </summary>
        public override void SyncTransform(
            in Vector3 worldPosition,
            in Quaternion worldRotation
        )
        {
            // 回転をローカル中心に適用する
            Vector3 rotatedOffset = worldRotation * _localCenter;

            // 回転補正済み中心を算出する
            Vector3 worldCenter = worldPosition + rotatedOffset;

            // 基底処理で中心と回転を設定する
            SetTransform(worldCenter, worldRotation);
        }
    }
}