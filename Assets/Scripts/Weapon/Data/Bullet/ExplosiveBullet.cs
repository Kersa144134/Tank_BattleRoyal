// ======================================================
// ExplosiveBullet.cs
// 作成者   : 高橋一翔
// 作成日   : 2025-12-12
// 更新日   : 2025-12-12
// 概要     : 榴弾の弾丸ロジッククラス
//            衝突または一定時間経過で爆発する
// ======================================================

using UnityEngine;

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

        /// <summary>弾丸飛行方向ベクトル</summary>
        private Vector3 _shootDirection;

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

        /// <summary>
        /// 弾丸の飛行方向を設定する
        /// </summary>
        /// <param name="direction">弾丸の飛行方向ベクトル</param>
        protected override void SetDirection(Vector3 direction)
        {
            _shootDirection = direction.normalized;
        }

        // ======================================================
        // 抽象メソッド
        // ======================================================

        /// <summary>
        /// 毎フレーム呼ばれる弾丸更新処理
        /// 移動・衝突判定・爆発を処理
        /// </summary>
        /// <param name="deltaTime">前フレームからの経過時間</param>
        protected override void Tick(float deltaTime)
        {
            // --------------------------------------------------
            // 移動処理
            // --------------------------------------------------
            CurrentPosition += _shootDirection * BulletSpeed * deltaTime;

            // Transform に反映
            ApplyToTransform();

            // --------------------------------------------------
            // 衝突判定
            // --------------------------------------------------
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 爆発処理
        /// </summary>
        private void Explode()
        {
            Debug.Log($"[ExplosiveBullet] 爆発発生 at {CurrentPosition}, 半径: {_explosionRadius}");
        }
    }
}