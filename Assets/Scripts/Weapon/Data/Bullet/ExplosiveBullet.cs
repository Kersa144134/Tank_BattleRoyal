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

        /// <summary>弾丸発射時の初期弾速</summary>
        private float _maxSpeed;

        /// <summary>弾丸発射時の初期高度</summary>
        private float _maxHeight;

        /// <summary>弾丸が地面に接触する最終高度</summary>
        private float _minHeight;

        /// <summary>爆発判定半径</summary>
        private float _explosionRadius;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>弾丸高度の補間用指数</summary>
        private const float Y_INTERPOLATION_EXPONENT = 2.0f;
        
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
            // 水平移動
            // --------------------------------------------------
            NextPosition += _shootDirection * BulletSpeed * deltaTime;

            // --------------------------------------------------
            // 高度座標補間
            // BulletSpeed が減少するにつれて Y が下降する
            // --------------------------------------------------
            if (BulletSpeed > 0f)
            {
                float t = 1f - (BulletSpeed / _maxSpeed);
                t = Mathf.Pow(t, Y_INTERPOLATION_EXPONENT);
                Vector3 pos = NextPosition;
                pos.y = Mathf.Lerp(_maxHeight, _minHeight, t);
                NextPosition = pos;
            }
            else
            {
                Vector3 pos = NextPosition;
                pos.y = _minHeight;
                NextPosition = pos;
            }

            // --------------------------------------------------
            // Transform に反映
            // --------------------------------------------------
            ApplyToTransform();
        }

        // ======================================================
        // BulletBase イベント
        // ======================================================

        /// <summary>
        /// 弾丸開始時の処理
        /// 高度座標補間に使用する初期弾速、初期高度を取得
        /// </summary>
        public override void OnEnter(int tankId, Vector3 position, Vector3 direction)
        {
            base.OnEnter(tankId, position, direction);

            // 初期弾速、初期高度を取得
            _maxSpeed = BulletSpeed;
            _maxHeight = position.y;

            // Transform の Y スケールの半分を地面接触時の最終 Y 座標に設定
            _minHeight = Transform.localScale.y * 0.5f;
        }
        
        /// <summary>
        /// 弾丸終了時の処理
        /// 爆発を発生させた後、基底クラスの終了処理を行う
        /// </summary>
        public override void OnExit()
        {
            // 爆発処理
            Explode();

            // 基底クラスの終了処理
            base.OnExit();
        }
        
        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 爆発処理
        /// </summary>
        private void Explode()
        {
            
        }
    }
}