// ======================================================
// ItemSlot.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-11
// 更新日時 : 2026-01-14
// 概要     : ItemData / Transform / 生成グリッドキーを保持するデータスロット
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
        // 構造体
        // ======================================================

        /// <summary>
        /// 生成グリッドを一意に識別するキー構造体
        /// SpawnPointIndex とローカルオフセットで構成される
        /// </summary>
        public readonly struct SpawnGridKey
        {
            /// <summary>基準となる SpawnPoint のインデックス</summary>
            public readonly int SpawnPointIndex;

            /// <summary>X方向のローカルオフセット</summary>
            public readonly int OffsetX;

            /// <summary>Z方向のローカルオフセット</summary>
            public readonly int OffsetZ;

            /// <summary>
            /// グリッドキーを初期化するコンストラクタ
            /// </summary>
            /// <param name="spawnPointIndex">SpawnPoint インデックス</param>
            /// <param name="offsetX">X方向オフセット</param>
            /// <param name="offsetZ">Z方向オフセット</param>
            public SpawnGridKey(
                int spawnPointIndex,
                int offsetX,
                int offsetZ)
            {
                SpawnPointIndex = spawnPointIndex;
                OffsetX = offsetX;
                OffsetZ = offsetZ;
            }
        }

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>有効状態フラグ</summary>
        private bool _isEnabled;

        /// <summary>アイテム表示用 Transform</summary>
        private readonly Transform _transform;

        /// <summary>アイテムの詳細データ</summary>
        private readonly ItemData _itemData;

        /// <summary>表示制御用 Renderer</summary>
        private readonly Renderer _renderer;

        /// <summary>
        /// 現在占有している生成グリッドキー
        /// Release 時に逆算を行わないために保持する
        /// </summary>
        private SpawnGridKey _spawnGridKey;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>Transform 参照</summary>
        public Transform Transform => _transform;

        /// <summary>ItemData 参照</summary>
        public ItemData ItemData => _itemData;

        /// <summary>有効状態</summary>
        public bool IsEnabled => _isEnabled;

        /// <summary>現在保持している生成グリッドキー</summary>
        public SpawnGridKey GridKey => _spawnGridKey;

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
        public ItemSlot(
            in Transform transform,
            in ItemData itemData)
        {
            // Transform を保持
            _transform = transform;

            // ItemData を保持
            _itemData = itemData;

            // 初期状態は無効
            _isEnabled = false;

            // グリッドキーを初期化
            _spawnGridKey = default;

            // Transform が存在する場合のみ Renderer を取得
            if (_transform != null)
            {
                // 子オブジェクト含めて Renderer を取得
                _renderer =
                    _transform.GetComponentInChildren<Renderer>(true);

                // Renderer が存在する場合のみ非表示に設定
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
        /// アイテムを有効化する
        /// </summary>
        public void Activate()
        {
            if (_transform == null)
            {
                return;
            }

            SetEnabled(true);
        }

        /// <summary>
        /// アイテムを無効化する
        /// </summary>
        public void Deactivate()
        {
            // すでに無効なら処理なし
            if (!_isEnabled)
            {
                return;
            }

            SetEnabled(false);

            // 無効化通知を発行
            OnDeactivated?.Invoke(this);
        }

        /// <summary>
        /// 生成グリッドキーを設定する
        /// </summary>
        /// <param name="spawnGridKey">割り当てられたキー</param>
        public void SetSpawnGridKey(
            in SpawnGridKey spawnGridKey)
        {
            _spawnGridKey = spawnGridKey;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 有効状態を Renderer に反映する
        /// </summary>
        /// <param name="isEnabled">設定する状態</param>
        private void SetEnabled(
            in bool isEnabled)
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