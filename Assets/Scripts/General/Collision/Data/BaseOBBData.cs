// ======================================================
// BaseOBBData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-16
// 更新日時 : 2026-02-17
// 概要     : OBB データ共通基底クラス
// ======================================================

using CollisionSystem.Interface;
using UnityEngine;

namespace CollisionSystem.Data
{
    /// <summary>
    /// OBB データの共通基底クラス
    /// </summary>
    public abstract class BaseOBBData : IOBBData
    {
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>ワールド空間における中心座標</summary>
        public Vector3 Center
        {
            get;
            protected set;
        }

        /// <summary>ローカル基準での半サイズ</summary>
        public Vector3 HalfSize
        {
            get;
            protected set;
        }

        /// <summary>ワールド空間における回転</summary>
        public Quaternion Rotation
        {
            get;
            protected set;
        }

        // --------------------------------------------------
        // ワールド軸キャッシュ
        // --------------------------------------------------
        /// <summary>ワールド右方向軸</summary>
        public Vector3 AxisRight
        {
            get;
            protected set;
        }

        /// <summary>ワールド上方向軸</summary>
        public Vector3 AxisUp
        {
            get;
            protected set;
        }

        /// <summary>ワールド前方向軸</summary>
        public Vector3 AxisForward
        {
            get;
            protected set;
        }

        // --------------------------------------------------
        // 射影計算用分離軸キャッシュ
        // --------------------------------------------------
        /// <summary>半サイズ適用後の右方向軸</summary>
        public Vector3 ScaledRight
        {
            get;
            protected set;
        }

        /// <summary>半サイズ適用後の上方向軸</summary>
        public Vector3 ScaledUp
        {
            get;
            protected set;
        }

        /// <summary>半サイズ適用後の前方向軸</summary>
        public Vector3 ScaledForward
        {
            get;
            protected set;
        }

        /// <summary>BroadPhase 用近似外接半径</summary>
        public float BoundingRadius
        {
            get;
            protected set;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ワールド Transform を同期する
        /// </summary>
        public virtual void SyncTransform(
            in Vector3 worldPosition,
            in Quaternion worldRotation
        )
        {
            // ワールド値をそのまま中心として設定する
            SetTransform(worldPosition, worldRotation);
        }

        /// <summary>
        /// 中心および回転を設定しキャッシュを更新する
        /// </summary>
        protected void SetTransform(
            in Vector3 center,
            in Quaternion rotation
        )
        {
            // ワールド中心を更新する
            Center = center;

            // ワールド回転を更新する
            Rotation = rotation;

            // 軸および射影キャッシュを再計算する
            UpdateCache();
        }

        // ======================================================
        // プロテクトメソッド
        // ======================================================

        /// <summary>
        /// 軸および射影用キャッシュを更新する
        /// </summary>
        protected void UpdateCache()
        {
            // 回転を適用したワールド右軸を算出する
            AxisRight = Rotation * Vector3.right;

            // 回転を適用したワールド上軸を算出する
            AxisUp = Rotation * Vector3.up;

            // 回転を適用したワールド前軸を算出する
            AxisForward = Rotation * Vector3.forward;

            // X 半サイズを適用した右軸を算出する
            ScaledRight = AxisRight * HalfSize.x;

            // Y 半サイズを適用した上軸を算出する
            ScaledUp = AxisUp * HalfSize.y;

            // Z 半サイズを適用した前軸を算出する
            ScaledForward = AxisForward * HalfSize.z;

            // XZ 平面上で最大の半サイズを取得する
            float maxHalfSize = Mathf.Max(HalfSize.x, HalfSize.z);

            // BroadPhase 用半径として保持する
            BoundingRadius = maxHalfSize;
        }
    }
}