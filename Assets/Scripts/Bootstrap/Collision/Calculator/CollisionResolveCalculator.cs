// ======================================================
// CollisionResolveCalculator.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : 衝突解決量を計算するクラス
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
        /// OBB 同士の衝突計算を行う計算器
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
        /// 衝突している 2 オブジェクトに対して押し戻し軸と距離を算出する
        /// </summary>
        /// <param name="contextA">押し戻し対象のオブジェクト A のコンテキスト</param>
        /// <param name="contextB">押し戻し対象のオブジェクト B のコンテキスト</param>
        /// <param name="forwardSpeedA">オブジェクト A の前進速度</param>
        /// <param name="forwardSpeedB">オブジェクト B の前進速度</param>
        /// <param name="resolveInfoA">計算結果としてのオブジェクト A の押し戻し情報</param>
        /// <param name="resolveInfoB">計算結果としてのオブジェクト B の押し戻し情報</param>
        public void CalculateResolveInfo(
            in BaseCollisionContext contextA,
            in BaseCollisionContext contextB,
            in float forwardSpeedA,
            in float forwardSpeedB,
            out CollisionResolveInfo resolveInfoA,
            out CollisionResolveInfo resolveInfoB
        )
        {
            // 初期化
            resolveInfoA = default;
            resolveInfoB = default;

            // OBB 更新
            contextA.UpdateOBB();
            contextB.UpdateOBB();

            // 移動ロック軸取得
            MovementLockAxis lockAxisA = contextA.LockAxis;
            MovementLockAxis lockAxisB = contextB.LockAxis;

            // 押し戻し軸と距離を算出
            if (!_boxCollisionCalculator.TryGetPushOutAxisAndDistance(
                contextA.OBB,
                contextB.OBB,
                out Vector3 resolveAxis,
                out float resolveDistance
            ))
            {
                return;
            }

            // --------------------------------------------------
            // 押し戻し方向補正
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
                forwardSpeedA,
                forwardSpeedB,
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
                forwardSpeedA,
                forwardSpeedB,
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
            }

            if (finalResolveB.sqrMagnitude > 0f && finalResolveB.sqrMagnitude < MIN_RESOLVE_DISTANCE * MIN_RESOLVE_DISTANCE)
            {
                finalResolveB = finalResolveB.normalized * MIN_RESOLVE_DISTANCE;
            }

            // --------------------------------------------------
            // CollisionResolveInfo 生成
            // --------------------------------------------------
            resolveInfoA = new CollisionResolveInfo(finalResolveA);
            resolveInfoB = new CollisionResolveInfo(finalResolveB);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 単一軸方向の押し戻し量を決定する
        /// </summary>
        /// <param name="axisValue">押し戻し軸の方向成分</param>
        /// <param name="resolveDistance">押し戻し量</param>
        /// <param name="forwardSpeedA">オブジェクト A の前進速度</param>
        /// <param name="forwardSpeedB">オブジェクト B の前進速度</param>
        /// <param name="lockAxisA">オブジェクト A のロック軸状態</param>
        /// <param name="lockAxisB">オブジェクト B のロック軸状態</param>
        /// <param name="isBMovable">オブジェクト B が完全にロックされていないか</param>
        /// <param name="outA">計算結果として出力されるオブジェクト A の押し戻し量</param>
        /// <param name="outB">計算結果として出力されるオブジェクト B の押し戻し量</param>
        private void ResolveAxis(
        float axisValue,
            in float resolveDistance,
            in float forwardSpeedA,
            in float forwardSpeedB,
            in MovementLockAxis lockAxisA,
            in MovementLockAxis lockAxisB,
            in bool isBMovable,
            out float outA,
            out float outB
        )
        {
            // 初期化
            outA = 0f;
            outB = 0f;

            // A がロックされている場合
            if (lockAxisA != 0)
            {
                if (isBMovable)
                {
                    outB = -axisValue * resolveDistance;
                }

                string lockedAxesA = "";
                if ((lockAxisA & MovementLockAxis.X) != 0) lockedAxesA += "X ";
                if ((lockAxisA & MovementLockAxis.Z) != 0) lockedAxesA += "Z ";

                return;
            }

            // B がロックされている場合
            if (lockAxisB != 0)
            {
                outA = axisValue * resolveDistance;

                string lockedAxesB = "";
                if ((lockAxisB & MovementLockAxis.X) != 0) lockedAxesB += "X ";
                if ((lockAxisB & MovementLockAxis.Z) != 0) lockedAxesB += "Z ";

                return;
            }

            // 両者が可動の場合は前進速度で分配する
            DistributeResolve(
                axisValue * resolveDistance,
                forwardSpeedA,
                forwardSpeedB,
                out outA,
                out outB
            );
        }

        /// <summary>
        /// 前進量を基準として押し戻し量を分配する
        /// </summary>
        /// <param name="resolveValue">総押し戻し量</param>
        /// <param name="forwardSpeedA">オブジェクト A の前進速度</param>
        /// <param name="forwardSpeedB">オブジェクト B の前進速度</param>
        /// <param name="outA">計算結果として出力されるオブジェクト A の押し戻し量</param>
        /// <param name="outB">計算結果として出力されるオブジェクト B の押し戻し量</param>
        private void DistributeResolve(
            in float resolveValue,
            in float forwardSpeedA,
            in float forwardSpeedB,
            out float outA,
            out float outB
        )
        {
            // 初期化
            outA = 0f;
            outB = 0f;

            // 両者が移動している場合
            if (!Mathf.Approximately(forwardSpeedA, 0f) &&
                !Mathf.Approximately(forwardSpeedB, 0f))
            {
                if (Mathf.Abs(forwardSpeedA) <= Mathf.Abs(forwardSpeedB))
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
            if (!Mathf.Approximately(forwardSpeedA, 0f))
            {
                outB = -resolveValue;
                return;
            }

            // B のみ移動している場合
            if (!Mathf.Approximately(forwardSpeedB, 0f))
            {
                outA = resolveValue;
            }
        }
    }
}