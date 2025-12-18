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
using TankSystem.Service;
using TankSystem.Utility;
using UnityEngine;

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

        private readonly TankCollisionContextFactory _collisionContextFactory;

        /// <summary>対 障害物 衝突判定サービス</summary>
        private VersusObstacleCollisionService _versusObstacleService;

        /// <summary>対 アイテム 衝突判定サービス</summary>
        private VersusItemCollisionService _versusItemService;

        /// <summary>対 戦車 衝突判定サービス</summary>
        private VersusTankCollisionService _versusTankService;

        // ======================================================
        // コンテキスト
        // ======================================================

        /// <summary>
        /// 現在有効な戦車衝突コンテキスト一覧
        /// </summary>
        private readonly List<TankCollisionContext> _tankContexts =
            new List<TankCollisionContext>();

        // ======================================================
        // IUpdatable 実装
        // ======================================================

        public void OnEnter()
        {
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
            // 衝突判定実行順
            // --------------------------------------------------
            // 1. 障害物（ロック軸が最優先）
            _versusObstacleService.UpdateCollisionChecks();

            // 2. 戦車同士（ロック軸を考慮）
            _versusTankService.UpdateCollisionChecks();

            // 3. アイテム（最終的な位置で判定）
            _versusItemService.UpdateCollisionChecks();
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
        /// アイテム情報を戦車 vs アイテム判定サービスへ登録する
        /// </summary>
        /// <param name="items">衝突判定対象となるアイテム一覧</param>
        public void SetObstacles(in List<ItemSlot> items)
        {
            _versusObstacleService.SetObstacleOBBs(_obbFactory);
        }

        /// <summary>
        /// アイテム情報を戦車 vs アイテム判定サービスへ登録する
        /// </summary>
        /// <param name="items">衝突判定対象となるアイテム一覧</param>
        public void SetItems(in List<ItemSlot> items)
        {
            _versusItemService.SetItemOBBs(_obbFactory, items);
        }

        // ======================================================
        // 初期化処理
        // ======================================================

        /// <summary>
        /// SceneObjectRegistry から戦車衝突コンテキストを生成する
        /// </summary>
        private void BuildTankContexts()
        {
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
                        tankTransform,
                        boxCollider,
                        rootManager
                    );

                // 生成したコンテキストを一覧へ登録する
                _tankContexts.Add(context);
            }
        }

        // ======================================================
        // イベントハンドラ
        // ======================================================

        /// <summary>
        /// 障害物衝突時の通知
        /// </summary>
        private void HandleObstacleHit(
            TankCollisionContext context,
            Transform obstacle
        )
        {
            
        }

        /// <summary>
        /// アイテム取得時の通知
        /// </summary>
        private void HandleItemHit(
            TankCollisionContext context,
            ItemSlot item
        )
        {
            
        }

        /// <summary>
        /// 戦車同士が接触した際の通知
        /// </summary>
        private void HandleTankHit(
            TankCollisionContext contextA,
            TankCollisionContext contextB
        )
        {
            
        }
    }
}