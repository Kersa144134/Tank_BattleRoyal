// ======================================================
// IOBBData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-16
// 更新日時 : 2026-02-17
// 概要     : OBB データの共通インターフェース
// ======================================================

using UnityEngine;

namespace CollisionSystem.Interface
{
    /// <summary>
    /// OBB データ共通インターフェース
    /// </summary>
    public interface IOBBData
    {
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// OBB の中心座標（ワールド基準）
        /// SAT 計算および距離判定に使用する
        /// </summary>
        Vector3 Center
        {
            get;
        }

        /// <summary>
        /// OBB の半サイズ（ローカル基準）
        /// 各軸方向への拡張量として使用する
        /// </summary>
        Vector3 HalfSize
        {
            get;
        }

        /// <summary>
        /// OBB の回転（ワールド基準）
        /// ローカル軸をワールド軸へ変換するために使用する
        /// </summary>
        Quaternion Rotation
        {
            get;
        }
    }
}