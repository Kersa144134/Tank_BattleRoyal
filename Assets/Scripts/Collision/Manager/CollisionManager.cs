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
using CollisionSystem.Interface;
using CollisionSystem.Service;
using CollisionSystem.Utility;
using ItemSystem.Data;
using ObstacleSystem.Data;
using SceneSystem.Interface;
using SceneSystem.Manager;
using TankSystem.Data;
using TankSystem.Manager;
using WeaponSystem.Data;

namespace CollisionSystem.Manager
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
        /// <summary>OBB の衝突判定を計算するクラス</summary>
        private readonly BoundingBoxCollisionCalculator _boxCollisionCalculator = new BoundingBoxCollisionCalculator();

        /// <summary>衝突解決量を計算するクラス</summary>
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

        /// <summary>CollisionContextFactory 参照</summary>
        private CollisionContextFactory _contextFactory;

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

        /// <summary>戦車コンテキストの OBB キャッシュ配列</summary>
        private IOBBData[] _tankOBBCache;

        /// <summary>円判定で重なった戦車コンテキストキャッシュリスト</summary>
        private readonly List<TankCollisionContext> _tankOverlapResults = new List<TankCollisionContext>(DEFAULT_OVERLAP_LIST_CAPACITY);

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>
        /// 戦車 ID と戦車コンテキストの対応を管理する辞書
        /// </summary>
        private readonly Dictionary<int, TankCollisionContext> _tankContextMap
            = new Dictionary<int, TankCollisionContext>();

        /// <summary>
        /// アイテムスロットとアイテムコンテキストの対応を管理する辞書
        /// </summary>
        private readonly Dictionary<ItemSlot, ItemCollisionContext> _itemContextMap
            = new Dictionary<ItemSlot, ItemCollisionContext>();

        /// <summary>
        /// 弾丸インスタンスと弾丸コンテキストの対応を管理する辞書
        /// </summary>
        private readonly Dictionary<BulletBase, BulletCollisionContext> _bulletContextMap
            = new Dictionary<BulletBase, BulletCollisionContext>();

        /// <summary>
        /// OBB インスタンスと戦車コンテキストの対応を管理する辞書
        /// </summary>
        private readonly Dictionary<IOBBData, TankCollisionContext> _obbToTankMap
            = new Dictionary<IOBBData, TankCollisionContext>();

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>衝突イベントの発火を担当するクラス</summary>
        public CollisionEventRouter EventRouter => _eventRouter;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>円重なり判定用 OBB リストの初期容量</summary>
        private const int DEFAULT_OVERLAP_LIST_CAPACITY = 16;

        // ======================================================
        // セッター
        // ======================================================

        /// <summary>
        /// シーン内オブジェクト管理用のレジストリー参照を設定する
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
            _contextFactory = new CollisionContextFactory(_obbFactory);
            _contextBuilder = new CollisionContextBuilder(_contextFactory, _sceneRegistry);
            _collisionResolverCalculator = new CollisionResolveCalculator(_boxCollisionCalculator);
            _eventRouter = new CollisionEventRouter(_collisionResolverCalculator);

            // --------------------------------------------------
            // コンテキスト構築
            // --------------------------------------------------
            // 戦車
            _tanks = _contextBuilder.BuildTankContexts();

            _tankContextMap.Clear();

            for (int i = 0; i < _tanks.Length; i++)
            {
                int tankId = _tanks[i].TankRootManager.TankId;

                // 重複防止
                if (!_tankContextMap.ContainsKey(tankId))
                {
                    _tankContextMap.Add(tankId, _tanks[i]);
                }
            }

            // 障害物
            _obstacles = _contextBuilder.BuildObstacleContexts();

            // --------------------------------------------------
            // 各衝突判定サービス生成
            // --------------------------------------------------
            // 戦車と障害物の OBB 衝突判定
            _tankVsObstacleService =
                new VersusStaticCollisionService<TankCollisionContext, ObstacleCollisionContext>(
                    _boxCollisionCalculator
                );
            // 戦車同士の OBB 衝突判定
            _tankVsTankService =
                new VersusDynamicCollisionService<TankCollisionContext, TankCollisionContext>(
                    _boxCollisionCalculator
                );
            // 戦車とアイテムの OBB 衝突判定
            _tankVsItemService =
                new VersusStaticCollisionService<TankCollisionContext, ItemCollisionContext>(
                    _boxCollisionCalculator
                );
            // 弾丸と障害物の OBB 衝突判定
            _bulletVsObstacleService =
                new VersusStaticCollisionService<BulletCollisionContext, ObstacleCollisionContext>(
                    _boxCollisionCalculator
                );
            // 弾丸と戦車の OBB 衝突判定
            _bulletVsTankService =
                new VersusDynamicCollisionService<BulletCollisionContext, TankCollisionContext>(
                    _boxCollisionCalculator
                );

            // 各衝突判定サービスを登録
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

            // コンテキストを各戦車に送る
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
        /// 指定 ID の戦車を衝突判定対象から削除する
        /// </summary>
        /// <param name="tankId">削除対象の TankID</param>
        public void UnregisterTank(in int tankId)
        {
            if (_tankContextMap.Count == 0)
            {
                return;
            }

            if (!_tankContextMap.ContainsKey(tankId))
            {
                return;
            }

            _tankContextMap.Remove(tankId);

            // 配列キャッシュ更新
            UpdateTanks();
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

        /// <summary>
        /// 指定円と水平面上で重なっている戦車コンテキストを取得
        /// </summary>
        /// <param name="center">円の中心位置</param>
        /// <param name="radius">円の半径</param>
        /// <returns>重なっている戦車コンテキストのリスト</returns>
        public List<TankCollisionContext> GetOverlappingTanksCircleHorizontal(
            in Vector3 center,
            in float radius
        )
        {
            _tankOverlapResults.Clear();

            UpdateTanks();
            
            // OBB 配列を渡して円と重なっている OBB を取得
            IOBBData[] overlappingOBBs = _boxCollisionCalculator.GetOverlappingOBBsCircleHorizontal(
                center,
                radius,
                _tankOBBCache
            );

            // 重なった OBB に対応する戦車コンテキストをリストに追加
            for (int i = 0; i < overlappingOBBs.Length; i++)
            {
                if (_obbToTankMap.TryGetValue(overlappingOBBs[i], out TankCollisionContext tank))
                {
                    _tankOverlapResults.Add(tank);
                }
            }

            return _tankOverlapResults;
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
        /// Dictionary の値をキャッシュ配列へ反映する共通処理
        /// </summary>
        /// <typeparam name="TKey">Dictionary のキー型</typeparam>
        /// <typeparam name="TValue">配列の要素型</typeparam>
        /// <param name="map">コピー元 Dictionary</param>
        /// <param name="cacheArray">更新対象のキャッシュ配列</param>
        private void UpdateCacheArray<TKey, TValue>(
            in Dictionary<TKey, TValue> map,
            ref TValue[] cacheArray
        )
        {
            // Dictionary に要素が存在しない場合空配列を代入
            if (map.Count == 0)
            {
                cacheArray = Array.Empty<TValue>();

                return;
            }

            // 配列が未生成または要素数が不一致の場合、必要な要素数で新規生成
            if (cacheArray == null || cacheArray.Length != map.Count)
            {
                cacheArray = new TValue[map.Count];
            }

            // Value を配列へコピー
            map.Values.CopyTo(cacheArray, 0);
        }

        /// <summary>
        /// Tank 用キャッシュ配列と OBB キャッシュを更新する
        /// </summary>
        private void UpdateTanks()
        {
            UpdateCacheArray(_tankContextMap, ref _tanks);

            // 辞書を再構築
            _obbToTankMap.Clear();

            // Tank が存在しない場合は OBB キャッシュも空にする
            if (_tanks.Length == 0)
            {
                _tankOBBCache = null;

                return;
            }

            // OBB キャッシュ配列が未生成、またはサイズ不一致の場合は再生成
            if (_tankOBBCache == null || _tankOBBCache.Length != _tanks.Length)
            {
                _tankOBBCache = new IOBBData[_tanks.Length];
            }

            // 各 TankCollisionContext から OBB をキャッシュへ反映
            for (int i = 0; i < _tanks.Length; i++)
            {
                TankCollisionContext tank = _tanks[i];

                IOBBData obb = tank.OBB;

                _tankOBBCache[i] = obb;

                // 辞書に登録
                if (obb != null)
                {
                    _obbToTankMap[obb] = tank;
                }
            }
        }

        /// <summary>
        /// アイテム用キャッシュ配列を更新する
        /// </summary>
        private void UpdateItems()
        {
            UpdateCacheArray(_itemContextMap, ref _items);
        }

        /// <summary>
        /// 弾丸用キャッシュ配列を更新する
        /// </summary>
        private void UpdateBullets()
        {
            UpdateCacheArray(_bulletContextMap, ref _bullets);
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

            // PreUpdate にコンテキストを渡す
            switch (service)
            {
                case VersusStaticCollisionService<TankCollisionContext, ObstacleCollisionContext> collisionService:
                    collisionService.PreUpdate(
                        contextsA as TankCollisionContext[],
                        contextsB as ObstacleCollisionContext[]
                    );
                    break;

                case VersusStaticCollisionService<TankCollisionContext, ItemCollisionContext> collisionService:
                    collisionService.PreUpdate(
                        contextsA as TankCollisionContext[],
                        contextsB as ItemCollisionContext[]
                    );
                    break;

                case VersusStaticCollisionService<BulletCollisionContext, ObstacleCollisionContext> collisionService:
                    collisionService.PreUpdate(
                        contextsA as BulletCollisionContext[],
                        contextsB as ObstacleCollisionContext[]
                    );
                    break;

                case VersusDynamicCollisionService<TankCollisionContext, TankCollisionContext> collisionService:
                    collisionService.PreUpdate(
                        contextsA as TankCollisionContext[],
                        contextsB as TankCollisionContext[]
                    );
                    break;

                case VersusDynamicCollisionService<BulletCollisionContext, TankCollisionContext> collisionService:
                    collisionService.PreUpdate(
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