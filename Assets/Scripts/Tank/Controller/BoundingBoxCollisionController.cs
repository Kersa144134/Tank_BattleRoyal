// ======================================================
// BoundingBoxCollisionController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-11
// 更新日時 : 2025-12-13
// 概要     : OBB 間の衝突判定および MTV（最小移動量）の算出を担当する
// ======================================================

using UnityEngine;
using TankSystem.Data;

namespace TankSystem.Controller
{
    /// <summary>
    /// OBB の距離計算および衝突判定を行うクラス
    /// </summary>
    public class BoundingBoxCollisionController
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 2つの OBB が衝突しているか判定する
        /// </summary>
        public bool IsColliding(in OBBData a, in OBBData b)
        {
            // 判定用の分離軸を生成
            Vector3[] axes = CreateFullTestAxes(a, b, out int axisCount);

            // 全軸で重なりを確認
            for (int i = 0; i < axisCount; i++)
            {
                // 一つでも分離軸があれば非衝突
                if (!IsOverlappingOnAxis(a, b, axes[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 2 つの OBB が侵入している前提で、
        /// 分離に必要な最小移動量（MTV）を算出する
        /// </summary>
        public bool TryCalculateMTV(
            in OBBData a,
            in OBBData b,
            out Vector3 minAxis,
            out float minOverlap
        )
        {
            // 初期化
            minAxis = Vector3.zero;
            minOverlap = float.MaxValue;

            // --------------------------------------------------
            // SAT 用の分離軸を取得（Y 回転のみ）
            // --------------------------------------------------

            Vector3[] axes = CreateHorizontalSATAxes(a, b);

            // --------------------------------------------------
            // 中心差分ベクトルを算出
            // --------------------------------------------------

            Vector3 centerDelta = CalculateCenterDelta(a, b);

            // --------------------------------------------------
            // 各軸で侵入量を評価
            // --------------------------------------------------

            for (int i = 0; i < axes.Length; i++)
            {
                // 軸を正規化
                Vector3 axis = axes[i].normalized;

                // 侵入量を算出
                if (!TryCalculateOverlapOnAxis(
                        a,
                        b,
                        centerDelta,
                        axis,
                        out float overlap
                    ))
                {
                    return false;
                }

                // 最小侵入量を更新
                if (overlap < minOverlap)
                {
                    minOverlap = overlap;
                    minAxis = axis;
                }
            }

            // 有効な MTV が得られたか確認
            return minAxis != Vector3.zero;
        }

        // ======================================================
        // SAT 構築関連
        // ======================================================

        /// <summary>
        /// Y 回転のみを考慮した SAT 用分離軸を生成する
        /// </summary>
        private Vector3[] CreateHorizontalSATAxes(in OBBData a, in OBBData b)
        {
            // OBB A のローカル軸
            Vector3 aForward = a.Rotation * Vector3.forward;
            Vector3 aRight = a.Rotation * Vector3.right;

            // OBB B のローカル軸
            Vector3 bForward = b.Rotation * Vector3.forward;
            Vector3 bRight = b.Rotation * Vector3.right;

            return new Vector3[]
            {
                aForward,
                aRight,
                bForward,
                bRight
            };
        }

        /// <summary>
        /// OBB 中心間の差分ベクトルを算出する
        /// </summary>
        private Vector3 CalculateCenterDelta(in OBBData a, in OBBData b)
        {
            // 中心差分を算出
            Vector3 delta = a.Center - b.Center;

            // Y 方向は判定対象外
            delta.y = 0f;

            return delta;
        }

        // ======================================================
        // 侵入量計算
        // ======================================================

        /// <summary>
        /// 指定軸上での侵入量を算出する
        /// </summary>
        private bool TryCalculateOverlapOnAxis(
            in OBBData a,
            in OBBData b,
            in Vector3 centerDelta,
            in Vector3 axis,
            out float overlap
        )
        {
            // 中心間距離を軸に射影
            float distance =
                Mathf.Abs(Vector3.Dot(centerDelta, axis));

            // OBB A の射影半径
            float projectionA =
                CalculateProjectionRadius(a, axis);

            // OBB B の射影半径
            float projectionB =
                CalculateProjectionRadius(b, axis);

            // 侵入量を算出
            overlap = projectionA + projectionB - distance;

            // 分離していれば非侵入
            return overlap > 0f;
        }

        /// <summary>
        /// OBB を指定軸に射影した半径を算出する
        /// </summary>
        private float CalculateProjectionRadius(
            in OBBData obb,
            in Vector3 axis
        )
        {
            // 各ローカル軸をワールド空間に変換
            Vector3 right =
                obb.Rotation * Vector3.right * obb.HalfSize.x;

            Vector3 up =
                obb.Rotation * Vector3.up * obb.HalfSize.y;

            Vector3 forward =
                obb.Rotation * Vector3.forward * obb.HalfSize.z;

            // 各成分を軸に射影して合算
            return
                Mathf.Abs(Vector3.Dot(right, axis)) +
                Mathf.Abs(Vector3.Dot(up, axis)) +
                Mathf.Abs(Vector3.Dot(forward, axis));
        }

        // ======================================================
        // IsColliding 用（完全 SAT）
        // ======================================================

        /// <summary>
        /// 完全 SAT 用の分離軸を生成する
        /// </summary>
        private Vector3[] CreateFullTestAxes(
            in OBBData a,
            in OBBData b,
            out int axisCount
        )
        {
            Vector3[] axesA =
            {
                a.Rotation * Vector3.right,
                a.Rotation * Vector3.up,
                a.Rotation * Vector3.forward
            };

            Vector3[] axesB =
            {
                b.Rotation * Vector3.right,
                b.Rotation * Vector3.up,
                b.Rotation * Vector3.forward
            };

            Vector3[] axes = new Vector3[15];
            axisCount = 0;

            // 各 OBB の軸
            for (int i = 0; i < 3; i++)
            {
                axes[axisCount++] = axesA[i];
                axes[axisCount++] = axesB[i];
            }

            // 外積軸
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Vector3 cross = Vector3.Cross(axesA[i], axesB[j]);

                    if (cross.sqrMagnitude > 1e-6f)
                    {
                        axes[axisCount++] = cross.normalized;
                    }
                }
            }

            return axes;
        }

        /// <summary>
        /// 指定軸上で OBB 同士が重なっているか判定する
        /// </summary>
        private bool IsOverlappingOnAxis(
            in OBBData a,
            in OBBData b,
            in Vector3 axis
        )
        {
            float projectionA = CalculateProjectionRadius(a, axis);
            float projectionB = CalculateProjectionRadius(b, axis);

            float distance =
                Mathf.Abs(Vector3.Dot(b.Center - a.Center, axis));

            return distance <= projectionA + projectionB;
        }
    }
}