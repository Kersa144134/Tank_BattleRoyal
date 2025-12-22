// ======================================================
// ItemSlot.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-11
// 更新日時 : 2025-12-22
// 概要     : ItemData と Transform を束ねたスロットクラス
// ======================================================

using System;
using UnityEngine;

namespace ItemSystem.Data
{
    /// <summary>
    /// ItemData と Transform を束ねたスロットクラス
    /// </summary>
    [Serializable]
    public sealed class ItemSlot
    {
        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// 自己 Deactivate 要求イベント
        /// </summary>
        public event Action<ItemSlot> OnDeactivateRequested;

        // ======================================================
        // インスペクタ設定
        // ======================================================

        /// <summary>アイテムの見た目となる Transform</summary>
        [SerializeField]
        private Transform _transform;

        /// <summary>登録するアイテムデータ ScriptableObject</summary>
        [SerializeField]
        private ItemData _itemData;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>アイテムが有効かどうかを示す内部フラグ</summary>
        private bool _isEnabled;

        /// <summary>生成された時刻</summary>
        private float _spawnTime;

        /// <summary>アイテムの生存時間（秒）</summary>
        private float _lifeTime;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>アイテムの Transform</summary>
        public Transform Transform => _transform;

        /// <summary>紐づいている ItemData</summary>
        public ItemData ItemData => _itemData;

        /// <summary>アイテムが有効かどうか</summary>
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                // 状態が変化しない場合は処理不要
                if (_isEnabled == value)
                {
                    return;
                }

                // 内部状態を更新
                _isEnabled = value;

                // 表示状態を同期
                ApplyRendererState();
            }
        }

        /// <summary>アイテムの生存時間（秒）</summary>
        public float LifeTime
        {
            get
            {
                return _lifeTime;
            }
            set
            {
                // マイナス値防止
                _lifeTime = Mathf.Max(0.0f, value);
            }
        }

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// ItemSlot を生成する
        /// </summary>
        /// <param name="transform">アイテム表示用 Transform</param>
        /// <param name="itemData">紐づける ItemData</param>
        public ItemSlot(Transform transform, ItemData itemData)
        {
            _transform = transform;
            _itemData = itemData;
            _isEnabled = false;
            _lifeTime = 0.0f;
            _spawnTime = 0.0f;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定した座標に移動し、生存時間を設定して生成状態にする
        /// </summary>
        /// <param name="position">生成座標</param>
        /// <param name="lifeTime">生存時間（秒）</param>
        public void Spawn(Vector3 position, float lifeTime)
        {
            // Transform が未設定の場合は処理しない
            if (_transform == null)
            {
                return;
            }

            // 生成座標を設定
            _transform.position = position;

            // 生存時間を設定
            LifeTime = lifeTime;
        }
        
        /// <summary>
        /// 毎フレーム呼び出される更新処理
        /// </summary>
        public void Update()
        {
            if (!_isEnabled)
            {
                return;
            }

            // 現在時刻を取得
            float currentTime = Time.time;

            // 生存時間を超過していない場合は処理不要
            if (currentTime - _spawnTime < LifeTime)
            {
                return;
            }

            // 自己 Deactivate 要求イベントを通知
            OnDeactivateRequested?.Invoke(this);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// IsEnabled の状態を Renderer に反映する
        /// </summary>
        private void ApplyRendererState()
        {
            // Transform 未設定時は処理不可
            if (_transform == null)
            {
                return;
            }

            // 子オブジェクト含め Renderer を 1 つ取得
            Renderer renderer = _transform.GetComponentInChildren<Renderer>(true);

            // Renderer が存在しない場合は処理不要
            if (renderer == null)
            {
                return;
            }

            // Renderer の有効状態を内部フラグに同期
            renderer.enabled = _isEnabled;
        }
    }
}