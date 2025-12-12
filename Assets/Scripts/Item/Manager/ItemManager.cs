// ======================================================
// ItemManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-12
// 更新日時 : 2025-12-12
// 概要     : アイテム ON/OFF 管理と Renderer 表示切替を担当する
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using TankSystem.Data;

namespace TankSystem.Manager
{
    /// <summary>
    /// アイテム ON/OFF 管理および表示切替を専任で担当するマネージャ
    /// MonoBehaviour 非継承
    /// </summary>
    public class ItemManager
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>SceneObjectRegistry が保持するアイテムリストの参照</summary>
        private List<ItemSlot> _itemSlots;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>外部へイベントを通知するためのデリゲート参照</summary>
        private Action<List<ItemSlot>> _onListChanged;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ItemManager(List<ItemSlot> slots, Action<List<ItemSlot>> callback)
        {
            _itemSlots = slots;
            _onListChanged = callback;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// アイテムを有効化
        /// </summary>
        public void AddItem(ItemSlot slot)
        {
            // Transform 参照で検索
            int index = _itemSlots.FindIndex(s => s.ItemTransform == slot.ItemTransform);
            if (index < 0)
            {
                return;
            }

            // 有効化
            ItemSlot target = _itemSlots[index];
            target.IsEnabled = true;

            // Renderer の表示切り替え
            ToggleRenderer(target.ItemTransform, true);

            // イベント発火
            _onListChanged?.Invoke(_itemSlots);
        }

        /// <summary>
        /// アイテムを無効化
        /// </summary>
        public void RemoveItem(ItemSlot slot)
        {
            // Transform 参照で検索
            int index = _itemSlots.FindIndex(s => s.ItemTransform == slot.ItemTransform);
            if (index < 0)
            {
                return;
            }

            // 無効化
            ItemSlot target = _itemSlots[index];
            target.IsEnabled = false;

            // Renderer の表示切り替え
            ToggleRenderer(target.ItemTransform, false);

            // イベント発火
            _onListChanged?.Invoke(_itemSlots);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// オブジェクトの Renderer.enabled を切り替える
        /// </summary>
        private void ToggleRenderer(Transform target, bool enabled)
        {
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = enabled;
            }
        }
    }
}