// ======================================================
// HomingBullet.cs
// 作成者   : 高橋一翔
// 作成日   : 2025-12-21
// 更新日   : 2025-12-21
// 概要     : 誘導弾ロジッククラス
//            発射時に設定されたターゲットに追従する
// ======================================================

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

        /// <summary>追従対象ターゲット</summary>
        private Transform _target;

        /// <summary>旋回速度</summary>
        private float _rotateSpeed;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>爆発半径の外部設定用プロパティ</summary>
        public float RotateSpeed
        {
            get => _rotateSpeed;
            set => _rotateSpeed = value;
        }

        // ======================================================
        // セッター
        // ======================================================

        /// <summary>
        /// 誘導弾パラメータを設定する
        /// </summary>
        /// <param name="target">追従ターゲット</param>
        /// <param name="rotateSpeed">旋回速度</param>
        public void SetParams(Transform target, float rotateSpeed)
        {
            _target = target;
            _rotateSpeed = rotateSpeed;
        }

        // ======================================================
        // BulletBase イベント
        // ======================================================

        /// <summary>
        /// 毎フレーム呼び出される弾丸更新処理
        /// ターゲット方向への移動方向を補間しながら更新する
        /// </summary>
        /// <param name="deltaTime">前フレームからの経過時間</param>
        public override void Tick(float deltaTime)
        {
            if (Transform == null)
            {
                return;
            }

            if (_target != null)
            {
                // HomingBullet.Tick 内
                Vector3 targetDir = (_target.position - Transform.position).normalized;

                // NextDirection を補間してターゲット方向へ向ける
                NextDirection = Vector3.RotateTowards(NextDirection, targetDir, _rotateSpeed * deltaTime * Mathf.Deg2Rad, 0f);

                // Transform.rotation を NextDirection に沿って回転
                Transform.rotation = Quaternion.LookRotation(NextDirection, Vector3.up);
            }

            // 移動・高度補間は基底クラスに任せる
            base.Tick(deltaTime);
        }
    }
}
