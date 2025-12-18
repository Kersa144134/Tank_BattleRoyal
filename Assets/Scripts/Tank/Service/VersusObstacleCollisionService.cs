// ======================================================
// VersusObstacleCollisionService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-18
// 概要     : 戦車と障害物の OBB 衝突判定処理を提供する
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using CollisionSystem.Calculator;
using CollisionSystem.Interface;
using ObstacleSystem.Data;
using TankSystem.Data;
using TankSystem.Interface;

namespace TankSystem.Service
{
    /// <summary>
    /// 戦車と障害物の衝突判定処理を専門に実装するサービス
    /// 障害物は Static OBB として扱われる
    /// </summary>
    public sealed class VersusObstacleCollisionService : ITankCollisionService
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>
        /// OBB 同士の水平方向衝突判定を行う計算器
        /// </summary>
        private readonly BoundingBoxCollisionCalculator _boxCollisionCalculator;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// 衝突判定対象となる戦車コンテキスト配列
        /// </summary>
        private readonly TankCollisionContext[] _tanks;

        /// <summary>
        /// シーン上に配置されている障害物コンテキスト配列
        /// </summary>
        private readonly ObstacleCollisionContext[] _obstacles;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// 戦車が障害物と衝突した際に通知されるイベント
        /// </summary>
        public event Action<TankCollisionContext, ObstacleCollisionContext> OnObstacleHit;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 戦車 vs 障害物 衝突判定サービスを生成する
        /// </summary>
        /// <param name="boxCollisionCalculator">OBB 同士の水平方向衝突判定を行う計算器</param>
        /// <param name="tanks">戦車コンテキスト</param>
        /// <param name="obstacles">障害物コンテキスト</param>
        public VersusObstacleCollisionService(
            in BoundingBoxCollisionCalculator boxCollisionCalculator,
            in TankCollisionContext[] tanks,
            in ObstacleCollisionContext[] obstacles
        )
        {
            _boxCollisionCalculator = boxCollisionCalculator;
            _tanks = tanks;
            _obstacles = obstacles;
        }

        // ======================================================
        // ITankCollisionService 実装
        // ======================================================

        /// <summary>
        /// 判定ループ開始前の事前処理
        /// 障害物は Static OBB のため更新処理は不要
        /// </summary>
        public void PreUpdate()
        {
            if (_tanks == null)
            {
                return;
            }

            // 全戦車の OBB を更新
            for (int i = 0; i < _tanks.Length; i++)
            {
                TankCollisionContext context = _tanks[i];
                context.UpdateOBB();
            }
        }

        /// <summary>
        /// 戦車と障害物の衝突判定を実行する
        /// </summary>
        public void Execute()
        {
            if (_obstacles == null || _obstacles.Length == 0)
            {
                return;
            }

            // --------------------------------------------------
            // 戦車 × 障害物 判定ループ
            // --------------------------------------------------
            for (int i = 0; i < _tanks.Length; i++)
            {
                // 判定対象戦車を取得する
                TankCollisionContext tankContext = _tanks[i];

                for (int o = 0; o < _obstacles.Length; o++)
                {
                    if (_obstacles[o] == null)
                    {
                        continue;
                    }

                    // OBB 衝突判定
                    if (!_boxCollisionCalculator.IsCollidingHorizontal(
                        tankContext.OBB,
                        _obstacles[o].OBB
                    ))
                    {
                        continue;
                    }

                    // 障害物衝突イベントを通知
                    OnObstacleHit?.Invoke(
                        tankContext,
                        _obstacles[o]
                    );
                }
            }
        }
    }
}