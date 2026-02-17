// ======================================================
// OBBCollisionCalculator.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-13
// 更新日時 : 2026-01-30
// 概要     : OBB 同士の衝突判定および侵入量算出を統合管理する計算クラス
// ======================================================

using UnityEngine;
using CollisionSystem.Data;
using CollisionSystem.Utility;

namespace CollisionSystem.Calculator
{
    /// <summary>
    /// OBB 衝突判定および解決量算出の制御を行う計算クラス
    /// </summary>
    public sealed class OBBCollisionCalculator
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>汎用的な重なり量計算を担当する数学ユーティリティ</summary>
        private readonly OverlapMath _overlapMath;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 必要な数学ユーティリティを注入して初期化する
        /// </summary>
        public OBBCollisionCalculator(in OverlapMath overlapMath)
        {
            _overlapMath = overlapMath;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// Y 軸回転のみを考慮し、OBB 同士が水平面上で重なっているかを判定する
        /// </summary>
        public bool IsCollidingHorizontal(
            in BaseOBBData a,
            in BaseOBBData b
        )
        {
            // A の水平軸を取得
            Vector3 aRight = a.AxisRight;
            Vector3 aForward = a.AxisForward;

            // B の水平軸を取得
            Vector3 bRight = b.AxisRight;
            Vector3 bForward = b.AxisForward;

            // A 前方向軸で評価
            if (!IsAxisOverlapping(a, b, aForward))
            {
                return false;
            }

            // A 右方向軸で評価
            if (!IsAxisOverlapping(a, b, aRight))
            {
                return false;
            }

            // B 前方向軸で評価
            if (!IsAxisOverlapping(a, b, bForward))
            {
                return false;
            }

            // B 右方向軸で評価
            if (!IsAxisOverlapping(a, b, bRight))
            {
                return false;
            }

            // 全軸で分離していなければ衝突
            return true;
        }

        /// <summary>
        /// OBB 同士が重なった場合の水平押し戻し軸と距離を算出する
        /// </summary>
        public bool TryGetPushOutAxisAndDistance(
            in BaseOBBData a,
            in BaseOBBData b,
            out Vector3 axis,
            out float overlap
        )
        {
            // 解決軸
            axis = Vector3.zero;
            // 最小侵入量
            overlap = float.MaxValue;

            // A の水平軸を取得
            Vector3 aRight = a.AxisRight;
            Vector3 aForward = a.AxisForward;

            // B の水平軸を取得
            Vector3 bRight = b.AxisRight;
            Vector3 bForward = b.AxisForward;

            // A 前方向軸で評価
            if (!TryUpdateMinimumOverlap(a, b, aForward, ref axis, ref overlap))
            {
                return false;
            }

            // A 右方向軸で評価
            if (!TryUpdateMinimumOverlap(a, b, aRight, ref axis, ref overlap))
            {
                return false;
            }

            // B 前方向軸で評価
            if (!TryUpdateMinimumOverlap(a, b, bForward, ref axis, ref overlap))
            {
                return false;
            }

            // B 右方向軸で評価
            if (!TryUpdateMinimumOverlap(a, b, bRight, ref axis, ref overlap))
            {
                return false;
            }

            // 解決軸があれば返却
            return axis != Vector3.zero;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 指定軸で重なりが存在するかを判定する
        /// </summary>
        private bool IsAxisOverlapping(
            in BaseOBBData a,
            in BaseOBBData b,
            in Vector3 axis
        )
        {
            // 指定軸上の重なり量を算出
            float overlap =
                _overlapMath.CalculateOBBOverlapOnAxis(a, b, axis);

            // 重なりが正であれば衝突
            return overlap > 0f;
        }

        /// <summary>
        /// 指定軸の侵入量を評価し、最小値を更新する
        /// </summary>
        private bool TryUpdateMinimumOverlap(
            in BaseOBBData a,
            in BaseOBBData b,
            in Vector3 axis,
            ref Vector3 bestAxis,
            ref float bestOverlap
        )
        {
            // 指定軸上の侵入量を算出
            float currentOverlap =
                _overlapMath.CalculateOBBOverlapOnAxis(a, b, axis);

            // 分離している場合は衝突なし
            if (currentOverlap <= 0f)
            {
                return false;
            }

            // より小さい侵入量であれば更新
            if (currentOverlap < bestOverlap)
            {
                bestOverlap = currentOverlap;
                bestAxis = axis;
            }

            return true;
        }
    }
}