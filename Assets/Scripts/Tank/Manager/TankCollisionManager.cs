// ======================================================
// TankCollisionManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : 各種戦車衝突判定サービスを統括管理する MonoBehaviour
// ======================================================

using CollisionSystem.Calculator;
using SceneSystem.Interface;
using System.Collections.Generic;
using TankSystem.Data;
using TankSystem.Interface;
using TankSystem.Service;
using TankSystem.Utility;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using static TankSystem.Interface.ITankCollisionService;

namespace TankSystem.Manager
{
    /// <summary>
    /// 戦車・障害物・アイテムの衝突判定を一元管理する MonoBehaviour
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

        /// <summary>
        /// OBB / OBB の衝突判定および MTV 計算を行う計算器
        /// </summary>
        private readonly BoundingBoxCollisionCalculator _boxCollisionCalculator = new BoundingBoxCollisionCalculator();

        /// <summary>OBB を生成するファクトリー</summary>
        private readonly OBBFactory _obbFactory = new OBBFactory();

        /// <summary>戦車衝突用コンテキストを生成するファクトリー</summary>
        private TankCollisionContextFactory _collisionContextFactory;

        /// <summary>
        /// 衝突判定サービス一覧
        /// 判定順は登録順で制御する
        /// </summary>
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
        /// 現在有効な戦車衝突コンテキスト一覧
        /// </summary>
        private readonly List<TankCollisionContext> _tankContexts = new List<TankCollisionContext>();

        // ======================================================
        // IUpdatable 実装
        // ======================================================

        public void OnEnter()
        {
            _collisionContextFactory = new TankCollisionContextFactory(_obbFactory);

            // --------------------------------------------------
            // 戦車コンテキスト構築
            // --------------------------------------------------
            BuildTankContexts();

            // --------------------------------------------------
            // 各サービス生成
            // --------------------------------------------------
            _versusObstacleService = new VersusObstacleCollisionService(_boxCollisionCalculator, _tankContexts, _sceneRegistry.Obstacles);
            _versusItemService = new VersusItemCollisionService(_boxCollisionCalculator, _tankContexts);
            _versusTankService = new VersusTankCollisionService(_boxCollisionCalculator, _tankContexts);

            // --------------------------------------------------
            // 初回 OBB 生成 
            // --------------------------------------------------
            _versusObstacleService.SetObstacleOBBs(_obbFactory);
            SetItems(_sceneRegistry.ItemSlots);

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
        // セッター
        // ======================================================

        /// <summary>
        /// アイテム情報を登録する
        /// </summary>
        /// <param name="items">衝突判定対象となるアイテム一覧</param>
        public void SetItems(in List<ItemSlot> items)
        {
            _versusItemService.SetItemOBBs(_obbFactory, items);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // 初期化
        // --------------------------------------------------
        /// <summary>
        /// SceneObjectRegistry から戦車衝突コンテキストを生成する
        /// </summary>
        private void BuildTankContexts()
        {
            if (_sceneRegistry == null)
            {
                return;
            }

            // 既存データをクリア
            _tankContexts.Clear();

            // 登録されている戦車 Transform を順に処理する
            for (int i = 0; i < _sceneRegistry.Tanks.Length; i++)
            {
                // 戦車識別用IDとしてインデックスを使用する
                int tankId = i;

                // 対象となる戦車 Transform を取得する
                Transform tankTransform = _sceneRegistry.Tanks[i];

                // Transform が未設定の場合は処理対象外とする
                if (tankTransform == null)
                {
                    continue;
                }

                // BoxCollider を持たない戦車は衝突対象外とする
                if (!tankTransform.TryGetComponent(out BoxCollider boxCollider))
                {
                    continue;
                }

                // 戦車のルート管理コンポーネントを取得する
                if (!tankTransform.TryGetComponent(out BaseTankRootManager rootManager))
                {
                    continue;
                }

                // 戦車の衝突コンテキストを生成する
                TankCollisionContext context =
                    _collisionContextFactory.Create(
                        tankId,
                        boxCollider,
                        rootManager
                    );

                // 生成したコンテキストを一覧へ登録する
                _tankContexts.Add(context);
            }
        }

        // --------------------------------------------------
        // イベントハンドラ
        // --------------------------------------------------
        /// <summary>
        /// 障害物衝突時の通知
        /// </summary>
        private void HandleObstacleHit(
            TankCollisionContext context,
            Transform obstacle
        )
        {
            // 衝突した戦車名と障害物名をログ出力する
            Debug.Log(
                $"[ObstacleHit] Tank:{context.TankId} Obstacle:{obstacle.name}"
            );
        }

        /// <summary>
        /// アイテム取得時の通知
        /// </summary>
        private void HandleItemHit(
            TankCollisionContext context,
            ItemSlot item
        )
        {
            // 取得した戦車とアイテム情報をログ出力する
            Debug.Log(
                $"[ItemHit] Tank:{context.TankId} Item:{item.ItemData.Name}"
            );
        }

        /// <summary>
        /// 戦車同士が接触した際の通知
        /// </summary>
        private void HandleTankHit(
            TankCollisionContext contextA,
            TankCollisionContext contextB
        )
        {
            // 接触した 2 台の戦車 ID をログ出力する
            Debug.Log($"[TankHit] TankA:{contextA.TankId} TankB:{contextB.TankId}");
        }
    }
}