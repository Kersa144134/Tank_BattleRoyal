// ======================================================
// FOVMath.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-22
// 更新日時 : 2025-12-22
// 概要     : 距離・角度判定のユーティリティクラス
// ======================================================

using UnityEngine;

namespace VisionSystem.Utility
{
    /// <summary>
    /// 距離・角度計算に基づく視界判定用ユーティリティ
    /// </summary>
    public sealed class FOVMath
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定対象が視界内にあるかを判定
        /// </summary>
        /// <param name="origin">視界中心 Transform</param>
        /// <param name="target">判定対象 Transform</param>
        /// <param name="fovAngle">視野角（全角）</param>
        /// <param name="viewDistance">視界距離</param>
        /// <returns>視界内であれば true</returns>
        public bool IsInFOV(
            in Transform origin,
            in Transform target,
            in float fovAngle,
            in float viewDistance)
        {
            // 視線ベクトルを算出
            Vector3 toTarget = target.position - origin.position;

            // 距離チェック
            if (toTarget.sqrMagnitude > viewDistance * viewDistance)
            {
                return false;
            }

            // 角度チェック
            float halfFOV = fovAngle * 0.5f;
            float angle = Vector3.Angle(origin.forward, toTarget);
            if (angle > halfFOV)
            {
                return false;
            }

            return true;
        }
    }
}