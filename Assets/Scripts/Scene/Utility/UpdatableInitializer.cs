// ======================================================
// UpdatableInitializer.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-02-02
// 概要     : シーン上の IUpdatable を収集し、
//            初期化済みコンテキストを生成する
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using SceneSystem.Interface;
using CameraSystem.Manager;
using CollisionSystem.Manager;
using ItemSystem.Manager;
using TankSystem.Manager;
using UISystem.Manager;
using WeaponSystem.Manager;
using SceneSystem.Data;
using SceneSystem.Manager;

namespace SceneSystem.Utility
{
    /// <summary>
    /// シーン上の IUpdatable 収集・初期化・参照キャッシュを行うクラス
    /// </summary>
    public sealed class UpdatableInitializer
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>IUpdatable を収集するコレクター</summary>
        private readonly UpdatableCollector _updatableCollector;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// Bootstrapper を生成
        /// </summary>
        /// <param name="updatableCollector">IUpdatable 収集用コレクター</param>
        public UpdatableInitializer(in UpdatableCollector updatableCollector)
        {
            _updatableCollector = updatableCollector;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// シーン上の IUpdatable を収集・初期化し、コンテキストを生成する
        /// </summary>
        /// <param name="components">収集対象の GameObject 配列</param>
        /// <returns>初期化済み UpdatableContext</returns>
        public UpdatableContext Initialize(in GameObject[] components)
        {
            // シーン上の全 IUpdatable を収集
            IUpdatable[] allUpdatables = _updatableCollector.Collect(components);

            // コンテキスト生成と参照キャッシュ
            return BuildContext(allUpdatables);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// IUpdatable から UpdatableContext を生成し、参照をキャッシュする
        /// </summary>
        private UpdatableContext BuildContext(in IUpdatable[] allUpdatables)
        {
            UpdatableContext context = new UpdatableContext();
            context.Updatables = allUpdatables;

            List<EnemyTankRootManager> enemies = new List<EnemyTankRootManager>();
            SceneObjectRegistry cacheSceneRegistry = null;

            foreach (IUpdatable updatable in allUpdatables)
            {
                if (updatable == null)
                {
                    continue;
                }

                // SceneObjectRegistry を取得
                if (updatable is SceneObjectRegistry registry)
                {
                    context.SceneObjectRegistry = registry;
                    cacheSceneRegistry = registry;
                }

                // BulletPool を取得
                if (updatable is BulletPool bulletPool)
                {
                    context.BulletPool = bulletPool;
                    bulletPool.SetSceneRegistry(cacheSceneRegistry);
                }

                // CameraManager を取得
                if (updatable is CameraManager cameraManager)
                {
                    context.CameraManager = cameraManager;
                    cameraManager.SetSceneRegistry(cacheSceneRegistry);
                }

                // CollisionManager を取得して SceneObjectRegistry を注入
                if (updatable is CollisionManager collisionManager)
                {
                    context.CollisionManager = collisionManager;
                    collisionManager.SetSceneRegistry(cacheSceneRegistry);
                }

                // ItemPool を取得
                if (updatable is ItemPool itemPool)
                {
                    context.ItemPool = itemPool;
                    itemPool.SetSceneRegistry(cacheSceneRegistry);
                }

                // UIManager を取得
                if (updatable is TitleUIManager titleUIManager)
                {
                    context.TitleUIManager = titleUIManager;
                }
                if (updatable is MainUIManager mainUIManager)
                {
                    context.MainUIManager = mainUIManager;
                    mainUIManager.SetSceneRegistry(cacheSceneRegistry);
                }
                if (updatable is ResultUIManager resultUIManager)
                {
                    context.ResultUIManager = resultUIManager;
                }

                // PlayerTank を取得
                if (updatable is PlayerTankRootManager playerTank)
                {
                    context.PlayerTank = playerTank;
                }

                // EnemyTank を収集
                if (updatable is EnemyTankRootManager enemyTank)
                {
                    enemies.Add(enemyTank);
                }

                // 各 IUpdatable の初期化処理を実行
                updatable.OnEnter();
            }

            // EnemyTank 配列をコンテキストに設定
            context.EnemyTanks = enemies.ToArray();

            return context;
        }
    }
}