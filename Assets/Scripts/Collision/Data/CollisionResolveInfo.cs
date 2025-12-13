// ======================================================
// CollisionResolveInfo.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-13
// 更新日時 : 2025-12-13
// 概要     : 衝突解消（MTV）に必要な最小移動量情報を保持するデータ構造体
// ======================================================

using UnityEngine;

namespace CollisionSystem.Data
{
    /// <summary>
    /// 衝突解消に必要な最小移動量（MTV）情報を表す構造体
    /// </summary>
    public struct CollisionResolveInfo
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// 押し戻し方向ベクトル
        /// 正規化済みであり、ResolveDistance と掛け合わせて使用する
        /// </summary>
        public Vector3 ResolveDirection;

        /// <summary>
        /// 押し戻しに必要な最小距離
        /// </summary>
        public float ResolveDistance;

        /// <summary>
        /// この解決情報が有効かどうか
        /// 衝突していない場合や算出失敗時は false となる
        /// </summary>
        public bool IsValid;
    }
}