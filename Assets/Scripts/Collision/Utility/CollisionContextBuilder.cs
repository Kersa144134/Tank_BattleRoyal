// ======================================================
// CollisionContextBuilder.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-18
// 更新日時 : 2025-12-18
// 概要     : 戦車・障害物・アイテムの衝突コンテキストをまとめて生成するクラス
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using ItemSystem.Data;
using ObstacleSystem.Data;
using SceneSystem.Manager;
using TankSystem.Data;
using TankSystem.Manager;
using WeaponSystem.Data;

namespace CollisionSystem.Utility
{
    /// <summary>
    /// SceneObjectRegistry から各衝突コンテキストを生成する専用ビルダー
    /// </summary>
    public sealed class CollisionContextBuilder
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>OBB ファクトリー参照</summary>
        private readonly OBBFactory _obbFactory;

        /// <summary>SceneObjectRegistry 参照</summary>
        private readonly SceneObjectRegistry _sceneRegistry;

        /// <summary>CollisionContextFactory 参照</summary>
        private readonly CollisionContextFactory _contextFactory;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// CollisionContextBuilder を生成する
        /// </summary>
        /// <param name="sceneRegistry">シーン上のオブジェクト管理レジストリ</param>
        /// <param name="obbFactory">OBB 生成用ファクトリー</param>
        public CollisionContextBuilder(
            SceneObjectRegistry sceneRegistry,
            OBBFactory obbFactory
        )
        {
            _sceneRegistry = sceneRegistry;
            _obbFactory = obbFactory;
            _contextFactory = new CollisionContextFactory(obbFactory);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 戦車用コンテキスト一覧を生成する
        /// </summary>
        public TankCollisionContext[] BuildTankContexts()
        {
            List<TankCollisionContext> tankContexts = new List<TankCollisionContext>();

            if (_sceneRegistry?.Tanks == null)
            {
                return tankContexts.ToArray();
            }

            // 各戦車 Transform を順に処理
            for (int i = 0; i < _sceneRegistry.Tanks.Length; i++)
            {
                Transform tankTransform = _sceneRegistry.Tanks[i];

                if (tankTransform == null)
                {
                    continue;
                }

                if (!tankTransform.TryGetComponent(out BaseTankRootManager rootManager))
                {
                    continue;
                }

                TankCollisionContext context = _contextFactory.CreateTankContext(rootManager);

                tankContexts.Add(context);
            }

            return tankContexts.ToArray();
        }

        /// <summary>
        /// 障害物用コンテキスト一覧を生成する
        /// </summary>
        public ObstacleCollisionContext[] BuildObstacleContexts()
        {
            // 可変長リストで一時的に格納
            List<ObstacleCollisionContext> obstacleContexts = new List<ObstacleCollisionContext>();

            if (_sceneRegistry?.Obstacles == null)
            {
                return obstacleContexts.ToArray();
            }

            for (int i = 0; i < _sceneRegistry.Obstacles.Length; i++)
            {
                Transform obstacle = _sceneRegistry.Obstacles[i];

                if (obstacle == null)
                {
                    continue;
                }

                ObstacleCollisionContext context =
                    _contextFactory.CreateObstacleContext(i, obstacle);

                obstacleContexts.Add(context);
            }

            return obstacleContexts.ToArray();
        }

        /// <summary>
        /// アイテム 1 件分の衝突コンテキストを生成する
        /// </summary>
        /// <param name="item">衝突判定対象となるアイテム</param>
        /// <returns>生成されたアイテム用衝突コンテキスト</returns>
        public ItemCollisionContext BuildItemContext(in ItemSlot item)
        {
            if (item == null || item.Transform == null)
            {
                return null;
            }

            return _contextFactory.CreateItemContext(item);
        }

        /// <summary>
        /// 弾丸 1 発分の衝突コンテキストを生成する
        /// </summary>
        /// <param name="bullet">衝突判定対象となる弾丸ロジック</param>
        /// <returns>生成された弾丸用衝突コンテキスト</returns>
        public BulletCollisionContext BuildBulletContext(in BulletBase bullet)
        {
            if (bullet == null || bullet.Transform == null)
            {
                return null;
            }

            return _contextFactory.CreateBulletContext(bullet);
        }
    }
}