
using CollisionSystem.Calculator;
using CollisionSystem.Interface;
using System;
using System.Collections.Generic;
using TankSystem.Data;
using TankSystem.Interface;
using TankSystem.Utility;
using UnityEngine;
using static UnityEditor.Progress;

namespace TankSystem.Service
{
    /// <summary>
    /// 戦車と障害物の衝突判定を担当する
    /// </summary>
    public sealed class VersusObstacleCollisionService
        : ITankCollisionService
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>
        /// OBB / OBB の衝突判定および MTV 計算を行う計算器
        /// </summary>
        private readonly BoundingBoxCollisionCalculator _boxCollisionCalculator;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>戦車コンテキスト一覧</summary>
        private readonly List<TankCollisionContext> _tanks;

        /// <summary>障害物 Transform 配列</summary>
        private readonly Transform[] _obstacles;

        /// <summary>障害物 OBB 配列</summary>
        private IOBBData[] _obstacleOBBs;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// 障害物と接触した際に通知されるイベント
        /// 引数は衝突した戦車のコンテキストと障害物 Transform
        /// </summary>
        public event Action<TankCollisionContext, Transform> OnObstacleHit;

        // ======================================================
        // コンストラクタ
        // ======================================================

        public VersusObstacleCollisionService(
            in BoundingBoxCollisionCalculator boxCollisionCalculator,
            in List<TankCollisionContext> tanks,
            in Transform[] obstacles
        )
        {
            _boxCollisionCalculator = boxCollisionCalculator;
            _tanks = tanks;
            _obstacles = obstacles;
        }

        // ======================================================
        // セッター
        // ======================================================

        public void SetObstacleOBBs(in OBBFactory obbFactory)
        {
            if (_obstacles == null || _obstacles.Length == 0)
            {
                _obstacleOBBs = Array.Empty<IOBBData>();
                return;
            }

            _obstacleOBBs = new IOBBData[_obstacles.Length];

            for (int i = 0; i < _obstacles.Length; i++)
            {
                _obstacleOBBs[i] = obbFactory.CreateStaticOBB(
                    _obstacles[i],
                    Vector3.zero,
                    Vector3.one
                );
            }
        }

        // ======================================================
        // パブリック
        // ======================================================

        public void UpdateCollisionChecks()
        {
            // 各戦車を順に処理する
            for (int i = 0; i < _tanks.Count; i++)
            {
                // 戦車 OBB を更新する
                _tanks[i].OBB.Update();

                // 障害物と衝突チェック
                for (int j = 0; j < _obstacleOBBs.Length; j++)
                {
                    // 無効な障害物は無視
                    if (_obstacleOBBs[j] == null)
                    {
                        continue;
                    }

                    // 水平方向の衝突判定
                    if (_boxCollisionCalculator.IsCollidingHorizontal(
                        _tanks[i].OBB,
                        _obstacleOBBs[j]))
                    {
                        OnObstacleHit?.Invoke(_tanks[i], _obstacles[j]);
                    }
                }
            }
        }
    }
}
