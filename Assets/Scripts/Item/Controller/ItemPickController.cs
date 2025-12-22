// ======================================================
// ItemPickController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-22
// 更新日時 : 2025-12-22
// 概要     : ItemSlot をランダム抽選で管理するコントローラー
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using ItemSystem.Data;

namespace ItemSystem.Manager
{
    /// <summary>
    /// ItemSlot をランダム抽選で管理するコントローラー
    /// </summary>
    public sealed class ItemPickController
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// Queue に格納された ItemSlot からランダムに 1 つ抽選
        /// </summary>
        /// <param name="inactiveQueue">未使用スロットの Queue</param>
        /// <returns>抽選された ItemSlot。存在しなければ null</returns>
        public ItemSlot PickRandom(Queue<ItemSlot> inactiveQueue)
        {
            // Queue が空なら抽選不可
            if (inactiveQueue.Count == 0)
            {
                return null;
            }

            // ランダムな抽選位置を生成
            int pickIndex = Random.Range(0, inactiveQueue.Count);

            // Queue をループして pickIndex 番目の要素を取得
            int currentIndex = 0;
            ItemSlot pickedSlot = null;
            foreach (var slot in inactiveQueue)
            {
                if (currentIndex == pickIndex)
                {
                    pickedSlot = slot;
                    break;
                }
                currentIndex++;
            }

            return pickedSlot;
        }
    }
}