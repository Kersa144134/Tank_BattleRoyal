// ======================================================
// ItemData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-11
// 更新日時 : 2025-12-11
// 概要     : アイテムデータクラス
// ======================================================

using UnityEngine;

namespace ItemSystem.Data
{
    /// <summary>
    /// すべてのアイテム ScriptableObject の共通基底クラス
    /// </summary>
    public abstract class ItemData : ScriptableObject
    {
        /// <summary>アイテム名（自動設定）</summary>
        public string Name;

        // エディタ上で変更があるたびに呼ばれる
        private void OnValidate()
        {
#if UNITY_EDITOR
            // アセット名を Name に設定
            Name = UnityEditor.AssetDatabase.GetAssetPath(this)
                .Split('/')
                [^1] // 最後の要素
                .Replace(".asset", "");
#endif
        }
    }
}