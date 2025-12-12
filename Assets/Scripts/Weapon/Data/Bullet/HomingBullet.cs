using UnityEngine;

namespace WeaponSystem.Data
{
    /// <summary>
    /// 誘導弾：ターゲットに追従する
    /// </summary>
    public class HomingBullet : BulletBase
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>弾丸飛行方向ベクトル</summary>
        private Vector3 _shootDirection;

        private Transform target;
        private float rotateSpeed;
        private float hitDistance;

        public float RotateSpeed { get; set; }

        public void SetParams(Transform tgt, float rotSpeed, float hitDist)
        {
            target = tgt;
            rotateSpeed = rotSpeed;
            hitDistance = hitDist;
        }

        /// <summary>
        /// 弾丸の飛行方向を設定する
        /// </summary>
        /// <param name="direction">弾丸の飛行方向ベクトル</param>
        protected override void SetDirection(Vector3 direction)
        {
            _shootDirection = direction.normalized;
        }

        protected override void Tick(float deltaTime)
        {
            if (target == null)
            {
                OnExit();
                return;
            }

            // ターゲット方向ベクトル
            Vector3 toTarget = (target.position - CurrentPosition).normalized;

            // 補間して旋回
            Vector3 newDir = Vector3.Lerp(
                (target.position - CurrentPosition).normalized,
                toTarget,
                rotateSpeed * deltaTime
            ).normalized;

            CurrentPosition += newDir * BulletSpeed * deltaTime;

            // 着弾判定
            if (Vector3.Distance(CurrentPosition, target.position) <= hitDistance)
            {
                OnExit();
            }
        }
    }
}