// ======================================================
// ObstacleCollisionContext.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-18
// 更新日時 : 2025-12-18
// 概要     : 障害物用の静的衝突コンテキスト
// ======================================================

using CollisionSystem.Data;
using CollisionSystem.Interface;
using UnityEngine;
using WeaponSystem.Interface;

namespace ObstacleSystem.Data
{
    /// <summary>
    /// 障害物 1 個分の衝突判定コンテキスト
    /// 静的物体として扱われ、移動予定座標は Transform から取得する
    /// </summary>
    public sealed class ObstacleCollisionContext
        : BaseCollisionContext,
        IStaticCollisionContext
    {
        // ======================================================
        // 固有プロパティ
        // ======================================================

        /// <summary>障害物を一意に識別する ID</summary>
        public int ObstacleId { get; private set; }
        
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
        /// 障害物用の衝突コンテキストを生成する
        /// </summary>
        /// <param name="obstacleId">障害物 ID</param>
        /// <param name="transform">障害物 Transform</param>
        /// <param name="obb">衝突判定用 OBB</param>
        /// <param name="damageable">障害物を管理するダメージ受付先</param>
        public ObstacleCollisionContext(
            in int obstacleId,
            in Transform transform,
            in IOBBData obb,
            in IDamageable damageable
        )
            : base(
                transform,
                obb,
                MovementLockAxis.All,
                damageable
            )
        {
            ObstacleId = obstacleId;
        }
    }
}