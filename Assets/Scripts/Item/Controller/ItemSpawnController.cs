// ======================================================
// ItemSpawnController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-22
// 更新日時 : 2026-01-07
// 概要     : アイテム生成位置を管理するコントローラー
// ======================================================

using System;
using UnityEngine;

namespace ItemSystem.Controller
{
    /// <summary>
    /// アイテム生成位置を管理するコントローラークラス
    /// </summary>
    public sealed class ItemSpawnController
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>生成ポイント Transform 配列</summary>
        private readonly Transform[] _spawnPoints;

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
        
        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// 生成可能になった際に呼び出されるイベント
        /// </summary>
        public event Action<Vector3> OnSpawnPositionDetermined;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 生成ポイントを登録する
        /// </summary>
        /// <param name="spawnPointsRoot">生成ポイントの親 Transform</param>
        public ItemSpawnController(in Transform spawnPointsRoot)
        {
            if (spawnPointsRoot == null)
            {
                _spawnPoints = Array.Empty<Transform>();
                return;
            }

            int childCount = spawnPointsRoot.childCount;
            _spawnPoints = new Transform[childCount];

            for (int i = 0; i < childCount; i++)
            {
                _spawnPoints[i] = spawnPointsRoot.GetChild(i);
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

            // 生成座標を取得
            if (!TryGetRandomSpawnPosition(out Vector3 position))
            {
                return;
            }

            // 生成座標確定イベントを通知
            OnSpawnPositionDetermined?.Invoke(position);
        }

        /// <summary>
        /// 初回生成処理を即時に実行する
        /// </summary>
        public void ExecuteInitialSpawn()
        {
            // 指定回数分の初回生成を実行
            for (int i = 0; i < INITIAL_SPAWN_COUNT; i++)
            {
                // ランダムな生成座標を取得
                if (!TryGetRandomSpawnPosition(out Vector3 position))
                {
                    continue;
                }

                // 生成座標確定イベントを通知
                OnSpawnPositionDetermined?.Invoke(position);
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 生成タイミングに到達しているか判定する
        /// </summary>
        /// <returns>生成可能なら true</returns>
        private bool CanSpawn()
        {
            float currentTime = Time.time;

            // 生成間隔未満なら不可
            if (currentTime - _lastSpawnTime < SPAWN_INTERVAL)
            {
                return false;
            }

            // 判定通過時に時刻更新
            _lastSpawnTime = currentTime;
            return true;
        }

        /// <summary>
        /// ランダムな生成座標を取得する
        /// </summary>
        /// <param name="position">取得した生成座標</param>
        /// <returns>取得成功なら true</returns>
        private bool TryGetRandomSpawnPosition(out Vector3 position)
        {
            // 初期化
            position = Vector3.zero;

            // 生成ポイントが存在しない場合は失敗
            if (_spawnPoints.Length == 0)
            {
                return false;
            }

            // 基準となる生成ポイントをランダムに選択
            Transform basePoint = _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Length)];

            // Transform が無効なら処理なし
            if (basePoint == null)
            {
                return false;
            }

            // ----------------------------------------------
            // オフセット段数を抽選
            // ----------------------------------------------
            int offsetIndexX =
                UnityEngine.Random.Range(-SPAWN_OFFSET_LEVEL_COUNT, SPAWN_OFFSET_LEVEL_COUNT + 1);

            int offsetIndexZ =
                UnityEngine.Random.Range(-SPAWN_OFFSET_LEVEL_COUNT, SPAWN_OFFSET_LEVEL_COUNT + 1);

            // ----------------------------------------------
            // 刻み幅を掛けて実際のオフセット値を算出
            // ----------------------------------------------
            int offsetX = offsetIndexX * SPAWN_OFFSET_STEP;
            int offsetZ = offsetIndexZ * SPAWN_OFFSET_STEP;

            // 基準座標を取得
            Vector3 basePos = basePoint.position;

            // 生成座標を構築
            position = new Vector3(
                basePos.x + offsetX,
                SPAWN_HEIGHT,
                basePos.z + offsetZ
            );

            return true;
        }
    }
}