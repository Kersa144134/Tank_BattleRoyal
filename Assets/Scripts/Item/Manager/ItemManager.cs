// ======================================================
// ItemManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-12
// 更新日時 : 2025-12-14
// 概要     : アイテム ON/OFF 管理と Renderer 表示切替を担当する
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using ItemSystem.Data;
using TransformSystem.Utility;

namespace TankSystem.Manager
{
    /// <summary>
    /// アイテム ON/OFF 管理および表示切替を担当するマネージャ
    /// </summary>
    public class ItemManager
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>アイテムリスト</summary>
        private List<ItemSlot> _itemSlots;
        
        /// <summary>メインカメラの Transform</summary>
        private readonly Transform _mainCameraTransform;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>有効なアイテムの FaceTarget 管理用辞書</summary>
        private Dictionary<ItemSlot, FaceTarget> _faceTargets = new Dictionary<ItemSlot, FaceTarget>();

        /// <summary>ItemSlot ごとの SpriteRenderer 管理用辞書</summary>
        private Dictionary<ItemSlot, SpriteRenderer> _spriteRenderers = new Dictionary<ItemSlot, SpriteRenderer>();

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>外部へイベントを通知するためのデリゲート参照</summary>
        private Action<List<ItemSlot>> _onListChanged;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// ItemManager のコンストラクタ
        /// </summary>
        /// <param name="slots">アイテムリスト</param>
        /// <param name="callback">アイテムリスト変更時に呼び出されるデリゲート</param>
        /// <param name="mainCameraTransform">FaceTarget で使用するメインカメラの Transform</param>
        public ItemManager(
            in List<ItemSlot> slots,
            in Action<List<ItemSlot>> callback,
            in Transform mainCameraTransform
        )
        {
            _itemSlots = slots;
            _onListChanged = callback;
            _mainCameraTransform = mainCameraTransform;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // アイテム登録
        // --------------------------------------------------
        /// <summary>
        /// アイテムを有効化
        /// </summary>
        /// <param name="slot">対象の ItemSlot</param>
        public void RegisterItem(ItemSlot slot)
        {
            // Transform 参照で検索
            int index = _itemSlots.FindIndex(s => s.Transform == slot.Transform);
            if (index < 0)
            {
                return;
            }

            ItemSlot target = _itemSlots[index];

            // FaceTarget を生成して管理
            if (!_faceTargets.ContainsKey(target))
            {
                _faceTargets[target] = new FaceTarget(target.Transform, _mainCameraTransform);
            }

            // イベント発火
            _onListChanged?.Invoke(_itemSlots);
        }

        /// <summary>
        /// アイテムを無効化
        /// </summary>
        /// <param name="slot">対象の ItemSlot</param>
        public void UnregisterItem(ItemSlot slot)
        {
            // Transform 参照で検索
            int index = _itemSlots.FindIndex(s => s.Transform == slot.Transform);
            if (index < 0)
            {
                return;
            }

            ItemSlot target = _itemSlots[index];

            // FaceTarget を破棄
            if (_faceTargets.ContainsKey(target))
            {
                _faceTargets.Remove(target);
            }

            // イベント発火
            _onListChanged?.Invoke(_itemSlots);
        }

        // --------------------------------------------------
        // アイテム更新
        // --------------------------------------------------
        /// <summary>
        /// 有効なアイテムを更新する
        /// </summary>
        public void UpdateItems()
        {
            foreach (KeyValuePair<ItemSlot, FaceTarget> kvp in _faceTargets)
            {
                ItemSlot slot = kvp.Key;

                // 有効なアイテムのみ更新処理を行う
                if (!slot.IsEnabled)
                {
                    continue;
                }

                slot.Update();
                
                ItemRotation(slot);
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 指定されたアイテムをカメラ方向に向ける
        /// </summary>
        /// <param name="slot">回転対象の ItemSlot</param>
        private void ItemRotation(in ItemSlot slot)
        {
            if (slot == null)
            {
                return;
            }

            // FaceTarget が登録されていない場合は処理しない
            if (!_faceTargets.TryGetValue(slot, out FaceTarget faceTarget))
            {
                return;
            }

            // カメラ方向へ回転させる
            faceTarget.UpdateRotation();
        }
    }
}