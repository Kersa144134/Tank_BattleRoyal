// ======================================================
// TankCollisionContextFactory.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : TankCollisionContext の生成責務を担うファクトリー
// ======================================================

using UnityEngine;
using CollisionSystem.Interface;
using TankSystem.Data;
using TankSystem.Manager;

namespace TankSystem.Utility
{
    /// <summary>
    /// 戦車衝突判定用コンテキストを生成するファクトリー
    /// Context の生成手順・依存関係をこのクラスに集約する
    /// </summary>
    public sealed class TankCollisionContextFactory
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>OBB を生成するファクトリー</summary>
        private readonly OBBFactory _obbFactory;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// TankCollisionContextFactory を生成する
        /// 必要となる生成依存オブジェクトを受け取る
        /// </summary>
        /// <param name="obbFactory">OBB 生成用ファクトリー</param>
        public TankCollisionContextFactory(in OBBFactory obbFactory)
        {
            _obbFactory = obbFactory;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 戦車 1 台分の衝突判定コンテキストを生成する
        /// </summary>
        /// <param name="tankId">戦車固有ID</param>
        /// <param name="tankTransform">戦車の Transform</param>
        /// <param name="boxCollider">戦車の BoxCollider</param>
        /// <param name="rootManager">戦車ルート管理クラス</param>
        /// <returns>生成された TankCollisionContext</returns>
        public TankCollisionContext Create(
            int tankId,
            Transform tankTransform,
            BoxCollider boxCollider,
            BaseTankRootManager rootManager
        )
        {
            // 衝突形状となる OBB データを生成する
            IOBBData obb =
                _obbFactory.CreateDynamicOBB(
                    tankTransform,
                    boxCollider.center,
                    boxCollider.size
                );

            // コンテキストを生成して返却する
            return new TankCollisionContext(
                tankId,
                tankTransform,
                obb,
                rootManager
            );
        }
    }
}