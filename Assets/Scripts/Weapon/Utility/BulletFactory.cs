// ======================================================
// BulletFactory.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-12
// 更新日時 : 2025-12-12
// 概要     : 弾丸ロジックを生成するファクトリー
//            弾丸タイプに応じて BulletBase 派生を生成する
// ======================================================

using WeaponSystem.Data;

namespace WeaponSystem.Factory
{
    /// <summary>
    /// 弾丸ロジックを生成するファクトリークラス
    /// BulletData から渡されたパラメータに基づき、
    /// BulletBase 派生クラスを生成する
    /// </summary>
    public sealed class BulletFactory
    {
        // ======================================================
        // フィールド
        // ======================================================

        // --------------------------------------------------
        // 共通
        // --------------------------------------------------
        /// <summary>弾丸の種類を保持</summary>
        private readonly BulletType _bulletType;

        /// <summary>弾速（共通）</summary>
        private readonly float _bulletSpeed;

        /// <summary>寿命（秒）</summary>
        private readonly float _lifetime;

        // --------------------------------------------------
        // 榴弾
        // --------------------------------------------------
        /// <summary>爆発半径</summary>
        private readonly float _explosiveRadius;

        // --------------------------------------------------
        // 徹甲弾
        // --------------------------------------------------
        /// <summary>貫通可能速度</summary>
        private readonly float _penetrationSpeed;

        // --------------------------------------------------
        // 誘導弾
        // --------------------------------------------------
        /// <summary>旋回速度</summary>
        private readonly float _rotateSpeed;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// ファクトリー生成時にパラメータを受け取り内部に保持する
        /// </summary>
        /// <param name="type">弾丸タイプ</param>
        /// <param name="speed">弾速（共通）</param>
        /// <param name="life">寿命</param>
        /// <param name="explosiveRadius">爆発半径</param>
        /// <param name="penetrationSpeed">貫通可能速度</param>
        /// <param name="rotateSpeed">旋回速度</param>
        public BulletFactory(
            BulletType type,
            float speed,
            float life,
            float explosiveRadius = 0f,
            float penetrationSpeed = 0f,
            float rotateSpeed = 0f)
        {
            _bulletType = type;
            _bulletSpeed = speed;
            _lifetime = life;
            _explosiveRadius = explosiveRadius;
            _penetrationSpeed = penetrationSpeed;
            _rotateSpeed = rotateSpeed;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 弾丸ロジックを生成する
        /// </summary>
        /// <returns>生成された BulletBase インスタンス</returns>
        public BulletBase Create()
        {
            // 生成する弾丸インスタンスを保持する変数
            BulletBase instance = null;

            // 弾丸タイプに応じて派生クラスを生成
            switch (_bulletType)
            {
                // --------------------------------------------------
                // 榴弾
                // --------------------------------------------------
                case BulletType.Explosive:
                    {
                        ExplosiveBullet bullet = new ExplosiveBullet
                        {
                            BulletSpeed = _bulletSpeed,
                            Lifetime = _lifetime,
                            ExplosiveRadius = _explosiveRadius
                        };
                        instance = bullet;
                    }
                    break;

                // --------------------------------------------------
                // 徹甲弾
                // --------------------------------------------------
                case BulletType.Penetration:
                    {
                        PenetrationBullet bullet = new PenetrationBullet
                        {
                            BulletSpeed = _bulletSpeed,
                            Lifetime = _lifetime,
                            PenetrationSpeed = _penetrationSpeed
                        };
                        instance = bullet;
                    }
                    break;

                // --------------------------------------------------
                // 誘導弾
                // --------------------------------------------------
                case BulletType.Homing:
                    {
                        HomingBullet bullet = new HomingBullet
                        {
                            BulletSpeed = _bulletSpeed,
                            Lifetime = _lifetime,
                            RotateSpeed = _rotateSpeed
                        };
                        instance = bullet;
                    }
                    break;
            }

            return instance;
        }
    }
}