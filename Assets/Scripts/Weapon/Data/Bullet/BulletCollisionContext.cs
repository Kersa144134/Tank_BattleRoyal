// ======================================================
// BulletCollisionContext.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-21
// 更新日時 : 2025-12-21
// 概要     : 弾丸衝突判定に必要な情報を一括管理するコンテキスト
// ======================================================

using UnityEngine;
using CollisionSystem.Data;
using CollisionSystem.Interface;

namespace WeaponSystem.Data
{
    /// <summary>
    /// 弾丸 1 発分の衝突判定コンテキスト
    /// 
    /// 戦車と異なり、移動ロック軸は持たず、
    /// 毎フレームの移動予定位置を基準に衝突判定を行う
    /// </summary>
    public sealed class BulletCollisionContext
        : BaseCollisionContext,
        IDynamicCollisionContext
    {
        // ======================================================
        // 固有プロパティ
        // ======================================================

        /// <summary>
        /// このコンテキストが管理する弾丸ロジック本体
        /// </summary>
        public BulletBase Bullet { get; private set; }

        // ======================================================
        // 抽象プロパティ
        // ======================================================

        /// <summary>
        /// 弾丸の次フレーム移動予定ワールド座標
        /// </summary>
        public override Vector3 NextPosition
        {
            get
            {
                return Bullet.NextPosition;
            }
        }

        /// <summary>
        /// 弾丸の次フレーム回転
        /// </summary>
        public override Quaternion NextRotation
        {
            get
            {
                return Bullet.NextRotation;
            }
        }

        /// <summary>
        /// 弾丸は移動ロックを行わないため常に None
        /// </summary>
        public override MovementLockAxis LockAxis
        {
            get
            {
                return MovementLockAxis.None;
            }
            protected set { }
        }
        
        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// BulletCollisionContext を生成する
        /// </summary>
        /// <param name="bullet">衝突判定対象となる弾丸ロジック</param>
        /// <param name="obb">弾丸に対応する OBB データ</param>
        public BulletCollisionContext(
            in BulletBase bullet,
            in IOBBData obb
        )
            : base(
                bullet.Transform,
                obb,
                MovementLockAxis.None,
                null
            )
        {
            // 弾丸参照を保持
            Bullet = bullet;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 弾丸用フレーム初期化処理
        /// </summary>
        public override void BeginFrame()
        {
            // Base 側の初期化のみ実行
            base.BeginFrame();
        }

        /// <summary>
        /// 弾丸用のフレーム確定処理
        /// </summary>
        public override void FinalizeLockAxis()
        {
            // 弾丸ではロック軸を確定する処理は存在しないため何もしない
        }
    }
}