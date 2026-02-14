// ======================================================
// IDamageable.cs
// 作成者   : 高橋一翔
// 作成日   : 2025-12-21
// 更新日   : 2025-12-21
// 概要     : ダメージを受けられるオブジェクトの共通インターフェース
// ======================================================

using UnityEngine;

namespace WeaponSystem.Interface
{
    /// <summary>
    /// ダメージを受けることができるインターフェース
    /// </summary>
    public interface IDamageable
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ダメージを受ける処理
        /// 耐久の減算を行う
        /// </summary>
        /// <param name="target">ダメージを受ける対象 Transform</param>
        /// <param name="damage">受けるダメージ量</param>
        void TakeDamage(in Transform target, in float damage);

        /// <summary>
        /// 装甲がダメージを受ける処理
        /// 徹甲弾に使用
        /// </summary>
        /// <param name="damage">ダメージを受ける対象 Transform</param>
        /// <param name="damage">装甲へのダメージ量</param>
        void TakeArmorDamage(in Transform target, in float damage) { }
    }
}