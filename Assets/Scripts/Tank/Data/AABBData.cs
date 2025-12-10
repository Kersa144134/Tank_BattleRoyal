// ======================================================
// AABBData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-10
// 更新日時 : 2025-12-10
// 概要     : AABBの中心座標と半サイズを保持するデータ構造体
// ======================================================

using UnityEngine;

namespace TankSystem.Data
{
    /// <summary>
    /// AABBのデータを保持する構造体
    /// </summary>
    public struct AABBData
    {
        // ======================================================
        // フィ－ルド
        // ======================================================

        /// <summary>
        /// ボックス中心点のワールド座標
        /// </summary>
        public Vector3 Center;

        /// <summary>
        /// ボックスの半サイズ
        /// </summary>
        public Vector3 HalfSize;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 中心座標と半サイズを指定して AABB データを生成する
        /// </summary>
        /// <param name="center">AABB の中心座標</param>
        /// <param name="halfSize">AABB の半サイズ</param>
        public AABBData(Vector3 center, Vector3 halfSize)
        {
            Center = center;
            HalfSize = halfSize;
        }
    }
}