using UnityEngine;

namespace WeaponSystem.Data
{
    /// <summary>
    /// 誘導弾：ターゲットに追従する
    /// </summary>
    public class HomingBullet : BulletBase
    {
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