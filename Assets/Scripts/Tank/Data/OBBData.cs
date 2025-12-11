// ======================================================
// OBBData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-11
// 更新日時 : 2025-12-11
// 概要     : OBBDataの中心座標・半サイズ・回転を保持するデータ構造体
// ======================================================

using UnityEngine;

namespace TankSystem.Data
{
    /// <summary>
    /// 回転を考慮した境界ボックスのデータ構造体
    /// </summary>
    public struct OBBData
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// ボックス中心点のワールド座標
        /// </summary>
        public Vector3 Center;

        /// <summary>
        /// ボックスの半サイズ
        /// </summary>
        public Vector3 HalfSize;

        /// <summary>
        /// ボックスの回転情報（ワールド空間）
        /// </summary>
        public Quaternion Rotation;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 中心座標・半サイズ・回転を指定して OBB データを生成する
        /// </summary>
        /// <param name="center">OBB の中心座標</param>
        /// <param name="halfSize">OBB の半サイズ</param>
        /// <param name="rotation">OBB の回転（ワールド空間）</param>
        public OBBData(in Vector3 center, in Vector3 halfSize, in Quaternion rotation)
        {
            Center = center;
            HalfSize = halfSize;
            Rotation = rotation;
        }
    }
}