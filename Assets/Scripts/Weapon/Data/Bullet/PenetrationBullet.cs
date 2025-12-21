// ======================================================
// PenetrationBullet.cs
// 作成者   : 高橋一翔
// 作成日   : 2025-12-12
// 更新日   : 2025-12-21
// 概要     : 徹甲弾の弾丸ロジッククラス
//            弾丸ヒット時に弾速が一定以上なら弾丸が消えずに貫通する
// ======================================================

using CollisionSystem.Data;
using TankSystem.Data;

namespace WeaponSystem.Data
{
    /// <summary>
    /// 徹甲弾ロジッククラス
    /// 発射方向に飛行し、弾丸ヒット時に弾速が一定以上なら弾丸が消えずに貫通する
    /// </summary>
    public class PenetrationBullet : BulletBase
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>貫通判定速度</summary>
        private float _penetrationSpeed;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>貫通判定速度の外部設定用プロパティ</summary>
        public float PenetrationSpeed
        {
            get => _penetrationSpeed;
            set => _penetrationSpeed = value;
        }

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>貫通時に減算する弾速</summary>
        private const float PENETRATION_SPEED_DECREMENT = 25f;

        // ======================================================
        // セッター
        // ======================================================

        /// <summary>
        /// 発射時のパラメータを設定する
        /// </summary>
        /// <param name="penetrationSpeed">貫通判定速度</param>
        public void SetParams(float penetrationSpeed)
        {
            _penetrationSpeed = penetrationSpeed;
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
        /// 弾丸ヒット時の処理
        /// 貫通判定速度を超えている場合は弾速を減算して消滅せず、
        /// それ以下なら通常通り弾丸を消滅させる
        /// </summary>
        /// <param name="collisionContext">衝突対象のコンテキスト</param>
        public override bool OnHit(in BaseCollisionContext collisionContext)
        {
            // 弾速が貫通速度以上なら貫通処理
            if (BulletSpeed >= _penetrationSpeed)
            {
                Penetrate(collisionContext);

                // 弾丸は消滅せず残す
                return false;
            }

            // 通常処理で消滅
            return base.OnHit(collisionContext);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 貫通処理
        /// 弾速を減算して残弾を残す
        /// 減算値は衝突対象の種類によって変化する
        /// </summary>
        /// <param name="collisionContext">衝突対象のコンテキスト</param>
        private void Penetrate(in BaseCollisionContext collisionContext)
        {
            // 減算値
            float decrement = PENETRATION_SPEED_DECREMENT;

            // 衝突対象が戦車なら減算値を変更
            if (collisionContext is TankCollisionContext)
            {
                const float TANK_DECREMENT_MULTIPLIER = 2f;
                decrement *= TANK_DECREMENT_MULTIPLIER;
            }

            // 弾速を減算
            BulletSpeed -= decrement / Mass;

            // BulletSpeed が 0 未満にならないように補正
            if (BulletSpeed < 0f)
            {
                BulletSpeed = 0f;
            }
        }
    }
}