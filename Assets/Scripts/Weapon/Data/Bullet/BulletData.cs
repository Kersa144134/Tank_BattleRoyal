// ======================================================
// BulletData.cs
// 作成者   : 高橋一翔
// 作成日   : 2025-12-12
// 更新日   : 2025-12-12
// 概要     : 弾丸の設定を ScriptableObject として保持し、弾丸ロジックの生成を担当する
// ======================================================

using UnityEngine;
using WeaponSystem.Factory;

namespace WeaponSystem.Data
{
    /// <summary>
    /// 弾丸設定データを保持する ScriptableObject
    /// BulletFactory と連携して BulletBase 派生クラスの
    /// インスタンスを生成する
    /// </summary>
    [CreateAssetMenu(fileName = "BulletData", menuName = "Weapon/BulletData")]
    public sealed class BulletData : ScriptableObject
    {
        // ======================================================
        // 弾丸パラメータ
        // ======================================================

        // --------------------------------------------------
        // 共通
        // --------------------------------------------------
        /// <summary>弾丸の種類</summary>
        public BulletType BulletType;

        /// <summary>弾速（共通）</summary>
        public float BulletSpeed;

        /// <summary>寿命（秒）</summary>
        public float Lifetime;

        // --------------------------------------------------
        // 榴弾
        // --------------------------------------------------
        /// <summary>榴弾の爆発半径</summary>
        public float ExplosiveRadius;

        // --------------------------------------------------
        // 徹甲弾
        // --------------------------------------------------
        /// <summary>徹甲弾の貫通可能速度</summary>
        public float PenetrationSpeed;

        // --------------------------------------------------
        // 誘導弾
        // --------------------------------------------------
        /// <summary>誘導弾の旋回速度</summary>
        public float RotateSpeed;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ScriptableObject から弾丸ロジックのインスタンスを生成
        /// BulletFactory に設定パラメータを渡して生成
        /// </summary>
        /// <returns>生成された BulletBase インスタンス</returns>
        public BulletBase CreateInstance()
        {
            // BulletFactory に共通/固有パラメータを渡してインスタンス化
            BulletFactory factory = new BulletFactory(
                BulletType,
                BulletSpeed,
                Lifetime,
                ExplosiveRadius,
                PenetrationSpeed,
                RotateSpeed
            );

            return factory.Create();
        }
    }
}