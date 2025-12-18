// ======================================================
// TankCollisionContext.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-18
// 概要     : 戦車衝突判定に必要な情報を一括管理するコンテキスト
//            BaseCollisionContext を継承し、移動予定状態を基準に扱う
// ======================================================

using CollisionSystem.Data;
using CollisionSystem.Interface;
using TankSystem.Manager;
using UnityEngine;

namespace TankSystem.Data
{
    /// <summary>
    /// 戦車 1 台分の衝突判定コンテキスト
    /// 移動確定前の「予定状態」を基準として判定を行う
    /// </summary>
    public sealed class TankCollisionContext : BaseCollisionContext
    {
        // ======================================================
        // 固有情報
        // ======================================================

        /// <summary>
        /// 戦車を一意に識別する ID
        /// 戦車同士の衝突対応付けに使用される
        /// </summary>
        public int TankId { get; private set; }

        /// <summary>
        /// 戦車の移動・回転・状態管理を統括するルート管理クラス
        /// 衝突解決結果の反映先として使用される
        /// </summary>
        public BaseTankRootManager TankRootManager { get; private set; }

        /// <summary>
        /// 次フレームで適用予定のワールド座標
        /// 衝突判定はこの座標を基準に行う
        /// </summary>
        public override Vector3 PlannedNextPosition => TankRootManager.PlannedNextPosition;

        /// <summary>
        /// 次フレームで適用予定の回転
        /// OBB の向きを決定するために使用される
        /// </summary>
        public override Quaternion PlannedNextRotation => TankRootManager.PlannedNextRotation;

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
        // パブリックっメソッド
        // ======================================================

        /// <summary>
        /// 衝突判定用の OBB データを設定する
        /// </summary>
        /// <param name="obb">設定する IOBBData インスタンス</param>
        public void SetOBB(IOBBData obb)
        {
            OBB = obb;
        }
    }
}