// ======================================================
// TankCollisionManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : 各種戦車衝突判定サービスを統括管理するマネージャー
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using CollisionSystem.Calculator;
using CollisionSystem.Data;
using ItemSystem.Data;
using ObstacleSystem.Data;
using SceneSystem.Interface;
using TankSystem.Data;
using TankSystem.Interface;
using TankSystem.Service;
using TankSystem.Utility;

namespace TankSystem.Manager
{
    /// <summary>
    /// 戦車・障害物・アイテムの衝突判定を一元管理するマネージャー
    /// 各衝突タイプごとに Service を分離し、実行順を制御する
    /// </summary>
    public sealed class TankCollisionManager : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // 列挙体
        // ======================================================

        /// <summary>
        /// 衝突判定の実行フェーズを表す列挙体
        /// </summary>
        private enum CollisionPhase
        {
            /// <summary>障害物との衝突判定フェーズ</summary>
            Obstacle,

            /// <summary>戦車同士の衝突判定フェーズ</summary>
            Tank,

            /// <summary>アイテムとの衝突判定フェーズ</summary>
            Item
        }

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
        // フィールド
        // ======================================================
        
        /// <summary>
        /// 衝突判定対象となる戦車コンテキスト配列
        /// </summary>
        private TankCollisionContext[] _tanks;

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
            _tanks = _contextBuilder.BuildTankContexts();
            ObstacleCollisionContext[] obstacleContexts = _contextBuilder.BuildObstacleContexts();
            List<ItemCollisionContext> itemContexts = _contextBuilder.BuildItemContexts(_sceneRegistry.ItemSlots);
            
            // --------------------------------------------------
            // 各サービス生成
            // --------------------------------------------------
            _versusObstacleService = new VersusObstacleCollisionService(_boxCollisionCalculator, _tanks, obstacleContexts);
            _versusItemService = new VersusItemCollisionService(_boxCollisionCalculator, _tanks, itemContexts);
            _versusTankService = new VersusTankCollisionService(_boxCollisionCalculator, _tanks);

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
            // ======================================================
            // フレーム初期化
            // ======================================================
            for (int i = 0; i < _tanks.Length; i++)
            {
                _tanks[i].BeginFrame();
            }

            // ======================================================
            // 障害物
            // ======================================================
            ExecuteCollisionService(_versusObstacleService, CollisionPhase.Obstacle);

            // --------------------------------------------------
            // 障害物由来の LockAxis を確定
            // --------------------------------------------------
            for (int i = 0; i < _tanks.Length; i++)
            {
                _tanks[i].FinalizeLockAxis();
            }

            // ======================================================
            // 戦車
            // ======================================================
            ExecuteCollisionService(_versusTankService, CollisionPhase.Tank);

            // --------------------------------------------------
            // 戦車衝突解決後に障害物衝突チェック
            // --------------------------------------------------
            ExecuteCollisionService(_versusObstacleService, CollisionPhase.Obstacle);

            // --------------------------------------------------
            // 再度 LockAxis を確定
            // --------------------------------------------------
            for (int i = 0; i < _tanks.Length; i++)
            {
                _tanks[i].FinalizeLockAxis();
            }

            // --------------------------------------------------
            // 再び戦車衝突チェック
            // --------------------------------------------------
            ExecuteCollisionService(_versusTankService, CollisionPhase.Tank);

            // ======================================================
            // アイテム
            // ======================================================
            ExecuteCollisionService(_versusItemService, CollisionPhase.Item);
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

        /// <summary>
        /// 指定された衝突判定サービスを 1 フェーズ分実行する
        /// </summary>
        /// <param name="service">実行対象の衝突判定サービス</param>
        /// <param name="phase">現在の衝突フェーズ</param>
        private void ExecuteCollisionService(
            ITankCollisionService service,
            CollisionPhase phase
        )
        {
            // サービスが未生成の場合は何もしない
            if (service == null)
            {
                return;
            }

            // フェーズ開始前の前処理
            service.PreUpdate();

            // 衝突判定および解決処理を実行
            service.Execute();
        }

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
                resolveInfoA: out CollisionResolveInfo resolveInfoA,
                resolveInfoB: out CollisionResolveInfo resolveInfoB
            );

            // --------------------------------------------------
            // 押し戻しが発生した軸を LockAxis に反映
            // --------------------------------------------------
            // この衝突によって発生したロック軸
            MovementLockAxis newLockAxis = MovementLockAxis.None;

            // X 方向に有意な押し戻しが発生しているか
            if (Mathf.Abs(resolveInfoA.ResolveVector.x) > 0f)
            {
                newLockAxis |= MovementLockAxis.X;
            }

            // Z 方向に有意な押し戻しが発生しているか
            if (Mathf.Abs(resolveInfoA.ResolveVector.z) > 0f)
            {
                newLockAxis |= MovementLockAxis.Z;
            }

            // 押し戻しが発生している場合のみ反映
            if (newLockAxis != MovementLockAxis.None)
            {
                // 両軸ロック時は意味的に All に正規化
                MovementLockAxis resolvedAxisA =
                    newLockAxis == (MovementLockAxis.X | MovementLockAxis.Z)
                        ? MovementLockAxis.All
                        : newLockAxis;

                // フレーム中のロック軸として累積
                context.AddPendingLockAxis(resolvedAxisA);
            }

            // 戦車の押し戻し反映
            context.TankRootManager.ApplyCollisionResolve(resolveInfoA);

            // 押し戻した位置で OBB を更新
            context.UpdateOBB();
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
                resolveInfoA: out CollisionResolveInfo resolveInfoA,
                resolveInfoB: out CollisionResolveInfo resolveInfoB
            );

            // 戦車の押し戻し反映
            contextA.TankRootManager.ApplyCollisionResolve(resolveInfoA);
            contextB.TankRootManager.ApplyCollisionResolve(resolveInfoB);

            // 押し戻した位置で OBB を更新
            contextA.UpdateOBB();
            contextB.UpdateOBB();
        }
    }
}