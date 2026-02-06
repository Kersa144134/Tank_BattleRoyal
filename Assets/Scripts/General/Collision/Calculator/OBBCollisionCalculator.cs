// ======================================================
// OBBCollisionCalculator.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-13
// 更新日時 : 2026-01-30
// 概要     : OBB 同士の衝突判定および侵入量算出を統合管理する計算クラス
// ======================================================

using UnityEngine;
using CollisionSystem.Interface;
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

        /// <summary>OBB の軸情報および射影計算を担当するユーティリティ</summary>
        private readonly OBBMath _obbMath;

        /// <summary>分離判定（重なり有無）を担当する数学ユーティリティ</summary>
        private readonly SeparationMath _separationMath;

        /// <summary>侵入量（押し戻し量）算出を担当する数学ユーティリティ</summary>
        private readonly PenetrationMath _penetrationMath;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 必要な数学コンポーネントを注入して初期化する
        /// </summary>
        public OBBCollisionCalculator(
            in OBBMath obbMath,
            in SeparationMath separationMath,
            in PenetrationMath penetrationMath
        )
        {
            _obbMath = obbMath;
            _separationMath = separationMath;
            _penetrationMath = penetrationMath;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// Y 軸回転のみを考慮し、OBB 同士が水平面上で重なっているかを判定する
        /// </summary>
        public bool IsCollidingHorizontal(
            in IOBBData a,
            in IOBBData b
        )
        {
            // OBB A の水平軸を取得
            _obbMath.GetAxes(
                a,
                out Vector3 aRight,
                out _,
                out Vector3 aForward
            );

            // OBB B の水平軸を取得
            _obbMath.GetAxes(
                b,
                out Vector3 bRight,
                out _,
                out Vector3 bForward
            );

            // A 前方向軸で分離していないか判定
            if (!_separationMath.IsOverlappingOnAxis(a, b, aForward))
            {
                return false;
            }

            // A 右方向軸で分離していないか判定
            if (!_separationMath.IsOverlappingOnAxis(a, b, aRight))
            {
                return false;
            }

            // B 前方向軸で分離していないか判定
            if (!_separationMath.IsOverlappingOnAxis(a, b, bForward))
            {
                return false;
            }

            // B 右方向軸で分離していないか判定
            if (!_separationMath.IsOverlappingOnAxis(a, b, bRight))
            {
                return false;
            }

            // すべての軸で分離していなければ重なっている
            return true;
        }

        /// <summary>
        /// OBB 同士が重なった場合の水平押し戻し軸と距離を算出する
        /// </summary>
        public bool TryGetPushOutAxisAndDistance(
            in IOBBData a,
            in IOBBData b,
            out Vector3 axis,
            out float overlap
        )
        {
            // 出力値を初期化
            axis = Vector3.zero;
            overlap = float.MaxValue;

            // OBB A の水平軸を取得
            _obbMath.GetAxes(
                a,
                out Vector3 aRight,
                out _,
                out Vector3 aForward
            );

            // OBB B の水平軸を取得
            _obbMath.GetAxes(
                b,
                out Vector3 bRight,
                out _,
                out Vector3 bForward
            );

            // A 前方向軸で侵入量を評価
            if (!TryEvaluateAxis(a, b, aForward, ref axis, ref overlap))
            {
                return false;
            }

            // A 右方向軸で侵入量を評価
            if (!TryEvaluateAxis(a, b, aRight, ref axis, ref overlap))
            {
                return false;
            }

            // B 前方向軸で侵入量を評価
            if (!TryEvaluateAxis(a, b, bForward, ref axis, ref overlap))
            {
                return false;
            }

            // B 右方向軸で侵入量を評価
            if (!TryEvaluateAxis(a, b, bRight, ref axis, ref overlap))
            {
                return false;
            }

            // 有効な解決軸が算出されていれば成功
            return axis != Vector3.zero;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 指定軸における侵入量を評価し、最小値を更新する
        /// </summary>
        private bool TryEvaluateAxis(
            in IOBBData a,
            in IOBBData b,
            in Vector3 rawAxis,
            ref Vector3 bestAxis,
            ref float bestOverlap
        )
        {
            // 判定軸を正規化
            Vector3 testAxis = rawAxis.normalized;

            // 指定軸で侵入が発生していない場合は失敗
            if (!_penetrationMath.TryCalculatePenetration(
                    a,
                    b,
                    testAxis,
                    out float currentPenetration
                ))
            {
                return false;
            }

            // より小さい侵入量であれば更新
            if (currentPenetration < bestOverlap)
            {
                bestOverlap = currentPenetration;
                bestAxis = testAxis;
            }

            return true;
        }
    }
}