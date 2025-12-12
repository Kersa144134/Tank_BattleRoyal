using UnityEngine;

namespace WeaponSystem.Data
{
    /// <summary>
    /// “Ob’eFŠÑ’Ê‚µ‘±‚¯A‚“x‚ªˆê’èˆÈ‰º‚ÅÁ–Å‚·‚é
    /// </summary>
    public class PenetrationBullet : BulletBase
    {
        private Vector3 direction;
        private float exitHeight;

        public float PenetrationSpeed { get; set; }

        public void SetParams(Vector3 dir, float height)
        {
            direction = dir.normalized;
            exitHeight = height;
        }

        protected override void Tick(float deltaTime)
        {
            CurrentPosition += direction * BulletSpeed * deltaTime;

            // ‚“x”»’è
            if (CurrentPosition.y <= exitHeight)
            {
                OnExit();
            }
        }
    }
}