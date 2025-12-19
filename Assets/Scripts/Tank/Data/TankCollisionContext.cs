// ======================================================
// TankCollisionContext.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-18
// 概要     : 戦車衝突判定に必要な情報を一括管理するコンテキスト
//            BaseCollisionContext を継承し、移動予定状態を基準に扱う
// ======================================================

using UnityEngine;
using CollisionSystem.Data;
using CollisionSystem.Interface;
using TankSystem.Manager;

namespace TankSystem.Data
{
    /// <summary>
    /// 戦車 1 台分の衝突判定コンテキスト
    /// 移動確定前の「予定状態」を基準として判定を行う
    /// </summary>
    public sealed class TankCollisionContext : BaseCollisionContext
    {
        // ======================================================
        // 固有プロパティ
        // ======================================================

        /// <summary>戦車を一意に識別する ID</summary>
        public int TankId { get; private set; }

        /// <summary>戦車の移動・回転・状態管理を統括するルート管理クラス</summary>
        public BaseTankRootManager TankRootManager { get; private set; }

        // ======================================================
        // 抽象プロパティ
        // ======================================================

        /// <summary>移動予定ワールド座標</summary>
        public override Vector3 NextPosition => TankRootManager.NextPosition;

        /// <summary>移動予定ワールド回転</summary>
        public override Quaternion NextRotation => TankRootManager.NextRotation;

        /// <summary>現フレームにおける移動ロック軸</summary>
        private MovementLockAxis _lockAxis = MovementLockAxis.None;
        public override MovementLockAxis LockAxis
        {
            get => _lockAxis;
            protected set => _lockAxis = value;
        }

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// TankCollisionContext を生成する
        /// </summary>
        /// <param name="tankId">戦車 ID</param>
        /// <param name="obb">戦車の OBB データ</param>
        /// <param name="tankRootManager">戦車ルート管理クラス</param>
        public TankCollisionContext(
            int tankId,
            IOBBData obb,
            BaseTankRootManager tankRootManager
        )
            : base(
                tankRootManager.transform,
                obb,
                tankRootManager.CurrentFrameLockAxis
            )
        {
            TankId = tankId;
            TankRootManager = tankRootManager;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 移動ロック軸を最新の状態に更新する
        /// </summary>
        /// <param name="lockAxis">更新する軸。null の場合は前フレームの CurrentFrameLockAxis を使用</param>
        public override void UpdateLockAxis(MovementLockAxis? lockAxis = null)
        {
            // 引数がある場合はそれを使用、ない場合は前フレームの値を使用
            _lockAxis = lockAxis ?? TankRootManager.CurrentFrameLockAxis;

            // 現フレームの LockAxis を更新
            TankRootManager.CurrentFrameLockAxis = _lockAxis;
        }
    }
}