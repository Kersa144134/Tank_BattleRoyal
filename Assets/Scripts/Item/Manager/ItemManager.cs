// ======================================================
// ItemManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-12
// 更新日時 : 2025-12-23
// 概要     : アイテム ON/OFF 管理と Renderer 表示切替を担当する
//            遅延登録/解除に対応
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
    /// Deferred 操作対応版
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
        // Deferred（遅延）操作用リスト
        // ======================================================

        /// <summary>遅延登録対象リスト</summary>
        private readonly List<ItemSlot> _slotsToRegister = new List<ItemSlot>();

        /// <summary>遅延解除対象リスト</summary>
        private readonly List<ItemSlot> _slotsToUnregister = new List<ItemSlot>();

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
        /// アイテムを遅延登録する
        /// </summary>
        /// <param name="slot">対象の ItemSlot</param>
        public void RegisterItem(ItemSlot slot)
        {
            if (slot == null)
            {
                Debug.LogWarning("RegisterItemDeferred: slot が null です。");
                return;
            }

            if (!_slotsToRegister.Contains(slot))
            {
                _slotsToRegister.Add(slot);
            }
        }

        /// <summary>
        /// アイテムを遅延解除する
        /// </summary>
        /// <param name="slot">対象の ItemSlot</param>
        public void UnregisterItem(ItemSlot slot)
        {
            if (slot == null)
            {
                Debug.LogWarning("UnregisterItemDeferred: slot が null です。");
                return;
            }

            if (!_slotsToUnregister.Contains(slot))
            {
                _slotsToUnregister.Add(slot);
            }
        }

        // --------------------------------------------------
        // アイテム更新
        // --------------------------------------------------
        /// <summary>
        /// 有効なアイテムを更新する
        /// </summary>
        public void UpdateItems()
        {
            // FaceTarget をコピーして列挙することでコレクション変更による例外を回避
            foreach (KeyValuePair<ItemSlot, FaceTarget> kvp in new Dictionary<ItemSlot, FaceTarget>(_faceTargets))
            {
                ItemSlot slot = kvp.Key;

                // 有効なアイテムのみ更新処理
                if (!slot.IsEnabled)
                {
                    continue;
                }

                slot.Update();

                ItemRotation(slot);
            }

            // 遅延登録/解除処理を反映
            ProcessDeferred();
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

            // カメラ方向へ回転
            faceTarget.UpdateRotation();
        }

        /// <summary>
        /// 遅延登録・解除の反映
        /// </summary>
        private void ProcessDeferred()
        {
            // 登録処理
            foreach (ItemSlot slot in _slotsToRegister)
            {
                if (slot == null)
                {
                    continue;
                }

                if (!_faceTargets.ContainsKey(slot))
                {
                    _faceTargets[slot] = new FaceTarget(slot.Transform, _mainCameraTransform);
                }
            }
            _slotsToRegister.Clear();

            // 解除処理
            foreach (ItemSlot slot in _slotsToUnregister)
            {
                if (slot == null)
                {
                    continue;
                }

                if (_faceTargets.ContainsKey(slot))
                {
                    _faceTargets.Remove(slot);
                }
            }
            _slotsToUnregister.Clear();

            // 更新後のイベント通知
            _onListChanged?.Invoke(_itemSlots);
        }
    }
}