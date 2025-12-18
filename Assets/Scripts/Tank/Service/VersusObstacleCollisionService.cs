// ======================================================
// VersusObstacleCollisionService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-18
// 概要     : 戦車と障害物の OBB 衝突判定処理を提供する
// ======================================================

using CollisionSystem.Calculator;
using CollisionSystem.Interface;
using System;
using System.Collections.Generic;
using TankSystem.Data;
using TankSystem.Interface;
using TankSystem.Utility;
using UnityEngine;

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
        /// 衝突判定対象となる戦車コンテキスト一覧
        /// </summary>
        private readonly List<TankCollisionContext> _tanks;

        /// <summary>
        /// シーン上に配置されている障害物 Transform 配列
        /// </summary>
        private readonly Transform[] _obstacles;

        /// <summary>
        /// 各障害物に対応する Static OBB 配列
        /// Transform 配列とインデックス対応している
        /// </summary>
        private IOBBData[] _obstacleOBBs;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// 戦車が障害物と衝突した際に通知されるイベント
        /// </summary>
        public event Action<TankCollisionContext, Transform> OnObstacleHit;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 戦車 vs 障害物 衝突判定サービスを生成する
        /// </summary>
        public VersusObstacleCollisionService(
            in BoundingBoxCollisionCalculator boxCollisionCalculator,
            in List<TankCollisionContext> tanks,
            in Transform[] obstacles
        )
        {
            // 衝突計算器参照を保持する
            _boxCollisionCalculator = boxCollisionCalculator;

            // 戦車コンテキスト一覧参照を保持する
            _tanks = tanks;

            // 障害物 Transform 配列参照を保持する
            _obstacles = obstacles;
        }

        // ======================================================
        // セッター
        // ======================================================

        /// <summary>
        /// 障害物 Transform から Static OBB を生成しキャッシュする
        /// </summary>
        public void SetObstacleOBBs(in OBBFactory obbFactory)
        {
            // 障害物が存在しない場合は空配列を設定
            if (_obstacles == null || _obstacles.Length == 0)
            {
                _obstacleOBBs = Array.Empty<IOBBData>();
                return;
            }

            // 障害物数分の OBB 配列を生成
            _obstacleOBBs = new IOBBData[_obstacles.Length];

            // 各障害物から Static OBB を生成
            for (int i = 0; i < _obstacles.Length; i++)
            {
                _obstacleOBBs[i] =
                    obbFactory.CreateStaticOBB(
                        _obstacles[i],
                        Vector3.zero,
                        Vector3.one
                    );
            }
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
            for (int i = 0; i < _tanks.Count; i++)
            {
                TankCollisionContext context = _tanks[i];
                context.OBB.Update();
            }
        }

        /// <summary>
        /// 戦車と障害物の衝突判定を実行する
        /// </summary>
        public void Execute()
        {
            if (_obstacleOBBs == null || _obstacleOBBs.Length == 0)
            {
                return;
            }

            // --------------------------------------------------
            // 戦車 × 障害物 判定ループ
            // --------------------------------------------------
            for (int i = 0; i < _tanks.Count; i++)
            {
                // 判定対象戦車を取得する
                TankCollisionContext tankContext = _tanks[i];

                for (int o = 0; o < _obstacleOBBs.Length; o++)
                {
                    if (_obstacleOBBs[o] == null)
                    {
                        continue;
                    }

                    // OBB 衝突判定
                    if (!_boxCollisionCalculator.IsCollidingHorizontal(
                        tankContext.OBB,
                        _obstacleOBBs[o]
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