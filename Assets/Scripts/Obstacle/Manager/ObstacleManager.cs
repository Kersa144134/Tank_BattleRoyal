// ======================================================
// ObstacleManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-12
// 更新日時 : 2026-01-07
// 概要     : 障害物の耐久値を一元管理する
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using WeaponSystem.Interface;

namespace TankSystem.Manager
{
    /// <summary>
    /// 障害物管理マネージャー
    /// </summary>
    public sealed class ObstacleManager : IDamageable
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>更新対象の障害物リスト</summary>
        private readonly List<Transform> _activeObstacles = new List<Transform>();
        
        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>障害物の耐久値を管理する辞書</summary>
        private readonly Dictionary<Transform, float> _obstacleDurabilities
            = new Dictionary<Transform, float>();

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>破壊可能オブジェクトのタグ名</summary>
        private const string BREAKABLE_TAG = "Breakable";

        /// <summary>障害物の耐久最大値</summary>
        private const float OBSTACLE_DURABILITY = 10f;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>フェーズ変更タイミング通知</summary>
        public event Action<Transform> OnObstacleBreaked;

        // ======================================================
        // IDamageable イベント
        // ======================================================

        /// <summary>
        /// 指定障害物へダメージを適用する
        /// </summary>
        /// <param name="target">ダメージ対象となる障害物</param>
        /// <param name="damage">適用するダメージ量</param>
        public void TakeDamage(in Transform target, in float damage)
        {
            if (target == null)
            {
                return;
            }

            // 無効な値は処理なし
            if (damage <= 0f)
            {
                return;
            }

            // 管理対象外なら処理なし
            if (!_obstacleDurabilities.TryGetValue(target, out float currentDurability))
            {
                return;
            }

            // 耐久値を減算する
            currentDurability -= damage;

            // 0以下なら破壊処理実行
            if (currentDurability <= 0f)
            {
                OnObstacleBreaked?.Invoke(target);
                return;
            }

            // 更新した耐久値を保存する
            _obstacleDurabilities[target] = currentDurability;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 更新対象として障害物を登録する
        /// </summary>
        /// <param name="obstacle">登録対象となる障害物</param>
        public void RegisterObstacle(in Transform obstacle)
        {
            // nullは無効
            if (obstacle == null)
            {
                return;
            }

            // Breakableタグ以外は登録しない
            if (!obstacle.CompareTag(BREAKABLE_TAG))
            {
                return;
            }

            // 既に登録済みなら処理なし
            if (_activeObstacles.Contains(obstacle))
            {
                return;
            }

            // 更新対象に追加
            _activeObstacles.Add(obstacle);

            // 耐久値を初期化
            _obstacleDurabilities.TryAdd(
                obstacle,
                OBSTACLE_DURABILITY
            );
        }

        /// <summary>
        /// 更新対象からアイテムスロットを解除する
        /// </summary>
        /// <param name="obstacle">解除対象となる障害物</param>
        public void UnregisterObstacle(in Transform obstacle)
        {
            if (obstacle == null)
            {
                return;
            }

            // 管理対象外なら処理なし
            if (!_activeObstacles.Contains(obstacle))
            {
                return;
            }

            // 更新対象から除外
            _activeObstacles.Remove(obstacle);

            // 耐久管理から除外
            _obstacleDurabilities.Remove(obstacle);

            // オブジェクトを非表示
            obstacle.GetComponent<Renderer>().enabled = false;
        }
    }
}