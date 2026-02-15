// ======================================================
// ItemPool.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-22
// 更新日時 : 2026-02-14
// 概要     : ItemSlot を管理するプールクラス
//            取得 / 返却をイベントで通知する
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using ItemSystem.Controller;
using ItemSystem.Data;
using SceneSystem.Data;
using SceneSystem.Interface;
using SceneSystem.Manager;

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

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>シーン上オブジェクトの Transform を一元管理するレジストリー</summary>
        private SceneObjectRegistry _sceneRegistry;

        /// <summary>アイテム抽選を担当するコントローラー</summary>
        private ItemDrawController _drawController = new ItemDrawController();

        /// <summary>アイテム生成を自動制御するコントローラー</summary>
        private ItemSpawnController _spawnController;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>アイテム初期生成を実行済みかどうか</summary>
        private bool _isInitialSpawnExecuted = false;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>未使用 ItemSlot を ItemData 単位で管理する辞書</summary>
        private readonly Dictionary<ItemData, Queue<ItemSlot>> _inactiveItems
            = new Dictionary<ItemData, Queue<ItemSlot>>();

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>各 ItemData ごとの初期生成数</summary>
        private const int INITIAL_ITEM_COUNT = 40;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>ItemSlot が有効化された際の通知イベント</summary>
        public event Action<ItemSlot> OnItemActivated;

        /// <summary>ItemSlot が無効化された際の通知イベント</summary>
        public event Action<ItemSlot> OnItemDeactivated;

        // ======================================================
        // セッター
        // ======================================================

        /// <summary>
        /// シーン内オブジェクト管理用のレジストリー参照を設定する
        /// </summary>
        /// <param name="sceneRegistry">シーンに存在する各種オブジェクト情報を一元管理するレジストリー</param>
        public void SetSceneRegistry(SceneObjectRegistry sceneRegistry)
        {
            _sceneRegistry = sceneRegistry;
        }

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void OnEnter()
        {
            // 生成制御コントローラーを生成
            _spawnController = new ItemSpawnController(_sceneRegistry.SpawnPointsRoot);

            // イベント購読
            _spawnController.OnSpawnPositionDetermined += HandleSpawnPositionDetermined;

            // プールを初期化
            InitializePool();
        }

        public void OnUpdate(in float unscaledDeltaTime, in float elapsedTime)
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

        public void OnPhaseEnter(in PhaseType phase)
        {
            // Playフェーズの場合
            if (phase == PhaseType.Play)
            {
                if (_isInitialSpawnExecuted)
                {
                    return;
                }

                // 初期生成を実行
                _spawnController.ExecuteInitialSpawn();

                // 実行済みフラグを立てる
                _isInitialSpawnExecuted = true;
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // プール操作
        // --------------------------------------------------
        /// <summary>
        /// 未使用プールから ItemSlot をランダムに取り出し、
        /// 指定グリッドキーと座標で有効化する
        /// </summary>
        /// <param name="spawnPosition">生成座標</param>
        /// <param name="spawnGridKey">割り当てられたグリッドキー</param>
        /// <param name="spawnPointType">SpawnPoint の種別</param>
        /// <returns>取得した ItemSlot</returns>
        private ItemSlot Activate(
            in Vector3 spawnPosition,
            in ItemSlot.SpawnGridKey spawnGridKey,
            in ItemSpawnController.SpawnPointType spawnPointType)
        {
            // 抽選コントローラーから未使用キューを取得
            Queue<ItemSlot> selectedQueue =
                _drawController.Draw(_inactiveItems, spawnPointType);

            // 抽選対象が存在しない場合は取得不可
            if (selectedQueue == null)
            {
                return null;
            }

            // 未使用キューから ItemSlot を取得
            ItemSlot slot =
                selectedQueue.Dequeue();

            // イベント購読
            slot.OnDeactivated += HandleSlotDeactivated;

            // 生成位置へ移動
            slot.Transform.position =
                spawnPosition;

            // グリッドキーを設定
            slot.SetSpawnGridKey(spawnGridKey);

            // 有効化イベントを通知
            OnItemActivated?.Invoke(slot);

            return slot;
        }

        /// <summary>
        /// ItemSlot を未使用状態としてプールへ返却する
        /// </summary>
        /// <param name="slot">返却対象の ItemSlot</param>
        private void Deactivate(in ItemSlot slot)
        {
            if (slot == null || slot.Transform == null)
            {
                return;
            }

            // イベント購読解除
            slot.OnDeactivated -= HandleSlotDeactivated;

            // プールへ返却
            _inactiveItems[slot.ItemData].Enqueue(slot);

            // 生成座標を解放
            _spawnController.ReleaseSpawnPosition(slot.GridKey);

            // 無効化イベント通知
            OnItemDeactivated?.Invoke(slot);
        }

        /// <summary>
        /// プールを初期化する
        /// </summary>
        private void InitializePool()
        {
            _inactiveItems.Clear();

            // アイテム定義を走査
            foreach (ItemEntry entry in _itemEntries)
            {
                // 不正定義は除外
                if (entry.ItemData == null ||
                    entry.ItemModel == null ||
                    entry.ItemData.name != entry.ItemModel.name)
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

        // --------------------------------------------------
        // イベントハンドラ
        // --------------------------------------------------
        /// <summary>
        /// 生成座標確定時の処理
        /// </summary>
        /// <param name="spawnPosition">生成座標</param>
        /// <param name="spawnGridKey">対応グリッドキー</param>
        /// <param name="spawnPointType">SpawnPoint の種別</param>
        private void HandleSpawnPositionDetermined(
            Vector3 spawnPosition,
            ItemSlot.SpawnGridKey spawnGridKey,
            ItemSpawnController.SpawnPointType spawnPointType)
        {
            Activate(spawnPosition, spawnGridKey, spawnPointType);
        }

        /// <summary>
        /// ItemSlot から無効化通知を受け取った際の処理
        /// </summary>
        /// <param name="slot">無効化された ItemSlot</param>
        private void HandleSlotDeactivated(ItemSlot slot)
        {
            if (slot == null)
            {
                return;
            }

            Deactivate(slot);
        }
    }
}