// ======================================================
// VersusTankCollisionService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : TankCollisionContext を用いて戦車同士の衝突判定・解決を行うサービス
// ======================================================

using CollisionSystem.Calculator;
using CollisionSystem.Data;
using System;
using System.Collections.Generic;
using TankSystem.Data;
using TankSystem.Interface;
using UnityEngine;

namespace TankSystem.Service
{
    /// <summary>
    /// 戦車とアイテムの衝突判定を担当する
    /// </summary>
    public sealed class VersusTankCollisionService : ITankCollisionService
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

        /// <summary>
        /// 衝突判定対象となる戦車コンテキスト一覧
        /// 登録順は外部で制御される
        /// </summary>
        private List<TankCollisionContext> _tanks;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// 戦車同士が接触した際に通知されるイベント
        /// 引数は衝突した 2 台の TankCollisionContext
        /// </summary>
        public event Action<TankCollisionContext, TankCollisionContext> OnTankHit;

        // ======================================================
        // コンストラクタ
        // ======================================================

        public VersusTankCollisionService(
            in BoundingBoxCollisionCalculator boxCollisionCalculator,
            in List<TankCollisionContext> tanks
        )
        {
            _boxCollisionCalculator = boxCollisionCalculator;
            _tanks = tanks;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 全戦車同士の衝突判定を実行し、
        /// 接触している組み合わせをイベントとして通知する
        /// </summary>
        public void UpdateCollisionChecks()
        {
            // コンテキストが未設定、または 2 台未満なら処理しない
            if (_tanks == null || _tanks.Count < 2)
            {
                return;
            }

            // --------------------------------------------------
            // 総当たり判定
            // --------------------------------------------------
            for (int i = 0; i < _tanks.Count - 1; i++)
            {
                // 判定対象戦車Aのコンテキストを取得
                TankCollisionContext contextA = _tanks[i];

                // 戦車Aの OBB を最新状態に更新
                contextA.OBB.Update();

                for (int j = i + 1; j < _tanks.Count; j++)
                {
                    // 判定対象戦車Bのコンテキストを取得
                    TankCollisionContext contextB = _tanks[j];

                    // 戦車Bの OBB を最新状態に更新
                    contextB.OBB.Update();

                    // 水平方向の OBB 衝突を判定
                    if (_boxCollisionCalculator.IsCollidingHorizontal(
                        contextA.OBB,
                        contextB.OBB))
                    {
                        // 衝突している戦車ペアを通知
                        OnTankHit?.Invoke(
                            contextA,
                            contextB
                        );
                    }
                }
            }
        }

        /// <summary>
        /// 衝突している 2 台の戦車に対して MTV を用いた押し戻し量を計算する
        /// 軸ロック状態を最優先に考慮し、押し戻し先を決定する
        /// </summary>
        /// <param name="contextA">戦車Aの衝突コンテキスト</param>
        /// <param name="contextB">戦車Bの衝突コンテキスト</param>
        /// <param name="deltaForwardA">戦車Aの前進量</param>
        /// <param name="deltaForwardB">戦車Bの前進量</param>
        /// <param name="resolveInfoA">戦車Aに適用する衝突解決情報</param>
        /// <param name="resolveInfoB">戦車Bに適用する衝突解決情報</param>
        public void CalculateResolveInfo(
            in TankCollisionContext contextA,
            in TankCollisionContext contextB,
            in float deltaForwardA,
            in float deltaForwardB,
            out CollisionResolveInfo resolveInfoA,
            out CollisionResolveInfo resolveInfoB
        )
        {
            // 初期化
            resolveInfoA = default;
            resolveInfoB = default;

            // 現フレームの移動ロック軸を取得
            MovementLockAxis lockAxisA =
                contextA.RootManager.CurrentFrameLockAxis;

            MovementLockAxis lockAxisB =
                contextB.RootManager.CurrentFrameLockAxis;

            // OBB を最新状態に更新
            contextA.OBB.Update();
            contextB.OBB.Update();

            // MTV を算出
            if (!_boxCollisionCalculator.TryCalculateHorizontalMTV(
                contextA.OBB,
                contextB.OBB,
                out Vector3 resolveAxis,
                out float resolveDistance
            ))
            {
                return;
            }

            // --------------------------------------------------
            // 押し戻し方向補正（中心差）
            // --------------------------------------------------
            Vector3 centerDelta =
                contextA.OBB.Center - contextB.OBB.Center;

            centerDelta.y = 0f;

            if (Vector3.Dot(resolveAxis, centerDelta) < 0f)
            {
                resolveAxis = -resolveAxis;
            }

            // --------------------------------------------------
            // 最終押し戻しベクトル算出
            // --------------------------------------------------
            Vector3 finalResolveA = Vector3.zero;
            Vector3 finalResolveB = Vector3.zero;

            // --------------------------
            // X 軸成分
            // --------------------------
            if ((lockAxisA & MovementLockAxis.X) != 0)
            {
                finalResolveB.x = -resolveAxis.x * resolveDistance;
            }
            else if ((lockAxisB & MovementLockAxis.X) != 0)
            {
                finalResolveA.x = resolveAxis.x * resolveDistance;
            }
            else
            {
                DistributeResolve(
                    resolveAxis.x * resolveDistance,
                    deltaForwardA,
                    deltaForwardB,
                    out finalResolveA.x,
                    out finalResolveB.x
                );
            }

            // --------------------------
            // Z 軸成分
            // --------------------------
            if ((lockAxisA & MovementLockAxis.Z) != 0)
            {
                finalResolveB.z = -resolveAxis.z * resolveDistance;
            }
            else if ((lockAxisB & MovementLockAxis.Z) != 0)
            {
                finalResolveA.z = resolveAxis.z * resolveDistance;
            }
            else
            {
                DistributeResolve(
                    resolveAxis.z * resolveDistance,
                    deltaForwardA,
                    deltaForwardB,
                    out finalResolveA.z,
                    out finalResolveB.z
                );
            }

            // --------------------------------------------------
            // CollisionResolveInfo 構築
            // --------------------------------------------------
            resolveInfoA = new CollisionResolveInfo
            {
                ResolveDirection = finalResolveA.normalized,
                ResolveDistance = finalResolveA.magnitude,
                IsValid = finalResolveA.sqrMagnitude > 0f
            };

            resolveInfoB = new CollisionResolveInfo
            {
                ResolveDirection = finalResolveB.normalized,
                ResolveDistance = finalResolveB.magnitude,
                IsValid = finalResolveB.sqrMagnitude > 0f
            };
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 前進量を基準として押し戻し量を A / B に分配する
        /// </summary>
        /// <param name="resolveValue">軸方向の押し戻し量</param>
        /// <param name="deltaA">戦車Aの前進量</param>
        /// <param name="deltaB">戦車Bの前進量</param>
        /// <param name="outA">戦車Aに割り当てる押し戻し量</param>
        /// <param name="outB">戦車Bに割り当てる押し戻し量</param>
        private void DistributeResolve(
            float resolveValue,
            float deltaA,
            float deltaB,
            out float outA,
            out float outB
        )
        {
            // 初期化
            outA = 0f;
            outB = 0f;

            // 両者が移動している場合は前進量が小さい側を優先
            if (!Mathf.Approximately(deltaA, 0f) &&
                !Mathf.Approximately(deltaB, 0f))
            {
                if (Mathf.Abs(deltaA) <= Mathf.Abs(deltaB))
                {
                    outA = resolveValue;
                }
                else
                {
                    outB = -resolveValue;
                }

                return;
            }

            // A のみ移動している場合
            if (!Mathf.Approximately(deltaA, 0f))
            {
                outB = -resolveValue;
                return;
            }

            // B のみ移動している場合
            if (!Mathf.Approximately(deltaB, 0f))
            {
                outA = resolveValue;
            }
        }
    }
}