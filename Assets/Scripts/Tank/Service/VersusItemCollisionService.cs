
using System;
using System.Collections.Generic;
using UnityEngine;
using CollisionSystem.Calculator;
using CollisionSystem.Interface;
using TankSystem.Data;
using TankSystem.Interface;
using TankSystem.Utility;

namespace TankSystem.Service
{
    /// <summary>
    /// 戦車とアイテムの衝突判定を担当する
    /// </summary>
    public sealed class VersusItemCollisionService
        : ITankCollisionService
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>
        /// OBB / OBB の衝突判定および MTV 計算を行う計算器
        /// </summary>
        private readonly BoundingBoxCollisionCalculator _boxCollisionCalculator;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>戦車コンテキスト一覧</summary>
        private readonly List<TankCollisionContext> _tanks;

        /// <summary>アイテム一覧</summary>
        private List<ItemSlot> _items;

        /// <summary>アイテム OBB 配列</summary>
        private IOBBData[] _itemOBBs;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// アイテムと接触した際に通知されるイベント
        /// 引数は衝突した戦車のコンテキストとアイテムスロット
        /// </summary>
        public event Action<TankCollisionContext, ItemSlot> OnItemHit;

        // ======================================================
        // コンストラクタ
        // ======================================================

        public VersusItemCollisionService(
            in BoundingBoxCollisionCalculator boxCollisionCalculator,
            in List<TankCollisionContext> tanks
        )
        {
            _boxCollisionCalculator = boxCollisionCalculator;
            _tanks = tanks;
        }

        // ======================================================
        // セッター
        // ======================================================

        public void SetItemOBBs(in OBBFactory obbFactory, in List<ItemSlot> items)
        {
            _items = items;

            if (items == null || items.Count == 0)
            {
                _itemOBBs = Array.Empty<IOBBData>();
                return;
            }

            _itemOBBs = new IOBBData[items.Count];

            for (int i = 0; i < items.Count; i++)
            {
                _itemOBBs[i] = obbFactory.CreateStaticOBB(
                    items[i].ItemTransform,
                    Vector3.zero,
                    Vector3.one
                );
            }
        }

        // ======================================================
        // パブリック
        // ======================================================

        public void UpdateCollisionChecks()
        {
            if (_items == null)
            {
                return;
            }

            for (int i = 0; i < _tanks.Count; i++)
            {
                _tanks[i].OBB.Update();

                for (int j = 0; j < _itemOBBs.Length; j++)
                {
                    if (!_items[j].IsEnabled)
                    {
                        continue;
                    }

                    if (_boxCollisionCalculator.IsCollidingHorizontal(
                        _tanks[i].OBB,
                        _itemOBBs[j]))
                    {
                        OnItemHit?.Invoke(_tanks[i], _items[j]);
                    }
                }
            }
        }
    }
}