// ======================================================
// ItemPool.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-22
// 更新日時 : 2026-01-07
// 概要     : ItemSlot を管理するプールクラス
//            取得 / 返却をイベントで通知する
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
    /// ItemSlot を管理するプールクラス
    /// </summary>
    public sealed class ItemPool : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // プライベートクラス
        // ======================================================

        /// <summary>
        /// ItemData と Prefab の定義
        /// </summary>
        [Serializable]
        private sealed class ItemEntry
        {
            /// <summary>アイテムデータ定義</summary>
            [SerializeField] public ItemData ItemData;

            /// <summary>表示用プレハブ</summary>
            [SerializeField] public GameObject ItemModel;
        }

        // ======================================================
        // インスペクタ
        // ======================================================

        /// <summary>アイテム定義配列</summary>
        [SerializeField]
        private ItemEntry[] _itemEntries;

        /// <summary>生成ポイントの親 Transform</summary>
        [SerializeField]
        private Transform _spawnPointsRoot;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>アイテム生成を自動制御するコントローラー</summary>
        private ItemSpawnController _spawnController;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>
        /// 未使用 ItemSlot を ItemData 単位で管理する辞書
        /// </summary>
        private readonly Dictionary<ItemData, Queue<ItemSlot>> _inactiveItems
            = new Dictionary<ItemData, Queue<ItemSlot>>();

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>各 ItemData ごとの初期生成数</summary>
        private const int INITIAL_ITEM_COUNT = 20;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// ItemSlot が有効化（取得）された際の通知イベント
        /// </summary>
        public event Action<ItemSlot> OnItemActivated;

        /// <summary>
        /// ItemSlot が無効化（返却）された際の通知イベント
        /// </summary>
        public event Action<ItemSlot> OnItemDeactivated;

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void OnEnter()
        {
            // 生成制御コントローラーを生成
            _spawnController = new ItemSpawnController(_spawnPointsRoot);

            // イベント購読
            _spawnController.OnSpawnPositionDetermined += HandleSpawnPositionDetermined;

            // プールを初期化
            InitializePool();
        }

        public void OnUpdate(in float playTime)
        {
            // 生成制御コントローラーを更新
            _spawnController.Update();
        }

        public void OnExit()
        {
            // 未使用プールをクリア
            _inactiveItems.Clear();

            // イベント購読解除
            _spawnController.OnSpawnPositionDetermined -= HandleSpawnPositionDetermined;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 未使用プールから ItemSlot をランダムに取り出す
        /// </summary>
        /// <param name="spawnPosition">生成座標</param>
        /// <returns>取得した ItemSlot</returns>
        public ItemSlot Activate(in Vector3 spawnPosition)
        {
            // 抽選により未使用キューを取得
            Queue<ItemSlot> selectedQueue = GetRandomInactiveQueue();

            // 抽選対象が存在しない場合は取得不可
            if (selectedQueue == null)
            {
                return null;
            }

            // 未使用キューから ItemSlot を取得
            ItemSlot slot = selectedQueue.Dequeue();

            // イベント購読
            slot.OnDeactivated += HandleSlotDeactivated;

            // 生成位置へ移動
            slot.Transform.position = spawnPosition;

            // 有効化イベントを通知
            OnItemActivated?.Invoke(slot);

            return slot;
        }

        /// <summary>
        /// ItemSlot を未使用状態としてプールへ返却する
        /// </summary>
        /// <param name="slot">返却対象の ItemSlot</param>
        public void Deactivate(in ItemSlot slot)
        {
            if (slot == null || slot.Transform == null)
            {
                return;
            }

            // イベント購読解除
            slot.OnDeactivated -= HandleSlotDeactivated;

            // プールへ返却
            _inactiveItems[slot.ItemData].Enqueue(slot);

            // 無効化イベント通知
            OnItemDeactivated?.Invoke(slot);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// プールを初期化する
        /// </summary>
        private void InitializePool()
        {
            // 管理辞書を初期化
            _inactiveItems.Clear();

            // アイテム定義を走査
            foreach (ItemEntry entry in _itemEntries)
            {
                // 不正定義は除外
                if (entry.ItemData == null || entry.ItemModel == null)
                {
                    continue;
                }

                // ItemData 単位の未使用キューを生成
                Queue<ItemSlot> queue = new Queue<ItemSlot>();
                _inactiveItems.Add(entry.ItemData, queue);

                // 初期数分の ItemSlot を生成
                for (int i = 0; i < INITIAL_ITEM_COUNT; i++)
                {
                    // モデルをインスタンス化
                    GameObject instance = Instantiate(
                        entry.ItemModel,
                        Vector3.zero,
                        Quaternion.identity,
                        transform
                    );

                    // ItemSlot を生成
                    ItemSlot slot = new ItemSlot(
                        instance.transform,
                        entry.ItemData
                    );

                    // 未使用キューへ登録
                    queue.Enqueue(slot);
                }
            }
        }

        /// <summary>
        /// 未使用状態の ItemSlot キューをランダムに抽選する
        /// </summary>
        /// <returns>抽選された未使用キュー</returns>
        private Queue<ItemSlot> GetRandomInactiveQueue()
        {
            // 抽選対象となる未使用キュー一覧
            List<Queue<ItemSlot>> drawingQueues = new List<Queue<ItemSlot>>();

            // ItemData 単位で未使用キューを走査
            foreach (Queue<ItemSlot> queue in _inactiveItems.Values)
            {
                // 要素が存在しないキューは抽選対象外
                if (queue.Count == 0)
                {
                    continue;
                }

                // 抽選対象として追加
                drawingQueues.Add(queue);
            }

            // 抽選対象が存在しない場合は null を返す
            if (drawingQueues.Count == 0)
            {
                return null;
            }

            // 抽選対象キューのインデックスをランダム取得
            int randomIndex = UnityEngine.Random.Range(0, drawingQueues.Count);

            // 抽選された未使用キューを返却
            return drawingQueues[randomIndex];
        }

        // --------------------------------------------------
        // イベントハンドラ
        // --------------------------------------------------
        /// <summary>
        /// 生成座標確定時の処理
        /// </summary>
        /// <param name="spawnPosition">生成座標</param>
        private void HandleSpawnPositionDetermined(Vector3 spawnPosition)
        {
            // 未使用スロットを取得
            ItemSlot slot = Activate(spawnPosition);
        }

        /// <summary>
        /// ItemSlot から無効化通知を受け取った際の処理
        /// </summary>
        /// <param name="slot">無効化された ItemSlot</param>
        private void HandleSlotDeactivated(ItemSlot slot)
        {
            // null ガード
            if (slot == null)
            {
                return;
            }

            // Pool 側の返却処理を実行
            Deactivate(slot);
        }
    }
}