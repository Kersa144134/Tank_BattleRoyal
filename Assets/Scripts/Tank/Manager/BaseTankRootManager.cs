// ======================================================
// BaseTankRootManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-04
// 更新日時 : 2026-01-09
// 概要     : 戦車の各種制御を統合管理する
// ======================================================

using CollisionSystem.Data;
using CollisionSystem.Interface;
using InputSystem.Data;
using InputSystem.Manager;
using SceneSystem.Data;
using SceneSystem.Interface;
using System;
using TankSystem.Controller;
using TankSystem.Data;
using TankSystem.Service;
using UnityEngine;
using VisionSystem.Calculator;
using WeaponSystem.Data;
using WeaponSystem.Interface;

namespace TankSystem.Manager
{
    /// <summary>
    /// 戦車の各種制御を統括するクラス
    /// </summary>
    public abstract class BaseTankRootManager : MonoBehaviour, IUpdatable, IDamageable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("ステータス")]
        /// <summary>ゲーム中に変動する戦車のパラメーター</summary>
        [SerializeField] private TankStatus _tankStatus;

        [Header("衝突設定")]
        /// <summary>戦車の衝突設定 Box のローカル中心座標</summary>
        [SerializeField] private Vector3 _hitBoxCenter;

        /// <summary>戦車の衝突設定 Boxのローカルスケール</summary>
        [SerializeField] private Vector3 _hitBoxScale;

        [Header("視界判定設定")]
        /// <summary>戦車の視界判定に侵入した際に表示するターゲットアイコン</summary>
        [SerializeField] private MeshRenderer _targetIcon;

        [Header("攻撃設定")]
        /// <summary>砲身の Transform</summary>
        [SerializeField] private Transform _turret;

        /// <summary>弾丸発射ローカル位置</summary>
        [SerializeField] private Transform _firePoint;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        // --------------------------------------------------
        // 攻撃力
        // --------------------------------------------------
        /// <summary>戦車の攻撃管理クラス</summary>
        private TankAttackManager _attackManager;

        /// <summary>視界判定のユースケースクラス</summary>
        private FieldOfViewCalculator _fieldOfViewCalculator = new FieldOfViewCalculator();

        /// <summary>砲塔回転制御コントローラー</summary>
        private TankTurretController _turretController;

        // --------------------------------------------------
        // 防御力
        // --------------------------------------------------
        /// <summary>戦車の耐久力管理クラス</summary>
        private TankDurabilityManager _durabilityManager;

        /// <summary>戦車の防御力管理クラス</summary>
        private TankDefenseManager _defenseManager;

        // --------------------------------------------------
        // 機動力
        // --------------------------------------------------
        /// <summary>戦車の機動管理クラス</summary>
        private TankMobilityManager _mobilityManager;

        /// <summary>左右キャタピラ入力から前進量・旋回量を算出するコントローラー</summary>
        private TankTrackController _trackController = new TankTrackController();

        // --------------------------------------------------
        // エフェクト
        // --------------------------------------------------
        /// <summary>戦車のエフェクト管理クラス</summary>
        private TankEffectController _effectController;

        // --------------------------------------------------
        // サービス
        // --------------------------------------------------
        /// <summary>戦車移動範囲制限サービス</summary>
        private TankMovementBoundaryService _boundaryService;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>現在インゲーム状態かどうか</summary>
        private bool _isInGame;

        /// <summary>戦車が機能停止状態かどうか</summary>
        private bool _isBroken;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>戦車を一意に識別する ID</summary>
        public int TankId { get; set; }

        /// <summary>戦車が機能停止状態かどうか</summary>
        public bool IsBroken => _isBroken;

        /// <summary>ゲーム中に変動する戦車のパラメーター</summary>
        public TankStatus TankStatus => _tankStatus;

        /// <summary>戦車の耐久力管理クラス</summary>
        public TankDurabilityManager DurabilityManager => _durabilityManager;

        /// <summary>戦車の衝突設定 Box のローカル中心座標</summary>
        public Vector3 HitBoxCenter => _hitBoxCenter;

        /// <summary>戦車の衝突設定 Boxのローカルスケール</summary>
        public Vector3 HitBoxScale => _hitBoxScale;

        /// <summary>自身の Transform</summary>
        public Transform Transform => transform;

        /// <summary>弾丸発射ローカル位置</summary>
        public Transform FirePoint => _firePoint;

        /// <summary>現在の前進移動速度</summary>
        public float CurrentForwardSpeed => _mobilityManager.CurrentForwardSpeed;

        /// <summary>移動予定ワールド座標</summary>
        public Vector3 NextPosition { get; set; }

        /// <summary>移動予定回転</summary>
        public Quaternion NextRotation { get; set; }

        /// <summary>今フレーム中に移動を制限すべき軸</summary>
        public MovementLockAxis CurrentFrameLockAxis { get; set; } = MovementLockAxis.None;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>戦車の移動許容距離半径</summary>
        private const float MOVEMENT_ALLOWED_RADIUS = 315f;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>入力モード切替ボタン押下時に発火するイベント</summary>
        public event Action OnInputModeChangeButtonPressed;

        /// <summary>攻撃モード切替ボタン押下時に発火するイベント</summary>
        public event Action OnFireModeChangeButtonPressed;

        /// <summary>弾丸が発射された際に発火するイベント</summary>
        public event Action<BaseTankRootManager, BulletType, Transform> OnFireBullet;

        /// <summary>耐久力が 0 になった際に発火するイベント</summary>
        public event Action<int> OnBroken;

        // ======================================================
        // 抽象メソッド
        // ======================================================

        /// <summary>
        /// 毎フレーム呼び出される入力更新処理
        /// </summary>
        /// <param name="leftStick">左スティック入力</param>
        /// <param name="rightStick">右キスティック入力</param>
        /// <param name="turretRotation">砲塔回転入力</param>
        /// <param name="inputModeChange">入力モード切替ボタン押下フラグ</param>
        /// <param name="fireModeChange">攻撃モード切替ボタン押下フラグ</param>
        /// <param name="leftFire">左攻撃ボタンの状態</param>
        /// <param name="rightFire">右攻撃ボタンの状態</param>
        protected abstract void UpdateInput(
            out Vector2 leftStick,
            out Vector2 rightStick,
            out float turretRotation,
            out bool inputModeChange,
            out bool fireModeChange,
            out ButtonState leftFire,
            out ButtonState rightFire
        );

        // ======================================================
        // セッター
        // ======================================================

        /// <summary>
        /// 戦車の Transform 配列と遮蔽物の OBB 配列を AttackManager に送る
        /// </summary>
        /// <param name="tankTransforms">戦車自身の Transform 配列</param>
        /// <param name="obstacleOBBs">遮蔽物 OBB 配列</param>
        public void SetContextData(
            in Transform[] tankTransforms,
            in IOBBData[] obstacleOBBs
        )
        {
            _attackManager.SetContextData(tankTransforms, obstacleOBBs);
        }
        
        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public virtual void OnEnter()
        {
            _boundaryService = new TankMovementBoundaryService(MOVEMENT_ALLOWED_RADIUS);
            _attackManager = new TankAttackManager(_tankStatus, _fieldOfViewCalculator, transform, _turret);
            _turretController = new TankTurretController(_tankStatus, _turret);
            _defenseManager = new TankDefenseManager(_tankStatus);
            _durabilityManager = new TankDurabilityManager(_tankStatus);
            _mobilityManager = new TankMobilityManager(
                _tankStatus,
                _trackController,
                _boundaryService,
                transform
            );
            _effectController = new TankEffectController(transform);

            // イベント購読
            _attackManager.OnFireBullet += HandleFireBullet;
            _durabilityManager.OnBroken += HandleBroken;
        }

        public virtual void OnUpdate(in float unscaledDeltaTime, in float elapsedTime)
        {
            // 破壊済みの場合は処理なし
            if (_isBroken)
            {
                return;
            }

            // --------------------------------------------------
            // デバッグ用（いずれ削除予定）
            // --------------------------------------------------
            if (TankId == 1)
            {
                if (Input.GetKeyDown(KeyCode.Backspace)) TakeDamage(20f);
            }

            // --------------------------------------------------
            // タイムスケール
            // --------------------------------------------------
            float deltaTime = Time.deltaTime;

            // --------------------------------------------------
            // 軸制限をリセット
            // --------------------------------------------------
            CurrentFrameLockAxis = MovementLockAxis.None;

            // --------------------------------------------------
            // 入力
            // --------------------------------------------------
            // 各種入力状態をまとめて取得
            UpdateInput(
                out Vector2 leftStick,
                out Vector2 rightStick,
                out float turretRotation,
                out bool inputModeChange,
                out bool fireModeChange,
                out ButtonState leftFire,
                out ButtonState rightFire
            );

            // モード切替
            if (inputModeChange)
            {
                OnInputModeChangeButtonPressed?.Invoke();
            }
            if (fireModeChange)
            {
                _attackManager.NextBulletType();

                OnFireModeChangeButtonPressed?.Invoke();
            }

            // 入力マッピングが UI 用の場合は処理なし
            if (InputManager.Instance.CurrentMappingIndex == 1)
            {
                return;
            }

            // インゲーム状態でなければ処理なし
            if (!_isInGame)
            {
                return;
            }
            
            // --------------------------------------------------
            // 攻撃
            // --------------------------------------------------
            // 攻撃処理
            _attackManager.UpdateAttack(unscaledDeltaTime, leftFire, rightFire);

            // 砲塔スケール
            _turretController.ApplyTurretScale();

            // 砲塔回転
            _turretController.ApplyTurretRotate(
                deltaTime,
                turretRotation
            );

            // --------------------------------------------------
            // 機動
            // --------------------------------------------------
            // 前進・旋回を適用した場合の移動結果を計算し、移動予定座標と回転を受け取る
            Vector3 calculatedPosition;
            Quaternion calculatedRotation;

            if (this is PlayerTankRootManager player)
            {
                _mobilityManager.CalculateMobility(
                    deltaTime,
                    player.InputMode,
                    leftStick,
                    rightStick,
                    out calculatedPosition,
                    out calculatedRotation
                );

                NextPosition = calculatedPosition;
                NextRotation = calculatedRotation;
            }
            // プレイヤーでない場合は入力モードをシングル固定
            else
            {
                _mobilityManager.CalculateMobility(
                    deltaTime,
                    TrackInputMode.Single,
                    leftStick,
                    rightStick,
                    out calculatedPosition,
                    out calculatedRotation
                );

                NextPosition = calculatedPosition;
                NextRotation = calculatedRotation;
            }
        }

        public virtual void OnLateUpdate(in float unscaledDeltaTime)
        {
            // 破壊済みの場合は処理なし
            if (_isBroken)
            {
                return;
            }

            // --------------------------------------------------
            // 入力
            // --------------------------------------------------
            // 入力マッピングが UI 用の場合は処理を中断
            if (InputManager.Instance.CurrentMappingIndex == 1)
            {
                return;
            }
            
            // --------------------------------------------------
            // 機動
            // --------------------------------------------------
            // 計算済みの移動・回転結果を Transform に適用する
            _mobilityManager.ApplyMobility(
                NextPosition,
                NextRotation
            );
        }

        public virtual void OnExit()
        {
            // イベント購読の解除
            _attackManager.OnFireBullet -= HandleFireBullet;
            _durabilityManager.OnBroken -= HandleBroken;
        }

        public void OnPhaseEnter(in PhaseType phase)
        {
            // Play フェーズ開始時にインゲーム状態
            if (phase == PhaseType.Play)
            {
                _isInGame = true;
            }
        }

        public void OnPhaseExit(in PhaseType phase)
        {
            // Play フェーズ終了時にインゲーム状態解除
            if (phase == PhaseType.Play)
            {
                _isInGame = false;
            }
        }

        // ======================================================
        // IDamageable イベント
        // ======================================================

        /// <summary>
        /// ダメージを受ける処理
        /// </summary>
        /// <param name="damage">受けるダメージ量</param>
        public void TakeDamage(in float damage)
        {
            // 破壊済みの場合は処理なし
            if (_isBroken)
            {
                return;
            }
            
            // 防御力を考慮した軽減後ダメージを算出
            float reducedDamage = _defenseManager.CalculateReducedDamage(damage);

            // 無効な値は処理なし
            if (reducedDamage <= 0f)
            {
                return;
            }

            // 耐久力管理クラスに最終ダメージを適用
            _durabilityManager.ApplyDamage(reducedDamage);


            // ダメージエフェクト再生
            _effectController.PlayDamage();
        }

        /// <summary>
        /// 装甲にダメージを与える
        /// </summary>
        /// <param name="armorDamage">装甲へのダメージ量</param>
        public void TakeArmorDamage(in float armorDamage)
        {
            // 破壊済みの場合は処理なし
            if (_isBroken)
            {
                return;
            }

            // 防御力管理クラスに装甲ダメージを委譲
            _defenseManager.ApplyArmorDamage(armorDamage);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 任意のパラメーターを指定量だけ増加
        /// </summary>
        /// <param name="param">増加させるパラメーター</param>
        /// <param name="amount">増加量</param>
        public void IncreaseParameter(in TankParam param, in int amount)
        {
            // 破壊済みの場合は処理なし
            if (_isBroken)
            {
                return;
            }

            _tankStatus.IncreaseParameter(param, amount);

            // パラメーター更新処理
            _attackManager.UpdateAttackParameter(_tankStatus);
            _defenseManager.UpdateDefenseParameter(_tankStatus);
            _durabilityManager.UpdateDurabilityParameter(_tankStatus);
            _mobilityManager.UpdateMobilityParameters(_tankStatus);
            _turretController.UpdateTurretParameter(_tankStatus);
        }

        /// <summary>
        /// キャタピラの入力モードを切り替える
        /// </summary>
        public void ChangeInputMode()
        {
            // プレイヤーでない場合は処理なし
            if (this is not PlayerTankRootManager playerTankRootManager)
            {
                return;
            }
            
            // 破壊済みの場合は処理なし
            if (_isBroken)
            {
                return;
            }

            playerTankRootManager.InputManager.ChangeInputMode();
        }

        /// <summary>
        /// ターゲットアイコンの表示を切り替える
        /// </summary>
        /// <param name="isActive">true で表示、false で非表示</param>
        public void ChangeTargetIcon(bool isActive)
        {
            if (_targetIcon != null)
            {
                // 破壊済みの場合は強制非表示
                if (_isBroken)
                {
                    _targetIcon.enabled = false;
                }
                else
                {
                    _targetIcon.enabled = isActive;
                }
            }
        }

        //// <summary>
        /// CollisionManager からの衝突通知を受けて、戦車のめり込みを解消する
        /// </summary>
        /// <param name="resolveInfo">呼び出し側で算出済みの押し戻し情報</param>
        public void ApplyCollisionResolve(in CollisionResolveInfo resolveInfo)
        {
            // 破壊済みの場合は処理なし
            if (_isBroken)
            {
                return;
            }

            NextPosition += resolveInfo.ResolveVector;
        }

        // ======================================================
        // イベントハンドラ
        // ======================================================

        /// <summary>
        /// 左右トリガー入力を元に攻撃を実行し、
        /// 弾丸タイプを通知する処理を行うハンドラ
        /// </summary>
        /// <param name="type">発射する弾丸の種類</param>
        /// <param name="target">弾丸の回転方向に指定するターゲット Transform</param>
        private void HandleFireBullet(BulletType type, Transform target = null)
        {
            // 破壊済みの場合は処理なし
            if (_isBroken)
            {
                return;
            }

            // 発射位置を取得
            Vector3 firePosition = _firePoint.position;

            // 発射方向は戦車前方
            Vector3 fireDirection = transform.forward;

            // イベントを外部に通知
            OnFireBullet?.Invoke(this, type, target);
        }

        /// <summary>
        /// 戦車が破壊された際に呼ばれる処理
        /// 入力・攻撃・移動を完全に停止する
        /// </summary>
        private void HandleBroken()
        {
            // 破壊済みの場合は処理なし
            if (_isBroken)
            {
                return;
            }

            // 機能停止フラグを立てる
            _isBroken = true;

            // 移動予定を現在位置に固定
            NextPosition = transform.position;
            NextRotation = transform.rotation;

            // ターゲットアイコンを非表示
            ChangeTargetIcon(false);

            // 爆発エフェクト再生
            _effectController.PlayExplosion();

            OnBroken?.Invoke(TankId);
        }
    }
}