// ======================================================
// ParamItemData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-11
// 更新日時 : 2025-12-21
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

        /// <summary>
        /// 影響する戦車パラメーター種別
        /// </summary>
        public TankParam ParamType;

        /// <summary>
        /// 増減値（-1:減少 / 1:増加）
        /// </summary>
        [Range(-1, 1)]
        public int Value;

        // ======================================================
        // Unityイベント
        // ======================================================

        /// <summary>
        /// ScriptableObject が有効化された際に呼ばれる
        /// </summary>
        private void OnEnable()
        {
            // 増減値とアイテム種別を正規化する
            NormalizeValueAndType();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Inspector 上で値が変更された際に呼ばれる
        /// </summary>
        private void OnValidate()
        {
            // Editor 上でも常に正しい状態に補正する
            NormalizeValueAndType();
        }
#endif

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// Value の補正と ItemType の自動設定を行う
        /// </summary>
        private void NormalizeValueAndType()
        {
            // Value が 0 の場合は無効値のため 1 に補正する
            if (Value == 0)
            {
                // 増加アイテムとして扱うため 1 を設定する
                Value = 1;
            }

            // Value が負数の場合は減少アイテムとして設定する
            if (Value < 0)
            {
                // パラメーター減少アイテムとして種別を設定する
                itemType = ItemType.ParamDecrease;

                // ここで処理を終了する
                return;
            }

            // それ以外は増加アイテムとして種別を設定する
            itemType = ItemType.ParamIncrease;
        }
    }
}