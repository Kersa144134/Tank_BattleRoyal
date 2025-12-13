// ======================================================
// OBBCollisionCalculator.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-13
// 更新日時 : 2025-12-13
// 概要     : OBB 同士の衝突判定および MTV 算出を統合する計算クラス
// ======================================================

using UnityEngine;
using CollisionSystem.Data;
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
        /// Y 軸回転のみを考慮した OBB 同士の衝突判定を行う
        /// </summary>
        public bool IsCollidingHorizontal(
            in OBBData a,
            in OBBData b
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

            // SAT 用の分離軸
            Vector3[] axes =
            {
                aForward,
                aRight,
                bForward,
                bRight
            };

            // 各軸で重なりを確認
            for (int i = 0; i < axes.Length; i++)
            {
                // 一つでも分離軸が存在すれば非衝突
                if (!_satMath.IsOverlappingOnAxis(a, b, axes[i]))
                {
                    return false;
                }
            }

            // 全軸で重なっていれば衝突
            return true;
        }

        /// <summary>
        /// Y 軸回転のみを考慮した MTV（最小移動量）を算出する
        /// </summary>
        public bool TryCalculateHorizontalMTV(
            in OBBData a,
            in OBBData b,
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

            // MTV 評価対象となる分離軸
            Vector3[] axes =
            {
                aForward,
                aRight,
                bForward,
                bRight
            };

            // 各軸で侵入量を評価
            for (int i = 0; i < axes.Length; i++)
            {
                // 判定軸を正規化
                Vector3 testAxis = axes[i].normalized;

                // 侵入していない軸があれば衝突していない
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
                if (currentOverlap < overlap)
                {
                    overlap = currentOverlap;
                    axis = testAxis;
                }
            }

            // 有効な MTV が算出できたか判定
            return axis != Vector3.zero;
        }
    }
}