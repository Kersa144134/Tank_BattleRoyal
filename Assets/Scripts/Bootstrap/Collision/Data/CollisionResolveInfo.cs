// ======================================================
// CollisionResolveInfo.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-13
// 更新日時 : 2025-12-13
// 概要     : 衝突解決に必要な移動情報を保持するデータ構造体
// ======================================================

using UnityEngine;

namespace CollisionSystem.Data
{
    /// <summary>
    /// 衝突解決に必要な移動情報を保持する構造体
    /// </summary>
    public struct CollisionResolveInfo
    {
        /// <summary>押し戻しベクトルそのもの</summary>
        public readonly Vector3 ResolveVector;

        /// <summary>押し戻し方向（正規化）</summary>
        public Vector3 ResolveDirection => ResolveVector.normalized;

        /// <summary>押し戻し距離</summary>
        public float ResolveDistance => ResolveVector.magnitude;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="resolveVector">押し戻しベクトル</param>
        public CollisionResolveInfo(Vector3 resolveVector)
        {
            ResolveVector = resolveVector;
        }
    }
}