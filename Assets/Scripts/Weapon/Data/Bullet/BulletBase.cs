// ======================================================
// BulletBase.cs
// 作成者   : 高橋一翔
// 作成日   : 2025-12-12
// 更新日   : 2025-12-14
// 概要     : 弾丸ロジックの抽象基底クラス
//            弾速・質量に基づく減衰処理を行い、生存時間を判定する
// ======================================================

using System;
using TankSystem.Data;
using UnityEngine;

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

        /// <summary>弾丸が現在有効かどうかを示すフラグ</summary>
        private bool _isEnabled;

        /// <summary>弾丸の生成座標</summary>
        protected Vector3 _spawnPosition;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>弾丸が現在有効かどうかを</summary>
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

        /// <summary>弾速の基準倍率</summary>
        public float BulletSpeed { get; set; }

        /// <summary>弾丸の質量の基準倍率</summary>
        public float Mass { get; set; }

        /// <summary>弾丸の Transform</summary>
        public Transform Transform { get; private set; }

        /// <summary>弾丸の現在座標</summary>
        public Vector3 CurrentPosition { get; protected set; }

        // --------------------------------------------------
        // 内部計算用プロパティ
        // --------------------------------------------------
        /// <summary>
        /// 弾速減衰率
        /// 大きな質量ほど減衰が緩やかになる
        /// </summary>
        protected virtual float Drag => 1f / Mass;

        // ======================================================
        // 抽象メソッド
        // ======================================================

        /// <summary>
        /// 弾丸の飛行方向を設定する抽象メソッド
        /// 派生クラスで具体的な方向ベクトル処理を実装
        /// </summary>
        /// <param name="direction">初期飛行方向</param>
        protected abstract void SetDirection(Vector3 direction);

        /// <summary>
        /// 弾丸の移動・衝突・追尾などの処理を行う抽象メソッド
        /// 派生クラスで具体的な処理を実装
        /// </summary>
        /// <param name="deltaTime">前フレームからの経過時間</param>
        protected abstract void Tick(float deltaTime);

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// 弾丸が終了したことを通知するイベント
        /// プール側で Despawn 処理を行うために使用する
        /// </summary>
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

            // 現在座標を Transform から取得
            CurrentPosition = transform.position;
        }

        // ======================================================
        // BulletBase イベント
        // ======================================================

        /// <summary>
        /// 発射時に戦車ステータスを元に弾丸性能を確定させる
        /// </summary>
        public virtual void ApplyTankStatus(in TankStatus tankStatus)
        {

        }

        /// <summary>
        /// 発射時に Spawn 位置と方向を注入
        /// </summary>
        /// <param name="position">発射位置</param>
        /// <param name="direction">飛行方向</param>
        public virtual void OnEnter(Vector3 position, Vector3 direction)
        {
            // 発射位置を保持
            _spawnPosition = position;
            CurrentPosition = position;

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
        /// 発射前に位置のみ変更したい場合に使用
        /// </summary>
        /// <param name="spawnPosition">セットする座標</param>
        public void SetSpawnPosition(in Vector3 spawnPosition)
        {
            CurrentPosition = spawnPosition;
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

            Transform.position = CurrentPosition;
        }
    }
}