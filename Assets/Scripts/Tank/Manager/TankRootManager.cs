// ======================================================
// TankRootManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-04
// 更新日時 : 2025-12-05
// 概要     : 戦車の各種制御を統合管理する
// ======================================================

using InputSystem.Data;
using ItemSystem.Data;
using SceneSystem.Interface;
using System;
using System.Collections.Generic;
using TankSystem.Controller;
using TankSystem.Data;
using TankSystem.Service;
using TankSystem.Utility;
using UnityEngine;
using static TankSystem.Data.TankInputKeys;

namespace TankSystem.Manager
{
    /// <summary>
    /// 戦車の各種制御を統括するクラス
    /// </summary>
    public class TankRootManager : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("コンポーネント参照")]
        /// <summary>障害物・アイテム・戦車などを保持する SceneObjectRegistry</summary>
        [SerializeField] private SceneObjectRegistry _sceneRegistry;

        [Header("ステータス")]
        /// <summary>ゲーム中に変動する戦車のパラメーター</summary>
        [SerializeField] private TankStatus _tankStatus;

        [Header("攻撃設定")]
        /// <summary>弾丸発射ローカル位置</summary>
        [SerializeField] private Transform _firePoint;

        [Header("当たり判定設定")]
        /// <summary>戦車本体の当たり判定中心位置</summary>
        [SerializeField] private Vector3 _hitboxCenter;

        /// <summary>戦車本体の当たり判定スケール</summary>
        [SerializeField] private Vector3 _hitboxSize;
        
        // ======================================================
        // コンポーネント参照
        // ======================================================

        // --------------------------------------------------
        // 攻撃力
        // --------------------------------------------------
        /// <summary>戦車の攻撃管理クラス</summary>
        private TankAttackManager _attackManager;

        // --------------------------------------------------
        // 防御力
        // --------------------------------------------------

        // --------------------------------------------------
        // 機動力
        // --------------------------------------------------
        /// <summary>戦車の機動管理クラス</summary>
        private TankMobilityManager _mobilityManager;

        /// <summary>左右キャタピラ入力から前進量・旋回量を算出するコントローラー</summary>
        private TankTrackController _trackController = new TankTrackController();

        /// <summary>AABB / OBB の距離計算および衝突判定を行うコントローラー</summary>
        private BoundingBoxCollisionController _boxCollisionController = new BoundingBoxCollisionController();

        /// <summary>OBB を生成するためのファクトリー</summary>
        private OBBFactory _obbFactory = new OBBFactory();

        // --------------------------------------------------
        // 入力
        // --------------------------------------------------
        /// <summary>戦車の入力管理クラス</summary>
        private TankInputManager _inputManager = new TankInputManager();

        // --------------------------------------------------
        // サービス
        // --------------------------------------------------
        /// <summary>戦車移動範囲制限サービス</summary>
        private TankMovementBoundaryService _boundaryService;

        /// <summary>戦車当たり判定サービス</summary>
        private TankCollisionService _collisionService;

        // ======================================================
        // フィールド
        // ======================================================

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>戦車の移動許容距離半径</summary>
        private const float MOVEMENT_ALLOWED_RADIUS = 200f;
        
        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// オプションボタン押下時に発火するイベント
        /// 外部から登録してUIやサウンド処理などを接続可能
        /// </summary>
        public event Action OnOptionButtonPressed;
        
        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void OnEnter()
        {
            // SceneObjectRegistry から必要なシーン情報を取得
            Transform[] obstacles = _sceneRegistry.Obstacles;
            List<ItemSlot> items = _sceneRegistry.ItemSlots;

            _attackManager = new TankAttackManager(_firePoint);

            _collisionService = new TankCollisionService(
                _obbFactory,
                _boxCollisionController,
                transform,
                _hitboxCenter,
                _hitboxSize,
                obstacles
            );

            _boundaryService = new TankMovementBoundaryService(MOVEMENT_ALLOWED_RADIUS);

            _mobilityManager = new TankMobilityManager(
                _trackController,
                _collisionService,
                _boundaryService,
                transform,
                _hitboxCenter,
                _hitboxSize,
                obstacles
            );

            _collisionService.SetItemOBBs(items);

            // イベント購読
            _collisionService.OnObstacleHit += HandleObstacleHit;
            _collisionService.OnItemHit += HandleItemHit;
            _sceneRegistry.OnItemListChanged += HandleItemListChanged;
        }

        public void OnUpdate()
        {
            // --------------------------------------------------
            // 入力
            // --------------------------------------------------
            // 入力取得
            _inputManager.UpdateInput();

            // 入力変換
            float leftMobility = _inputManager.LeftStick.y;
            float rightMobility = _inputManager.RightStick.y;

            // --------------------------------------------------
            // オプション
            // --------------------------------------------------
            foreach (ButtonState state in _inputManager.GetButtonStates(TankInputKeys.INPUT_OPTION))
            {
                if (state != null && state.Down)
                {
                    OnOptionButtonPressed?.Invoke();
                    break;
                }
            }

            // --------------------------------------------------
            // 攻撃
            // --------------------------------------------------
            // 辞書から攻撃ボタンを取得して更新
            // ButtonState leftAttackButton = _inputManager.GetButton(TankInputKeys.INPUT_EXPLOSIVE_FIRE);
            // ButtonState rightAttack = _inputManager.GetButton(TankInputKeys.INPUT_EXPLOSIVE_FIRE);

            // 攻撃処理
            // _attackManager.UpdateAttack(leftAttackButton, rightAttack);

            // --------------------------------------------------
            // 機動
            // --------------------------------------------------
            // 前進・旋回処理
            _mobilityManager.ApplyMobility(_tankStatus.HorsePower, leftMobility, rightMobility);

            // --------------------------------------------------
            // 衝突判定
            // --------------------------------------------------
            _collisionService.UpdateCollisionChecks();
        }

        public void OnLateUpdate()
        {
            
        }

        public void OnExit()
        {
            // イベント購読の解除
            _collisionService.OnObstacleHit -= HandleObstacleHit;
            _collisionService.OnItemHit -= HandleItemHit;
            _sceneRegistry.OnItemListChanged -= HandleItemListChanged;
        }

        public void OnPhaseEnter()
        {

        }

        public void OnPhaseExit()
        {

        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // イベントハンドラ
        // --------------------------------------------------
        /// <summary>
        /// 障害物に衝突したときの処理を行うハンドラ
        /// </summary>
        /// <param name="obstacle">衝突した障害物の Transform</param>
        private void HandleObstacleHit(Transform obstacle)
        {
            _mobilityManager.CheckObstaclesCollision(obstacle);
        }

        /// <summary>
        /// アイテムに衝突したときの処理を行うハンドラ
        /// </summary>
        /// <param name="itemSlot">衝突したアイテムの Slot</param>
        private void HandleItemHit(ItemSlot itemSlot)
        {
            ItemData data = itemSlot.ItemData;

            // 型判定で ParamItemData か WeaponItemData を判別
            if (data is ParamItemData param)
            {
                // 戦車パラメーターを増加
                _tankStatus.IncreaseParameter(param.Type, param.Value);
            }
            else if (data is WeaponItemData weapon)
            {
                // 武装アイテム取得処理
            }
            else
            {
                return;
            }

            // アイテム削除
            _sceneRegistry.RemoveItem(itemSlot);
        }

        /// <summary>
        /// SceneObjectRegistry のアイテム更新イベント受信時に
        /// OBB 情報を再生成する
        /// </summary>
        private void HandleItemListChanged(List<ItemSlot> newList)
        {
            _collisionService.SetItemOBBs(newList);
        }
    }
}