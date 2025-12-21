// ======================================================
// BulletBase.cs
// 作成者   : 高橋一翔
// 作成日   : 2025-12-12
// 更新日   : 2025-12-14
// 概要     : 弾丸ロジックの抽象基底クラス
//            弾速・質量に基づく減衰処理を行い、生存時間を判定する
// ======================================================

using System;
using UnityEngine;
using TankSystem.Data;

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

        /// <summary>弾丸の生成座標</summary>
        protected Vector3 _spawnPosition;

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

        /// <summary>弾丸の現在座標</summary>
        public Vector3 CurrentPosition { get; protected set; }

        /// <summary>弾丸の移動予定座標</summary>
        public Vector3 NextPosition { get; set; }

        /// <summary>弾丸の移動予定回転</summary>
        public Quaternion NextRotation { get; set; }

        // --------------------------------------------------
        // 内部計算用プロパティ
        // --------------------------------------------------
        /// <summary>
        /// 弾速減衰率
        /// 大きな質量ほど減衰が緩やかになる
        /// </summary>
        protected float Drag => BASE_BULLET_DRAG / Mass;

        // ======================================================
        // 定数
        // ======================================================

        // --------------------------------------------------
        // 基準値
        // --------------------------------------------------
        /// <summary>基準となる弾速</summary>
        private const float BASE_BULLET_SPEED = 75f;

        /// <summary>基準となる弾丸質量</summary>
        private const float BASE_BULLET_MASS = 1f;

        /// <summary>基準となる弾速減衰値</summary>
        private const float BASE_BULLET_DRAG = 50f;

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
        // 抽象メソッド
        // ======================================================

        /// <summary>
        /// 弾丸の飛行方向を設定する抽象メソッド
        /// </summary>
        /// <param name="direction">初期飛行方向</param>
        protected abstract void SetDirection(Vector3 direction);

        /// <summary>
        /// 弾丸の移動・衝突・追尾などの処理を行う抽象メソッド
        /// </summary>
        /// <param name="deltaTime">前フレームからの経過時間</param>
        protected abstract void Tick(float deltaTime);

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

            // 初期状態は非アクティブ
            IsEnabled = false;
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
            // 弾速を計算
            // BASE_BULLET_SPEED: 基準となる弾速
            // BASE_BARREL_SCALE: 基準砲身倍率
            // tankStatus.BarrelScale: 戦車ステータスに応じた砲身スケール
            // BARREL_SCALE_MULTIPLIER: 砲身スケール1あたりの補正係数
            BulletSpeed = BASE_BULLET_SPEED * (BASE_BARREL_SCALE + tankStatus.BarrelScale * BARREL_SCALE_MULTIPLIER);

            // 弾丸の質量を計算
            // BASE_BULLET_MASS: 基準となる弾丸質量
            // BASE_PROJECTILE_MASS: 基準質量倍率
            // tankStatus.ProjectileMass: 戦車ステータスに応じた弾丸質量
            // PROJECTILE_MASS_MULTIPLIER: 弾丸質量1あたりの補正係数
            Mass = BASE_BULLET_MASS * (BASE_PROJECTILE_MASS + tankStatus.ProjectileMass * PROJECTILE_MASS_MULTIPLIER);
        }

        /// <summary>
        /// 発射時に Spawn 位置・方向・弾丸 ID を注入
        /// </summary>
        /// <param name="tankId">弾丸の所有者である戦車 ID</param>
        /// <param name="position">発射位置</param>
        /// <param name="direction">飛行方向</param>
        public virtual void OnEnter(int tankId, Vector3 position, Vector3 direction)
        {
            _spawnPosition = position;
            CurrentPosition = position;

            // 弾丸 ID を設定
            BulletId = tankId;

            // 弾丸を有効化
            IsEnabled = true;

            // 飛行方向を設定
            SetDirection(direction);

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
            if (!IsEnabled)
            {
                return;
            }

            // --------------------------------------------------
            // 弾速減衰
            // --------------------------------------------------
            BulletSpeed -= Drag * deltaTime;

            // 弾速がゼロ以下になった場合は非アクティブ化
            if (BulletSpeed <= 0f)
            {
                BulletSpeed = 0f;
                OnExit();
                return;
            }

            // --------------------------------------------------
            // 弾丸固有処理
            // --------------------------------------------------
            Tick(deltaTime);

            // 座標を Transform に反映
            ApplyToTransform();
        }

        /// <summary>
        /// 弾丸の終了処理
        /// アクティブ状態を解除する
        /// </summary>
        public virtual void OnExit()
        {
            IsEnabled = false;

            // プールへ戻すための通知を発行
            OnDespawnRequested?.Invoke(this);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 弾丸の初期位置をセットする
        /// </summary>
        /// <param name="spawnPosition">セットする座標</param>
        public void SetSpawnPosition(in Vector3 spawnPosition)
        {
            CurrentPosition = spawnPosition;
            NextPosition = CurrentPosition;
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

            CurrentPosition = NextPosition;
            Transform.position = CurrentPosition;
        }
    }
}