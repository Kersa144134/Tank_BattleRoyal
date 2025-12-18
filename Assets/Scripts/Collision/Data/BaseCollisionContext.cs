// ======================================================
// BaseCollisionContext.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : 衝突解決計算で使用する共通コンテキスト基底クラス
// ======================================================

using CollisionSystem.Interface;
using UnityEngine;

namespace CollisionSystem.Data
{
    /// <summary>
    /// 衝突判定および衝突解決処理で使用される共通コンテキスト
    /// 戦車・障害物など「OBB を持つ衝突対象」の最小単位を定義する
    /// </summary>
    public abstract class BaseCollisionContext
    {
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>衝突判定に使用する OBB データ</summary>
        public IOBBData OBB
        {
            get;
            protected set;
        }

        /// <summary>衝突対象の Transform</summary>
        public Transform Transform
        {
            get;
            protected set;
        }

        // ======================================================
        // 抽象プロパティ
        // ======================================================

        /// <summary>衝突判定の基準となる、移動予定ワールド座標</summary>
        public abstract Vector3 NextPosition { get; }

        /// <summary>衝突判定の基準となる、移動予定ワールド回転</summary>
        public abstract Quaternion NextRotation { get; }

        /// <summary>現フレームにおける移動ロック軸</summary>
        public abstract MovementLockAxis LockAxis
        {
            get;
            protected set;
        }

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// BaseCollisionContext を生成する
        /// </summary>
        /// <param name="transform">衝突対象の Transform</param>
        /// <param name="obb">衝突判定用 OBB</param>
        /// <param name="lockAxis">移動ロック軸</param>
        protected BaseCollisionContext(
            Transform transform,
            IOBBData obb,
            MovementLockAxis lockAxis
        )
        {
            Transform = transform;
            OBB = obb;
            LockAxis = lockAxis;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// OBB を最新の予定座標・回転に更新する
        /// </summary>
        public void UpdateOBB()
        {
            if (OBB is DynamicOBBData dynamicOBB)
            {
                dynamicOBB.Update(NextPosition, NextRotation);
            }
        }

        /// <summary>
        /// 移動ロック軸 を最新の状態に更新する
        /// </summary>
        public virtual void UpdateLockAxis(MovementLockAxis lockAxis)
        {
            LockAxis = lockAxis;
        }
    }
}