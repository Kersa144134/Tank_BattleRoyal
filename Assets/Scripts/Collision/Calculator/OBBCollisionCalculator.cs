// ======================================================
// OBBCollisionCalculator.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-13
// 更新日時 : 2025-12-15
// 概要     : OBB 同士の衝突判定および MTV 算出を統合する計算クラス
// ======================================================

using UnityEngine;
using CollisionSystem.Data;
using CollisionSystem.Interface;
using CollisionSystem.Utility;

namespace CollisionSystem.Calculator
{
    /// <summary>
    /// OBB 衝突計算のファサードクラス
    /// </summary>
    public class OBBCollisionCalculator
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>OBB の射影計算を担当する数学ユーティリティ</summary>
        private readonly OBBMath _obbMath;

        /// <summary>SAT による重なり判定を担当するユーティリティ</summary>
        private readonly SATMath _satMath;

        /// <summary>MTV（侵入量）算出を担当するユーティリティ</summary>
        private readonly MTVMath _mtvMath;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 必要なコンポーネントを注入して生成する
        /// </summary>
        /// <param name="obbMath">OBB の射影計算を担当する数学ユーティリティ</param>
        /// <param name="satMath">SAT による重なり判定を担当するユーティリティ</param>
        /// <param name="mtvMath">MTV（侵入量）算出を担当するユーティリティ</param>
        public OBBCollisionCalculator(in OBBMath obbMath, in SATMath satMath, in MTVMath mtvMath)
        {
            _obbMath = obbMath;
            _satMath = satMath;
            _mtvMath = mtvMath;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// Y 軸回転のみを考慮し、
        /// OBB 同士が水平面上で衝突しているかを SAT により判定する
        /// </summary>
        /// <param name="a">衝突判定を行う対象 OBB A</param>
        /// <param name="b">衝突判定を行う対象 OBB B</param>
        /// <returns>すべての分離軸で重なりが確認された場合は true、確認されなかった場合は false</returns>
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

            // OBB A の前方向で分離していないか判定
            if (!_satMath.IsOverlappingOnAxis(a, b, aForward))
            {
                return false;
            }

            // OBB A の右方向で分離していないか判定
            if (!_satMath.IsOverlappingOnAxis(a, b, aRight))
            {
                return false;
            }

            // OBB B の前方向で分離していないか判定
            if (!_satMath.IsOverlappingOnAxis(a, b, bForward))
            {
                return false;
            }

            // OBB B 右方向で分離していないか判定
            if (!_satMath.IsOverlappingOnAxis(a, b, bRight))
            {
                return false;
            }

            // すべての軸で分離していなければ衝突
            return true;
        }

        /// <summary>
        /// Y 軸回転のみを考慮し、
        /// OBB 同士の衝突時に必要となる最小移動量（MTV）を算出する
        /// </summary>
        /// <param name="a">MTV 算出を行う対象 OBB A</param>
        /// <param name="b">MTV 算出を行う対象 OBB B</param>
        /// <param name="axis">算出された最小侵入量に対応する分離軸</param>
        /// <param name="overlap">算出された最小侵入量</param>
        /// <returns>有効な MTV が算出できた場合は true、衝突していないなど算出できなかった場合は false</returns>
        public bool TryCalculateHorizontalMTV(
            in IOBBData a,
            in IOBBData b,
            out Vector3 axis,
            out float overlap
        )
        {
            // 出力初期化
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

            // OBB A の前方向の軸で侵入量を評価
            if (!TryEvaluateAxis(a, b, aForward, ref axis, ref overlap))
            {
                return false;
            }

            // OBB A の右方向の軸で侵入量を評価
            if (!TryEvaluateAxis(a, b, aRight, ref axis, ref overlap))
            {
                return false;
            }

            // OBB B の前方向の軸で侵入量を評価
            if (!TryEvaluateAxis(a, b, bForward, ref axis, ref overlap))
            {
                return false;
            }

            // OBB B の右方向の軸で侵入量を評価
            if (!TryEvaluateAxis(a, b, bRight, ref axis, ref overlap))
            {
                return false;
            }

            // 有効な MTV が算出できていれば true
            return axis != Vector3.zero;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 指定した分離軸候補における侵入量を評価し、
        /// 現在保持している最小侵入量（MTV）と比較して必要に応じて更新する
        /// </summary>
        /// <param name="a">侵入判定を行う対象 OBB A</param>
        /// <param name="b">侵入判定を行う対象 OBB B</param>
        /// <param name="rawAxis">正規化前の侵入判定用分離軸候補</param>
        /// <param name="bestAxis">現在の最小侵入量に対応する分離軸</param>
        /// <param name="bestOverlap">現在の最小侵入量</param>
        /// <returns>指定軸で侵入が存在する場合は true、存在しない場合は false</returns>
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

            // 指定軸で侵入していない場合は非衝突
            if (!_mtvMath.TryCalculateOverlap(
                    a,
                    b,
                    testAxis,
                    out float currentOverlap
                ))
            {
                return false;
            }

            // 最小侵入量を更新
            if (currentOverlap < bestOverlap)
            {
                bestOverlap = currentOverlap;
                bestAxis = testAxis;
            }

            return true;
        }
    }
}