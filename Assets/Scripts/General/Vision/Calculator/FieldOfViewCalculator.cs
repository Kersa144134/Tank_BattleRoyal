// ======================================================
// FieldOfViewCalculator.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-22
// 更新日時 : 2026-03-10
// 概要     : 視界判定計算クラス
//            XZ 平面のみで視界判定を行う
// ======================================================

using UnityEngine;
using CollisionSystem.Data;
using VisionSystem.Utility;

namespace VisionSystem.Calculator
{
    /// <summary>
    /// 視界判定計算クラス
    /// 見下ろし型ゲーム向けに Y 軸を無視し、
    /// XZ 平面のみで視界判定を行う
    /// </summary>
    public sealed class FieldOfViewCalculator
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>遮蔽物判定用ユーティリティ</summary>
        private readonly LOSMath _losMath = new LOSMath();

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>現在視界内に存在する対象数</summary>
        private int _visibleTargetCount;

        /// <summary>現在有効な遮蔽物数</summary>
        private int _obstacleCount;

        /// <summary>視界内対象を距離順で保持する配列</summary>
        private readonly BaseCollisionContext[] _visibleTargetsArray
            = new BaseCollisionContext[MAX_TARGETS];

        /// <summary>視界対象の距離の 2 乗値を保持する配列</summary>
        private readonly float[] _visibleTargetDistances = new float[MAX_TARGETS];

        /// <summary>距離フィルタ後の遮蔽物キャッシュ配列</summary>
        private readonly BaseOBBData[] _obstacleCache = new BaseOBBData[MAX_OBSTACLES];

        /// <summary>FOV 値のキャッシュ</summary>
        private float _cachedFOV;

        /// <summary>半視野角 cos 値のキャッシュ</summary>
        private float _cachedHalfFOVCos;

        /// <summary>半視野角 cos の 2 乗値のキャッシュ</summary>
        private float _cachedHalfFOVCosSqr;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>視界対象最大数</summary>
        private const int MAX_TARGETS = 64;

        /// <summary>遮蔽物最大数</summary>
        private const int MAX_OBSTACLES = 128;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 視界内対象を取得
        /// </summary>
        public int GetVisibleTargets(
            in Transform origin,
            in BaseCollisionContext[] targets,
            in BaseOBBData[] obstacles,
            in float fovAngle,
            in float viewDistance,
            ref BaseCollisionContext[] outArray)
        {
            Vector3 originPos = origin.position;
            Vector3 originForward = origin.forward;

            // XZ 平面用 forward ベクトル
            Vector3 originForwardXZ = new Vector3(originForward.x, 0f, originForward.z);

            // --------------------------------------------------
            // FOV キャッシュ更新
            // --------------------------------------------------
            if (_cachedFOV != fovAngle)
            {
                _cachedFOV = fovAngle;

                // 半視野角をラジアンへ変換
                float halfRad = fovAngle * 0.5f * Mathf.Deg2Rad;

                // cos を計算
                _cachedHalfFOVCos = Mathf.Cos(halfRad);

                // cos の2乗
                _cachedHalfFOVCosSqr = _cachedHalfFOVCos * _cachedHalfFOVCos;
            }

            // 360 度視界かどうか判定
            bool isFullView =fovAngle >= 360f;

            // 視界距離の 2 乗を算出し、sqrt を回避
            float viewDistanceSqr = viewDistance * viewDistance;

            _visibleTargetCount = 0;

            // --------------------------------------------------
            // 遮蔽物の距離フィルタ処理
            // --------------------------------------------------
            _obstacleCount = 0;

            for (int i = 0; i < obstacles.Length; i++)
            {
                BaseOBBData obstacle = obstacles[i];

                // origin から obstacle 中心へのベクトル
                Vector3 delta = obstacle.Center - originPos;

                // 距離の 2 乗
                float sqrDist = delta.sqrMagnitude;

                // 視界距離外ならスキップ
                if (sqrDist > viewDistanceSqr)
                {
                    continue;
                }

                // キャッシュへ追加
                _obstacleCache[_obstacleCount] = obstacle;

                _obstacleCount++;
            }

            // --------------------------------------------------
            // ターゲット探索
            // --------------------------------------------------
            for (int i = 0; i < targets.Length; i++)
            {
                BaseOBBData targetOBB = targets[i].OBB;

                // origin から obstacle 中心へのベクトル
                Vector3 toTarget =
                    targetOBB.Center - originPos;

                // 距離の 2 乗
                float sqrDistance = toTarget.sqrMagnitude;

                // ターゲット OBB の半径
                float radius = targetOBB.BoundingRadius;

                // 半径の 2 乗
                float radiusSqr = radius * radius;

                // --------------------------------------------------
                // 距離判定（ブロードフェーズ）
                // --------------------------------------------------
                if (sqrDistance >
                    viewDistanceSqr + radiusSqr)
                {
                    continue;
                }

                // --------------------------------------------------
                // FOV 判定（ナローフェーズ）
                // --------------------------------------------------
                if (!isFullView)
                {
                    // ターゲット方向を XZ 平面へ投影
                    Vector3 toTargetXZ = new Vector3( toTarget.x, 0f, toTarget.z);

                    // XZ 平面距離の 2 乗
                    float sqrDistanceXZ = toTargetXZ.sqrMagnitude;

                    // 内積計算
                    float dot = Vector3.Dot(originForwardXZ, toTargetXZ);

                    // 後方なら視界外
                    if (dot <= 0f)
                    {
                        continue;
                    }

                    // FOV 角度判定
                    if (dot * dot < sqrDistanceXZ * _cachedHalfFOVCosSqr)
                    {
                        continue;
                    }
                }

                // --------------------------------------------------
                // 遮蔽物判定
                // --------------------------------------------------
                bool blocked = false;

                for (int j = 0; j < _obstacleCount; j++)
                {
                    BaseOBBData obstacle = _obstacleCache[j];

                    // 自身を除外
                    if (obstacle == targetOBB)
                    {
                        continue;
                    }

                    // LoS 判定
                    if (_losMath.IsLineIntersectOBB(
                        originPos,
                        targetOBB.Center,
                        obstacle))
                    {
                        blocked = true;

                        break;
                    }
                }

                if (blocked)
                {
                    continue;
                }

                // 距離順に挿入
                InsertVisibleTarget(targets[i], sqrDistance);
            }

            // 結果をコピー
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
        /// 距離順でターゲットを挿入
        /// </summary>
        private void InsertVisibleTarget(
            BaseCollisionContext target,
            float sqrDistance)
        {
            int low = 0;
            int high = _visibleTargetCount;

            // --------------------------------------------------
            // 二分探索
            // --------------------------------------------------
            while (low < high)
            {
                // 探索範囲の中央インデックス
                int mid = (low + high) / 2;

                // --------------------------------------------------
                // 中央要素の距離の 2 乗
                // --------------------------------------------------
                float midDist = _visibleTargetDistances[mid];

                // 対象の距離が中央要素未満の場合、手前に挿入
                if (sqrDistance < midDist)
                {
                    high = mid;
                }
                // 対象の距離が中央要素以上の場合、後方に挿入
                else
                {
                    low = mid + 1;
                }
            }

            int insertIndex = low;

            // --------------------------------------------------
            // 配列シフト
            // --------------------------------------------------
            for (int i = _visibleTargetCount; i > insertIndex; i--)
            {
                // Transform配列を 1 つ後ろへ移動
                _visibleTargetsArray[i] =
                    _visibleTargetsArray[i - 1];

                // 距離の 2 乗配列も同様に 1 つ後ろへ移動
                _visibleTargetDistances[i] =
                    _visibleTargetDistances[i - 1];
            }

            // --------------------------------------------------
            // ターゲット挿入
            // --------------------------------------------------
            _visibleTargetsArray[insertIndex] = target;

            // 挿入したターゲットの距離の 2 乗を保存
            _visibleTargetDistances[insertIndex] = sqrDistance;

            // 視界対象数を更新
            _visibleTargetCount++;
        }
    }
}