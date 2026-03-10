// ======================================================
// FieldOfViewCalculator.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-22
// 更新日時 : 2026-02-18
// 概要     : 視界判定計算クラス
// ======================================================

using UnityEngine;
using CollisionSystem.Data;
using VisionSystem.Utility;

namespace VisionSystem.Calculator
{
    /// <summary>
    /// 視界判定計算クラス
    /// </summary>
    public sealed class FieldOfViewCalculator
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>遮蔽物判定用ユーティリティ</summary>
        private LOSMath _losMath = new LOSMath();

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>視界内対象物を距離順で保持する固定配列</summary>
        private readonly Transform[] _visibleTargetsArray = new Transform[MAX_TARGETS];

        /// <summary>現在視界内に存在する対象の数</summary>
        private int _visibleTargetCount;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>視界判定対象の最大数</summary>
        private const int MAX_TARGETS = 64;

        /// <summary>線分交差判定用の小さな許容値（浮動小数点誤差対策）</summary>
        private const float LINE_INTERSECTION_EPSILON = 0.0001f;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 視界内にある対象物を取得
        /// </summary>
        /// <param name="origin">視界の中心 Transform</param>
        /// <param name="targets">判定対象 BaseCollisionContext 配列</param>
        /// <param name="obstacles">遮蔽物 OBB 配列</param>
        /// <param name="fovAngle">視野角（全角）</param>
        /// <param name="viewDistance">視界距離</param>
        /// <param name="outArray">結果を書き込む配列</param>
        /// <returns>視界内対象の数</returns>
        public int GetVisibleTargets(
            in Transform origin,
            in BaseCollisionContext[] targets,
            in BaseOBBData[] obstacles,
            in float fovAngle,
            in float viewDistance,
            ref Transform[] outArray
        )
        {
            _visibleTargetCount = 0;

            // 平方根計算を回避するため視界距離の二乗値を算出
            float viewDistanceSqr = viewDistance * viewDistance;

            // 360度視界かどうかを判定
            bool isFullView = fovAngle >= 360f;

            // 視野角判定用の cosθ を事前計算
            float halfFOVCos = isFullView
                ? -1f
                : Mathf.Cos( fovAngle * 0.5f * Mathf.Deg2Rad);

            for (int i = 0; i < targets.Length; i++)
            {
                BaseOBBData targetOBB = targets[i].OBB;

                // --------------------------------------------------
                // ブロードフェーズ
                // 距離判定
                // --------------------------------------------------
                // 視界中心から対象 OBB 中心へのベクトルを算出
                Vector3 toTarget = targetOBB.Center - origin.position;

                // ベクトルの二乗長さを取得
                float sqrDistance = toTarget.sqrMagnitude;

                // 対象 OBB の半径の二乗値を算出
                float radiusSqr =
                    targetOBB.BoundingRadius *
                    targetOBB.BoundingRadius;

                // 視界距離 + OBB半径を超えている場合は視界外とする
                if (sqrDistance > viewDistanceSqr + radiusSqr)
                {
                    continue;
                }

                // --------------------------------------------------
                // 視野角判定
                // --------------------------------------------------
                // 360度視界の場合は角度判定をスキップ
                if (!isFullView)
                {
                    // 正規化用ベクトルをコピー
                    Vector3 dir = toTarget;

                    // ベクトル長さの二乗を取得
                    float magnitude = dir.sqrMagnitude;

                    // ゼロ除算回避のため十分な長さがある場合のみ正規化
                    if (magnitude > LINE_INTERSECTION_EPSILON)
                    {
                        // 平方根で正規化
                        dir /=  Mathf.Sqrt(magnitude);
                    }

                    // 視線方向との内積を取得
                    float dot = Vector3.Dot( origin.forward, dir);

                    // 半視野角より外側の場合は視界外と判定
                    if (dot < halfFOVCos)
                    {
                        continue;
                    }
                }

                // --------------------------------------------------
                // 遮蔽物判定
                // --------------------------------------------------
                // 遮蔽フラグを初期化
                bool blocked = false;

                for (int j = 0; j < obstacles.Length; j++)
                {
                    BaseOBBData obstacle = obstacles[j];

                    // 対象自身は遮蔽判定から除外
                    if (obstacle == targetOBB)
                    {
                        continue;
                    }

                    // 原点から対象中心への線分が障害物に交差するか
                    if (_losMath.IsLineIntersectOBB(
                        origin.position,
                        targetOBB.Center,
                        obstacle))
                    {
                        // 1つでも遮蔽物があれば遮蔽状態と判定
                        blocked = true;

                        break;
                    }
                }

                // --------------------------------------------------
                // 距離順登録
                // --------------------------------------------------
                // 遮蔽されていない場合のみ登録
                if (!blocked)
                {
                    // 距離順で配列に挿入
                    InsertVisibleTarget(
                        origin.position,
                        targets[i].Transform,
                        sqrDistance);
                }
            }

            // 内部配列の内容を呼び出し側配列へコピーする
            for (int i = 0; i < _visibleTargetCount; i++)
            {
                outArray[i] = _visibleTargetsArray[i];
            }

            return _visibleTargetCount;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 二分探索で距離順に対象を挿入
        /// </summary>
        private void InsertVisibleTarget(Vector3 originPos, Transform target, float targetSqrDistance)
        {
            int low = 0;
            int high = _visibleTargetCount;

            // 二分探索で挿入位置を決定
            while (low < high)
            {
                int mid = (low + high) / 2;
                float midDistSqr = (_visibleTargetsArray[mid].position - originPos).sqrMagnitude;
                if (targetSqrDistance < midDistSqr)
                    high = mid;
                else
                    low = mid + 1;
            }

            int insertIndex = low;

            // 配列内で後ろにシフト
            for (int k = _visibleTargetCount; k > insertIndex; k--)
            {
                _visibleTargetsArray[k] = _visibleTargetsArray[k - 1];
            }

            // 挿入
            _visibleTargetsArray[insertIndex] = target;
            _visibleTargetCount++;
        }
    }
}