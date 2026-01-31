// ======================================================
// ExplosiveBullet.cs
// 作成者   : 高橋一翔
// 作成日   : 2025-12-12
// 更新日   : 2025-12-21
// 概要     : 榴弾の弾丸ロジッククラス
//            衝突または一定時間経過で爆発する
// ======================================================

using CollisionSystem.Data;
using System;
using UnityEngine;
using WeaponSystem.Interface;

namespace WeaponSystem.Data
{
    /// <summary>
    /// 榴弾ロジッククラス
    /// 発射方向に飛行し、衝突または一定時間経過で爆発する
    /// </summary>
    public class ExplosiveBullet : BulletBase
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>爆発判定半径</summary>
        private float _explosionRadius;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>爆発半径の外部設定用プロパティ</summary>
        public float ExplosiveRadius
        {
            get => _explosionRadius;
            set => _explosionRadius = value;
        }

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>弾丸が爆発したことを通知する</summary>
        public event Action<ExplosiveBullet, Vector3, float> OnExploded;

        // ======================================================
        // セッター
        // ======================================================

        /// <summary>
        /// 発射時のパラメータを設定する
        /// </summary>
        /// <param name="radius">爆発判定半径</param>
        public void SetParams(float radius)
        {
            _explosionRadius = radius;
        }

        // ======================================================
        // 抽象メソッド
        // ======================================================

        /// <summary>
        /// 毎フレーム呼ばれる弾丸更新処理
        /// 移動・衝突判定・爆発を処理
        /// </summary>
        /// <param name="deltaTime">前フレームからの経過時間</param>
        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
        }

        // ======================================================
        // BulletBase イベント
        // ======================================================

        /// <summary>
        /// 弾丸終了時の処理
        /// 爆発を発生させた後、基底クラスの終了処理を行う
        /// </summary>
        /// <param name="immediate">true の場合は即時終了処理を行う</param>
        public override void OnExit(in bool immediate = false)
        {
            if (!immediate)
            {
                // 爆発処理
                Explode();
            }

            // 基底クラスの終了処理
            base.OnExit();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 弾丸が対象に与える爆風ダメージ処理
        /// </summary>
        public void ApplyExplodeDamage(in BaseCollisionContext[] collisionContexts)
        {
            if (collisionContexts == null)
            {
                return;
            }

            for (int i = 0; i < collisionContexts.Length; i++)
            {
                IDamageable damageable = collisionContexts[i].Transform.GetComponent<IDamageable>();

                // 質量が高いほど、ダメージへの影響が段階的に大きくなるよう補正する
                float massFactor =
                    Mathf.Pow(Mass, MASS_DAMAGE_POWER);

                // 最終ダメージ算出
                float damage = BASE_BULLET_DAMAGE + massFactor * BASE_BULLET_DAMAGE_MULTIPLIER;

                // ダメージ適用
                damageable.TakeDamage(damage);
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 爆発処理
        /// </summary>
        private void Explode()
        {
            // 爆発半径を質量に応じて計算
            float explosionRadius = Mass * _explosionRadius;

            OnExploded?.Invoke(this, Transform.position, explosionRadius);
        }
    }
}