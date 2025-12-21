// ======================================================
// CollisionManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-21
// 概要     : 各種戦車衝突判定サービスを統括管理するマネージャー
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using CollisionSystem.Calculator;
using CollisionSystem.Data;
using CollisionSystem.Service;
using CollisionSystem.Interface;
using CollisionSystem.Utility;
using ItemSystem.Data;
using ObstacleSystem.Data;
using SceneSystem.Interface;
using TankSystem.Data;
using TankSystem.Service;
using WeaponSystem.Data;

namespace TankSystem.Manager
{
    /// <summary>
    /// 戦車・障害物・アイテムの衝突判定を一元管理するマネージャー
    /// 各衝突タイプごとに Service を分離し、実行順を制御する
    /// </summary>
    public sealed class CollisionManager : MonoBehaviour, IUpdatable
    {
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
        private readonly List<ICollisionService> _collisionServices = new List<ICollisionService>();

        // --------------------------------------------------
        // 静的衝突判定サービス
        // --------------------------------------------------
        /// <summary>戦車と障害物の OBB 衝突判定を担当するサービス</summary>
        private VersusStaticCollisionService<TankCollisionContext, ObstacleCollisionContext> _tankVsObstacleService;

        /// <summary>戦車とアイテムの OBB 衝突判定を担当するサービス</summary>
        private VersusStaticCollisionService<TankCollisionContext, ItemCollisionContext> _tankVsItemService;

        /// <summary>弾丸と障害物の OBB 衝突判定を担当するサービス</summary>
        private VersusStaticCollisionService<BulletCollisionContext, ObstacleCollisionContext> _bulletVsObstacleService;

        // --------------------------------------------------
        // 動的衝突判定サービス
        // --------------------------------------------------
        /// <summary>戦車同士の OBB 衝突判定を担当するサービス</summary>
        private VersusDynamicCollisionService<TankCollisionContext, TankCollisionContext> _tankVsTankService;

        /// <summary>弾丸と戦車の OBB 衝突判定を担当するサービス</summary>
        private VersusDynamicCollisionService<BulletCollisionContext, TankCollisionContext> _bulletVsTankService;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// 衝突判定対象となる戦車コンテキスト配列
        /// </summary>
        private TankCollisionContext[] _tanks;

        /// <summary>
        /// 衝突判定対象となる障害物コンテキスト配列
        /// </summary>
        private ObstacleCollisionContext[] _obstacles;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>
        /// Bullet と対応する BulletCollisionContext を管理するマップ
        /// </summary>
        private readonly Dictionary<BulletBase, BulletCollisionContext> _bulletContextMap
            = new Dictionary<BulletBase, BulletCollisionContext>();
        
        /// <summary>ItemSlot と対応する ItemCollisionContext を管理するマップ</summary>
        private readonly Dictionary<ItemSlot, ItemCollisionContext> _itemContextMap
            = new Dictionary<ItemSlot, ItemCollisionContext>();

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>戦車がアイテムを取得した際に通知されるイベント</summary>
        public event Action<BaseTankRootManager, ItemSlot> OnItemGet;

        /// <summary>弾丸が衝突した際に通知されるイベント</summary>
        public event Action<BulletBase> OnBulletHit;

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
            // 戦車・障害物 コンテキスト構築
            // --------------------------------------------------
            _tanks = _contextBuilder.BuildTankContexts();
            _obstacles = _contextBuilder.BuildObstacleContexts();

            // --------------------------------------------------
            // 各衝突判定サービス生成
            // --------------------------------------------------
            // 戦車と障害物の OBB 衝突判定サービスを生成
            _tankVsObstacleService =
                new VersusStaticCollisionService<TankCollisionContext, ObstacleCollisionContext>(
                    _boxCollisionCalculator
                );
            // 戦車同士の OBB 衝突判定サービスを生成
            _tankVsTankService =
                new VersusDynamicCollisionService<TankCollisionContext, TankCollisionContext>(
                    _boxCollisionCalculator
                );
            // 戦車とアイテムの OBB 衝突判定サービスを生成
            _tankVsItemService =
                new VersusStaticCollisionService<TankCollisionContext, ItemCollisionContext>(
                    _boxCollisionCalculator
                );
            // 弾丸と障害物の OBB 衝突判定サービスを生成
            _bulletVsObstacleService =
                new VersusStaticCollisionService<BulletCollisionContext, ObstacleCollisionContext>(
                    _boxCollisionCalculator
                );
            // 弾丸と戦車の OBB 衝突判定サービスを生成
            _bulletVsTankService =
                new VersusDynamicCollisionService<BulletCollisionContext, TankCollisionContext>(
                    _boxCollisionCalculator
                );

            // --------------------------------------------------
            // 各衝突判定サービスを登録
            // --------------------------------------------------
            _collisionServices.Add(_tankVsObstacleService);
            _collisionServices.Add(_tankVsTankService);
            _collisionServices.Add(_tankVsItemService);
            _collisionServices.Add(_bulletVsObstacleService);
            _collisionServices.Add(_bulletVsTankService);

            // --------------------------------------------------
            // イベント購読
            // --------------------------------------------------
            if (_tankVsObstacleService != null)
            {
                _tankVsObstacleService.OnStaticHit += HandleTankHitObstacle;
            }
            if (_tankVsTankService != null)
            {
                _tankVsTankService.OnDynamicHit += HandleTankHitTank;
            }
            if (_tankVsItemService != null)
            {
                _tankVsItemService.OnStaticHit += HandleTankHitItem;
            }
            if (_bulletVsObstacleService != null)
            {
                _bulletVsObstacleService.OnStaticHit += HandleBulletHitObstacle;
            }
            if (_bulletVsTankService != null)
            {
                _bulletVsTankService.OnDynamicHit += HandleBulletHitTank;
            }

            InitializeItems(_sceneRegistry.ItemSlots);
        }

        public void OnUpdate()
        {
            // --------------------------------------------------
            // 初期化
            // --------------------------------------------------
            for (int i = 0; i < _tanks.Length; i++)
            {
                _tanks[i].BeginFrame();
            }

            // --------------------------------------------------
            // 障害物
            // --------------------------------------------------
            ExecuteCollisionService(_tankVsObstacleService, _tanks, _obstacles);

            // 障害物由来の LockAxis を確定
            for (int i = 0; i < _tanks.Length; i++)
            {
                _tanks[i].FinalizeLockAxis();
            }

            // --------------------------------------------------
            // 戦車
            // --------------------------------------------------
            ExecuteCollisionService(_tankVsTankService, _tanks);

            // 戦車衝突解決後に障害物衝突チェック
            ExecuteCollisionService(_tankVsObstacleService, _tanks, _obstacles);

            // 再度 LockAxis を確定
            for (int i = 0; i < _tanks.Length; i++)
            {
                _tanks[i].FinalizeLockAxis();
            }

            // 再び戦車衝突チェック
            ExecuteCollisionService(_tankVsTankService, _tanks);

            // --------------------------------------------------
            // アイテム
            // --------------------------------------------------
            // ItemCollisionContext 配列を生成して Dictionary からコピー
            ItemCollisionContext[] items = new ItemCollisionContext[_itemContextMap.Count];
            _itemContextMap.Values.CopyTo(items, 0);

            ExecuteCollisionService(_tankVsItemService, _tanks, items);

            // --------------------------------------------------
            // 弾丸
            // --------------------------------------------------
            // BulletCollisionContext 配列を生成して Dictionary からコピー
            BulletCollisionContext[] bullets = new BulletCollisionContext[_bulletContextMap.Count];
            _bulletContextMap.Values.CopyTo(bullets, 0);

            ExecuteCollisionService(_bulletVsObstacleService, bullets, _obstacles);
            ExecuteCollisionService(_bulletVsTankService, bullets, _tanks);
        }

        public void OnExit()
        {
            // --------------------------------------------------
            // イベント購読解除
            // --------------------------------------------------
            if (_tankVsObstacleService != null)
            {
                _tankVsObstacleService.OnStaticHit += HandleTankHitObstacle;
            }
            if (_tankVsTankService != null)
            {
                _tankVsTankService.OnDynamicHit += HandleTankHitTank;
            }
            if (_tankVsItemService != null)
            {
                _tankVsItemService.OnStaticHit += HandleTankHitItem;
            }
            if (_bulletVsObstacleService != null)
            {
                _bulletVsObstacleService.OnStaticHit += HandleBulletHitObstacle;
            }
            if (_bulletVsTankService != null)
            {
                _bulletVsTankService.OnDynamicHit += HandleBulletHitTank;
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 弾丸を衝突判定対象として追加する
        /// </summary>
        /// <param name="bullet">追加対象となる弾丸ロジック</param>
        public void RegisterBullet(in BulletBase bullet)
        {
            if (_contextBuilder == null || bullet == null)
            {
                return;
            }

            // 既に登録済みの場合は何もしない
            if (_bulletContextMap.ContainsKey(bullet))
            {
                return;
            }

            // 弾丸用衝突コンテキストを生成
            BulletCollisionContext context =
                _contextBuilder.BuildBulletContext(bullet);

            // 内部マップに追加
            _bulletContextMap.Add(bullet, context);
        }

        /// <summary>
        /// 弾丸を衝突判定対象から削除する
        /// </summary>
        /// <param name="bullet">削除対象となる弾丸ロジック</param>
        public void UnregisterBullet(in BulletBase bullet)
        {
            if (bullet == null)
            {
                return;
            }

            // 未登録の場合は何もしない
            if (_bulletContextMap.TryGetValue(
                bullet,
                out BulletCollisionContext context
            ) == false)
            {
                return;
            }

            // 内部マップから削除
            _bulletContextMap.Remove(bullet);
        }

        /// <summary>
        /// アイテムを衝突判定対象として追加する
        /// </summary>
        /// <param name="item">追加対象となるアイテムスロット</param>
        public void RegisterItem(in ItemSlot item)
        {
            if (_contextBuilder == null || item == null)
            {
                return;
            }

            // 既に登録済みの場合は何もしない
            if (_itemContextMap.ContainsKey(item))
            {
                return;
            }

            // アイテム用衝突コンテキストを生成
            ItemCollisionContext context =
                _contextBuilder.BuildItemContext(item);

            // 内部マップに追加
            _itemContextMap.Add(item, context);
        }

        /// <summary>
        /// アイテムを衝突判定対象から削除する
        /// </summary>
        /// <param name="item">削除対象となるアイテムスロット</param>
        public void UnregisterItem(in ItemSlot item)
        {
            if (item == null)
            {
                return;
            }

            // 未登録の場合は何もしない
            if (_itemContextMap.TryGetValue(item, out ItemCollisionContext context) == false)
            {
                return;
            }

            // 内部マップから削除
            _itemContextMap.Remove(item);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// シーン開始時に存在するアイテムを衝突判定対象として初期化する
        /// </summary>
        /// <param name="items">初期登録対象となるアイテム一覧</param>
        private void InitializeItems(in List<ItemSlot> items)
        {
            if (_contextBuilder == null || items == null)
            {
                return;
            }

            // 内部マップを初期化
            _itemContextMap.Clear();

            // 初期アイテムを登録
            foreach (ItemSlot item in items)
            {
                RegisterItem(item);
            }
        }

        /// <summary>
        /// 指定された衝突判定サービスを 1 フェーズ分実行する
        /// </summary>
        /// <param name="service">実行対象の衝突判定サービス</param>
        /// <param name="contextsA">コンテキストA配列</param>
        /// <param name="contextsB">コンテキストB配列</param>
        private void ExecuteCollisionService(
            ICollisionService service,
            BaseCollisionContext[] contextsA,
            BaseCollisionContext[] contextsB = null
        )
        {
            if (service == null)
            {
                return;
            }

            // --------------------------------------------------
            // 型ごとにキャストして PreUpdate にコンテキストを渡す
            // --------------------------------------------------
            switch (service)
            {
                case VersusStaticCollisionService<TankCollisionContext, ObstacleCollisionContext> s:
                    s.PreUpdate(
                        contextsA as TankCollisionContext[],
                        contextsB as ObstacleCollisionContext[]
                    );
                    break;

                case VersusStaticCollisionService<TankCollisionContext, ItemCollisionContext> s:
                    s.PreUpdate(
                        contextsA as TankCollisionContext[],
                        contextsB as ItemCollisionContext[]
                    );
                    break;

                case VersusStaticCollisionService<BulletCollisionContext, ObstacleCollisionContext> s:
                    s.PreUpdate(
                        contextsA as BulletCollisionContext[],
                        contextsB as ObstacleCollisionContext[]
                    );
                    break;

                case VersusDynamicCollisionService<TankCollisionContext, TankCollisionContext> s:
                    s.PreUpdate(
                        contextsA as TankCollisionContext[],
                        contextsB as TankCollisionContext[]
                    );
                    break;

                case VersusDynamicCollisionService<BulletCollisionContext, TankCollisionContext> s:
                    s.PreUpdate(
                        contextsA as BulletCollisionContext[],
                        contextsB as TankCollisionContext[]
                    );
                    break;

                default:
                    break;
            }

            // 衝突判定および解決処理を実行
            service.Execute();
        }


        // ======================================================
        // イベントハンドラ
        // ======================================================

        // --------------------------------------------------
        // 戦車
        // --------------------------------------------------
        /// <summary>
        /// 戦車が障害物に衝突した際の処理
        /// </summary>
        private void HandleTankHitObstacle(
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
        /// 戦車がアイテムに接触した際の処理
        /// </summary>
        private void HandleTankHitItem(
            TankCollisionContext context,
            ItemCollisionContext item
        )
        {
            BaseTankRootManager tankRootManager = context.TankRootManager;
            ItemSlot itemSlot = item.ItemSlot;

            // アイテム取得イベントを通知
            OnItemGet?.Invoke(tankRootManager, itemSlot);
        }

        /// <summary>
        /// 戦車同士が接触した際の処理
        /// </summary>
        private void HandleTankHitTank(
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

        // --------------------------------------------------
        // 弾丸
        // --------------------------------------------------
        /// <summary>
        /// 弾丸が障害物に接触した際の処理
        /// </summary>
        private void HandleBulletHitObstacle(
            BulletCollisionContext bulletContext,
            ObstacleCollisionContext obstacle
        )
        {
            BulletBase bullet = bulletContext.Bullet;

            // 弾丸ロジック側に衝突を通知
            bullet.OnExit();

            // 弾丸衝突イベントを通知
            OnBulletHit?.Invoke(bullet);
        }

        /// <summary>
        /// 弾丸が戦車に接触した際の処理
        /// </summary>
        private void HandleBulletHitTank(
            BulletCollisionContext bulletContext,
            TankCollisionContext tank
        )
        {
            BulletBase bullet = bulletContext.Bullet;

            // 発射元戦車なら処理をスキップ
            if (bullet.BulletId == tank.TankRootManager.TankId)
            {
                return;
            }

            // 弾丸ロジック側に衝突を通知
            bullet.OnExit();

            // 弾丸衝突イベントを通知
            OnBulletHit?.Invoke(bullet);
        }
    }
}