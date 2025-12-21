// ======================================================
// ItemCollisionContext.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-18
// 更新日時 : 2025-12-18
// 概要     : アイテム用の静的衝突コンテキスト
// ======================================================

using CollisionSystem.Data;
using CollisionSystem.Interface;
using ItemSystem.Data;
using TankSystem.Data;
using UnityEngine;

namespace ItemSystem.Data
{
    /// <summary>
    /// アイテム 1 個分の衝突判定コンテキスト
    /// 静的物体として扱われ、移動予定座標は Transform から取得する
    /// </summary>
    public sealed class ItemCollisionContext
        : BaseCollisionContext,
        IStaticCollisionContext
    {
        // ======================================================
        // 固有プロパティ
        // ======================================================

        /// <summary>関連付けられたアイテム情報</summary>
        public ItemSlot ItemSlot
        {
            get;
            private set;
        }

        // ======================================================
        // 抽象プロパティ
        // ======================================================

        /// <summary>移動予定ワールド座標</summary>
        public override Vector3 NextPosition => Transform.position;

        /// <summary>移動予定ワールド回転</summary>
        public override Quaternion NextRotation => Transform.rotation;

        /// <summary>現フレームにおける移動ロック軸</summary>
        private MovementLockAxis _lockAxis = MovementLockAxis.All;
        public override MovementLockAxis LockAxis
        {
            get => _lockAxis;
            protected set => _lockAxis = value;
        }

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// アイテム用の衝突コンテキストを生成する
        /// </summary>
        /// <param name="item">アイテム情報（Transform やデータを含む）</param>
        /// <param name="obb">衝突判定用 OBB</param>
        public ItemCollisionContext(
            in ItemSlot item,
            in IOBBData obb
        )
            : base(
                item.Transform,
                obb,
                MovementLockAxis.All
            )
        {
            // アイテム情報を保持
            ItemSlot = item;
        }
    }
}