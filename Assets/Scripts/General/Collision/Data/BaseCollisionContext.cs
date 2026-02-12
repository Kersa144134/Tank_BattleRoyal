// ======================================================
// BaseCollisionContext.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : 衝突解決計算で使用する共通コンテキスト基底クラス
// ======================================================

using UnityEngine;
using CollisionSystem.Interface;

namespace CollisionSystem.Data
{
    /// <summary>
    /// 衝突判定および衝突解決処理で使用される共通コンテキスト
    /// OBB を持つ衝突対象の最小単位を定義する
    /// </summary>
    public abstract class BaseCollisionContext
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// フレーム中に発生した移動ロック軸の累積値
        /// 衝突計算中はここに OR で蓄積される
        /// </summary>
        private MovementLockAxis _pendingLockAxis;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>衝突判定に使用する OBB データ</summary>
        public IOBBData OBB
        {
            get;
            protected set;
        }

        /// <summary>衝突対象の Transform</summary>
        public Transform Transform
        {
            get;
            protected set;
        }

        /// <summary>
        /// このフレーム中に衝突解決が一度でも行われたか
        /// 外部からは読み取り専用
        /// </summary>
        public bool IsResolvedThisFrame
        {
            get;
            private set;
        }

        // ======================================================
        // 抽象プロパティ
        // ======================================================

        /// <summary>衝突判定の基準となる移動予定ワールド座標</summary>
        public abstract Vector3 NextPosition { get; }

        /// <summary>衝突判定の基準となる移動予定ワールド回転</summary>
        public abstract Quaternion NextRotation { get; }

        /// <summary>
        /// 現フレームで有効な移動ロック軸
        /// フレーム末の FinalizeLockAxis により確定される
        /// </summary>
        public abstract MovementLockAxis LockAxis
        {
            get;
            protected set;
        }

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// BaseCollisionContext を生成する
        /// </summary>
        /// <param name="transform">衝突対象の Transform</param>
        /// <param name="obb">衝突判定用 OBB</param>
        /// <param name="lockAxis">初期移動ロック軸</param>
        protected BaseCollisionContext(
            Transform transform,
            IOBBData obb,
            MovementLockAxis lockAxis
        )
        {
            Transform = transform;
            OBB = obb;
            LockAxis = lockAxis;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// OBB を最新の予定座標・回転に更新する
        /// </summary>
        public void UpdateOBB()
        {
            if (OBB is DynamicOBBData dynamicOBB)
            {
                dynamicOBB.Update(NextPosition, NextRotation);
            }
        }

        /// <summary>
        /// フレーム開始時に内部状態を初期化する
        /// </summary>
        public virtual void BeginFrame()
        {
            // フレーム中に累積されるロック軸を初期化する
            _pendingLockAxis = MovementLockAxis.None;

            // このフレームで衝突解決が行われたかを初期化する
            IsResolvedThisFrame = false;
        }

        /// <summary>
        /// フレーム中に発生した移動ロック軸を累積する
        /// </summary>
        /// <param name="axis">今回の衝突で発生したロック軸</param>
        public void AddPendingLockAxis(
            in MovementLockAxis axis
        )
        {
            // None の場合は衝突解決が発生していないため無視する
            if (axis == MovementLockAxis.None)
            {
                return;
            }

            // フレーム中のロック軸として OR で累積する
            _pendingLockAxis |= axis;

            // このフレームで衝突解決が行われたことを記録する
            IsResolvedThisFrame = true;
        }

        /// <summary>
        /// フレーム中に蓄積された移動ロック軸を確定させる
        /// 衝突がなかった場合は None が設定される
        /// </summary>
        public virtual void FinalizeLockAxis()
        {
            // X と Z の両方がロックされている場合
            if (_pendingLockAxis == (MovementLockAxis.X | MovementLockAxis.Z))
            {
                // All として扱う
                LockAxis = MovementLockAxis.All;
            }
            else
            {
                // フレーム中に発生したロック軸を反映する
                LockAxis = _pendingLockAxis;
            }

            // フレームフラグをリセットする
            IsResolvedThisFrame = false;
        }
    }
}