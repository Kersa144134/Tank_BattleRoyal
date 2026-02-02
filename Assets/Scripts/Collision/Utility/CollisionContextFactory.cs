// ======================================================
// CollisionContextFactory.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-18
// 更新日時 : 2025-12-18
// 概要     : 衝突コンテキスト生成を統括するファクトリー
//            戦車、障害物、アイテムの静的・動的 OBB に対応
// ======================================================

using UnityEngine;
using CollisionSystem.Interface;
using ItemSystem.Data;
using ObstacleSystem.Data;
using TankSystem.Data;
using TankSystem.Manager;
using WeaponSystem.Data;

namespace CollisionSystem.Utility
{
    /// <summary>
    /// 衝突判定用コンテキストを生成する汎用ファクトリー
    /// OBB 初期生成と Context 構築のみを責務とする
    /// </summary>
    public sealed class CollisionContextFactory
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>OBB を生成するためのファクトリー</summary>
        private readonly OBBFactory _obbFactory;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// CollisionContextFactory を生成する
        /// </summary>
        /// <param name="obbFactory">OBB 生成用ファクトリー</param>
        public CollisionContextFactory(in OBBFactory obbFactory)
        {
            _obbFactory = obbFactory;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 戦車用の動的衝突コンテキストを生成する
        /// </summary>
        /// <param name="tankRootManager">戦車の移動・回転を管理するルート管理クラス</param>
        /// <returns>生成された TankCollisionContext</returns>
        public TankCollisionContext CreateTankContext(in BaseTankRootManager tankRootManager)
        {
            // 動的 OBB を生成
            IOBBData obb = _obbFactory.CreateDynamicOBB(tankRootManager.HitBoxCenter, tankRootManager.HitBoxScale);

            // TankCollisionContext を構築して返却
            return new TankCollisionContext(tankRootManager.TankId, obb, tankRootManager);
        }

        /// <summary>
        /// 弾丸用の動的衝突コンテキストを生成する
        /// </summary>
        /// <param name="bullet">衝突判定対象となる弾丸ロジック本体</param>
        /// <returns>生成された BulletCollisionContext</returns>
        public BulletCollisionContext CreateBulletContext(in BulletBase bullet
        )
        {
            // 動的 OBB を生成
            IOBBData obb = _obbFactory.CreateDynamicOBB(Vector3.zero, bullet.Transform.lossyScale);

            // BulletCollisionContext を構築して返却
            return new BulletCollisionContext(bullet, obb);
        }

        /// <summary>
        /// 静的障害物用の衝突コンテキストを生成する
        /// </summary>
        /// <param name="obstacleId">障害物 ID</param>
        /// <param name="obstacleTransform">障害物 Transform</param>
        /// <returns>生成された ObstacleCollisionContext</returns>
        public ObstacleCollisionContext CreateObstacleContext(
            in int obstacleId,
            in Transform obstacleTransform
        )
        {
            // 静的 OBB を生成
            IOBBData obb = _obbFactory.CreateStaticOBB(
                obstacleTransform.position,
                obstacleTransform.rotation,
                Vector3.zero,
                obstacleTransform.lossyScale
            );

            // ObstacleCollisionContext を構築して返却
            return new ObstacleCollisionContext(obstacleId, obstacleTransform, obb);
        }

        /// <summary>
        /// アイテム用の衝突コンテキストを生成する
        /// </summary>
        /// <param name="item">対象の ItemSlot</param>
        /// <returns>生成された ItemCollisionContext</returns>
        public ItemCollisionContext CreateItemContext(ItemSlot item)
        {
            if (item == null || item.Transform == null)
            {
                return null;
            }

            // 静的 OBB を生成
            IOBBData obb = _obbFactory.CreateStaticOBB(
                item.Transform.position,
                item.Transform.rotation,
                Vector3.zero,
                item.Transform.lossyScale
            );

            // ItemCollisionContext を構築して返却
            return new ItemCollisionContext(item, obb);
        }
    }
}