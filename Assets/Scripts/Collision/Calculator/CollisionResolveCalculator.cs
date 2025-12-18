// ======================================================
// CollisionResolveCalculator.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : MTV を用いた衝突解決量を計算する責務クラス
// ======================================================

using CollisionSystem.Data;
using UnityEngine;

namespace CollisionSystem.Calculator
{
    /// <summary>
    /// 衝突している 2 オブジェクト間の押し戻し情報を算出する
    /// 戦車同士・戦車 vs 障害物の両方に対応する
    /// </summary>
    public sealed class CollisionResolveCalculator
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// OBB 同士の MTV 計算を行う計算器
        /// </summary>
        private readonly BoundingBoxCollisionCalculator _boxCollisionCalculator;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// CollisionResolveCalculator を生成する
        /// </summary>
        /// <param name="calculator">OBB 衝突計算器</param>
        public CollisionResolveCalculator(
            in BoundingBoxCollisionCalculator calculator
        )
        {
            _boxCollisionCalculator = calculator;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 衝突している 2 オブジェクトに対して MTV を用いた押し戻し量を計算する
        /// </summary>
        /// <param name="contextA">解決対象 A</param>
        /// <param name="contextB">解決対象 B</param>
        /// <param name="deltaForwardA">A の前進量</param>
        /// <param name="deltaForwardB">B の前進量（固定物なら 0）</param>
        /// <param name="isBMovable">B が移動可能かどうか</param>
        /// <param name="resolveInfoA">A に適用する解決情報</param>
        /// <param name="resolveInfoB">B に適用する解決情報</param>
        public void CalculateResolveInfo(
            in BaseCollisionContext contextA,
            in BaseCollisionContext contextB,
            in float deltaForwardA,
            in float deltaForwardB,
            bool isBMovable,
            out CollisionResolveInfo resolveInfoA,
            out CollisionResolveInfo resolveInfoB
        )
        {
            // 初期化
            resolveInfoA = default;
            resolveInfoB = default;

            // A 側の移動ロック軸を取得する
            MovementLockAxis lockAxisA =
                contextA.LockAxis;

            // B 側は固定物の場合は全軸ロック扱いとする
            MovementLockAxis lockAxisB =
                isBMovable ? contextB.LockAxis : MovementLockAxis.X | MovementLockAxis.Z;

            // OBB を最新状態に更新する
            contextA.OBB.Update();
            contextB.OBB.Update();

            // MTV を算出する
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
            // X 軸
            // --------------------------
            ResolveAxis(
                resolveAxis.x,
                resolveDistance,
                deltaForwardA,
                deltaForwardB,
                lockAxisA,
                lockAxisB,
                isBMovable,
                out finalResolveA.x,
                out finalResolveB.x
            );

            // --------------------------
            // Z 軸
            // --------------------------
            ResolveAxis(
                resolveAxis.z,
                resolveDistance,
                deltaForwardA,
                deltaForwardB,
                lockAxisA,
                lockAxisB,
                isBMovable,
                out finalResolveA.z,
                out finalResolveB.z
            );

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
        /// 単一軸方向の押し戻し量を決定する
        /// </summary>
        private void ResolveAxis(
            float axisValue,
            float resolveDistance,
            float deltaA,
            float deltaB,
            MovementLockAxis lockAxisA,
            MovementLockAxis lockAxisB,
            bool isBMovable,
            out float outA,
            out float outB
        )
        {
            // 初期化
            outA = 0f;
            outB = 0f;

            // A がロックされている場合は B のみ押し戻す
            if ((lockAxisA & MovementLockAxis.X) != 0)
            {
                if (isBMovable)
                {
                    outB = -axisValue * resolveDistance;
                }
                return;
            }

            // B がロックされている場合は A のみ押し戻す
            if ((lockAxisB & MovementLockAxis.X) != 0)
            {
                outA = axisValue * resolveDistance;
                return;
            }

            // 両者が可動の場合は前進量で分配する
            DistributeResolve(
                axisValue * resolveDistance,
                deltaA,
                deltaB,
                out outA,
                out outB
            );
        }

        /// <summary>
        /// 前進量を基準として押し戻し量を分配する
        /// </summary>
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

            // 両者が移動している場合
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