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
using TankSystem.Data;
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
        public void AddItem(ItemSlot slot)
        {
            // Transform 参照で検索
            int index = _itemSlots.FindIndex(s => s.Transform == slot.Transform);
            if (index < 0)
            {
                return;
            }

            // 有効化
            ItemSlot target = _itemSlots[index];
            target.IsEnabled = true;

            // FaceTarget を生成して管理
            if (!_faceTargets.ContainsKey(target))
            {
                _faceTargets[target] = new FaceTarget(target.Transform, _mainCameraTransform);
            }

            // SpriteRenderer を登録
            if (!_spriteRenderers.ContainsKey(target))
            {
                SpriteRenderer spriteRenderer = target.Transform.GetComponentInChildren<SpriteRenderer>(true);

                if (spriteRenderer != null)
                {
                    _spriteRenderers[target] = spriteRenderer;
                }
            }

            // Renderer の表示切り替え
            ToggleRenderer(target.Transform, true);

            // イベント発火
            _onListChanged?.Invoke(_itemSlots);
        }

        /// <summary>
        /// アイテムを無効化
        /// </summary>
        /// <param name="slot">対象の ItemSlot</param>
        public void RemoveItem(ItemSlot slot)
        {
            // Transform 参照で検索
            int index = _itemSlots.FindIndex(s => s.Transform == slot.Transform);
            if (index < 0)
            {
                return;
            }

            // 無効化
            ItemSlot target = _itemSlots[index];
            target.IsEnabled = false;

            // Renderer の表示切り替え
            ToggleRenderer(target.Transform, false);

            // FaceTarget を破棄
            if (_faceTargets.ContainsKey(target))
            {
                _faceTargets.Remove(target);
            }

            // イベント発火
            _onListChanged?.Invoke(_itemSlots);
        }

        // --------------------------------------------------
        // アイテム回転
        // --------------------------------------------------
        /// <summary>
        /// 有効なアイテムをカメラ方向に向ける
        /// </summary>
        public void UpdateItemRotations()
        {
            foreach (KeyValuePair<ItemSlot, FaceTarget> kvp in _faceTargets)
            {
                ItemSlot slot = kvp.Key;
                FaceTarget faceTarget = kvp.Value;

                // IsEnabled のみ回転
                if (slot.IsEnabled)
                {
                    faceTarget.UpdateRotation();
                }
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 指定オブジェクトの子オブジェクトにある SpriteRenderer の enabled を切り替える
        /// </summary>
        /// <param name="target">SpriteRenderer を操作する対象の Transform</param>
        /// <param name="enabled">有効化する場合は true、無効化する場合は false</param>
        private void ToggleRenderer(Transform target, bool enabled)
        {
            if (target == null)
            {
                return;
            }

            // 子オブジェクトにある SpriteRenderer を取得
            SpriteRenderer spriteRenderer = target.GetComponentInChildren<SpriteRenderer>(true);
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = enabled;
            }
        }
    }
}