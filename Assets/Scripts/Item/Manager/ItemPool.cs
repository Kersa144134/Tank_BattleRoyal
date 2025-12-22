// ======================================================
// ItemPool.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-22
// 更新日時 : 2025-12-22
// 概要     : ItemSlot を単位として管理するアイテムプール
//            使用中 / 未使用中を Dictionary で管理する
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using ItemSystem.Controller;
using ItemSystem.Data;
using SceneSystem.Interface;

namespace ItemSystem.Manager
{
    /// <summary>
    /// ItemSlot を基準にアイテムを管理するプールクラス
    /// </summary>
    public sealed class ItemPool : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("アイテムスロット")]
        /// <summary>使用対象となるアイテムスロット配列</summary>
        [SerializeField] private ItemSlot[] _itemSlots;

        [Header("生成ポイント")]
        /// <summary>生成ポイント Transform 配列</summary>
        [SerializeField] private Transform _spawnPointsRoot;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>
        /// ランダム抽選・再抽選・上限管理を行うコントローラークラス
        /// </summary>
        private ItemPickController _itemPickController = new ItemPickController();

        /// <summary>
        /// アイテムの生成・生存管理を制御するコントローラークラス
        /// </summary>
        private ItemSpawnController _itemSpawnController;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>現在使用中の ItemSlot を ItemData 単位で管理</summary>
        private readonly Dictionary<ItemData, List<ItemSlot>> _activeItems
            = new Dictionary<ItemData, List<ItemSlot>>();

        /// <summary>未使用状態の ItemSlot を ItemData 単位で管理</summary>
        private readonly Dictionary<ItemData, Queue<ItemSlot>> _inactiveItems
            = new Dictionary<ItemData, Queue<ItemSlot>>();

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>各 ItemData ごとの初期生成数</summary>
        private const int INITIAL_ITEM_COUNT = 5;
        
        // ======================================================
        // イベント
        // ======================================================

        /// <summary>アイテムが使用状態になった通知イベント</summary>
        public event Action<ItemSlot> OnItemActivated;

        /// <summary>アイテムが未使用状態に戻った通知イベント</summary>
        public event Action<ItemSlot> OnItemDeactivated;

        // ======================================================
        // IUpdatable 実装
        // ======================================================

        public void OnEnter()
        {
            _itemSpawnController = new ItemSpawnController(_spawnPointsRoot);

            // プール初期化処理を委譲
            InitializePool();

            // イベント購読
            _itemSpawnController.OnSpawnTimingReached += HandleSpawnTimingReached;
            _itemSpawnController.OnItemSpawned += HandleItemSpawned;
        }

        public void OnUpdate()
        {
            _itemSpawnController.Update();
        }

        public void OnExit()
        {
            // 使用中スロットの全無効化
            foreach (List<ItemSlot> list in _activeItems.Values)
            {
                foreach (ItemSlot slot in list)
                {
                    slot.IsEnabled = false;
                }
            }
            
            // 管理データを全消去
            _activeItems.Clear();
            _inactiveItems.Clear();
            
            // イベント購読解除
            _itemSpawnController.OnSpawnTimingReached -= HandleSpawnTimingReached;
            _itemSpawnController.OnItemSpawned -= HandleItemSpawned;

            foreach (List<ItemSlot> list in _activeItems.Values)
            {
                foreach (ItemSlot slot in list)
                {
                    slot.OnDeactivateRequested -= Deactivate;
                }
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ランダムに未使用スロットを取得して有効化し、生成ポイントに配置する
        /// </summary>
        /// <returns>生成可能な ItemSlot。存在しない場合は null</returns>
        public ItemSlot Activate()
        {
            if (_itemPickController == null)
            {
                return null;
            }

            // 未使用スロットが存在する Queue をランダムに選択
            Queue<ItemSlot> selectedQueue = null;

            foreach (KeyValuePair<ItemData, Queue<ItemSlot>> kvp in _inactiveItems)
            {
                if (kvp.Value.Count > 0)
                {
                    selectedQueue = kvp.Value;
                    break;
                }
            }

            if (selectedQueue == null)
            {
                // 在庫なし
                return null;
            }

            // Queue を PickRandom に渡して抽選
            ItemSlot slot = _itemPickController.PickRandom(selectedQueue);
            if (slot == null)
            {
                return null;
            }

            // 使用中リストに登録
            if (!_activeItems.ContainsKey(slot.ItemData))
            {
                _activeItems.Add(slot.ItemData, new List<ItemSlot>());
            }
            _activeItems[slot.ItemData].Add(slot);

            // 生成ポイントに配置
            _itemSpawnController.SpawnItemAtRandomPoint(slot);

            // スロットを有効化
            slot.IsEnabled = true;

            Debug.Log(slot.ItemData);

            return slot;
        }

        /// <summary>
        /// 指定した ItemSlot を未使用状態へ戻す
        /// </summary>
        /// <param name="slot">非アクティブ化する ItemSlot</param>
        public void Deactivate(ItemSlot slot)
        {
            // ItemData を取得
            ItemData itemData = slot.ItemData;

            // 使用中管理に存在しない場合は処理不可
            if (!_activeItems.TryGetValue(itemData, out List<ItemSlot> list))
            {
                return;
            }

            // 使用中リストから削除
            list.Remove(slot);

            // スロットを無効化
            slot.IsEnabled = false;

            // 未使用キューへ戻す
            _inactiveItems[itemData].Enqueue(slot);

            // 使用終了イベント通知
            OnItemDeactivated?.Invoke(slot);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>ItemSlot プールを構築する</summary>
        private void InitializePool()
        {
            // プレハブ配列を走査
            foreach (ItemSlot sourceSlot in _itemSlots)
            {
                // ItemData / Transform 未設定は対象外
                if (sourceSlot.ItemData == null || sourceSlot.Transform == null)
                {
                    continue;
                }

                // 未使用・使用中管理用辞書の初期化
                if (!_inactiveItems.ContainsKey(sourceSlot.ItemData))
                {
                    _inactiveItems.Add(sourceSlot.ItemData, new Queue<ItemSlot>());
                    _activeItems.Add(sourceSlot.ItemData, new List<ItemSlot>());
                }

                // 初期インスタンス生成
                for (int i = 0; i < INITIAL_ITEM_COUNT; i++)
                {
                    // GameObject を Instantiate
                    GameObject instanceGO = Instantiate(
                        sourceSlot.Transform.gameObject,
                        transform
                    );

                    // ItemSlot インスタンス生成
                    ItemSlot instance = new ItemSlot(instanceGO.transform, sourceSlot.ItemData);

                    // 初期状態は無効
                    instance.IsEnabled = false;

                    // 未使用キューに追加
                    _inactiveItems[sourceSlot.ItemData].Enqueue(instance);

                    // イベント購読
                    instance.OnDeactivateRequested += Deactivate;
                }
            }
        }

        // ======================================================
        // イベントハンドラ
        // ======================================================

        /// <summary>
        /// 生成タイミング到達時に呼ばれるハンドラ
        /// </summary>
        private void HandleSpawnTimingReached()
        {
            ItemSlot slot = Activate();
        }

        /// <summary>
        /// アイテムが実際に生成されたときに呼ばれるハンドラ
        /// </summary>
        /// <param name="slot">生成された ItemSlot</param>
        private void HandleItemSpawned(ItemSlot slot)
        {
            if (slot == null)
            {
                return;
            }

            OnItemActivated?.Invoke(slot);
        }
    }
}
