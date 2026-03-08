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
        // コンポーネント参照
        // ======================================================

        /// <summary>IUpdatable を収集するコレクター</summary>
        private readonly UpdatableCollector _updatableCollector;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>収集した IUpdatable コンテキスト</summary>
        private UpdatableContext _allUpdatablesContext;

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
        public UpdatableContext InitializeUpdatables(in GameObject[] components)
        {
            // シーン上の全 IUpdatable を収集
            IUpdatable[] allUpdatables = _updatableCollector.Collect(components);

            // コンテキスト生成
            _allUpdatablesContext = BuildContext(allUpdatables);

            return _allUpdatablesContext;
        }

        /// <summary>
        /// 収集済み IUpdatable の終了処理を実行する
        /// </summary>
        public void FinalizeUpdatables()
        {
            if (_allUpdatablesContext == null)
            {
                return;
            }

            foreach (IUpdatable updatable in _allUpdatablesContext.Updatables)
            {
                if (updatable == null)
                {
                    continue;
                }

                // 各 IUpdatable の終了処理を実行
                updatable.OnExit();
            }
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
            SceneObjectRegistry sceneObjectRegistryCache = null;

            foreach (IUpdatable updatable in allUpdatables)
            {
                if (updatable == null)
                {
                    continue;
                }

                // 各 IUpdatable を取得
                if (updatable is SceneObjectRegistry sceneObjectRegistry)
                {
                    context.SceneObjectRegistry = sceneObjectRegistry;
                    sceneObjectRegistryCache = sceneObjectRegistry;
                }
                if (updatable is BulletPool bulletPool)
                {
                    context.BulletPool = bulletPool;
                    bulletPool.SetSceneRegistry(sceneObjectRegistryCache);
                }
                if (updatable is CameraManager cameraManager)
                {
                    context.CameraManager = cameraManager;
                    cameraManager.SetSceneRegistry(sceneObjectRegistryCache);
                }
                if (updatable is CollisionManager collisionManager)
                {
                    context.CollisionManager = collisionManager;
                    collisionManager.SetSceneRegistry(sceneObjectRegistryCache);
                }
                if (updatable is ItemPool itemPool)
                {
                    context.ItemPool = itemPool;
                    itemPool.SetSceneRegistry(sceneObjectRegistryCache);
                }
                if (updatable is TitleUIManager titleUIManager)
                {
                    context.TitleUIManager = titleUIManager;
                }
                if (updatable is MainUIManager mainUIManager)
                {
                    context.MainUIManager = mainUIManager;
                    mainUIManager.SetSceneRegistry(sceneObjectRegistryCache);
                }
                if (updatable is ResultUIManager resultUIManager)
                {
                    context.ResultUIManager = resultUIManager;
                }
                if (updatable is PlayerTankRootManager playerTank)
                {
                    context.PlayerTank = playerTank;
                }
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