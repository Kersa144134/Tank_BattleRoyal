// ======================================================
// TankCollisionContext.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : 戦車衝突判定に必要な情報を一括管理するコンテキスト
// ======================================================

using TankSystem.Manager;
using CollisionSystem.Interface;
using UnityEngine;

namespace TankSystem.Data
{
    /// <summary>
    /// 戦車1台分の衝突判定・衝突解決に必要な情報をまとめたコンテキスト
    /// TankCollisionManager / 各 CollisionService から参照される
    /// </summary>
    public sealed class TankCollisionContext
    {
        // ======================================================
        // 基本情報
        // ======================================================

        /// <summary>
        /// 戦車を一意に識別するためのID
        /// TankVsTankCollisionService における対応付けに使用される
        /// </summary>
        public int TankId { get; private set; }

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>
        /// 戦車のルート Transform
        /// OBB 更新や衝突後の位置計算の基準となる
        /// </summary>
        public Transform Transform { get; private set; }

        /// <summary>
        /// 戦車の衝突形状を表す OBB データ
        /// 毎フレーム Update が呼ばれる
        /// </summary>
        public IOBBData OBB { get; private set; }

        /// <summary>
        /// 戦車の移動・回転・衝突解決を統括するルート管理クラス
        /// 衝突解決結果の反映先として使用される
        /// </summary>
        public BaseTankRootManager RootManager { get; private set; }

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 戦車衝突コンテキストを生成する
        /// </summary>
        /// <param name="tankId">戦車ID</param>
        /// <param name="transform">戦車の Transform</param>
        /// <param name="obb">戦車の OBB データ</param>
        /// <param name="rootManager">戦車のルート管理クラス</param>
        public TankCollisionContext(
            int tankId,
            Transform transform,
            IOBBData obb,
            BaseTankRootManager rootManager
        )
        {
            // 戦車IDを設定する
            TankId = tankId;

            // Transform 参照を保持する
            Transform = transform;

            // OBB データ参照を保持する
            OBB = obb;

            // ルート管理クラス参照を保持する
            RootManager = rootManager;
        }
    }
}