// ======================================================
// ItemDrawController.cs
// 作成者   : 高橋一翔
// 更新日時 : 2026-01-07
// 更新日時 : 2026-02-14
// 概要     : 未使用 ItemSlot 群から抽選を行う制御クラス
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using ItemSystem.Data;

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
        /// <returns>抽選された未使用キュー</returns>
        public Queue<ItemSlot> Draw(
            in Dictionary<ItemData, Queue<ItemSlot>> inactiveItems
        )
        {
            if (inactiveItems == null)
            {
                return null;
            }

            // 抽選対象となるキューの総数
            int availableQueueCount = 0;

            // 1回目の走査：要素を持つキュー数を数える
            foreach (Queue<ItemSlot> queue in inactiveItems.Values)
            {
                if (queue.Count == 0)
                {
                    continue;
                }

                // 抽選対象カウントを増加させる
                availableQueueCount++;
            }

            // 抽選対象が存在しない場合は null を返却
            if (availableQueueCount == 0)
            {
                return null;
            }

            // 抽選対象範囲でランダムインデックスを取得
            int targetIndex
                = Random.Range(0, availableQueueCount);

            // 現在の有効キュー走査インデックス
            int currentIndex = 0;

            // 2回目の走査：対象インデックスに一致するキューを取得
            foreach (Queue<ItemSlot> queue in inactiveItems.Values)
            {
                if (queue.Count == 0)
                {
                    continue;
                }

                // 抽選インデックスと一致した場合に返却
                if (currentIndex == targetIndex)
                {
                    return queue;
                }

                // 有効キューインデックスを進める
                currentIndex++;
            }

            return null;
        }
    }
}