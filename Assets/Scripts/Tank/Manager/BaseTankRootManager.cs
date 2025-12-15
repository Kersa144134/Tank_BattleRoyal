// ======================================================
// TankRootManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-04
// 更新日時 : 2025-12-05
// 概要     : 戦車の各種制御を統合管理する
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using CollisionSystem.Calculator;
using InputSystem.Data;
using ItemSystem.Data;
using SceneSystem.Interface;
using TankSystem.Controller;
using TankSystem.Data;
using TankSystem.Service;
using TankSystem.Utility;
using WeaponSystem.Data;

namespace TankSystem.Manager
{
    /// <summary>
    /// 戦車の各種制御を統括するクラス
    /// </summary>
    public abstract class BaseTankRootManager : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("コンポーネント参照")]
        /// <summary>シーン上のオブジェクト Transform を保持するレジストリー</summary>
        [SerializeField] private SceneObjectRegistry _sceneRegistry;

        [Header("ステータス")]
        /// <summary>ゲーム中に変動する戦車のパラメーター</summary>
        [SerializeField] private TankStatus _tankStatus;

        [Header("攻撃設定")]
        /// <summary>砲身の Transform</summary>
        [SerializeField] private Transform _turret;

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
        private BoundingBoxCollisionCalculator _boxCollisionCalculator = new BoundingBoxCollisionCalculator();

        /// <summary>OBB を生成するためのファクトリー</summary>
        private OBBFactory _obbFactory = new OBBFactory();

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
        // プロパティ
        // ======================================================

        /// <summary>ゲーム中に変動する戦車のパラメーター</summary>
        public TankStatus TankStatus => _tankStatus;

        /// <summary>弾丸発射ローカル位置</summary>
        public Transform FirePoint => _firePoint;
        
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>戦車の移動許容距離半径</summary>
        private const float MOVEMENT_ALLOWED_RADIUS = 315f;
        
        // ======================================================
        // イベント
        // ======================================================

        /// <summary>オプションボタン押下時に発火するイベント</summary>
        public event Action OnOptionButtonPressed;

        /// <summary>弾丸が発射された際に発火するイベント</summary>
        public event Action<BaseTankRootManager, BulletType> OnFireBullet;

        // ======================================================
        // 抽象メソッド
        // ======================================================

        /// <summary>
        /// 毎フレーム呼び出される入力更新処理を実装する抽象メソッド
        /// プレイヤーの場合はプレイヤー入力を、敵AIの場合は自動制御入力を設定する
        /// </summary>
        /// <param name="leftMobility">左キャタピラ入力から算出される前進/後退量</param>
        /// <param name="rightMobility">右キャタピラ入力から算出される前進/後退量</param>
        /// <param name="optionPressed">オプションボタン押下フラグ</param>
        /// <param name="leftFire">左攻撃ボタンの状態</param>
        /// <param name="rightFire">右攻撃ボタンの状態</param>
        protected abstract void UpdateInput(
            out float leftMobility,
            out float rightMobility,
            out bool optionPressed,
            out ButtonState leftFire,
            out ButtonState rightFire
        );
        
        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public virtual void OnEnter()
        {
            // SceneObjectRegistry から必要なシーン情報を取得
            Transform[] obstacles = _sceneRegistry.Obstacles;
            List<ItemSlot> items = _sceneRegistry.ItemSlots;

            _attackManager = new TankAttackManager(_firePoint);

            _collisionService = new TankCollisionService(
                _obbFactory,
                _boxCollisionCalculator,
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
            _attackManager.OnFireBullet += HandleFireBullet;
            _collisionService.OnObstacleHit += HandleObstacleHit;
            _collisionService.OnItemHit += HandleItemHit;
            _sceneRegistry.OnItemListChanged += HandleItemListChanged;
        }

        public virtual void OnUpdate()
        {
            // --------------------------------------------------
            // 入力
            // --------------------------------------------------
            UpdateInput(out float leftMobility,
                out float rightMobility,
                out bool optionPressed,
                out ButtonState leftFire,
                out ButtonState rightFire
            );

            // --------------------------------------------------
            // オプション
            // --------------------------------------------------
            if (optionPressed)
            {
                OnOptionButtonPressed?.Invoke();
            }

            // --------------------------------------------------
            // 攻撃
            // --------------------------------------------------
            // 攻撃処理
            _attackManager.UpdateAttack(leftFire, rightFire);

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

        public virtual void OnExit()
        {
            // イベント購読の解除
            _attackManager.OnFireBullet -= HandleFireBullet;
            _collisionService.OnObstacleHit -= HandleObstacleHit;
            _collisionService.OnItemHit -= HandleItemHit;
            _sceneRegistry.OnItemListChanged -= HandleItemListChanged;
        }

        // ======================================================
        // イベントハンドラ
        // ======================================================

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
        /// OBB 情報を再生成する処理を行うハンドラ
        /// </summary>
        private void HandleItemListChanged(List<ItemSlot> newList)
        {
            _collisionService.SetItemOBBs(newList);
        }

        /// <summary>
        /// 左右トリガー入力を元に攻撃を実行し、
        /// 弾丸タイプを通知する処理を行うハンドラ
        /// </summary>
        private void HandleFireBullet(BulletType type)
        {
            // 発射位置を取得
            Vector3 firePosition = _firePoint.position;

            // 発射方向は戦車前方
            Vector3 fireDirection = transform.forward;

            // イベントを外部に通知
            OnFireBullet?.Invoke(this, type);
        }
    }
}