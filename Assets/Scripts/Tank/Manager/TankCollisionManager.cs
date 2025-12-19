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
using CollisionSystem.Utility;
using ItemSystem.Data;
using ObstacleSystem.Data;
using SceneSystem.Interface;
using TankSystem.Data;
using TankSystem.Interface;
using TankSystem.Service;

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
        // コンポーネント参照
        // ======================================================

        /// <summary>シーン上の戦車・障害物を一元管理するレジストリー</summary>
        private SceneObjectRegistry _sceneRegistry;

        /// <summary>OBB の衝突判定および MTV 計算を行う計算器</summary>
        private readonly BoundingBoxCollisionCalculator _boxCollisionCalculator = new BoundingBoxCollisionCalculator();

        /// <summary>MTV を用いた衝突解決量を計算するクラス</summary>
        private  CollisionResolveCalculator _collisionResolverCalculator;

        /// <summary>OBB を生成するファクトリー</summary>
        private readonly OBBFactory _obbFactory = new OBBFactory();

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
        // 辞書
        // ======================================================

        /// <summary>ItemSlot と対応する ItemCollisionContext を管理するマップ</summary>
        private readonly Dictionary<ItemSlot, ItemCollisionContext> _contextMap
            = new Dictionary<ItemSlot, ItemCollisionContext>();

        // ======================================================
        // セッター
        // ======================================================

        /// <summary>
        /// シーン内オブジェクト管理用のレジストリ参照を設定する
        /// </summary>
        /// <param name="sceneRegistry">シーンに存在する各種オブジェクト情報を一元管理するレジストリー</param>
        public void SetSceneRegistry(SceneObjectRegistry sceneRegistry)
        {
            _sceneRegistry = sceneRegistry;
        }

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

            InitializeItems(_sceneRegistry.ItemSlots);
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
        /// シーン開始時に存在するアイテムを衝突判定対象として初期化する
        /// </summary>
        /// <param name="items">初期登録対象となるアイテム一覧</param>
        public void InitializeItems(in List<ItemSlot> items)
        {
            if (_contextBuilder == null || _versusItemService == null || items == null)
            {
                return;
            }

            // 既存登録をすべて解除
            foreach (ItemCollisionContext context in _contextMap.Values)
            {
                _versusItemService.RemoveItemContext(context);
            }

            // 内部マップを初期化
            _contextMap.Clear();

            // 初期アイテムを登録
            foreach (ItemSlot item in items)
            {
                AddItem(item);
            }
        }

        /// <summary>
        /// アイテムを衝突判定対象として追加する
        /// </summary>
        /// <param name="item">追加対象となるアイテムスロット</param>
        public void AddItem(in ItemSlot item)
        {
            if (_contextBuilder == null || _versusItemService == null || item == null)
            {
                return;
            }

            // 既に登録済みの場合は何もしない
            if (_contextMap.ContainsKey(item))
            {
                return;
            }

            // アイテム用衝突コンテキストを生成
            ItemCollisionContext context =
                _contextBuilder.BuildItemContext(item);

            // 内部マップに追加
            _contextMap.Add(item, context);

            // 衝突サービス側にも追加
            _versusItemService.AddItemContext(context);
        }

        /// <summary>
        /// アイテムを衝突判定対象から削除する
        /// </summary>
        /// <param name="item">削除対象となるアイテムスロット</param>
        public void RemoveItem(in ItemSlot item)
        {
            if (_versusItemService == null || item == null)
            {
                return;
            }

            // 未登録の場合は何もしない
            if (_contextMap.TryGetValue(item, out ItemCollisionContext context) == false)
            {
                return;
            }

            // 内部マップから削除
            _contextMap.Remove(item);

            // 衝突サービス側からも削除
            _versusItemService.RemoveItemContext(context);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 指定された衝突判定サービスを 1 フェーズ分実行する
        /// </summary>
        /// <param name="service">実行対象の衝突判定サービス</param>
        /// <param name="phase">現在の衝突フェーズ</param>
        private void ExecuteCollisionService(in ITankCollisionService service, in CollisionPhase phase)
        {
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
            // --------------------------------------------------
            // 衝突解決計算
            // --------------------------------------------------
            _collisionResolverCalculator.CalculateResolveInfo(
                context,
                obstacle,
                context.TankRootManager.CurrentForwardSpeed,
                0f,
                out CollisionResolveInfo resolveInfoA,
                out CollisionResolveInfo resolveInfoB
            );

            // --------------------------------------------------
            // LockAxis 反映
            // --------------------------------------------------
            // この衝突によって発生したロック軸
            MovementLockAxis newLockAxis = MovementLockAxis.None;

            // X 方向に押し戻しが発生しているか
            if (Mathf.Abs(resolveInfoA.ResolveVector.x) > 0f)
            {
                newLockAxis |= MovementLockAxis.X;
            }

            // Z 方向に押し戻しが発生しているか
            if (Mathf.Abs(resolveInfoA.ResolveVector.z) > 0f)
            {
                newLockAxis |= MovementLockAxis.Z;
            }

            if (newLockAxis != MovementLockAxis.None)
            {
                // 両軸ロック時は All
                MovementLockAxis resolvedAxisA =
                    newLockAxis == (MovementLockAxis.X | MovementLockAxis.Z)
                        ? MovementLockAxis.All
                        : newLockAxis;

                // ロック軸の累積
                context.AddPendingLockAxis(resolvedAxisA);
            }

            // --------------------------------------------------
            // 押し戻し反映
            // --------------------------------------------------
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
            // --------------------------------------------------
            // 衝突解決計算
            // --------------------------------------------------
            _collisionResolverCalculator.CalculateResolveInfo(
                contextA,
                contextB,
                contextA.TankRootManager.CurrentForwardSpeed,
                contextB.TankRootManager.CurrentForwardSpeed,
                out CollisionResolveInfo resolveInfoA,
                out CollisionResolveInfo resolveInfoB
            );

            // --------------------------------------------------
            // 押し戻し反映
            // --------------------------------------------------
            contextA.TankRootManager.ApplyCollisionResolve(resolveInfoA);
            contextB.TankRootManager.ApplyCollisionResolve(resolveInfoB);

            // 押し戻した位置で OBB を更新
            contextA.UpdateOBB();
            contextB.UpdateOBB();
        }
    }
}