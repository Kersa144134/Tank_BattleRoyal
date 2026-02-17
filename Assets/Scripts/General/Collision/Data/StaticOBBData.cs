// ======================================================
// StaticOBBData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-16
// 更新日時 : 2026-02-17
// 概要     : 静的オブジェクト用 OBB データ
// ======================================================

using UnityEngine;

namespace CollisionSystem.Data
{
    /// <summary>
    /// 静的 OBB データ
    /// 生成時に Transform を確定させる
    /// </summary>
    public sealed class StaticOBBData : BaseOBBData
    {
        // ======================================================
        // コンストラクタ
        // ======================================================

        public StaticOBBData(
            in Vector3 center,
            in Vector3 halfSize,
            in Quaternion rotation
        )
        {
            // 半サイズを設定する
            HalfSize = halfSize;

            // 初期 Transform を同期する
            SyncTransform(center, rotation);
        }
    }
}