// ======================================================
// TankCollisionContext.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-18
// 概要     : 戦車衝突判定に必要な情報を一括管理するコンテキスト
//            BaseCollisionContext を継承し、移動予定状態を基準に扱う
// ======================================================

using UnityEngine;
using CollisionSystem.Interface;
using TankSystem.Manager;

namespace CollisionSystem.Data
{
    /// <summary>
    /// 戦車の衝突判定コンテキスト
    /// </summary>
    public sealed class TankCollisionContext
        : BaseCollisionContext,
          IDynamicCollisionContext
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>OBB 重複更新防止用フラグ</summary>
        private bool _isUpdated;

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
            BaseOBBData obb,
            BaseTankRootManager tankRootManager
        )
            : base(
                tankRootManager.transform,
                obb,
                tankRootManager.CurrentFrameLockAxis,
                tankRootManager
            )
        {
            TankId = tankId;
            TankRootManager = tankRootManager;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 戦車用のフレーム初期化処理
        /// TankRootManager の LockAxis を Context に反映する
        /// </summary>
        public override void BeginFrame()
        {
            base.BeginFrame();

            // OBB 更新フラグをリセットする
            _isUpdated = false;

            if (TankRootManager == null)
            {
                return;
            }

            // ロック軸を Context に反映
            LockAxis = TankRootManager.CurrentFrameLockAxis;
        }

        /// <summary>
        /// OBB を最新の予定座標・回転に更新する
        /// </summary>
        public override void UpdateOBB()
        {
            // 更新済みなら処理なし
            if (_isUpdated)
            {
                return;
            }

            base.UpdateOBB();

            // フラグ更新
            _isUpdated = true;
        }

        /// <summary>
        /// 戦車用のロック軸確定処理
        /// Base の確定結果を TankRootManager に反映する
        /// </summary>
        public override void FinalizeLockAxis()
        {
            // Base 側で LockAxis を確定させる
            base.FinalizeLockAxis();

            // TankRootManager が存在しない場合は何もしない
            if (TankRootManager == null)
            {
                return;
            }

            // 現フレーム確定済みの LockAxis を Tank 側に反映する
            TankRootManager.CurrentFrameLockAxis = LockAxis;
        }

        /// <summary>
        /// OBB 更新フラグを解除する
        /// </summary>
        public void ResetOBBUpdate()
        {
            _isUpdated = false;
        }
    }
}