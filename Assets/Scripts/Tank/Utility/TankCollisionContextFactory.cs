// ======================================================
// TankCollisionContextFactory.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-18
// 概要     : TankCollisionContext の生成責務を担うファクトリー
//            Transform を Context に保持させない設計に対応
// ======================================================

using CollisionSystem.Data;
using CollisionSystem.Interface;
using TankSystem.Data;
using TankSystem.Manager;
using UnityEngine;
using static UnityEditor.U2D.ScriptablePacker;

namespace TankSystem.Utility
{
    /// <summary>
    /// 戦車衝突判定用コンテキストを生成するファクトリー
    /// OBB 初期生成と Context 構築のみを責務とする
    /// </summary>
    public sealed class TankCollisionContextFactory
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// OBB を生成するためのファクトリー
        /// </summary>
        private readonly OBBFactory _obbFactory;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// TankCollisionContextFactory を生成する
        /// </summary>
        /// <param name="obbFactory">OBB 生成用ファクトリー</param>
        public TankCollisionContextFactory(
            in OBBFactory obbFactory
        )
        {
            // OBB ファクトリー参照を保持する
            _obbFactory = obbFactory;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 戦車 1 台分の衝突判定コンテキストを生成する
        /// </summary>
        /// <param name="tankId">戦車固有 ID</param>
        /// <param name="boxCollider">衝突形状定義に使用する BoxCollider</param>
        /// <param name="tankRootManager">戦車の移動・回転を管理するルート管理クラス</param>
        /// <returns>生成された TankCollisionContext</returns>
        public TankCollisionContext Create(
            int tankId,
            BoxCollider boxCollider,
            BaseTankRootManager tankRootManager
        )
        {
            // 戦車用の動的 OBB データを生成
            TankCollisionContext context = new TankCollisionContext(
                tankId,
                null,
                tankRootManager
            );

            // OBB データを衝突判定コンテキストに注入
            context.SetOBB(
                _obbFactory.CreateDynamicOBB(context, boxCollider.center, boxCollider.size)
            );

            return context;
        }
    }
}