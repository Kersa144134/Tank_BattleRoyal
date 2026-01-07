// ======================================================
// ItemSlot.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-11
// 更新日時 : 2026-01-07
// 概要     : ItemData と Transform を保持するデータスロット
// ======================================================

using System;
using UnityEngine;

namespace ItemSystem.Data
{
    /// <summary>
    /// アイテム 1 つ分のデータスロット
    /// </summary>
    public sealed class ItemSlot
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>有効状態フラグ</summary>
        private bool _isEnabled;

        /// <summary>アイテム表示用 Transform</summary>
        private readonly Transform _transform;

        /// <summary>アイテムの詳細データを保持する ItemData</summary>
        private readonly ItemData _itemData;

        /// <summary>表示制御用 Renderer</summary>
        private readonly Renderer _renderer;

        /// <summary>生成時刻</summary>
        private float _spawnTime;

        /// <summary>生存時間（秒）</summary>
        private float _lifeTime;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>Transform 参照</summary>
        public Transform Transform => _transform;

        /// <summary>ItemData 参照</summary>
        public ItemData ItemData => _itemData;

        /// <summary>有効状態</summary>
        public bool IsEnabled => _isEnabled;

        /// <summary>生成時刻</summary>
        public float SpawnTime => _spawnTime;

        /// <summary>生存時間</summary>
        public float LifeTime => _lifeTime;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// 無効化要求イベント
        /// </summary>
        public event Action<ItemSlot> OnDeactivated;
        
        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// ItemSlot を生成する
        /// </summary>
        /// <param name="transform">表示用 Transform</param>
        /// <param name="itemData">ItemData</param>
        public ItemSlot(Transform transform, ItemData itemData)
        {
            // Transform を保持
            _transform = transform;

            // ItemData を保持
            _itemData = itemData;

            // 初期状態は無効
            _isEnabled = false;

            // 初期時刻をリセット
            _spawnTime = 0.0f;

            // 生存時間を初期化
            _lifeTime = 0.0f;

            // Transform が存在する場合のみ Renderer を取得
            if (_transform != null)
            {
                _renderer = _transform.GetComponentInChildren<Renderer>(true);

                // 初期状態では非表示
                if (_renderer != null)
                {
                    _renderer.enabled = false;
                }
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定位置・寿命で有効化する
        /// </summary>
        public void Activate(in float lifeTime)
        {
            // Transform 未設定は無効
            if (_transform == null)
            {
                return;
            }

            // 生成時刻を記録
            _spawnTime = Time.time;

            // 生存時間を設定（マイナス防止）
            _lifeTime = Mathf.Max(0.0f, lifeTime);

            // 有効化
            SetEnabled(true);
        }

        /// <summary>
        /// 無効化する
        /// </summary>
        public void Deactivate()
        {
            // すでに無効なら処理なし
            if (!_isEnabled)
            {
                return;
            }
            
            // 表示・状態を無効化
            SetEnabled(false);

            // 無効化通知を発行
            OnDeactivated?.Invoke(this);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 有効状態を Renderer に反映する
        /// </summary>
        private void SetEnabled(bool isEnabled)
        {
            // 状態変化がない場合は処理なし
            if (_isEnabled == isEnabled)
            {
                return;
            }

            // 内部状態を更新
            _isEnabled = isEnabled;

            // Renderer が存在しない場合は処理なし
            if (_renderer == null)
            {
                return;
            }

            // Renderer の有効状態を同期
            _renderer.enabled = _isEnabled;
        }
    }
}