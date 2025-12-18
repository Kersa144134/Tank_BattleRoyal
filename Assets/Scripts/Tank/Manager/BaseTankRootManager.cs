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
using CollisionSystem.Data;
using InputSystem.Data;
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

        [Header("防御設定")]
        /// <summary>戦車本体の BoxCollider</summary>
        [SerializeField] private BoxCollider _tankCollider;
        
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

        // --------------------------------------------------
        // サービス
        // --------------------------------------------------
        /// <summary>戦車移動範囲制限サービス</summary>
        private TankMovementBoundaryService _boundaryService;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>移動予定ワールド座標</summary>
        private Vector3 _plannedNextPosition;

        /// <summary>移動予定回転</summary>
        private Quaternion _plannedNextRotation;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>ゲーム中に変動する戦車のパラメーター</summary>
        public TankStatus TankStatus => _tankStatus;

        /// <summary>キャタピラ入力モード</summary>
        public TrackInputMode InputMode => _trackController.InputMode;

        /// <summary>弾丸発射ローカル位置</summary>
        public Transform FirePoint => _firePoint;

        /// <summary>移動予定ワールド座標</summary>
        public Vector3 PlannedNextPosition => _plannedNextPosition;

        /// <summary>移動予定回転</summary>
        public Quaternion PlannedNextRotation => _plannedNextRotation;

        /// <summary>前フレームからの移動量</summary>
        public float DeltaForward => _mobilityManager.DeltaForward;

        /// <summary>
        /// 今フレーム中に移動を制限すべき軸
        /// </summary>
        public MovementLockAxis CurrentFrameLockAxis { get; private set; } = MovementLockAxis.None;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>戦車の移動許容距離半径</summary>
        private const float MOVEMENT_ALLOWED_RADIUS = 315f;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>入力モード切替ボタン押下時に発火するイベント</summary>
        public event Action OnModeChangeButtonPressed;

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
        /// <param name="leftMobility">左キャタピラ入力から算出される前進/旋回量</param>
        /// <param name="rightMobility">右キャタピラ入力から算出される前進/旋回量</param>
        /// <param name="modeChange">入力モード切替ボタン押下フラグ</param>
        /// <param name="option">オプションボタン押下フラグ</param>
        /// <param name="leftFire">左攻撃ボタンの状態</param>
        /// <param name="rightFire">右攻撃ボタンの状態</param>
        protected abstract void UpdateInput(
            out Vector2 leftMobility,
            out Vector2 rightMobility,
            out bool modeChange,
            out bool option,
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
            _boundaryService = new TankMovementBoundaryService(MOVEMENT_ALLOWED_RADIUS);
            _mobilityManager = new TankMobilityManager(
                _trackController,
                _boundaryService,
                transform
            );

            // イベント購読
            _attackManager.OnFireBullet += HandleFireBullet;
        }

        public virtual void OnUpdate()
        {
            // --------------------------------------------------
            // フレーム開始時に軸制限をリセット
            // --------------------------------------------------
            CurrentFrameLockAxis = MovementLockAxis.None;

            // --------------------------------------------------
            // 入力
            // --------------------------------------------------
            UpdateInput(out Vector2 leftMobility,
                out Vector2 rightMobility,
                out bool modeChange,
                out bool option,
                out ButtonState leftFire,
                out ButtonState rightFire
            );

            // --------------------------------------------------
            // 入力モード切替
            // --------------------------------------------------
            if (modeChange)
            {
                OnModeChangeButtonPressed?.Invoke();
            }

            // --------------------------------------------------
            // オプション
            // --------------------------------------------------
            if (option)
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
            // 前進・旋回を適用した場合の移動結果を計算し、予定位置と回転を受け取る
            _mobilityManager.CalculateMobilityResult(
                _tankStatus,
                leftMobility,
                rightMobility,
                out _plannedNextPosition,
                out _plannedNextRotation
            );
        }

        public virtual void OnLateUpdate()
        {
            // --------------------------------------------------
            // 機動
            // --------------------------------------------------
            // 計算済みの移動・回転結果を Transform に適用する
            _mobilityManager.ApplyPlannedTransform(
                _plannedNextPosition,
                _plannedNextRotation
            );
        }

        public virtual void OnExit()
        {
            // イベント購読の解除
            _attackManager.OnFireBullet -= HandleFireBullet;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// キャタピラの入力モードを切り替える
        /// </summary>
        public void ChangeInputMode()
        {
            _trackController.ChangeInputMode();
        }

        /// <summary>
        /// TankCollisionService からの衝突通知を受けて、戦車のめり込みを解消する
        /// </summary>
        /// <param name="resolveInfo">呼び出し側で算出済みの押し戻し情報</param>
        public void ApplyCollisionResolve(in CollisionResolveInfo resolveInfo)
        {
            _mobilityManager.ApplyCollisionResolve(resolveInfo);
        }
        
        // ======================================================
        // イベントハンドラ
        // ======================================================

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