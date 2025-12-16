// ======================================================
// StaticOBBData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-16
// 更新日時 : 2025-12-16
// 概要     : 静的オブジェクト用 OBB データ
// ======================================================

using UnityEngine;
using CollisionSystem.Interface;

namespace CollisionSystem.Data
{
    /// <summary>
    /// 静的 OBB データ
    /// </summary>
    public class StaticOBBData : IOBBData
    {
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>OBB の中心座標</summary>
        public Vector3 Center { get; private set; }

        /// <summary>OBB の半サイズ</summary>
        public Vector3 HalfSize { get; private set; }

        /// <summary>OBB の回転</summary>
        public Quaternion Rotation { get; private set; }

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 静的 OBB データを初期化する
        /// </summary>
        /// <param name="center">中心座標（ワールド基準）</param>
        /// <param name="halfSize">半サイズ（ローカル基準）</param>
        /// <param name="rotation">回転（ワールド基準）</param>
        public StaticOBBData(in Vector3 center, in Vector3 halfSize, in Quaternion rotation)
        {
            Center = center;
            HalfSize = halfSize;
            Rotation = rotation;
        }
    }
}