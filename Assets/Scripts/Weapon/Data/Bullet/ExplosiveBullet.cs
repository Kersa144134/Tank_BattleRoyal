using UnityEngine;

namespace WeaponSystem.Data
{
    /// <summary>
    /// 榴弾（着弾地点で爆発し消滅）
    /// </summary>
    public class ExplosiveBullet : BulletBase
    {
        private Vector3 shootDirection;
        private float explosionRadius;
        private LayerMask hitMask;

        public float ExplosiveRadius { get; set; }

        // --------------------------------------------------
        // 発射時パラメータを外部から受け取る
        // --------------------------------------------------
        public void SetParams(Vector3 direction, float radius, LayerMask mask)
        {
            shootDirection = direction.normalized;
            explosionRadius = radius;
            hitMask = mask;
        }

        // --------------------------------------------------
        // 弾丸更新（移動・衝突判定）
        // --------------------------------------------------
        protected override void Tick(float deltaTime)
        {
            // 座標更新（Base の CurrentPosition へ書き込む）
            CurrentPosition += shootDirection * BulletSpeed * deltaTime;

            // 衝突チェック
            if (Physics.CheckSphere(CurrentPosition, explosionRadius, hitMask))
            {
                Explode();
                OnExit();
            }
        }

        // --------------------------------------------------
        // 爆発処理（ダメージやエフェクト）
        // --------------------------------------------------
        private void Explode()
        {
            // 爆発処理
        }
    }
}