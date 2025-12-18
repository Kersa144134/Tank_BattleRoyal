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
using TankSystem.Data;
using TankSystem.Utility;

namespace TankSystem.Manager
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

                if (!tankTransform.TryGetComponent(out BoxCollider boxCollider))
                {
                    continue;
                }

                if (!tankTransform.TryGetComponent(out BaseTankRootManager rootManager))
                {
                    continue;
                }

                // TankCollisionContext を生成
                TankCollisionContext context = _contextFactory.CreateTankContext(
                    i,
                    boxCollider,
                    rootManager
                );

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

            foreach (Transform obstacle in _sceneRegistry.Obstacles)
            {
                if (obstacle == null)
                {
                    continue;
                }

                if (!obstacle.TryGetComponent(out BoxCollider boxCollider))
                {
                    continue;
                }

                ObstacleCollisionContext context = _contextFactory.CreateObstacleContext(
                    obstacle,
                    boxCollider
                );

                // リストに追加
                obstacleContexts.Add(context);
            }

            // 配列に変換して返却
            return obstacleContexts.ToArray();
        }

        /// <summary>
        /// アイテム用コンテキスト一覧を生成する
        /// </summary>
        /// <param name="items">衝突判定対象となるアイテム一覧</param>
        public List<ItemCollisionContext> BuildItemContexts(in List<ItemSlot> items)
        {
            List<ItemCollisionContext> itemContexts = new List<ItemCollisionContext>();

            foreach (ItemSlot item in items)
            {
                if (item == null || item.Transform == null)
                {
                    continue;
                }

                ItemCollisionContext context = _contextFactory.CreateItemContext(
                    item
                );

                itemContexts.Add(context);
            }

            return itemContexts;
        }
    }
}