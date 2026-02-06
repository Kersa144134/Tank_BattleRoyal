// ======================================================
// IOBBData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-16
// 更新日時 : 2025-12-16
// 概要     : OBB データの共通インターフェース
//            静的・動的の OBBData を共通で扱うためのインターフェース
// ======================================================

using UnityEngine;

namespace CollisionSystem.Interface
{
    /// <summary>
    /// OBB データ共通インターフェース
    /// </summary>
    public interface IOBBData
    {
        /// <summary>OBB の中心座標（ワールド基準）</summary>
        Vector3 Center { get; }

        /// <summary>OBB の半サイズ（ローカル基準）</summary>
        Vector3 HalfSize { get; }

        /// <summary>OBB の回転（ワールド基準）</summary>
        Quaternion Rotation { get; }

        /// <summary>
        /// OBB の状態を更新する
        /// 動的 OBB は外部座標・回転を渡して更新できる
        /// </summary>
        /// <param name="plannedPosition">基準となるワールド座標</param>
        /// <param name="plannedRotation">基準となる回転</param>
        void Update(in Vector3 plannedPosition, in Quaternion plannedRotation) { }
    }
}