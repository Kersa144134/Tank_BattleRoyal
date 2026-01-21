// ======================================================
// CollisionEventRouter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-21
// 概要     : 衝突後の処理を委譲するイベントルーター
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using CollisionSystem.Calculator;
using CollisionSystem.Data;
using ItemSystem.Data;
using ObstacleSystem.Data;
using TankSystem.Data;
using TankSystem.Manager;
using WeaponSystem.Data;

namespace CollisionSystem.Utility
{
    /// <summary>
    /// 衝突イベントのハンドリングを一元管理するクラス
    /// 衝突判定後の処理（押し戻し・LockAxis・アイテム取得・弾丸衝突通知）を委譲
    /// </summary>
    public sealed class CollisionEventRouter
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>MTV を用いた衝突解決量を計算するクラス</summary>
        private CollisionResolveCalculator _collisionResolverCalculator;

        /// <summary>弾丸と対象の衝突履歴</summary>
        private readonly HashSet<(int bulletId, int targetId)> _hitHistory = new HashSet<(int, int)>();

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>戦車がアイテムを取得した際に通知されるイベント</summary>
        public event Action<BaseTankRootManager, ItemSlot> OnItemGet;

        /// <summary>弾丸が衝突した際に通知されるイベント</summary>
        public event Action<BulletBase, BaseCollisionContext> OnBulletHit;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public CollisionEventRouter(in CollisionResolveCalculator collisionResolverCalculator)
        {
            _collisionResolverCalculator = collisionResolverCalculator;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // 戦車
        // --------------------------------------------------
        /// <summary>
        /// 戦車が障害物に衝突した際の処理
        /// 押し戻し計算と LockAxis 反映を行う
        /// </summary>
        /// <param name="context">衝突した戦車のコンテキスト</param>
        /// <param name="obstacle">衝突した障害物のコンテキスト</param>
        public void HandleTankHitObstacle(
            TankCollisionContext context,
            ObstacleCollisionContext obstacle
        )
        {
            // --------------------------------------------------
            // 衝突解決計算
            // --------------------------------------------------
            _collisionResolverCalculator.CalculateResolveInfo(
                context,
                obstacle,
                context.TankRootManager.CurrentForwardSpeed,
                0f,
                out CollisionResolveInfo resolveInfoA,
                out CollisionResolveInfo resolveInfoB
            );

            // --------------------------------------------------
            // LockAxis 計算
            // --------------------------------------------------
            MovementLockAxis newLockAxis = MovementLockAxis.None;

            // X 方向に押し戻しが発生している場合
            if (Mathf.Abs(resolveInfoA.ResolveVector.x) > 0f)
            {
                newLockAxis |= MovementLockAxis.X;
            }

            // Z 方向に押し戻しが発生している場合
            if (Mathf.Abs(resolveInfoA.ResolveVector.z) > 0f)
            {
                newLockAxis |= MovementLockAxis.Z;
            }

            if (newLockAxis != MovementLockAxis.None)
            {
                MovementLockAxis resolvedAxis =
                    newLockAxis == (MovementLockAxis.X | MovementLockAxis.Z)
                        ? MovementLockAxis.All
                        : newLockAxis;

                // ロック軸を戦車コンテキストに累積
                context.AddPendingLockAxis(resolvedAxis);
            }

            // --------------------------------------------------
            // 押し戻し反映
            // --------------------------------------------------
            context.TankRootManager.ApplyCollisionResolve(resolveInfoA);

            // OBB を押し戻し位置に更新
            context.UpdateOBB();
        }

        /// <summary>
        /// 戦車同士が衝突した際の処理
        /// 押し戻し計算を行い OBB を更新
        /// </summary>
        /// <param name="contextA">戦車Aのコンテキスト</param>
        /// <param name="contextB">戦車Bのコンテキスト</param>
        public void HandleTankHitTank(
            TankCollisionContext contextA,
            TankCollisionContext contextB
        )
        {
            // --------------------------------------------------
            // 衝突解決計算
            // --------------------------------------------------
            _collisionResolverCalculator.CalculateResolveInfo(
                contextA,
                contextB,
                contextA.TankRootManager.CurrentForwardSpeed,
                contextB.TankRootManager.CurrentForwardSpeed,
                out CollisionResolveInfo resolveInfoA,
                out CollisionResolveInfo resolveInfoB
            );

            // --------------------------------------------------
            // 押し戻し反映
            // --------------------------------------------------
            contextA.TankRootManager.ApplyCollisionResolve(resolveInfoA);
            contextB.TankRootManager.ApplyCollisionResolve(resolveInfoB);

            // OBB 更新
            contextA.UpdateOBB();
            contextB.UpdateOBB();
        }

        /// <summary>
        /// 戦車がアイテムに接触した際の処理
        /// アイテム取得イベントを通知
        /// </summary>
        /// <param name="context">戦車コンテキスト</param>
        /// <param name="item">アイテムコンテキスト</param>
        public void HandleTankHitItem(
            TankCollisionContext context,
            ItemCollisionContext item
        )
        {
            OnItemGet?.Invoke(context.TankRootManager, item.ItemSlot);
        }

        // --------------------------------------------------
        // 弾丸
        // --------------------------------------------------
        /// <summary>
        /// 弾丸が障害物に衝突した際の処理
        /// Base タグの障害物は無視
        /// </summary>
        /// <param name="bulletContext">弾丸コンテキスト</param>
        /// <param name="obstacle">障害物コンテキスト</param>
        public void HandleBulletHitObstacle(
            BulletCollisionContext bulletContext,
            ObstacleCollisionContext obstacle
        )
        {
            if (obstacle.Transform != null)
            {
                return;
            }

            // 対象障害物が Base ならスキップ
            if (obstacle.Transform.CompareTag("Base"))
            {
                return;
            }

            BulletBase bullet = bulletContext.Bullet;
            int obstacleId = obstacle.ObstacleId;

            // すでにヒット済みならスキップ
            if (!_hitHistory.Add((bullet.BulletId, obstacleId)))
            {
                return;
            }

            // 衝突イベント通知
            OnBulletHit?.Invoke(bullet, obstacle);
        }

        /// <summary>
        /// 弾丸が戦車に衝突した際の処理
        /// 発射元戦車は無視
        /// </summary>
        /// <param name="bulletContext">弾丸コンテキスト</param>
        /// <param name="tank">戦車コンテキスト</param>
        public void HandleBulletHitTank(
            BulletCollisionContext bulletContext,
            TankCollisionContext tank
        )
        {
            BulletBase bullet = bulletContext.Bullet;

            // 発射元戦車は処理をスキップ
            if (bullet.BulletId == tank.TankRootManager.TankId)
            {
                return;
            }

            int tankId = tank.TankRootManager.TankId;

            // すでにヒット済みならスキップ
            if (!_hitHistory.Add((bullet.BulletId, tankId)))
            {
                return;
            }

            // 衝突イベント通知
            OnBulletHit?.Invoke(bullet, tank);
        }

        /// <summary>
        /// 弾丸がプールに戻る際に履歴をクリア
        /// </summary>
        public void ClearHitHistory(BulletBase bullet)
        {
            _hitHistory.RemoveWhere(pair => pair.bulletId == bullet.BulletId);
        }
    }
}