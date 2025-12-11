// ======================================================
// ParamItemData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-11
// 更新日時 : 2025-12-11
// 概要     : 戦車のパラメーターアイテム用 ScriptableObject
// ======================================================

using UnityEngine;
using TankSystem.Data;

namespace ItemSystem.Data
{
    /// <summary>
    /// 戦車のパラメーターを増減させるアイテム ScriptableObject
    /// </summary>
    [CreateAssetMenu(menuName = "Items/ParamItem")]
    public class ParamItemData : ItemData
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>影響する戦車パラメーター種別</summary>
        public TankParam Type;

        /// <summary>増減値</summary>
        [Range(-1, 1)]
        public int Value;
    }
}