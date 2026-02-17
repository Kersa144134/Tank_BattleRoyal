// ======================================================
// AreaCollisionContext.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-18
// 更新日時 : 2025-12-18
// 概要     : アイテム用の静的衝突コンテキスト
// ======================================================

using UnityEngine;
using CollisionSystem.Interface;

namespace CollisionSystem.Data
{
    /// <summary>
    /// エリアの衝突判定コンテキスト
    /// </summary>
    public sealed class AreaCollisionContext
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
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// エリア用の衝突コンテキストを生成する
        /// </summary>
        /// <param name="area">エリア Transform</param>
        /// <param name="obb">衝突判定用 OBB</param>
        public AreaCollisionContext(
            in Transform area,
            in BaseOBBData obb
        )
            : base(
                area,
                obb,
                MovementLockAxis.All,
                null
            )
        {
        }
    }
}