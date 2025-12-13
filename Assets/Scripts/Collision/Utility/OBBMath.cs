// ======================================================
// OBBMath.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-13
// 更新日時 : 2025-12-13
// 概要     : OBB に対する純粋な幾何・数学計算のみを担当するユーティリティクラス
// ======================================================

using UnityEngine;
using CollisionSystem.Data;

namespace CollisionSystem.Utility
{
    /// <summary>
    /// OBB に関する純数学的な計算を提供するクラス
    /// </summary>
    public class OBBMath
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// OBB のローカル 3 軸をワールド空間ベクトルとして取得する
        /// </summary>
        /// <param name="obb">対象となる OBB データ</param>
        /// <param name="right">ワールド空間での右方向軸</param>
        /// <param name="up">ワールド空間での上方向軸</param>
        /// <param name="forward">ワールド空間での前方向軸</param>
        public void GetAxes(
            in OBBData obb,
            out Vector3 right,
            out Vector3 up,
            out Vector3 forward
        )
        {
            right = obb.Rotation * Vector3.right;
            up = obb.Rotation * Vector3.up;
            forward = obb.Rotation * Vector3.forward;
        }

        /// <summary>
        /// 指定した分離軸に対して OBB を射影した際の投影半径を算出する
        /// </summary>
        /// <param name="obb">対象となる OBB データ</param>
        /// <param name="axis">正規化済みの判定軸</param>
        /// <returns>指定軸方向への投影半径</returns>
        public float CalculateProjectionRadius(
            in OBBData obb,
            in Vector3 axis
        )
        {
            Vector3 right =
                obb.Rotation *
                Vector3.right *
                obb.HalfSize.x;

            Vector3 up =
                obb.Rotation *
                Vector3.up *
                obb.HalfSize.y;

            Vector3 forward =
                obb.Rotation *
                Vector3.forward *
                obb.HalfSize.z;

            // 各軸成分を判定軸へ射影し、絶対値を合算
            return
                Mathf.Abs(Vector3.Dot(right, axis)) +
                Mathf.Abs(Vector3.Dot(up, axis)) +
                Mathf.Abs(Vector3.Dot(forward, axis));
        }
    }
}