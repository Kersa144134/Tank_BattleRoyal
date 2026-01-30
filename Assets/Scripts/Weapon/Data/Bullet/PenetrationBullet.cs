// ======================================================
// PenetrationBullet.cs
// 作成者   : 高橋一翔
// 作成日   : 2025-12-12
// 更新日   : 2025-12-21
// 概要     : 徹甲弾の弾丸ロジッククラス
//            弾丸ヒット時に弾速が一定以上なら弾丸が消えずに貫通する
// ======================================================

using UnityEngine;
using CollisionSystem.Data;
using ObstacleSystem.Data;
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

        // --------------------------------------------------
        // 基準値
        // --------------------------------------------------
        /// <summary>基準となる装甲へのダメージ</summary>
        private const float BASE_ARMOR_DAMAGE = 2f;

        /// <summary>基準となる装甲へのダメージ倍率</summary>
        private const float BASE_ARMOR_DAMAGE_MULTIPLIER = 0.05f;

        /// <summary>弾速による装甲ダメージへの増幅係数</summary>
        private const float ARMOR_SPEED_DAMAGE_POWER = 1.25f;

        /// <summary>弾丸の質量による装甲ダメージへの増幅係数</summary>
        private const float ARMOR_MASS_DAMAGE_POWER = 1.5f;

        /// <summary>基準となる弾速減算値</summary>
        private const float BASE_PENETRATION_SPEED_DECREMENT = 15f;

        /// <summary>基準となる障害物貫通時の弾速減算値</summary>
        private const float BASE_OBSTACLE_DECREMENT = 1.5f;

        /// <summary>障害物スケールに応じた減算値倍率加算値</summary>
        private const float OBSTACLE_SCALE_MULTIPLIER = 0.2f;

        // --------------------------------------------------
        // パラメーター
        // --------------------------------------------------
        /// <summary>基準となる装甲ステータス</summary>
        private const float BASE_ARMOR = 1.5f;

        /// <summary>装甲1あたりの倍率加算値</summary>
        private const float ARMOR_MULTIPLIER = 0.075f;

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

                // ダメージ処理
                SetDamageTarget(collisionContext);
                ApplyDamage();

                // 爆発エフェクト再生
                _effectController.PlayExplosion();

                // 弾丸は消滅せず残す
                return false;
            }

            // 通常処理で消滅
            return base.OnHit(collisionContext);
        }

        /// <summary>
        /// 弾丸が対象に与えるダメージ処理
        /// </summary>
        public override void ApplyDamage()
        {
            // 通常ダメージ処理を先に実行
            base.ApplyDamage();

            // ダメージ対象が存在しない場合は処理なし
            if (_damageTarget == null)
            {
                return;
            }
            
            // 弾速、質量が高いほど、ダメージへの影響が段階的に大きくなるよう補正する
            float speedFactor =
                Mathf.Pow(BulletSpeed, ARMOR_SPEED_DAMAGE_POWER);

            float massFactor =
                Mathf.Pow(Mass, ARMOR_MASS_DAMAGE_POWER);

            // 最終ダメージ算出
            float damage = BASE_ARMOR_DAMAGE + speedFactor * massFactor * BASE_ARMOR_DAMAGE_MULTIPLIER;

            // 装甲ダメージを適用
            _damageTarget.TakeArmorDamage(damage);

            _damageTarget = null;
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
            float decrement = BASE_PENETRATION_SPEED_DECREMENT;

            // 衝突対象に応じて処理を分岐
            if (collisionContext is TankCollisionContext tank)
            {
                int tankArmor = tank.TankRootManager.TankStatus.Armor;
                decrement *= BASE_ARMOR + tankArmor * ARMOR_MULTIPLIER;
            }
            if (collisionContext is ObstacleCollisionContext obstacle)
            {
                float obstacleScale = Mathf.Min(obstacle.Transform.lossyScale.x, obstacle.Transform.lossyScale.z);
                decrement *= BASE_OBSTACLE_DECREMENT + obstacleScale * OBSTACLE_SCALE_MULTIPLIER;
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