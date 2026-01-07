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

        /// <summary>生成判定間隔（秒）</summary>
        private const float SPAWN_INTERVAL = 1.0f;

        /// <summary>生成時の Y 座標</summary>
        private const float SPAWN_HEIGHT = 1.5f;

        /// <summary>XZ 方向のオフセット範囲</summary>
        private const int OFFSET_RANGE = 10;

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
            if (CanSpawn() == false)
            {
                return;
            }

            // 生成座標を取得
            if (TryGetRandomSpawnPosition(out Vector3 position) == false)
            {
                return;
            }

            // 生成座標確定イベントを通知
            OnSpawnPositionDetermined?.Invoke(position);
        }

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
            position = Vector3.zero;

            if (_spawnPoints.Length == 0)
            {
                return false;
            }

            Transform basePoint =
                _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Length)];

            if (basePoint == null)
            {
                return false;
            }

            // XZ オフセットを整数単位で算出
            int offsetX = UnityEngine.Random.Range(-OFFSET_RANGE, OFFSET_RANGE + 1);
            int offsetZ = UnityEngine.Random.Range(-OFFSET_RANGE, OFFSET_RANGE + 1);

            Vector3 basePos = basePoint.position;

            position = new Vector3(
                basePos.x + offsetX,
                SPAWN_HEIGHT,
                basePos.z + offsetZ
            );

            return true;
        }
    }
}