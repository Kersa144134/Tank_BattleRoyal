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
using SceneSystem.Manager;
using TankSystem.Data;
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

        // --------------------------------------------------
        // Transform 関連
        // --------------------------------------------------
        /// <summary>シーン上オブジェクトの Transform を一元管理するレジストリー</summary>
        private SceneObjectRegistry _sceneRegistry;

        // --------------------------------------------------
        // 計算関連
        // --------------------------------------------------
        /// <summary>OBB の衝突判定および MTV を計算するクラス</summary>
        private readonly BoundingBoxCollisionCalculator _boxCollisionCalculator = new BoundingBoxCollisionCalculator();

        /// <summary>MTV を用いた衝突解決量を計算するクラス</summary>
        private  CollisionResolveCalculator _collisionResolverCalculator;

        // --------------------------------------------------
        // 衝突イベント関連
        // --------------------------------------------------
        /// <summary>衝突イベントの発火を担当するクラス</summary>
        private CollisionEventRouter _eventRouter;

        // --------------------------------------------------
        // コンテキスト関連
        // --------------------------------------------------
        /// <summary>OBB を生成するファクトリー</summary>
        private readonly OBBFactory _obbFactory = new OBBFactory();

        /// <summary>戦車衝突用コンテキストを生成するビルダー</summary>
        private CollisionContextBuilder _contextBuilder;

        // --------------------------------------------------
        // 衝突判定サービス関連
        // --------------------------------------------------
        /// <summary>衝突判定サービス一覧</summary>
        private readonly List<ICollisionService> _collisionServices = new List<ICollisionService>();

        /// <summary>戦車と障害物の OBB 衝突判定を担当するサービス</summary>
        private VersusStaticCollisionService<TankCollisionContext, ObstacleCollisionContext> _tankVsObstacleService;

        /// <summary>戦車とアイテムの OBB 衝突判定を担当するサービス</summary>
        private VersusStaticCollisionService<TankCollisionContext, ItemCollisionContext> _tankVsItemService;

        /// <summary>弾丸と障害物の OBB 衝突判定を担当するサービス</summary>
        private VersusStaticCollisionService<BulletCollisionContext, ObstacleCollisionContext> _bulletVsObstacleService;

        /// <summary>戦車同士の OBB 衝突判定を担当するサービス</summary>
        private VersusDynamicCollisionService<TankCollisionContext, TankCollisionContext> _tankVsTankService;

        /// <summary>弾丸と戦車の OBB 衝突判定を担当するサービス</summary>
        private VersusDynamicCollisionService<BulletCollisionContext, TankCollisionContext> _bulletVsTankService;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>戦車コンテキスト配列</summary>
        private TankCollisionContext[] _tanks;

        /// <summary>障害物コンテキスト配列</summary>
        private ObstacleCollisionContext[] _obstacles;

        /// <summary>アイテムコンテキストの配列</summary>
        private ItemCollisionContext[] _items;

        /// <summary>弾丸コンテキストの配列</summary>
        private BulletCollisionContext[] _bullets;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>ItemSlot と対応する ItemCollisionContext を管理するマップ</summary>
        private readonly Dictionary<ItemSlot, ItemCollisionContext> _itemContextMap
            = new Dictionary<ItemSlot, ItemCollisionContext>();

        /// <summary>
        /// Bullet と対応する BulletCollisionContext を管理するマップ
        /// </summary>
        private readonly Dictionary<BulletBase, BulletCollisionContext> _bulletContextMap
            = new Dictionary<BulletBase, BulletCollisionContext>();

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>衝突イベントの発火を担当するクラス</summary>
        public CollisionEventRouter EventRouter => _eventRouter;

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
        // IUpdatable イベント
        // ======================================================

        public void OnEnter()
        {
            // --------------------------------------------------
            // クラス生成
            // --------------------------------------------------
            _contextBuilder = new CollisionContextBuilder(_sceneRegistry, _obbFactory);
            _collisionResolverCalculator = new CollisionResolveCalculator(_boxCollisionCalculator);
            _eventRouter = new CollisionEventRouter(_collisionResolverCalculator);

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
            _tankVsObstacleService.OnStaticHit += _eventRouter.HandleTankHitObstacle;
            _tankVsTankService.OnDynamicHit += _eventRouter.HandleTankHitTank;
            _tankVsItemService.OnStaticHit += _eventRouter.HandleTankHitItem;
            _bulletVsObstacleService.OnStaticHit += _eventRouter.HandleBulletHitObstacle;
            _bulletVsTankService.OnDynamicHit += _eventRouter.HandleBulletHitTank;

            SendContextData();
        }

        public void OnUpdate(in float unscaledDeltaTime, in float elapsedTime)
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
            ExecuteCollisionService(_tankVsTankService, _tanks, _tanks);

            // 戦車衝突解決後に障害物衝突チェック
            ExecuteCollisionService(_tankVsObstacleService, _tanks, _obstacles);

            // 再度 LockAxis を確定
            for (int i = 0; i < _tanks.Length; i++)
            {
                _tanks[i].FinalizeLockAxis();
            }

            // 再び戦車衝突チェック
            ExecuteCollisionService(_tankVsTankService, _tanks, _tanks);

            // --------------------------------------------------
            // アイテム
            // --------------------------------------------------
            ExecuteCollisionService(_tankVsItemService, _tanks, _items);

            // --------------------------------------------------
            // 弾丸
            // --------------------------------------------------
            ExecuteCollisionService(_bulletVsObstacleService, _bullets, _obstacles);
            ExecuteCollisionService(_bulletVsTankService, _bullets, _tanks);
        }

        public void OnExit()
        {
            // --------------------------------------------------
            // イベント購読解除
            // --------------------------------------------------
            _tankVsObstacleService.OnStaticHit -= _eventRouter.HandleTankHitObstacle;
            _tankVsTankService.OnDynamicHit -= _eventRouter.HandleTankHitTank;
            _tankVsItemService.OnStaticHit -= _eventRouter.HandleTankHitItem;
            _bulletVsObstacleService.OnStaticHit -= _eventRouter.HandleBulletHitObstacle;
            _bulletVsTankService.OnDynamicHit -= _eventRouter.HandleBulletHitTank;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

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

            // 既に登録済みの場合は処理しない
            if (_itemContextMap.ContainsKey(item))
            {
                return;
            }

            // アイテム用の衝突コンテキストを生成
            ItemCollisionContext context = _contextBuilder.BuildItemContext(item);

            // 内部マップに追加
            _itemContextMap.Add(item, context);

            // キャッシュ配列を更新
            UpdateItems();
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

            // 未登録の場合は処理しない
            if (!_itemContextMap.ContainsKey(item))
            {
                return;
            }

            // 内部マップから削除
            _itemContextMap.Remove(item);

            // キャッシュ配列を更新
            UpdateItems();
        }

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

            // 既に登録済みの場合は処理しない
            if (_bulletContextMap.ContainsKey(bullet))
            {
                return;
            }

            // 弾丸用衝突コンテキストを生成
            BulletCollisionContext context = _contextBuilder.BuildBulletContext(bullet);

            // 内部マップに追加
            _bulletContextMap.Add(bullet, context);

            // キャッシュ配列を更新
            UpdateBullets();
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

            // 未登録の場合は処理しない
            if (!_bulletContextMap.ContainsKey(bullet))
            {
                return;
            }

            // 内部マップから削除
            _bulletContextMap.Remove(bullet);

            // ヒット履歴を削除
            _eventRouter.ClearHitHistory(bullet);

            // キャッシュ配列を更新
            UpdateBullets();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 戦車の Transform 配列と障害物の OBB 配列を
        /// 各戦車マネージャーにセットする
        /// </summary>
        private void SendContextData()
        {
            if (_tanks == null || _obstacles == null)
            {
                return;
            }

            // 戦車ごとに Transform と OBB 配列をセット
            Transform[] tankTransforms = new Transform[_tanks.Length];
            IOBBData[] tankOBBs = new IOBBData[_tanks.Length];

            for (int i = 0; i < _tanks.Length; i++)
            {
                TankCollisionContext tank = _tanks[i];
                tankTransforms[i] = tank.TankRootManager.Transform;
            }

            // 障害物 Transform と OBB 配列をキャッシュ
            Transform[] obstacleTransforms = new Transform[_obstacles.Length];
            IOBBData[] obstacleOBBs = new IOBBData[_obstacles.Length];
            for (int i = 0; i < _obstacles.Length; i++)
            {
                ObstacleCollisionContext obstacle = _obstacles[i];
                obstacleOBBs[i] = obstacle.OBB;
            }

            // 各戦車マネージャーにセット
            for (int i = 0; i < _tanks.Length; i++)
            {
                BaseTankRootManager tankManager = _tanks[i].TankRootManager;
                if (tankManager != null)
                {
                    tankManager.SetContextData(
                        tankTransforms,
                        obstacleOBBs
                    );
                }
            }
        }

        /// <summary>
        /// アイテム用キャッシュ配列を更新する
        /// </summary>
        private void UpdateItems()
        {
            if (_itemContextMap.Count == 0)
            {
                // アイテムがない場合は空配列を割り当て
                _items = Array.Empty<ItemCollisionContext>();
            }
            else
            {
                // 配列サイズが不足している場合は再生成
                if (_items == null || _items.Length != _itemContextMap.Count)
                {
                    _items = new ItemCollisionContext[_itemContextMap.Count];
                }

                // Dictionary の値をキャッシュ配列にコピー
                _itemContextMap.Values.CopyTo(_items, 0);
            }
        }

        /// <summary>
        /// 弾丸用キャッシュ配列を更新する
        /// </summary>
        private void UpdateBullets()
        {
            if (_bulletContextMap.Count == 0)
            {
                // 弾丸がない場合は空配列を割り当て
                _bullets = Array.Empty<BulletCollisionContext>();
            }
            else
            {
                // 配列サイズが不足している場合は再生成
                if (_bullets == null || _bullets.Length != _bulletContextMap.Count)
                {
                    _bullets = new BulletCollisionContext[_bulletContextMap.Count];
                }

                // Dictionary の値をキャッシュ配列にコピー
                _bulletContextMap.Values.CopyTo(_bullets, 0);
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
    }
}