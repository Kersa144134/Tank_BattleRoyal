// ======================================================
// TankCollisionManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : 各種戦車衝突判定サービスを統括管理するマネージャー
// ======================================================

using CollisionSystem.Calculator;
using CollisionSystem.Data;
using ItemSystem.Data;
using ObstacleSystem.Data;
using SceneSystem.Interface;
using System.Collections.Generic;
using TankSystem.Data;
using TankSystem.Interface;
using TankSystem.Service;
using TankSystem.Utility;
using UnityEngine;

namespace TankSystem.Manager
{
    /// <summary>
    /// 戦車・障害物・アイテムの衝突判定を一元管理するマネージャー
    /// 各衝突タイプごとに Service を分離し、実行順を制御する
    /// </summary>
    public sealed class TankCollisionManager : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("コンポーネント参照")]
        /// <summary>
        /// シーン上の戦車・障害物を一元管理するレジストリ
        /// </summary>
        [SerializeField]
        private SceneObjectRegistry _sceneRegistry;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>OBB の衝突判定および MTV 計算を行う計算器</summary>
        private readonly BoundingBoxCollisionCalculator _boxCollisionCalculator = new BoundingBoxCollisionCalculator();

        /// <summary>MTV を用いた衝突解決量を計算するクラス</summary>
        private CollisionResolveCalculator _collisionResolverCalculator;

        /// <summary>OBB を生成するファクトリー</summary>
        private readonly OBBFactory _obbFactory = new OBBFactory();

        /// <summary>戦車衝突用コンテキストを生成するファクトリー</summary>
        private CollisionContextFactory _collisionContextFactory;

        /// <summary>戦車衝突用コンテキストを生成するビルダー</summary>
        private CollisionContextBuilder _contextBuilder;

        /// <summary>衝突判定サービス一覧</summary>
        private readonly List<ITankCollisionService> _collisionServices = new List<ITankCollisionService>();

        /// <summary>対 障害物 衝突判定サービス</summary>
        private VersusObstacleCollisionService _versusObstacleService;

        /// <summary>対 アイテム 衝突判定サービス</summary>
        private VersusItemCollisionService _versusItemService;

        /// <summary>対 戦車 衝突判定サービス</summary>
        private VersusTankCollisionService _versusTankService;

        // ======================================================
        // IUpdatable 実装
        // ======================================================

        public void OnEnter()
        {
            // --------------------------------------------------
            // クラス生成
            // --------------------------------------------------
            _contextBuilder = new CollisionContextBuilder(_sceneRegistry, _obbFactory);
            _collisionResolverCalculator = new CollisionResolveCalculator(_boxCollisionCalculator);

            // --------------------------------------------------
            // 戦車・障害物・アイテム コンテキスト構築
            // --------------------------------------------------
            TankCollisionContext[] tankContexts = _contextBuilder.BuildTankContexts();
            ObstacleCollisionContext[] obstacleContexts = _contextBuilder.BuildObstacleContexts();
            List<ItemCollisionContext> itemContexts = _contextBuilder.BuildItemContexts(_sceneRegistry.ItemSlots);
            
            // --------------------------------------------------
            // 各サービス生成
            // --------------------------------------------------
            _versusObstacleService = new VersusObstacleCollisionService(_boxCollisionCalculator, tankContexts, obstacleContexts);
            _versusItemService = new VersusItemCollisionService(_boxCollisionCalculator, tankContexts, itemContexts);
            _versusTankService = new VersusTankCollisionService(_boxCollisionCalculator, tankContexts);

            // --------------------------------------------------
            // 判定順に各衝突判定サービスを登録
            // --------------------------------------------------
            // 障害物
            _collisionServices.Add(_versusObstacleService);
            // 戦車同士
            _collisionServices.Add(_versusTankService);
            // アイテム
            _collisionServices.Add(_versusItemService);

            // --------------------------------------------------
            // イベント購読
            // --------------------------------------------------
            if (_versusObstacleService != null)
            {
                _versusObstacleService.OnObstacleHit += HandleObstacleHit;
            }
            if (_versusItemService != null)
            {
                _versusItemService.OnItemHit += HandleItemHit;
            }
            if (_versusTankService != null)
            {
                _versusTankService.OnTankHit += HandleTankHit;
            }

            UpdateItems(_sceneRegistry.ItemSlots);
        }

        public void OnUpdate()
        {
            // --------------------------------------------------
            // 衝突判定サービス実行
            // --------------------------------------------------
            for (int s = 0; s < _collisionServices.Count; s++)
            {
                ITankCollisionService service = _collisionServices[s];

                service.PreUpdate();
                service.Execute();
            }
        }

        public void OnExit()
        {
            // --------------------------------------------------
            // イベント解除
            // --------------------------------------------------
            if (_versusObstacleService != null)
            {
                _versusObstacleService.OnObstacleHit -= HandleObstacleHit;
            }
            if (_versusItemService != null)
            {
                _versusItemService.OnItemHit -= HandleItemHit;
            }
            if (_versusTankService != null)
            {
                _versusTankService.OnTankHit -= HandleTankHit;
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// アイテム情報を更新する
        /// </summary>
        /// <param name="items">衝突判定対象となるアイテム一覧</param>
        public void UpdateItems(in List<ItemSlot> items)
        {
            if (_contextBuilder == null || _versusItemService == null || items == null)
            {
                return;
            }

            // アイテムコンテキストを生成
            List<ItemCollisionContext> itemContexts = _contextBuilder.BuildItemContexts(items);

            // VersusItemCollisionService に設定
            _versusItemService.SetItemContexts(itemContexts);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // イベントハンドラ
        // --------------------------------------------------
        /// <summary>
        /// 障害物衝突時の処理
        /// </summary>
        private void HandleObstacleHit(
            TankCollisionContext context,
            ObstacleCollisionContext obstacle
        )
        {
            // 衝突解決計算
            _collisionResolverCalculator.CalculateResolveInfo(
                contextA: context,
                contextB: obstacle,
                deltaForwardA: context.TankRootManager.DeltaForward,
                deltaForwardB: 0f,
                isBMovable: false,
                resolveInfoA: out CollisionResolveInfo resolveInfoA,
                resolveInfoB: out CollisionResolveInfo resolveInfoB
            );

            // 戦車の押し戻し反映
            context.TankRootManager.ApplyCollisionResolve(resolveInfoA);

            // ログ出力
            Debug.Log($"[ObstacleHit] Tank:{context.TankId} Obstacle:{obstacle.Transform.name} ResolveVector:{resolveInfoA.ResolveVector}");
        }

        /// <summary>
        /// アイテム取得時の通知
        /// </summary>
        private void HandleItemHit(
            TankCollisionContext context,
            ItemCollisionContext item
        )
        {
            Debug.Log($"[ItemHit] Tank:{context.TankId} Item:{item.ItemSlot.ItemData.Name}");
        }

        /// <summary>
        /// 戦車同士が接触した際の処理
        /// </summary>
        private void HandleTankHit(
            TankCollisionContext contextA,
            TankCollisionContext contextB
        )
        {
            // 衝突解決計算
            _collisionResolverCalculator.CalculateResolveInfo(
                contextA: contextA,
                contextB: contextB,
                deltaForwardA: contextA.TankRootManager.DeltaForward,
                deltaForwardB: contextB.TankRootManager.DeltaForward,
                isBMovable: true,
                resolveInfoA: out CollisionResolveInfo resolveInfoA,
                resolveInfoB: out CollisionResolveInfo resolveInfoB
            );

            // 戦車の押し戻し反映
            contextA.TankRootManager.ApplyCollisionResolve(resolveInfoA);
            contextB.TankRootManager.ApplyCollisionResolve(resolveInfoB);

            Debug.Log($"[TankHit] TankA:{contextA.TankId} TankB:{contextB.TankId}");
        }
    }
}