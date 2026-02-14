// ======================================================
// ItemSpawnController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-22
// 更新日時 : 2026-02-15
// 概要     : アイテム生成位置を管理するコントローラー
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using ItemSystem.Data;

namespace ItemSystem.Controller
{
    /// <summary>
    /// アイテム生成位置を管理するコントローラークラス
    /// SpawnPoint単位でグリッドプールを保持し、
    /// 生成タイミング到達時に各SpawnPointから1つずつ生成する。
    /// </summary>
    public sealed class ItemSpawnController
    {
        // ======================================================
        // 列挙体
        // ======================================================

        /// <summary>
        /// SpawnPoint の種別を表す列挙型
        /// </summary>
        public enum SpawnPointType
        {
            None,
            Supply,
            ParamBobus
        }

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>生成ポイント Transform 配列</summary>
        private readonly Transform[] spawnPoints;

        /// <summary>SpawnPoint ごとの利用可能グリッドプール</summary>
        private readonly Dictionary<int, List<ItemSlot.SpawnGridKey>> spawnGridPool;

        /// <summary>最終生成時刻</summary>
        private float lastSpawnTime;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>初回生成回数</summary>
        private const int INITIAL_SPAWN_COUNT = 3;

        /// <summary>生成判定間隔（秒）</summary>
        private const float SPAWN_INTERVAL = 5.0f;

        /// <summary>グリッド座標オフセット刻み幅/summary>
        private const int SPAWN_OFFSET_STEP = 10;

        /// <summary>グリッドオフセット段数</summary>
        private const int SPAWN_OFFSET_LEVEL_COUNT = 2;

        /// <summary>生成時のY座標固定値</summary>
        private const float SPAWN_HEIGHT = 1.5f;

        /// <summary>Supply 判定用タグ名</summary>
        private const string SUPPLY_TAG = "Supply";

        /// <summary>ParamBonus 判定用タグ名</summary>
        private const string PARAM_BONUS_TAG = "ParamBonus";

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// 生成座標決定通知イベント
        /// </summary>
        public event Action<Vector3, ItemSlot.SpawnGridKey, SpawnPointType>
            OnSpawnPositionDetermined;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 生成ポイントを登録し、SpawnPoint 単位のグリッドプールを初期化する
        /// </summary>
        /// <param name="spawnPointsRoot">SpawnPoint の親 Transform</param>
        public ItemSpawnController(in Transform spawnPointsRoot)
        {
            // spawnPointsRoot が null の場合は空構造で初期化
            if (spawnPointsRoot == null)
            {
                spawnPoints = Array.Empty<Transform>();
                spawnGridPool = new Dictionary<int, List<ItemSlot.SpawnGridKey>>(0);
                return;
            }

            // 子 Transform 数を取得
            int childCount =
                spawnPointsRoot.childCount;

            // SpawnPoint 配列を生成
            spawnPoints =
                new Transform[childCount];

            // 子 Transform を順番に格納
            for (int i = 0; i < childCount; i++)
            {
                spawnPoints[i] =
                    spawnPointsRoot.GetChild(i);
            }

            // 1 SpawnPoint あたりのグリッド数を算出
            int gridPerSpawnPoint =
                (SPAWN_OFFSET_LEVEL_COUNT * 2 + 1) *
                (SPAWN_OFFSET_LEVEL_COUNT * 2 + 1);

            // SpawnPoint 数分の容量を確保
            spawnGridPool =
                new Dictionary<int, List<ItemSlot.SpawnGridKey>>(childCount);

            // SpawnPoint ごとにグリッドリストを生成
            for (int sp = 0; sp < childCount; sp++)
            {
                // 各 SpawnPoint 専用リストを生成
                List<ItemSlot.SpawnGridKey> gridList =
                    new List<ItemSlot.SpawnGridKey>(gridPerSpawnPoint);

                for (int x = -SPAWN_OFFSET_LEVEL_COUNT;
                    x <= SPAWN_OFFSET_LEVEL_COUNT;
                    x++)
                {
                    for (int z = -SPAWN_OFFSET_LEVEL_COUNT;
                        z <= SPAWN_OFFSET_LEVEL_COUNT;
                        z++)
                    {
                        // SpawnPointIndex とオフセット値を保持するキー生成
                        ItemSlot.SpawnGridKey key =
                            new ItemSlot.SpawnGridKey(sp, x, z);

                        // リストへ追加
                        gridList.Add(key);
                    }
                }

                // 辞書へ登録
                spawnGridPool.Add(sp, gridList);
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 初回生成処理を即時実行する
        /// インターバル判定を無視して生成する
        /// </summary>
        public void ExecuteInitialSpawn()
        {
            // SpawnPoint 総数を取得
            int spawnPointCount =
                spawnPoints.Length;

            if (spawnPointCount == 0)
            {
                return;
            }

            // 指定回数分生成を試行
            for (int i = 0;
                i < INITIAL_SPAWN_COUNT;
                i++)
            {
                // 全 SpawnPoint を順に処理
                for (int sp = 0;
                    sp < spawnPointCount;
                    sp++)
                {
                    // 指定 SpawnPoint から生成を試行
                    if (!TryGetSpawnPositionFromSpawnPoint(
                        sp,
                        out Vector3 position,
                        out ItemSlot.SpawnGridKey key,
                        out SpawnPointType type))
                    {
                        continue;
                    }

                    // 生成決定イベント通知
                    OnSpawnPositionDetermined?.Invoke(
                        position,
                        key,
                        type
                    );
                }
            }
        }
        
        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update()
        {
            if (!CanSpawn())
            {
                return;
            }

            // SpawnPoint 総数を取得
            int spawnPointCount =
                spawnPoints.Length;

            // 全 SpawnPoint を順に処理
            for (int i = 0; i < spawnPointCount; i++)
            {
                if (!TryGetSpawnPositionFromSpawnPoint(
                    i,
                    out Vector3 position,
                    out ItemSlot.SpawnGridKey key,
                    out SpawnPointType type))
                {
                    continue;
                }

                // 生成決定イベント通知
                OnSpawnPositionDetermined?.Invoke(
                    position,
                    key,
                    type
                );
            }
        }

        /// <summary>
        /// 使用済みグリッドキーを解放し、対応 SpawnPoint へ戻す
        /// </summary>
        /// <param name="spawnKey">解放対象キー</param>
        public void ReleaseSpawnPosition(
            in ItemSlot.SpawnGridKey spawnKey)
        {
            // 対応 SpawnPoint が存在しない場合は処理なし
            if (!spawnGridPool.ContainsKey(
                spawnKey.SpawnPointIndex))
            {
                return;
            }

            // 元の SpawnPoint リストへ再追加
            spawnGridPool[spawnKey.SpawnPointIndex]
                .Add(spawnKey);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 生成タイミング判定
        /// </summary>
        /// <returns>生成可能なら true</returns>
        private bool CanSpawn()
        {
            // 現在時刻を取得
            float currentTime =
                Time.time;

            // インターバル未満なら生成不可
            if (currentTime - lastSpawnTime <
                SPAWN_INTERVAL)
            {
                return false;
            }

            // 最終生成時刻を更新
            lastSpawnTime =
                currentTime;

            return true;
        }

        /// <summary>
        /// 指定 SpawnPoint から1つ生成座標を取得する
        /// </summary>
        /// <param name="spawnPointIndex">対象 SpawnPoint インデックス</param>
        /// <param name="position">生成座標</param>
        /// <param name="spawnKey">対応キー</param>
        /// <param name="spawnPointType">SpawnPoint 種別</param>
        /// <returns>取得成功なら true</returns>
        private bool TryGetSpawnPositionFromSpawnPoint(
            int spawnPointIndex,
            out Vector3 position,
            out ItemSlot.SpawnGridKey spawnKey,
            out SpawnPointType spawnPointType)
        {
            // 初期化
            position = Vector3.zero;
            spawnKey = default;
            spawnPointType = SpawnPointType.None;

            // SpawnPoint チェック
            if (!spawnGridPool.ContainsKey(
                spawnPointIndex))
            {
                return false;
            }

            // グリッドキー取得
            if (!TryDequeueSpawnGridKey(
                spawnPointIndex,
                out spawnKey))
            {
                return false;
            }

            // 座標算出
            position =
                CalculateSpawnWorldPosition(
                    spawnPointIndex,
                    spawnKey
                );

            // SpawnPoint 種別取得
            spawnPointType =
                GetSpawnPointType(
                    spawnPointIndex
                );

            return true;
        }

        /// <summary>
        /// 指定 SpawnPoint からグリッドキーを1つ取得し削除する
        /// </summary>
        /// <param name="spawnPointIndex">対象 SpawnPoint インデックス</param>
        /// <param name="spawnKey">取得キー</param>
        /// <returns>取得成功なら true</returns>
        private bool TryDequeueSpawnGridKey(
            int spawnPointIndex,
            out ItemSlot.SpawnGridKey spawnKey)
        {
            spawnKey = default;

            List<ItemSlot.SpawnGridKey> gridList =
                spawnGridPool[spawnPointIndex];

            if (gridList.Count == 0)
            {
                return false;
            }

            // ランダム抽選
            int randomIndex =
                UnityEngine.Random.Range(
                    0,
                    gridList.Count
                );

            spawnKey =
                gridList[randomIndex];

            // --------------------------------------------------
            // スワップ削除
            // --------------------------------------------------
            // リスト末尾インデックスを取得する
            int lastIndex =
                gridList.Count - 1;

            // 抽選された要素を末尾要素で上書き
            gridList[randomIndex] =
                gridList[lastIndex];

            // 末尾要素を削除する
            gridList.RemoveAt(
                lastIndex
            );

            return true;
        }

        /// <summary>
        /// グリッドキーからワールド座標を算出する
        /// </summary>
        /// <param name="spawnPointIndex">SpawnPoint インデックス</param>
        /// <param name="spawnKey">グリッドキー</param>
        /// <returns>ワールド座標</returns>
        private Vector3 CalculateSpawnWorldPosition(
            int spawnPointIndex,
            in ItemSlot.SpawnGridKey spawnKey)
        {
            Transform basePoint =
                spawnPoints[spawnPointIndex];

            // 基準ワールド座標取得
            Vector3 basePos =
                basePoint.position;

            // オフセット加算
            Vector3 worldPosition =
                new Vector3(
                    basePos.x +
                    spawnKey.OffsetX *
                    SPAWN_OFFSET_STEP,
                    SPAWN_HEIGHT,
                    basePos.z +
                    spawnKey.OffsetZ *
                    SPAWN_OFFSET_STEP
                );

            return worldPosition;
        }

        /// <summary>
        /// SpawnPoint の種別を取得する
        /// </summary>
        /// <param name="spawnPointIndex">SpawnPoint インデックス</param>
        /// <returns>SpawnPoint 種別</returns>
        private SpawnPointType GetSpawnPointType(
            int spawnPointIndex)
        {
            Transform basePoint =
                spawnPoints[spawnPointIndex];

            string tagName =
                basePoint.tag;

            // --------------------------------------------------
            // タグ分類
            // --------------------------------------------------
            switch (tagName)
            {
                // Supply タグ
                case SUPPLY_TAG:
                    return SpawnPointType.Supply;

                // ParamBonus タグ
                case PARAM_BONUS_TAG:
                    return SpawnPointType.ParamBobus;

                // その他
                default:
                    return SpawnPointType.None;
            }
        }
    }
}