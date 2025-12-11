// ======================================================
// ItemSlot.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-11
// 更新日時 : 2025-12-11
// 概要     : TankRootManager でアイテム ScriptableObject を登録可能にする構造体
// ======================================================

using System;
using UnityEngine;
using ItemSystem.Data;

namespace TankSystem.Data
{
    /// <summary>
    /// インスペクタで ScriptableObject を登録するための構造体
    /// </summary>
    [Serializable]
    public struct ItemSlot
    {
        /// <summary>アイテムの Transform</summary>
        public Transform ItemTransform;

        /// <summary>登録するアイテムデータベース ScriptableObject</summary>
        public ItemData ItemData;

        /// <summary>このアイテムセットを有効にするか</summary>
        public bool IsEnabled;
    }
}