// ======================================================
// VersusTankCollisionService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-18
// 概要     : 戦車同士の OBB 衝突判定を担当するサービス
//            判定ループは内部で完結させる
// ======================================================

using CollisionSystem.Calculator;
using System;
using System.Collections.Generic;
using TankSystem.Data;
using TankSystem.Interface;
using UnityEngine;

namespace TankSystem.Service
{
    /// <summary>
    /// 戦車同士の衝突判定を専門に処理するサービス
    /// 各戦車ペアの重複判定を防ぎつつ判定を行う
    /// </summary>
    public sealed class VersusTankCollisionService : ITankCollisionService
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

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// 戦車同士が接触した際に通知されるイベント
        /// </summary>
        public event Action<TankCollisionContext, TankCollisionContext> OnTankHit;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 戦車 vs 戦車 衝突判定サービスを生成する
        /// </summary>
        /// <param name="boxCollisionCalculator">OBB 同士の水平方向衝突判定を行う計算器</param>
        /// <param name="tanks">戦車コンテキスト</param>
        public VersusTankCollisionService(
            in BoundingBoxCollisionCalculator boxCollisionCalculator,
            in TankCollisionContext[] tanks
        )
        {
            _boxCollisionCalculator = boxCollisionCalculator;
            _tanks = tanks;
        }

        // ======================================================
        // ITankCollisionService 実装
        // ======================================================

        /// <summary>
        /// 判定ループ開始前の事前処理
        /// 全戦車の OBB を最新状態に更新する
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
        /// 戦車同士の衝突判定を実行する
        /// </summary>
        public void Execute()
        {
            if (_tanks == null || _tanks.Length < 2)
            {
                return;
            }

            // --------------------------------------------------
            // 戦車 × 戦車 判定ループ
            // --------------------------------------------------
            for (int i = 0; i < _tanks.Length - 1; i++)
            {
                // 判定基準となる戦車 A
                TankCollisionContext tankA = _tanks[i];

                for (int j = i + 1; j < _tanks.Length; j++)
                {
                    // 判定対象となる戦車 B
                    TankCollisionContext tankB = _tanks[j];

                    // OBB 衝突判定
                    if (!_boxCollisionCalculator.IsCollidingHorizontal(
                        tankA.OBB,
                        tankB.OBB
                    ))
                    {
                        continue;
                    }

                    // 戦車同士の衝突イベントを通知
                    OnTankHit?.Invoke(
                        tankA,
                        tankB
                    );
                }
            }
        }
    }
}