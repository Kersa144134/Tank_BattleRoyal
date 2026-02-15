// ======================================================
// ItemDrawController.cs
// 作成者   : 高橋一翔
// 更新日時 : 2026-01-07
// 更新日時 : 2026-02-14
// 概要     : 未使用 ItemSlot 群から抽選を行う制御クラス
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using ItemSystem.Controller;
using ItemSystem.Data;
using TankSystem.Data;

namespace ItemSystem.Manager
{
    /// <summary>
    /// 未使用 ItemSlot 群から抽選を行う制御クラス
    /// </summary>
    public sealed class ItemDrawController
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 未使用アイテム群からランダムに1つ抽選する
        /// </summary>
        /// <param name="inactiveItems">ItemData 単位で管理された未使用 ItemSlot 辞書</param>
        /// <param name="spawnPointType">SpawnPoint の種別</param>
        /// <returns>抽選された未使用キュー</returns>
        public Queue<ItemSlot> Draw(
            in Dictionary<ItemData, Queue<ItemSlot>> inactiveItems,
            in ItemSpawnController.SpawnPointType spawnPointType)
        {
            // 辞書が存在しない場合は抽選不可
            if (inactiveItems == null)
            {
                return null;
            }

            // --------------------------------------------------
            // 抽選対象の重み合計
            // --------------------------------------------------
            float totalWeight = 0.0f;

            foreach (KeyValuePair<ItemData, Queue<ItemSlot>> pair
                in inactiveItems)
            {
                Queue<ItemSlot> queue =
                    pair.Value;

                if (queue.Count == 0)
                {
                    continue;
                }

                // フィルタ条件に合致しない場合は対象外
                if (!IsEligibleForSpawn(
                        pair.Key,
                        spawnPointType))
                {
                    continue;
                }

                // ItemData 側の重みを加算
                totalWeight += pair.Key.SpawnWeight;
            }

            // 抽選対象が存在しない場合は null
            if (totalWeight <= 0.0f)
            {
                return null;
            }

            // --------------------------------------------------
            // 0 ～ totalWeight の乱数を生成
            // --------------------------------------------------
            float randomValue =
                Random.value * totalWeight;

            float cumulativeWeight = 0.0f;

            // --------------------------------------------------
            // 重み累積抽選
            // --------------------------------------------------
            foreach (KeyValuePair<ItemData, Queue<ItemSlot>> pair
                in inactiveItems)
            {
                Queue<ItemSlot> queue =
                    pair.Value;

                if (queue.Count == 0)
                {
                    continue;
                }

                if (!IsEligibleForSpawn(
                        pair.Key,
                        spawnPointType))
                {
                    continue;
                }

                // 重みを累積
                cumulativeWeight +=
                    pair.Key.SpawnWeight;

                // 乱数値が累積値を下回ったら決定
                if (randomValue <= cumulativeWeight)
                {
                    return queue;
                }
            }

            return null;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 指定 ItemData がスポーン条件を満たすか判定する
        /// </summary>
        /// <param name="itemData">判定対象 ItemData</param>
        /// <param name="spawnPointType">SpawnPoint 種別</param>
        /// <returns>抽選対象なら true</returns>
        private bool IsEligibleForSpawn(
            in ItemData itemData,
            in ItemSpawnController.SpawnPointType spawnPointType)
        {
            // --------------------------------------------------
            // 通常スポーン
            // --------------------------------------------------
            if (spawnPointType == ItemSpawnController.SpawnPointType.None)
            {
                return true;
            }

            // --------------------------------------------------
            // ParamBobus 専用スポーン
            // --------------------------------------------------
            if (spawnPointType == ItemSpawnController.SpawnPointType.ParamBobus)
            {
                // パラメーター増加アイテム のみ対象
                if (itemData.Type == ItemType.ParamIncrease)
                {
                    return true;
                }
            }

            // --------------------------------------------------
            // Supply 専用スポーン
            // --------------------------------------------------
            if (spawnPointType == ItemSpawnController.SpawnPointType.Supply)
            {
                // パラメーター増加アイテム のみ対象
                if (itemData.Type != ItemType.ParamIncrease)
                {
                    return false;
                }

                ParamItemData paramItemData =
                    itemData as ParamItemData;

                if (paramItemData == null)
                {
                    return false;
                }

                // Fuel または Ammo を対象
                if (paramItemData.ParamType == TankParam.Fuel ||
                    paramItemData.ParamType == TankParam.Ammo)
                {
                    return true;
                }
            }

            // --------------------------------------------------
            // Armory 専用スポーン
            // --------------------------------------------------
            if (spawnPointType == ItemSpawnController.SpawnPointType.Armory)
            {
                // パラメーター増加アイテム のみ対象
                if (itemData.Type != ItemType.ParamIncrease)
                {
                    return false;
                }

                ParamItemData paramItemData =
                    itemData as ParamItemData;

                if (paramItemData == null)
                {
                    return false;
                }

                // Barrel, ProjectileMass, ReloadTime を対象
                if (paramItemData.ParamType == TankParam.Barrel ||
                    paramItemData.ParamType == TankParam.ProjectileMass ||
                    paramItemData.ParamType == TankParam.ReloadTime)
                {
                    return true;
                }
            }

            return false;
        }
    }
}