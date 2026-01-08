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

        /// <summary>メインカメラ Transform</summary>
        private readonly Transform _mainCameraTransform;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>アイテム生成時の PlayTime を管理する辞書</summary>
        private readonly Dictionary<ItemSlot, float> _spawnTimes
            = new Dictionary<ItemSlot, float>();

        /// <summary>指定方向へ向ける FaceTarget の実行対象管理辞書</summary>
        private readonly Dictionary<ItemSlot, FaceTarget> _faceTargets
            = new Dictionary<ItemSlot, FaceTarget>();

        // ======================================================
        // 遅延削除
        // ======================================================

        /// <summary>
        /// 無効化対象のアイテムを一時的に保持するキュー
        /// </summary>
        private readonly Queue<ItemSlot> _pendingDeactivateSlots
            = new Queue<ItemSlot>();

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>アイテムの生存時間（秒）</summary>
        private const float ITEM_LIFE_TIME = 15.0f;

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
        public void RegisterItem(in ItemSlot slot, in float playTime)
        {
            // null ガード
            if (slot == null)
            {
                return;
            }

            // 重複登録防止
            if (_activeSlots.Contains(slot))
            {
                return;
            }

            // 更新対象に追加
            _activeSlots.Add(slot);

            // 生成時の PlayTime を記録
            _spawnTimes.Add(slot, playTime);

            // FaceTarget を生成して管理
            _faceTargets.Add(
                slot,
                new FaceTarget(slot.Transform, _mainCameraTransform)
            );

            // スロットを有効化
            slot.Activate(ITEM_LIFE_TIME);
        }

        /// <summary>
        /// 更新対象からアイテムスロットを解除する
        /// </summary>
        public void UnregisterItem(ItemSlot slot)
        {
            // null ガード
            if (slot == null)
            {
                return;
            }

            // 管理対象でなければ終了
            if (!_activeSlots.Contains(slot))
            {
                return;
            }

            // 更新対象から除外
            _activeSlots.Remove(slot);

            // 生成時間管理から除外
            _spawnTimes.Remove(slot);

            // FaceTarget の実行管理から除外
            _faceTargets.Remove(slot);
        }

        /// <summary>
        /// 全アイテムを更新する
        /// </summary>
        /// <param name="playTime">Play フェーズ中のみ進行する経過時間</param>
        public void UpdateItems(float playTime)
        {
            // 時間が進まないフレームは処理なし
            if (playTime <= 0.0f)
            {
                return;
            }

            // 後ろから走査
            for (int i = _activeSlots.Count - 1; i >= 0; i--)
            {
                // 更新対象スロットを取得
                ItemSlot slot = _activeSlots[i];

                // 無効スロットは更新しない
                if (!slot.IsEnabled)
                {
                    continue;
                }

                // 生成からの経過時間を算出
                float elapsed = playTime - _spawnTimes[slot];

                // 生存時間超過判定
                if (elapsed >= ITEM_LIFE_TIME)
                {
                    // 遅延削除キューに積む
                    _pendingDeactivateSlots.Enqueue(slot);
                    continue;
                }

                // カメラ追従回転更新
                _faceTargets[slot].UpdateRotation();
            }
        }

        /// <summary>
        /// アイテム無効化処理を実行する
        /// </summary>
        public void DeactivateItems()
        {
            // キューが空になるまで処理
            while (_pendingDeactivateSlots.Count > 0)
            {
                ItemSlot slot = _pendingDeactivateSlots.Dequeue();

                // 既に管理外なら処理しない
                if (!_activeSlots.Contains(slot))
                {
                    continue;
                }

                // スロットを無効化
                slot.Deactivate();

                // 管理対象から除外
                _activeSlots.Remove(slot);

                // 生成時間管理から除外
                _spawnTimes.Remove(slot);

                // FaceTarget 管理から除外
                _faceTargets.Remove(slot);
            }
        }
    }
}