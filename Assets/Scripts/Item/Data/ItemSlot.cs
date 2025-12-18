// ======================================================
// ItemSlot.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-11
// 更新日時 : 2025-12-12
// 概要     : TankRootManager でアイテム ScriptableObject を登録可能にするクラス
// ======================================================

using System;
using UnityEngine;
using ItemSystem.Data;

namespace TankSystem.Data
{
    /// <summary>
    /// インスペクタで ScriptableObject を登録するためのクラス
    /// </summary>
    [Serializable]
    public class ItemSlot
    {
        /// <summary>アイテムの Transform</summary>
        public Transform Transform;

        /// <summary>登録するアイテムデータベース ScriptableObject</summary>
        public ItemData ItemData;

        /// <summary>このアイテムセットを有効にするか</summary>
        public bool IsEnabled;
    }
}