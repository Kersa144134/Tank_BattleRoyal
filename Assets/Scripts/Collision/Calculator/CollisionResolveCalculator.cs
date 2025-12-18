// ======================================================
// CollisionResolveCalculator.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : MTV を用いた衝突解決量を計算するクラス
// ======================================================

using UnityEngine;
using CollisionSystem.Data;

namespace CollisionSystem.Calculator
{
    /// <summary>
    /// 衝突している 2 オブジェクト間の押し戻し情報を算出するクラス
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
        // 定数
        // ======================================================

        /// <summary>微小押し戻し量の下限値</summary>
        private const float MIN_RESOLVE_DISTANCE = 0.001f;

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
        /// 微小量でも Vector3 を保持し、フレームごとの揺れを防ぐ
        /// </summary>
        public void CalculateResolveInfo(
            in BaseCollisionContext contextA,
            in BaseCollisionContext contextB,
            in float deltaForwardA,
            in float deltaForwardB,
            out CollisionResolveInfo resolveInfoA,
            out CollisionResolveInfo resolveInfoB
        )
        {
            // 初期化
            resolveInfoA = default;
            resolveInfoB = default;

            // 移動ロック軸取得
            MovementLockAxis lockAxisA = contextA.LockAxis;
            MovementLockAxis lockAxisB = contextB.LockAxis;

            // OBB を最新状態に更新
            contextA.UpdateOBB();
            contextB.UpdateOBB();

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
            Vector3 centerDelta = contextA.OBB.Center - contextB.OBB.Center;
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

            // X 軸押し戻し量計算
            ResolveAxis(
                resolveAxis.x,
                resolveDistance,
                deltaForwardA,
                deltaForwardB,
                lockAxisA & MovementLockAxis.X,
                lockAxisB & MovementLockAxis.X,
                contextB.LockAxis == MovementLockAxis.All,
                out finalResolveA.x,
                out finalResolveB.x
            );

            // Z 軸押し戻し量計算
            ResolveAxis(
                resolveAxis.z,
                resolveDistance,
                deltaForwardA,
                deltaForwardB,
                lockAxisA & MovementLockAxis.Z,
                lockAxisB & MovementLockAxis.Z,
                contextB.LockAxis == MovementLockAxis.All,
                out finalResolveA.z,
                out finalResolveB.z
            );

            // --------------------------------------------------
            // 微小押し戻し量補正
            // --------------------------------------------------
            if (finalResolveA.sqrMagnitude > 0f && finalResolveA.sqrMagnitude < MIN_RESOLVE_DISTANCE * MIN_RESOLVE_DISTANCE)
            {
                finalResolveA = finalResolveA.normalized * MIN_RESOLVE_DISTANCE;
                Debug.Log($"[ResolveVector] a {finalResolveA}");
            }

            if (finalResolveB.sqrMagnitude > 0f && finalResolveB.sqrMagnitude < MIN_RESOLVE_DISTANCE * MIN_RESOLVE_DISTANCE)
            {
                finalResolveB = finalResolveB.normalized * MIN_RESOLVE_DISTANCE;
            }

            // --------------------------------------------------
            // CollisionResolveInfo 構築（normalized せずそのままベクトル保持）
            // --------------------------------------------------
            resolveInfoA = new CollisionResolveInfo(finalResolveA);
            resolveInfoB = new CollisionResolveInfo(finalResolveB);

            // --------------------------------------------------
            // 押し戻しが発生した軸を LockAxis に反映
            // --------------------------------------------------
            MovementLockAxis newLockAxis = 0;

            const float EPS = 0.001f; // 微小量判定用

            if (Mathf.Abs(finalResolveA.x) > EPS)
            {
                newLockAxis |= MovementLockAxis.X;
            }

            if (Mathf.Abs(finalResolveA.z) > EPS)
            {
                newLockAxis |= MovementLockAxis.Z;
            }

            contextA.UpdateLockAxis(
                newLockAxis == (MovementLockAxis.X | MovementLockAxis.Z)
                ? MovementLockAxis.All
                : newLockAxis
            );

            // --------------------------------------------------
            // デバッグログ
            // --------------------------------------------------
            Debug.Log($"[ResolveVector] b {finalResolveA}");
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
            if (lockAxisA != 0)
            {
                if (isBMovable)
                {
                    outB = -axisValue * resolveDistance;
                }
                return;
            }

            // B がロックされている場合は A のみ押し戻す
            if (lockAxisB != 0)
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