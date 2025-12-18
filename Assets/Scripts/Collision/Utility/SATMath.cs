// ======================================================
// SATMath.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-13
// 更新日時 : 2025-12-13
// 概要     : SAT（Separating Axis Theorem）による OBB 同士の分離判定を担当するユーティリティクラス
// ======================================================

using UnityEngine;
using CollisionSystem.Interface;

namespace CollisionSystem.Utility
{
    /// <summary>
    /// SAT に基づいた OBB の分離判定ロジックを提供
    /// </summary>
    public sealed class SATMath
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
        public SATMath(in OBBMath obbMath)
        {
            _obbMath = obbMath;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定された軸において、2 つの OBB が重なっているか判定する
        /// </summary>
        /// <param name="a">OBB A</param>
        /// <param name="b">OBB B</param>
        /// <param name="axis">分離判定に使用する軸（正規化済み前提）</param>
        /// <returns>投影区間が重なっていれば true、分離していれば false</returns>
        public bool IsOverlappingOnAxis(
            in IOBBData a,
            in IOBBData b,
            in Vector3 axis
        )
        {
            // 各 OBB の指定軸への投影半径を計算
            float projectionA = _obbMath.CalculateProjectionRadius(a, axis);
            float projectionB = _obbMath.CalculateProjectionRadius(b, axis);

            // OBB 中心間距離を軸方向へ射影
            float distance = Mathf.Abs(Vector3.Dot(b.Center - a.Center, axis));

            // 投影区間が重なっているか判定
            return distance <= projectionA + projectionB;
        }
    }
}