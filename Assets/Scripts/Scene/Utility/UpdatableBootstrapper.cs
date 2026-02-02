// ======================================================
// UpdatableBootstrapper.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : IUpdatable の収集・初期化・参照キャッシュを行う
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using CameraSystem.Manager;
using ItemSystem.Manager;
using SceneSystem.Data;
using SceneSystem.Interface;
using SceneSystem.Manager;
using TankSystem.Manager;
using UISystem.Manager;
using WeaponSystem.Manager;

namespace SceneSystem.Utility
{
    /// <summary>
    /// シーン内に存在する IUpdatable を収集し、
    /// 初期化および参照キャッシュを行うクラス
    /// </summary>
    public sealed class UpdatableBootstrapper
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>IUpdatable を実装しているコンポーネントを取得するクラス</summary>
        private readonly UpdatableCollector _updatableCollector;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// UpdateManager を生成する
        /// </summary>
        /// <param name="updatableCollector">IUpdatable 収集用コレクター</param>
        public UpdatableBootstrapper(UpdatableCollector updatableCollector)
        {
            _updatableCollector = updatableCollector;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// IUpdatable を初期化し、参照コンテキストを生成する
        /// </summary>
        /// <param name="components">IUpdatable を含む GameObject 配列</param>
        /// <param name="typeNames">取得対象の型名配列（null または空で全取得）</param>
        /// <returns>初期化済み参照をまとめたコンテキスト</returns>
        public UpdatableContext Initialize(GameObject[] components, string[] typeNames = null)
        {
            // IUpdatable をすべて収集（型指定があれば抽象クラスや派生クラスも取得）
            IUpdatable[] updatables = _updatableCollector.Collect(components, typeNames);

            // EnemyTankRootManager を一時格納するためのキャッシュ
            List<EnemyTankRootManager> enemies = new List<EnemyTankRootManager>();

            // SceneObjectRegistry を他クラスへ注入するためのキャッシュ
            SceneObjectRegistry cacheSceneObjectRegistry = null;

            // コンテキスト生成
            UpdatableContext context = new UpdatableContext();

            // 初期化と参照キャッシュ
            foreach (IUpdatable updatable in updatables)
            {
                if (updatable == null)
                {
                    continue;
                }

                // SceneObjectRegistry を取得
                if (updatable is SceneObjectRegistry sceneObjectRegistry)
                {
                    context.SceneObjectRegistry = sceneObjectRegistry;
                    cacheSceneObjectRegistry = sceneObjectRegistry;
                }

                // BulletPool を取得
                if (updatable is BulletPool bulletPool)
                {
                    context.BulletPool = bulletPool;
                }

                // CameraManager を取得
                if (updatable is CameraManager cameraManager)
                {
                    context.CameraManager = cameraManager;
                }

                // CollisionManager を取得
                if (updatable is CollisionManager collisionManager)
                {
                    context.CollisionManager = collisionManager;
                    collisionManager.SetSceneRegistry(cacheSceneObjectRegistry);
                }

                // ItemPool を取得
                if (updatable is ItemPool itemPool)
                {
                    context.ItemPool = itemPool;
                }

                // UIManager を取得
                if (updatable is TitleUIManager titleUIManager)
                {
                    context.UIManager = titleUIManager;
                }
                if (updatable is MainUIManager mainUIManager)
                {
                    context.UIManager = mainUIManager;
                }
                if (updatable is ResultUIManager resultUIManager)
                {
                    context.UIManager = resultUIManager;
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

            // Enemy 配列を確定
            context.EnemyTanks = enemies.ToArray();

            // IUpdatable 配列を保持
            context.Updatables = updatables;

            return context;
        }
    }
}