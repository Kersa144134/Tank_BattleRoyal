// ======================================================
// IOBBData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-16
// 更新日時 : 2025-12-16
// 概要     : OBB データの共通インターフェイス
//            静的・動的の OBBData を共通で扱うためのインターフェイス
// ======================================================

using UnityEngine;

namespace CollisionSystem.Interface
{
    /// <summary>
    /// OBB データ共通インターフェイス
    /// </summary>
    public interface IOBBData
    {
        /// <summary>OBB の中心座標（ワールド基準）</summary>
        Vector3 Center { get; }

        /// <summary>OBB の半サイズ（ローカル基準）</summary>
        Vector3 HalfSize { get; }

        /// <summary>OBB の回転（ワールド基準）</summary>
        Quaternion Rotation { get; }

        /// <summary>動的OBB用の更新メソッド</summary>
        void Update() { }
    }
}