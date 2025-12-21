// ======================================================
// ObstacleCollisionContext.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-18
// 更新日時 : 2025-12-18
// 概要     : 障害物用の静的衝突コンテキスト
// ======================================================

using UnityEngine;
using CollisionSystem.Data;
using CollisionSystem.Interface;

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
        // IBulletHittable プロパティ
        // ======================================================

        /// <summary>
        /// 弾丸の衝突判定に使用する OBB
        /// </summary>
        public IOBBData Bounding
        {
            get
            {
                return OBB;
            }
        }

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 障害物用の衝突コンテキストを生成する
        /// </summary>
        /// <param name="transform">障害物 Transform</param>
        /// <param name="obb">衝突判定用 OBB</param>
        public ObstacleCollisionContext(
            Transform transform,
            IOBBData obb
        )
            : base(
                transform,
                obb,
                MovementLockAxis.All
            )
        {
        }
    }
}