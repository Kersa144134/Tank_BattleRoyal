// ======================================================
// ItemManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-12
// 更新日時 : 2026-01-07
// 概要     : アイテムの寿命・回転・登録管理を一元管理する
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using ItemSystem.Data;
using TransformSystem.Utility;

namespace TankSystem.Manager
{
    /// <summary>
    /// アイテム管理マネージャ
    /// </summary>
    public sealed class ItemManager
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>更新対象のアイテムスロット</summary>
        private readonly List<ItemSlot> _activeSlots = new List<ItemSlot>();

        /// <summary>FaceTarget 管理辞書</summary>
        private readonly Dictionary<ItemSlot, FaceTarget> _faceTargets
            = new Dictionary<ItemSlot, FaceTarget>();

        /// <summary>メインカメラ Transform</summary>
        private readonly Transform _mainCameraTransform;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// ItemManager を生成する
        /// </summary>
        public ItemManager(Transform mainCameraTransform)
        {
            _mainCameraTransform = mainCameraTransform;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 更新対象としてアイテムスロットを登録する
        /// </summary>
        public void RegisterItem(ItemSlot slot)
        {
            if (slot == null)
            {
                return;
            }

            // 既に登録済みなら処理しない
            if (_activeSlots.Contains(slot))
            {
                return;
            }

            // 更新対象に追加
            _activeSlots.Add(slot);

            // FaceTarget を生成して管理
            _faceTargets.Add(
                slot,
                new FaceTarget(slot.Transform, _mainCameraTransform)
            );

            // スロットを有効化
            slot.Activate(15f);
        }

        /// <summary>
        /// 更新対象からアイテムスロットを解除する
        /// </summary>
        public void UnregisterItem(ItemSlot slot)
        {
            if (slot == null)
            {
                return;
            }

            // 管理対象でなければ終了
            if (_activeSlots.Contains(slot) == false)
            {
                return;
            }

            // 管理対象から除外
            _activeSlots.Remove(slot);

            // FaceTarget を破棄
            _faceTargets.Remove(slot);
        }

        /// <summary>
        /// 全アイテムを更新する
        /// </summary>
        public void UpdateItems()
        {
            // 後ろから走査
            for (int i = _activeSlots.Count - 1; i >= 0; i--)
            {
                ItemSlot slot = _activeSlots[i];

                // 無効スロットは更新しない
                if (!slot.IsEnabled)
                {
                    continue;
                }

                // 経過時間を算出
                float elapsed = Time.time - slot.SpawnTime;

                // 生存時間超過判定
                if (elapsed >= slot.LifeTime)
                {
                    DeactivateItem(slot);
                    continue;
                }

                // カメラ追従回転更新
                _faceTargets[slot].UpdateRotation();
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// アイテムを無効化する
        /// </summary>
        private void DeactivateItem(ItemSlot slot)
        {
            // スロットを無効化
            slot.Deactivate();

            // 更新対象から除外
            _activeSlots.Remove(slot);

            // FaceTarget を破棄
            _faceTargets.Remove(slot);
        }
    }
}