// ======================================================
// FieldOfViewCalculator.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-22
// 更新日時 : 2025-12-22
// 概要     : Transform ベースの視界判定ユースケースクラス
//            二分探索を用いて距離順に視界内対象物を維持
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using CollisionSystem.Interface;
using VisionSystem.Utility;

namespace VisionSystem.Calculator
{
    /// <summary>
    /// 視界判定のユースケースクラス
    /// 距離・角度・遮蔽物を考慮して視界内の対象物を判定する
    /// </summary>
    public sealed class FieldOfViewCalculator
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>距離・角度判定用ユーティリティ</summary>
        private readonly FOVMath _fovMath;

        /// <summary>遮蔽物判定用ユーティリティ</summary>
        private readonly LOSMath _losMath;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>視界内にある対象物を格納するバッファ</summary>
        private readonly List<Transform> _visibleTargetsBuffer;

        // ======================================================
        // 定数
        // ======================================================

        private const int MAX_TARGETS = 128;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 視界判定計算クラスを生成する
        /// </summary>
        public FieldOfViewCalculator()
        {
            _fovMath = new FOVMath();
            _losMath = new LOSMath();
            _visibleTargetsBuffer = new List<Transform>(MAX_TARGETS);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 視界内にある対象物を取得
        /// </summary>
        /// <param name="origin">視界の中心 Transform</param>
        /// <param name="targets">判定対象の Transform 配列</param>
        /// <param name="obstacles">遮蔽物 OBB 配列</param>
        /// <param name="fovAngle">視野角（全角）</param>
        /// <param name="viewDistance">視界距離</param>
        /// <returns>距離順で遮蔽されていない視界内対象物リスト</returns>
        public List<Transform> GetVisibleTargets(
            Transform origin,
            in Transform[] targets,
            in IOBBData[] obstacles,
            in float fovAngle,
            in float viewDistance
        )
        {
            // バッファを再利用するため、リストの長さを調整
            _visibleTargetsBuffer.Clear();

            // --------------------------------------------------
            // 判定ループ
            // --------------------------------------------------
            for (int i = 0; i < targets.Length; i++)
            {
                Transform target = targets[i];

                // 距離・角度判定
                if (!_fovMath.IsInFOV(origin, target, fovAngle, viewDistance))
                    continue;

                // 遮蔽物判定
                bool blocked = false;
                for (int j = 0; j < obstacles.Length; j++)
                {
                    IOBBData obstacle = obstacles[j];

                    // 対象物自身は判定除外
                    if (target.TryGetComponent<IOBBData>(out IOBBData targetOBB) && targetOBB == obstacle)
                        continue;

                    if (_losMath.IsLineIntersectOBB(origin.position, target.position, obstacle))
                    {
                        blocked = true;
                        break;
                    }
                }

                if (!blocked)
                {
                    // --------------------------------------------------
                    // 二分探索で距離順の挿入位置を決定
                    // --------------------------------------------------
                    int insertIndex = BinarySearchInsertIndex(origin.position, target);
                    _visibleTargetsBuffer.Insert(insertIndex, target);
                }
            }

            return _visibleTargetsBuffer;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 二分探索を使用して距離順の挿入位置を決定
        /// </summary>
        /// <param name="originPos">原点位置</param>
        /// <param name="target">挿入対象 Transform</param>
        /// <returns>挿入すべきインデックス</returns>
        private int BinarySearchInsertIndex(in Vector3 originPos, in Transform target)
        {
            float targetDistSqr = (target.position - originPos).sqrMagnitude;

            int low = 0;
            int high = _visibleTargetsBuffer.Count;

            // 二分探索ループ
            while (low < high)
            {
                int mid = (low + high) / 2;
                float midDistSqr = (_visibleTargetsBuffer[mid].position - originPos).sqrMagnitude;

                if (targetDistSqr < midDistSqr)
                    high = mid;
                else
                    low = mid + 1;
            }

            // low が挿入位置
            return low;
        }
    }
}