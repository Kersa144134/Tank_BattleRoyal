// ======================================================
// ItemSpawnController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-22
// 更新日時 : 2026-02-14
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
        private readonly Transform[] _spawnPoints;

        /// <summary>利用可能グリッドキープール</summary>
        private readonly List<ItemSlot.SpawnGridKey> _availableSpawnGridKeys;

        /// <summary>前回生成判定時刻</summary>
        private float _lastSpawnTime;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>初回生成回数</summary>
        private const int INITIAL_SPAWN_COUNT = 40;

        /// <summary>生成判定間隔（秒）</summary>
        private const float SPAWN_INTERVAL = 0.5f;

        /// <summary>生成時の Y 座標</summary>
        private const float SPAWN_HEIGHT = 1.5f;

        /// <summary>生成座標オフセットの刻み幅</summary>
        private const int SPAWN_OFFSET_STEP = 10;

        /// <summary>生成座標オフセット段数</summary>
        private const int SPAWN_OFFSET_LEVEL_COUNT = 2;

        /// <summary>Supply 判定用タグ名</summary>
        private const string SUPPLY_TAG = "Supply";

        /// <summary>ParamBonus 判定用タグ名</summary>
        private const string PARAM_BONUS_TAG = "ParamBonus";

        // ======================================================
        // イベント
        // ======================================================

        //// <summary>
        /// 生成座標決定通知イベント
        /// </summary>
        public event Action<Vector3, ItemSlot. SpawnGridKey, SpawnPointType> OnSpawnPositionDetermined;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 生成ポイントを登録し、グリッドプールを初期化する
        /// </summary>
        /// <param name="spawnPointsRoot">生成ポイントの親 Transform</param>
        public ItemSpawnController(in Transform spawnPointsRoot)
        {
            // null の場合は空配列を設定
            if (spawnPointsRoot == null)
            {
                _spawnPoints = Array.Empty<Transform>();

                _availableSpawnGridKeys =
                    new List<ItemSlot.SpawnGridKey>(0);

                return;
            }

            int childCount =
                spawnPointsRoot.childCount;
            _spawnPoints =
                new Transform[childCount];

            // 子Transformを格納
            for (int i = 0; i < childCount; i++)
            {
                _spawnPoints[i] =
                    spawnPointsRoot.GetChild(i);
            }

            // --------------------------------------------------
            // 最大グリッド数を算出
            // --------------------------------------------------
            int gridPerSpawnPoint =
                (SPAWN_OFFSET_LEVEL_COUNT * 2 + 1) *
                (SPAWN_OFFSET_LEVEL_COUNT * 2 + 1);

            int totalGridCount =
                childCount * gridPerSpawnPoint;

            // 容量を事前確保
            _availableSpawnGridKeys =
                new List<ItemSlot.SpawnGridKey>(totalGridCount);

            // --------------------------------------------------
            // グリッドキープール生成
            // --------------------------------------------------
            for (int sp = 0; sp < childCount; sp++)
            {
                for (int x = -SPAWN_OFFSET_LEVEL_COUNT; x <= SPAWN_OFFSET_LEVEL_COUNT; x++)
                {
                    for (int z = -SPAWN_OFFSET_LEVEL_COUNT; z <= SPAWN_OFFSET_LEVEL_COUNT; z++)
                    {
                        ItemSlot.SpawnGridKey key =
                            new ItemSlot.SpawnGridKey(sp, x, z);

                        _availableSpawnGridKeys.Add(key);
                    }
                }
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 自動生成判定を行う更新処理
        /// </summary>
        public void Update()
        {
            // 生成可能でなければ終了
            if (!CanSpawn())
            {
                return;
            }

            // プール方式で生成座標を取得
            if (!TryGetRandomSpawnPosition(
                out Vector3 position,
                out ItemSlot.SpawnGridKey key,
                out SpawnPointType spawnPointType))
            {
                return;
            }

            // 生成座標確定イベントを通知
            OnSpawnPositionDetermined?.Invoke(position, key, spawnPointType);
        }

        /// <summary>
        /// 初回生成処理を即時に実行する
        /// </summary>
        public void ExecuteInitialSpawn()
        {
            for (int i = 0; i < INITIAL_SPAWN_COUNT; i++)
            {
                if (!TryGetRandomSpawnPosition(
                    out Vector3 position,
                    out ItemSlot.SpawnGridKey key,
                    out SpawnPointType spawnPointType))
                {
                    continue;
                }

                OnSpawnPositionDetermined?.Invoke(position, key, spawnPointType);
            }
        }

        /// <summary>
        /// グリッドキーを解放しプールへ戻す
        /// </summary>
        /// <param name="spawnKey">解放対象キー</param>
        public void ReleaseSpawnPosition(
            in ItemSlot.SpawnGridKey spawnKey)
        {
            _availableSpawnGridKeys.Add(spawnKey);
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
            float currentTime =
                Time.time;

            // 経過時間がインターバル未満なら処理なし
            if (currentTime - _lastSpawnTime < SPAWN_INTERVAL)
            {
                return false;
            }

            // 最終生成時刻を更新
            _lastSpawnTime =
                currentTime;

            return true;
        }

        /// <summary>
        /// プール方式でランダム生成座標を取得する
        /// </summary>
        /// <param name="position">生成座標（ワールド座標）</param>
        /// <param name="spawnKey">対応する生成グリッドキー</param>
        /// <param name="spawnPointType">SpawnPoint の種別</param>
        /// <returns>取得成功なら true</returns>
        private bool TryGetRandomSpawnPosition(
            out Vector3 position,
            out ItemSlot.SpawnGridKey spawnKey,
            out SpawnPointType spawnPointType)
        {
            position =
                Vector3.zero;

            spawnKey =
                default;

            spawnPointType = SpawnPointType.None;

            // 利用可能なグリッドキーが存在しない場合は処理なし
            if (_availableSpawnGridKeys.Count == 0)
            {
                return false;
            }

            // --------------------------------------------------
            // グリッドキー取得
            // --------------------------------------------------
            // 利用可能キーの中からランダム取得
            int randomIndex =
                UnityEngine.Random.Range(
                    0,
                    _availableSpawnGridKeys.Count
                );
            spawnKey =
                _availableSpawnGridKeys[randomIndex];

            // --------------------------------------------------
            // スワップ削除
            // --------------------------------------------------
            // 末尾インデックスを算出
            int lastIndex =
                _availableSpawnGridKeys.Count - 1;

            // ランダムインデックス要素に末尾インデックス要素を上書き
            _availableSpawnGridKeys[randomIndex] =
                _availableSpawnGridKeys[lastIndex];

            // 末尾インデックス要素を削除し、シフト処理を抑制
            _availableSpawnGridKeys.RemoveAt(lastIndex);

            // --------------------------------------------------
            // 生成座標算出
            // --------------------------------------------------
            // 基準となる SpawnPoint を取得
            Transform basePoint =
                _spawnPoints[spawnKey.SpawnPointIndex];

            // 基準ワールド座標を取得
            Vector3 basePos =
                basePoint.position;

            // オフセット値を加算して最終生成座標を算出
            position = new Vector3(
                basePos.x +
                spawnKey.OffsetX * SPAWN_OFFSET_STEP,
                SPAWN_HEIGHT,
                basePos.z +
                spawnKey.OffsetZ * SPAWN_OFFSET_STEP
            );

            // SpawnPoint のタグ判定
            if (basePoint.CompareTag(SUPPLY_TAG))
            {
                spawnPointType =
                    SpawnPointType.Supply;
            }
            else if (basePoint.CompareTag(PARAM_BONUS_TAG))
            {
                spawnPointType =
                    SpawnPointType.ParamBobus;
            }

            return true;
        }
    }
}