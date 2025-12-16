// ======================================================
// MTVMath.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-13
// 更新日時 : 2025-12-13
// 概要     : OBB 同士の MTV（最小押し出し量）を算出するユーティリティクラス
// ======================================================

using UnityEngine;
using CollisionSystem.Interface;

namespace CollisionSystem.Utility
{
    /// <summary>
    /// SAT 前提で、指定軸上の侵入量（MTV 成分）を計算するクラス
    /// </summary>
    public class MTVMath
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>OBB の射影計算を担当する数学ユーティリティ</summary>
        private readonly OBBMath _obbMath;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 必要なコンポーネントを注入して生成する
        /// </summary>
        /// <param name="obbMath">OBB の射影計算を担当する数学ユーティリティ</param>
        public MTVMath(in OBBMath obbMath)
        {
            _obbMath = obbMath;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定された分離軸上での侵入量を算出する
        /// </summary>
        /// <param name="a">OBB A</param>
        /// <param name="b">OBB B</param>
        /// <param name="axis">正規化済みの分離軸</param>
        /// <param name="overlap">算出された侵入量</param>
        /// <returns>侵入していれば true、分離していれば false</returns>
        public bool TryCalculateOverlap(
            in IOBBData a,
            in IOBBData b,
            in Vector3 axis,
            out float overlap
        )
        {
            // 中心差分ベクトルを算出
            Vector3 centerDelta = a.Center - b.Center;

            // 軸方向の中心間距離を算出
            float distance = Mathf.Abs(Vector3.Dot(centerDelta, axis));

            // 各 OBB の射影半径を算出
            float projectionA = _obbMath.CalculateProjectionRadius(a, axis);
            float projectionB = _obbMath.CalculateProjectionRadius(b, axis);

            // 侵入量を算出
            overlap = projectionA + projectionB - distance;

            // 侵入判定（正の値で侵入）
            return overlap > 0f;
        }
    }
}