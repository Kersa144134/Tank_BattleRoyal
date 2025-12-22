// ======================================================
// ItemSpawnController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-22
// 更新日時 : 2025-12-22
// 概要     : アイテムの自動生成・生存管理を制御し、生成タイミング通知と実生成を分離したクラス
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using ItemSystem.Data;

namespace ItemSystem.Controller
{
    /// <summary>
    /// アイテムの生成・生存管理を制御するコントローラクラス
    /// </summary>
    public sealed class ItemSpawnController
    {
        // ======================================================
        // プライベートクラス
        // ======================================================

        /// <summary>
        /// 生成ポイントごとの管理状態
        /// </summary>
        private sealed class SpawnPointState
        {
            /// <summary>生成ポイント Transform</summary>
            public Transform SpawnPoint;

            /// <summary>現在生存中のアイテムスロット</summary>
            public readonly List<ItemSlot> AliveItems = new List<ItemSlot>();
        }

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>アイテム生成ポイント配列</summary>
        private readonly Transform[] _spawnPoints;

        /// <summary>生成ポイントごとの管理ステート</summary>
        private readonly List<SpawnPointState> _spawnPointStates = new List<SpawnPointState>();

        /// <summary>生成タイミング管理時刻</summary>
        private float _lastSpawnTime;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>1生成ポイントあたりの同時生存上限</summary>
        private const int MAX_ALIVE_COUNT_PER_POINT = 3;
        // 
        /// <summary>アイテムの生存時間（秒）</summary>
        private const float ITEM_LIFE_TIME = 15.0f;

        /// <summary>生成判定間隔（秒）</summary>
        private const float SPAWN_INTERVAL = 1.0f;

        /// <summary>生成時の Y 座標</summary>
        private const float SPAWN_HEIGHT = 1.5f;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>生成タイミングが到達したことを通知するイベント</summary>
        public event Action OnSpawnTimingReached;

        /// <summary>アイテムが実際に生成されたことを通知するイベント</summary>
        public event Action<ItemSlot> OnItemSpawned;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// ItemPickController を注入して生成
        /// </summary>
        /// <param name="spawnPointsRoot">生成ポイントの親 Transform</param>
        public ItemSpawnController(in Transform spawnPointsRoot)
        {

            if (spawnPointsRoot == null)
            {
                _spawnPoints = Array.Empty<Transform>();
                return;
            }

            // 子 Transform を生成ポイントとして登録
            int childCount = spawnPointsRoot.childCount;
            _spawnPoints = new Transform[childCount];

            for (int i = 0; i < childCount; i++)
            {
                _spawnPoints[i] = spawnPointsRoot.GetChild(i);
            }

            // 生成ポイントごとの管理ステートを初期化
            foreach (Transform point in _spawnPoints)
            {
                SpawnPointState state = new SpawnPointState
                {
                    SpawnPoint = point
                };
                _spawnPointStates.Add(state);
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 毎フレーム呼び出す更新処理
        /// 生成タイミングを判定してイベント通知のみを行う
        /// </summary>
        public void Update()
        {
            foreach (SpawnPointState state in _spawnPointStates)
            {
                TrySpawnTiming();
            }
        }

        /// <summary>
        /// 指定した ItemSlot をランダムな生成ポイントに配置する
        /// 実際の生存管理リストへ登録し、生成イベントを通知する
        /// </summary>
        /// <param name="slot">生成対象の ItemSlot</param>
        /// <returns>生成成功時 true、失敗時 false</returns>
        public bool SpawnItemAtRandomPoint(ItemSlot slot)
        {
            if (slot == null || _spawnPointStates.Count == 0)
            {
                return false;
            }

            // ランダムに生成ポイントを選択
            int index = UnityEngine.Random.Range(0, _spawnPointStates.Count);
            SpawnPointState state = _spawnPointStates[index];

            // 同じ生成ポイントでの上限チェック
            int samePointCount = state.AliveItems.FindAll(s => s.ItemData == slot.ItemData).Count;
            if (samePointCount >= MAX_ALIVE_COUNT_PER_POINT)
            {
                return false;
            }

            // 生成座標取得
            if (!TryGetSpawnPosition(state, out Vector3 spawnPosition))
            {
                return false;
            }

            // アイテム生成
            slot.Spawn(spawnPosition, ITEM_LIFE_TIME);

            // 生存管理リストへ登録
            state.AliveItems.Add(slot);

            // 生成イベント通知
            OnItemSpawned?.Invoke(slot);

            return true;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 生成タイミング判定用メソッド
        /// 上限・間隔チェックを行い、イベントで通知
        /// </summary>
        /// <param name="state">生成ポイント管理ステート</param>
        private void TrySpawnTiming()
        {
            float currentTime = Time.time;

            // 前回生成から一定間隔経過していなければ処理しない
            if (currentTime - _lastSpawnTime < SPAWN_INTERVAL)
            {
                return;
            }

            // 最終生成時刻を更新
            _lastSpawnTime = currentTime;

            // 生成タイミング到達イベントを通知
            OnSpawnTimingReached?.Invoke();
        }

        /// <summary>
        /// 生成ポイントの XZ スケールを基準とした
        /// 長方形範囲内の生成座標を取得
        /// </summary>
        /// <param name="state">生成ポイント管理ステート</param>
        /// <param name="spawnPosition">取得した生成座標</param>
        /// <returns>座標取得成功なら true</returns>
        private bool TryGetSpawnPosition(
            SpawnPointState state,
            out Vector3 spawnPosition)
        {
            if (state.SpawnPoint == null)
            {
                spawnPosition = Vector3.zero;
                return false;
            }

            Vector3 basePos = state.SpawnPoint.position;

            float range = 1.0f;
            float offsetX = UnityEngine.Random.Range(-range, range);
            float offsetZ = UnityEngine.Random.Range(-range, range);

            spawnPosition = new Vector3(basePos.x + offsetX, SPAWN_HEIGHT, basePos.z + offsetZ);
            return true;
        }
    }
}