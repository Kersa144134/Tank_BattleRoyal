// ======================================================
// BulletBase.cs
// 作成者   : 高橋一翔
// 作成日   : 2025-12-12
// 更新日   : 2025-12-12
// 概要     : 弾丸ロジックの抽象基底クラス
// ======================================================

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

        /// <summary>時間による寿命判定で使用されるタイマー</summary>
        protected float _lifetimeTimer;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>弾丸が現在有効かどうかを示すフラグ</summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            protected set
            {
                _isEnabled = value;
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

        /// <summary>弾丸の Transform</summary>
        public Transform Transform { get; private set; }

        /// <summary>弾丸の現在座標</summary>
        public Vector3 CurrentPosition { get; protected set; }

        /// <summary>弾丸の速度</summary>
        public float BulletSpeed { get; set; }

        /// <summary>弾丸の寿命</summary>
        public float Lifetime { get; set; }

        // ======================================================
        // 抽象メソッド
        // ======================================================

        /// <summary>
        /// 派生クラス固有の移動・衝突・追尾などの処理
        /// </summary>
        protected abstract void Tick(float deltaTime);

        // ======================================================
        // 初期化処理
        // ======================================================

        /// <summary>
        /// Transform を受け取り内部に保持する初期化処理
        /// プール生成時に一度だけ実行される
        /// </summary>
        public void Initialize(Transform transform)
        {
            Transform = transform;
            IsEnabled = false;
            CurrentPosition = transform.position;
        }

        // ======================================================
        // IBullet イベント
        // ======================================================

        /// <summary>
        /// 弾丸が使用開始されたフレームで呼び出される
        /// </summary>
        public virtual void OnEnter(Vector3 direction)
        {
            IsEnabled = true;
            _lifetimeTimer = 0f;
        }

        /// <summary>
        /// 毎フレーム呼び出される更新処理
        /// </summary>
        public virtual void OnUpdate(float deltaTime)
        {
            if (!IsEnabled)
            {
                return;
            }

            _lifetimeTimer += deltaTime;

            if (_lifetimeTimer >= Lifetime)
            {
                OnExit();
                return;
            }

            Tick(deltaTime);
            ApplyToTransform();
        }

        /// <summary>
        /// 弾丸の終了処理
        /// </summary>
        public virtual void OnExit()
        {
            IsEnabled = false;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 弾丸の初期位置をセットする
        /// </summary>
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