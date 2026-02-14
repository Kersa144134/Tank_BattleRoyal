// ======================================================
// WeaponItemData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-11
// 更新日時 : 2025-12-11
// 概要     : 戦車の武装アイテム用 ScriptableObject
// ======================================================

using UnityEngine;
using WeaponSystem.Data;

namespace ItemSystem.Data
{
    /// <summary>
    /// 戦車の武装を追加または変更するアイテム ScriptableObject
    /// </summary>
    [CreateAssetMenu(menuName = "Items/WeaponItem")]
    public class WeaponItemData : ItemData
    {
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>武装種別</summary>
        public WeaponType WeaponType;

        /// <summary>武装の攻撃力</summary>
        public float Damage;

        /// <summary>再装填時間</summary>
        public float ReloadTime;

        // ======================================================
        // Unity イベント
        // ======================================================

        /// <summary>
        /// ScriptableObject が有効化された際に呼ばれる
        /// </summary>
        private void OnEnable()
        {
            // 武装アイテムとして種別を固定設定する
            itemType = ItemType.Weapon;
        }
    }
}