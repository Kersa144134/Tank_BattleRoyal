// ======================================================
// BulletBase.cs
// 作成者   : 高橋一翔
// 作成日   : 2025-12-12
// 更新日   : 2026-01-21
// 概要     : 弾丸ロジックの抽象基底クラス
//            弾速・質量に基づく減衰処理を行い、生存時間を判定する
// ======================================================

using System;
using UnityEngine;
using CollisionSystem.Data;
using TankSystem.Data;
using WeaponSystem.Interface;

namespace WeaponSystem.Data
{
    /// <summary>
    /// すべての弾丸ロジックの基底クラス
    /// </summary>
    public abstract class BulletBase
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>弾丸の所有者 ID</summary>
        protected int _bulletId;

        /// <summary>弾丸が有効かどうか</summary>
        protected bool _isEnabled;

        /// <summary>弾丸発射時の初期弾速</summary>
        private float _maxSpeed;

        /// <summary>弾丸発射時の初期高度</summary>
        private float _maxHeight;

        /// <summary>弾丸が地面に接触する最終高度</summary>
        private float _minHeight;

        /// <summary>ダメージ対象</summary>
        protected IDamageable _damageTarget;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>弾丸の所有者 ID</summary>
        public int BulletId
        {
            get => _bulletId;
            protected set => _bulletId = value;
        }
        
        /// <summary>弾丸が有効かどうか</summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            protected set
            {
                _isEnabled = value;

                // Transform がある場合は Renderer を有効/無効
                if (Transform != null)
                {
                    Renderer renderer = Transform.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.enabled = _isEnabled;
                    }
                }
            }
        }

        /// <summary>弾速</summary>
        public float BulletSpeed { get; set; }

        /// <summary>弾丸の質量</summary>
        public float Mass { get; set; }

        /// <summary>弾丸の Transform</summary>
        public Transform Transform { get; protected set; }

        /// <summary>弾丸の移動予定座標</summary>
        public Vector3 NextPosition { get; set; }

        /// <summary>弾丸の移動予定方向</summary>
        public Vector3 NextDirection { get; set; }

        /// <summary>弾丸の移動予定回転</summary>
        public Quaternion NextRotation
        {
            get
            {
                // NextDirection がゼロベクトルの場合は現在の Transform.rotation を返す
                if (NextDirection.sqrMagnitude < 1e-6f && Transform != null)
                {
                    return Transform.rotation;
                }

                // 上方向ベクトルを基準にして LookRotation を計算
                return Quaternion.LookRotation(NextDirection.normalized, Vector3.up);
            }
        }

        // --------------------------------------------------
        // 内部計算用プロパティ
        // --------------------------------------------------
        /// <summary>弾速減衰率</summary>
        protected float Drag => BASE_BULLET_DRAG / Mass;

        // ======================================================
        // 定数
        // ======================================================

        // --------------------------------------------------
        // 基準値
        // --------------------------------------------------
        /// <summary>基準となる弾速</summary>
        private const float BASE_BULLET_SPEED = 100f;

        /// <summary>基準となる弾丸質量</summary>
        private const float BASE_BULLET_MASS = 1f;

        /// <summary>基準となる弾速減衰値</summary>
        private const float BASE_BULLET_DRAG = 75f;

        /// <summary>基準となる弾丸高度の補間指数</summary>
        public const float BASE_BULLET_HEIGHT_INTERPOLATION_EXPONENT = 1.5f;

        /// <summary>基準となる弾丸のダメージ</summary>
        private const float BASE_BULLET_DAMAGE = 10f;

        // --------------------------------------------------
        // パラメーター
        // --------------------------------------------------
        /// <summary>基準となる砲身ステータス</summary>
        private const float BASE_BARREL_SCALE = 1f;

        /// <summary>砲身1あたりの倍率加算値</summary>
        private const float BARREL_SCALE_MULTIPLIER = 0.05f;

        /// <summary>基準となる質量ステータス</summary>
        private const float BASE_PROJECTILE_MASS = 1f;

        /// <summary>質量1あたりの倍率加算値</summary>
        private const float PROJECTILE_MASS_MULTIPLIER = 0.05f;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>弾丸が終了したことを通知する</summary>
        public event Action<BulletBase> OnDespawnRequested;
        
        // ======================================================
        // 初期化
        // ======================================================

        /// <summary>
        /// Transform を受け取り内部に保持する初期化処理
        /// プール生成時に一度だけ実行される
        /// </summary>
        /// <param name="transform">弾丸の Transform</param>
        public void Initialize(Transform transform)
        {
            Transform = transform;

            // 無効化
            IsEnabled = false;
        }

        // ======================================================
        // セッター
        // ======================================================

        /// <summary>
        /// 弾丸の初期位置をセットする
        /// </summary>
        /// <param name="spawnPosition">セットする座標</param>
        public void SetSpawnPosition(in Vector3 spawnPosition)
        {
            NextPosition = spawnPosition;
        }

        /// <summary>
        /// 衝突対象からダメージ対象を設定
        /// </summary>
        /// <param name="collisionContext">衝突コンテキスト</param>
        private void SetDamageTarget(in BaseCollisionContext collisionContext)
        {
            if (collisionContext.Transform == null)
            {
                _damageTarget = null;
                return;
            }

            // Transform から IDamageable を取得
            IDamageable damageable = collisionContext.Transform.GetComponent<IDamageable>();

            // 取得できた場合のみターゲットに設定
            _damageTarget = damageable;
        }

        // ======================================================
        // BulletBase イベント
        // ======================================================

        /// <summary>
        /// 発射時に戦車ステータスを元に弾丸性能を確定させる
        /// </summary>
        /// <param name="tankStatus">発射元戦車のステータス</param>
        public virtual void ApplyTankStatus(in TankStatus tankStatus)
        {
            // 弾丸の質量を計算
            Mass = BASE_BULLET_MASS * (BASE_PROJECTILE_MASS + tankStatus.ProjectileMass * PROJECTILE_MASS_MULTIPLIER);

            // 弾速を計算
            BulletSpeed = BASE_BULLET_SPEED * (BASE_BARREL_SCALE + tankStatus.BarrelScale * BARREL_SCALE_MULTIPLIER) / Mass;
        }

        /// <summary>
        /// 発射時に Spawn 位置・方向・弾丸 ID を注入
        /// </summary>
        /// <param name="tankId">弾丸の所有者である戦車 ID</param>
        /// <param name="position">発射位置</param>
        /// <param name="direction">飛行方向</param>
        public virtual void OnEnter(int tankId, Vector3 position, Vector3 direction)
        {
            NextPosition = position;
            NextDirection = direction.normalized;

            // 初期弾速、初期高度を取得
            _maxSpeed = BulletSpeed;
            _maxHeight = position.y;

            // Transform の Y スケールの半分を地面接触時の最終高度に設定
            _minHeight = Transform.localScale.y * 0.5f;

            // 弾丸 ID を設定
            BulletId = tankId;

            // 弾丸移動方向を設定
            if (Transform != null)
            {
                Transform.rotation = Quaternion.LookRotation(NextDirection, Vector3.up);
            }

            // 有効化
            IsEnabled = true;

            // Transform に反映
            ApplyToTransform();
        }

        /// <summary>
        /// 毎フレーム呼び出される更新処理
        /// 弾速減衰および速度ゼロ判定で生存判定を行う
        /// </summary>
        /// <param name="deltaTime">前フレームからの経過時間</param>
        public virtual void OnUpdate(float deltaTime)
        {
            if (!IsEnabled || Transform == null)
            {
                return;
            }

            Tick(deltaTime);
        }

        /// <summary>
        /// 弾丸の移動・衝突・追尾などの処理を行う抽象メソッド
        /// </summary>
        /// <param name="deltaTime">前フレームからの経過時間</param>
        public virtual void Tick(float deltaTime)
        {
            if (Transform == null)
            {
                return;
            }

            // --------------------------------------------------
            // 弾速減衰
            // --------------------------------------------------
            BulletSpeed -= Drag * deltaTime;
            if (BulletSpeed <= 0f)
            {
                BulletSpeed = 0f;
                OnExit();
                return;
            }

            // --------------------------------------------------
            // 水平移動
            // --------------------------------------------------
            Vector3 move = NextDirection.normalized * BulletSpeed * deltaTime;
            NextPosition += move;

            // --------------------------------------------------
            // 高度座標補間
            // --------------------------------------------------
            float t = 1f - (BulletSpeed / _maxSpeed);
            t = Mathf.Pow(t, BASE_BULLET_HEIGHT_INTERPOLATION_EXPONENT);
            Vector3 pos = NextPosition;
            pos.y = Mathf.Lerp(_maxHeight, _minHeight, t);
            NextPosition = pos;

            // --------------------------------------------------
            // Transform 反映
            // --------------------------------------------------
            ApplyToTransform();
        }

        /// <summary>
        /// 弾丸の終了処理
        /// アクティブ状態を解除し、必要ならダメージを適用
        /// </summary>
        /// <param name="immediate">true の場合は即時終了処理を行う</param>
        public virtual void OnExit(in bool immediate = false)
        {
            if (!immediate)
            {
                // ダメージ適用
                ApplyDamage();
            }

            // 無効化
            IsEnabled = false;

            // プールへ戻す通知
            OnDespawnRequested?.Invoke(this);
        }

        /// <summary>
        /// 弾丸が何かに衝突した際の共通処理
        /// </summary>
        /// <param name="collisionContext">衝突対象のコンテキスト</param>
        /// <returns>
        /// true: 弾丸が消える場合
        /// false: 弾丸が消えない場合
        /// </returns>
        public virtual bool OnHit(in BaseCollisionContext collisionContext)
        {
            SetDamageTarget(collisionContext);
            
            OnExit();

            return true;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// CurrentPosition を Transform に反映する
        /// </summary>
        protected void ApplyToTransform()
        {
            if (Transform == null)
            {
                return;
            }

            Transform.position = NextPosition;
        }

        /// <summary>
        /// 弾丸が対象に与えるダメージ処理
        /// </summary>
        protected virtual void ApplyDamage()
        {
            if (_damageTarget == null)
            {
                return;
            }

            float damage = BASE_BULLET_DAMAGE + BulletSpeed * Mass;

            _damageTarget.TakeDamage(damage);
            _damageTarget = null;
        }
    }
}