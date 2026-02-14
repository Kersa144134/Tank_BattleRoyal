// ======================================================
// ItemData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-11
// 更新日時 : 2026-02-15
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
        // 定数
        // ======================================================

        /// <summary>
        /// 最小重み値
        /// 0未満を防止するための下限値
        /// </summary>
        private const float MIN_WEIGHT = 0.0f;

        /// <summary>
        /// デフォルト重み値
        /// </summary>
        private const float DEFAULT_WEIGHT = 1.0f;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>アイテム名</summary>
        [SerializeField] protected string itemName;

        /// <summary>アイテム種別</summary>
        [SerializeField] protected ItemType itemType;

        /// <summary>
        /// 抽選重み
        /// 大きいほど抽選されやすくなる
        /// </summary>
        [SerializeField, Min(MIN_WEIGHT)]
        protected float spawnWeight = DEFAULT_WEIGHT;

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
                return itemType;
            }
        }

        /// <summary>
        /// 抽選重み
        /// 0未満にならないよう補正して返す
        /// </summary>
        public float SpawnWeight
        {
            get
            {
                // 0未満防止
                if (spawnWeight < MIN_WEIGHT)
                {
                    return MIN_WEIGHT;
                }

                return spawnWeight;
            }
        }
    }
}