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

        /// <summary>戦車 Transform キャッシュ配列</summary>
        private Transform[] _tankTransformsCache;

        /// <summary>アイテム Transform  キャッシュ配列</summary>
        private Transform[] _itemTransformsCache;

        /// <summary>障害物 OBB キャッシュ配列</summary>
        private IOBBData[] _obstacleOBBsCache;

        /// <summary>円判定用統合 OBB キャッシュ配列</summary>
        private IOBBData[] _circleQueryOBBCache;

        /// <summary>円判定で重なったコンテキストキャッシュリスト</summary>
        private readonly List<BaseCollisionContext> _overlapContextResults = new List<BaseCollisionContext>(DEFAULT_OVERLAP_LIST_CAPACITY);

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>
        /// 戦車 ID と戦車コンテキストの対応を管理する辞書
        /// </summary>
        private readonly Dictionary<int, TankCollisionContext> _tankContextMap
            = new Dictionary<int, TankCollisionContext>();

        /// <summary>
        /// 障害物 Transform と障害物コンテキストの対応を管理する辞書
        /// </summary>
        private readonly Dictionary<Transform, ObstacleCollisionContext> _obstacleContextMap
            = new Dictionary<Transform, ObstacleCollisionContext>();

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
        /// OBB と戦車コンテキストの対応を管理する辞書
        /// </summary>
        private readonly Dictionary<IOBBData, TankCollisionContext> _obbToTankMap
            = new Dictionary<IOBBData, TankCollisionContext>();

        /// <summary>
        /// OBB と障害物コンテキストの対応を管理する辞書
        /// </summary>
        private readonly Dictionary<IOBBData, ObstacleCollisionContext> _obbToObstacleMap
            = new Dictionary<IOBBData, ObstacleCollisionContext>();

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>衝突イベントの発火を担当するクラス</summary>
        public CollisionEventRouter EventRouter => _eventRouter;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>円重なり判定用 OBB リストの初期容量</summary>
        private const int DEFAULT_OVERLAP_LIST_CAPACITY = 32;

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
            _contextFactory = new CollisionContextFactory(_obbFactory, _sceneRegistry);
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

            _obstacleContextMap.Clear();

            for (int i = 0; i < _obstacles.Length; i++)
            {
                ObstacleCollisionContext context = _obstacles[i];

                if (context != null && context.Transform != null)
                {
                    _obstacleContextMap[context.Transform] = context;
                }
            }

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

            // 障害物による LockAxis を確定
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
            // キャッシュ更新
            // --------------------------------------------------
            // UpdateTanks();
            // UpdateObstacles();

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
            UnregisterInternal(
                tankId,
                _tankContextMap,
                UpdateTanks
            );

            SendContextData();
        }

        /// <summary>
        /// 障害物を衝突判定対象から削除する
        /// </summary>
        /// <param name="obstacle">削除対象となる障害物</param>
        public void UnregisterObstacle(in Transform obstacle)
        {
            UnregisterInternal(
                obstacle,
                _obstacleContextMap,
                UpdateObstacles
            );

            SendContextData();
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

            // コンテキスト生成
            ItemCollisionContext context
                = _contextBuilder.BuildItemContext(item);

            RegisterInternal(
                item,
                context,
                _itemContextMap,
                UpdateItems
            );

            SendContextData();
        }

        /// <summary>
        /// アイテムを衝突判定対象から削除する
        /// </summary>
        /// <param name="item">削除対象となるアイテムスロット</param>
        public void UnregisterItem(in ItemSlot item)
        {
            UnregisterInternal(
                item,
                _itemContextMap,
                UpdateItems
            );

            SendContextData();
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

            // コンテキスト生成
            BulletCollisionContext context
                = _contextBuilder.BuildBulletContext(bullet);

            RegisterInternal(
                bullet,
                context,
                _bulletContextMap,
                UpdateBullets
            );
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

            // ヒット履歴を削除
            _eventRouter.ClearHitHistory(bullet);

            UnregisterInternal(
                bullet,
                _bulletContextMap,
                UpdateBullets
            );
        }

        /// <summary>
        /// 指定円と水平面上で重なっているコンテキストを取得
        /// </summary>
        /// <param name="center">円の中心位置</param>
        /// <param name="radius">円の半径</param>
        /// <returns>重なっているコンテキストのリスト</returns>
        public List<BaseCollisionContext> GetOverlappingCircleHorizontal(
            in Vector3 center,
            in float radius
        )
        {
            _overlapContextResults.Clear();

            // 戦車キャッシュ更新
            UpdateTanks();

            // 障害物キャッシュ更新
            UpdateObstacles();

            // 統合OBBキャッシュ再構築
            RebuildCircleQueryOBBCache();

            // 円と重なっている OBB を取得
            IOBBData[] overlappingOBBs =
                _boxCollisionCalculator.GetOverlappingOBBsCircleHorizontal(
                    center,
                    radius,
                    _circleQueryOBBCache
                );

            // OBB → Context 変換（戦車と障害物の両方を参照）
            for (int i = 0; i < overlappingOBBs.Length; i++)
            {
                IOBBData obb = overlappingOBBs[i];

                // 戦車マップ優先
                if (_obbToTankMap.TryGetValue(obb, out TankCollisionContext tankContext))
                {
                    _overlapContextResults.Add(tankContext);
                    continue;
                }

                // 戦車が無ければ障害物マップ
                if (_obbToObstacleMap.TryGetValue(obb, out ObstacleCollisionContext obstacleContext))
                {
                    _overlapContextResults.Add(obstacleContext);
                }
            }

            return _overlapContextResults;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 戦車とアイテムの Transform 配列と障害物の OBB 配列を
        /// 各戦車マネージャーにセットする
        /// </summary>
        private void SendContextData()
        {
            if (_tanks == null || _obstacles == null || _items == null)
            {
                return;
            }

            // 戦車 Transform 配列
            if (_tankTransformsCache == null || _tankTransformsCache.Length != _tanks.Length)
            {
                _tankTransformsCache = new Transform[_tanks.Length];
            }

            for (int i = 0; i < _tanks.Length; i++)
            {
                _tankTransformsCache[i] = _tanks[i].TankRootManager.Transform;
            }

            // アイテム Transform 配列
            if (_itemTransformsCache == null || _itemTransformsCache.Length != _items.Length)
            {
                _itemTransformsCache = new Transform[_items.Length];
            }

            for (int i = 0; i < _items.Length; i++)
            {
                _itemTransformsCache[i] = _items[i].Transform;
            }

            // 障害物 OBB 配列
            if (_obstacleOBBsCache == null || _obstacleOBBsCache.Length != _obstacles.Length)
            {
                _obstacleOBBsCache = new IOBBData[_obstacles.Length];
            }

            for (int i = 0; i < _obstacles.Length; i++)
            {
                _obstacleOBBsCache[i] = _obstacles[i].OBB;
            }

            // 各戦車へセット
            for (int i = 0; i < _tanks.Length; i++)
            {
                BaseTankRootManager tankManager = _tanks[i].TankRootManager;
                if (tankManager == null) continue;

                tankManager.SetTargetData(_tankTransformsCache, _itemTransformsCache);
                tankManager.SetObstacleData(_obstacleOBBsCache);
            }
        }

        /// <summary>
        /// 衝突コンテキストの汎用登録処理
        /// </summary>
        /// <typeparam name="TKey">辞書のキー型</typeparam>
        /// <typeparam name="TContext">衝突コンテキストの型</typeparam>
        /// <param name="key">登録対象となるキー</param>
        /// <param name="context">登録する衝突コンテキスト</param>
        /// <param name="map">キーとコンテキストを管理する辞書</param>
        /// <param name="updateCache">登録成功時に実行するキャッシュ更新処理</param>
        private void RegisterInternal<TKey, TContext>(
            in TKey key,
            in TContext context,
            in Dictionary<TKey, TContext> map,
            in Action updateCache
        )
        {
            // 既定値チェック
            if (EqualityComparer<TKey>.Default.Equals(key, default))
            {
                return;
            }

            if (context == null)
            {
                return;
            }

            // 既に登録済みの場合は処理なし
            if (!map.TryAdd(key, context))
            {
                return;
            }

            // キャッシュ更新
            updateCache?.Invoke();
        }

        /// <summary>
        /// 衝突コンテキストの汎用削除処理
        /// </summary>
        /// <typeparam name="TKey">辞書のキー型</typeparam>
        /// <typeparam name="TContext">衝突コンテキストの型</typeparam>
        /// <param name="key">削除対象となるキー</param>
        /// <param name="map">キーとコンテキストを管理する辞書</param>
        /// <param name="updateCache">削除成功時に実行するキャッシュ更新処理</param>
        private void UnregisterInternal<TKey, TContext>(
            in TKey key,
            in Dictionary<TKey, TContext> map,
            in Action updateCache
        )
        {
            // 既定値チェック
            if (EqualityComparer<TKey>.Default.Equals(key, default))
            {
                return;
            }

            // 存在しない場合は処理なし
            if (!map.Remove(key))
            {
                return;
            }

            // キャッシュ更新
            updateCache?.Invoke();
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
        /// Tank 用キャッシュ配列を更新する
        /// </summary>
        private void UpdateTanks()
        {
            UpdateCacheArray(_tankContextMap, ref _tanks);

            if (_tanks == null || _tanks.Length == 0)
            {
                return;
            }

            for (int i = 0; i < _tanks.Length; i++)
            {
                IOBBData obb = _tanks[i].OBB;

                if (obb != null)
                {
                    _obbToTankMap[obb] = _tanks[i];
                }
            }
        }

        /// <summary>
        /// 障害物用キャッシュ配列を更新する
        /// </summary>
        private void UpdateObstacles()
        {
            UpdateCacheArray(_obstacleContextMap, ref _obstacles);

            if (_obstacles == null || _obstacles.Length == 0)
            {
                return;
            }

            for (int i = 0; i < _obstacles.Length; i++)
            {
                IOBBData obb = _obstacles[i].OBB;

                if (obb != null)
                {
                    _obbToObstacleMap[obb] = _obstacles[i];
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
        /// 戦車と障害物の OBB を 1 配列へ統合する
        /// </summary>
        private void RebuildCircleQueryOBBCache()
        {
            int tankCount =
                _tanks != null ? _tanks.Length : 0;
            int obstacleCount =
                _obstacles != null ? _obstacles.Length : 0;
            int totalCount = tankCount + obstacleCount;

            // 対象が存在しない場合キャッシュを無効化
            if (totalCount == 0)
            {
                _circleQueryOBBCache = null;
                return;
            }

            // 配列未生成またはサイズ不一致の場合
            if (_circleQueryOBBCache == null ||
                _circleQueryOBBCache.Length != totalCount)
            {
                // 新規配列生成
                _circleQueryOBBCache =
                    new IOBBData[totalCount];
            }

            int index = 0;

            // 戦車OBBを統合配列へコピー
            for (int i = 0; i < tankCount; i++)
            {
                _circleQueryOBBCache[index++] =
                    _tanks[i].OBB;
            }

            // 障害物OBBを統合配列へコピー
            for (int i = 0; i < obstacleCount; i++)
            {
                _circleQueryOBBCache[index++] =
                    _obstacles[i].OBB;
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