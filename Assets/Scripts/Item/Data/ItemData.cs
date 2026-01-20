// ======================================================
// ItemData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-11
// 更新日時 : 2025-12-21
// 概要     : アイテムデータ基底クラス
// ======================================================

using UnityEngine;

namespace ItemSystem.Data
{
    /// <summary>
    /// アイテム種別を定義する列挙型
    /// </summary>
    public enum ItemType
    {
        /// <summary>パラメーター増加アイテム</summary>
        ParamIncrease,

        /// <summary>パラメーター減少アイテム</summary>
        ParamDecrease,

        /// <summary>武装アイテム</summary>
        Weapon,
    }

    /// <summary>
    /// すべてのアイテム ScriptableObject の共通基底クラス
    /// </summary>
    public abstract class ItemData : ScriptableObject
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>アイテム名</summary>
        [SerializeField] protected string itemName;

        /// <summary>アイテム種別</summary>
        [SerializeField] protected ItemType itemType;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// アイテム名
        /// </summary>
        public string Name
        {
            get
            {
                // アイテム名を返す
                return itemName;
            }
        }
        
        /// <summary>
        /// アイテム種別
        /// </summary>
        public ItemType Type
        {
            get
            {
                // アイテムの種別を返す
                return itemType;
            }
        }
    }
}