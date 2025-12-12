using UnityEngine;

namespace WeaponSystem.Data
{
    /// <summary>
    /// 徹甲弾：貫通し続け、高度が一定以下で消滅する
    /// </summary>
    public class PenetrationBullet : BulletBase
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>弾丸飛行方向ベクトル</summary>
        private Vector3 _shootDirection;

        private Vector3 direction;
        private float exitHeight;

        public float PenetrationSpeed { get; set; }

        public void SetParams(Vector3 dir, float height)
        {
            direction = dir.normalized;
            exitHeight = height;
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
            CurrentPosition += direction * BulletSpeed * deltaTime;

            // 高度判定
            if (CurrentPosition.y <= exitHeight)
            {
                OnExit();
            }
        }
    }
}