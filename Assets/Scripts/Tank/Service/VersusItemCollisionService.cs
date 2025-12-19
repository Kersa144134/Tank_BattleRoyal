// ======================================================
// VersusItemCollisionService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-18
// 概要     : 戦車とアイテムの OBB 衝突判定を担当するサービス
// ======================================================

using CollisionSystem.Calculator;
using CollisionSystem.Interface;
using ItemSystem.Data;
using ObstacleSystem.Data;
using System;
using System.Collections.Generic;
using TankSystem.Data;
using TankSystem.Interface;
using TankSystem.Utility;
using UnityEngine;

namespace TankSystem.Service
{
    /// <summary>
    /// 戦車とアイテム間の衝突判定を専門に処理するサービス
    /// アイテムは取得専用であり、押し戻し処理は行わない
    /// </summary>
    public sealed class VersusItemCollisionService : ITankCollisionService
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>
        /// OBB 同士の水平方向衝突判定を行う計算器
        /// </summary>
        private readonly BoundingBoxCollisionCalculator _boxCollisionCalculator;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// 衝突判定対象となる戦車コンテキスト配列
        /// </summary>
        private readonly TankCollisionContext[] _tanks;

        /// <summary>衝突判定対象として登録されているアイテムコンテキスト一覧</summary>
        private readonly List<ItemCollisionContext> _items = new List<ItemCollisionContext>();
        
        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// 戦車がアイテムと接触した際に通知されるイベント
        /// </summary>
        public event Action<TankCollisionContext, ItemCollisionContext> OnItemHit;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 戦車 vs アイテム 衝突判定サービスを生成する
        /// </summary>
        /// <param name="boxCollisionCalculator">OBB 同士の水平方向衝突判定を行う計算器</param>
        /// <param name="tanks">戦車コンテキスト</param>
        /// <param name="items">アイテムコンテキスト</param>
        public VersusItemCollisionService(
            in BoundingBoxCollisionCalculator boxCollisionCalculator,
            in TankCollisionContext[] tanks,
            in List<ItemCollisionContext> items
        )
        {
            _boxCollisionCalculator = boxCollisionCalculator;
            _tanks = tanks;
            _items = items;
        }

        // ======================================================
        // ITankCollisionService 実装
        // ======================================================

        /// <summary>
        /// 判定ループ開始前の事前処理
        /// アイテム側にはフレーム更新が無いため処理なし
        /// </summary>
        public void PreUpdate()
        {
            if (_tanks == null)
            {
                return;
            }

            // 全戦車の OBB を更新
            for (int i = 0; i < _tanks.Length; i++)
            {
                TankCollisionContext context = _tanks[i];
                context.UpdateOBB();
            }
        }

        /// <summary>
        /// 戦車とアイテムの衝突判定を実行する
        /// </summary>
        public void Execute()
        {
            if (_items == null)
            {
                return;
            }

            // --------------------------------------------------
            // 戦車 × アイテム 判定ループ
            // --------------------------------------------------
            for (int i = 0; i < _tanks.Length; i++)
            {
                // 判定対象戦車を取得する
                TankCollisionContext tankContext = _tanks[i];

                for (int t = 0; t < _items.Count; t++)
                {
                    if (!_items[t].ItemSlot.IsEnabled)
                    {
                        continue;
                    }

                    // OBB 衝突判定
                    if (!_boxCollisionCalculator.IsCollidingHorizontal(
                        tankContext.OBB,
                        _items[t].OBB
                    ))
                    {
                        continue;
                    }

                    // アイテム取得イベントを通知
                    OnItemHit?.Invoke(
                        tankContext,
                        _items[t]
                    );
                }
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 初期アイテムコンテキストを一括で登録する
        /// 既存の登録内容はすべて破棄される
        /// </summary>
        /// <param name="contexts">初期登録対象となるアイテムコンテキスト一覧</param>
        public void InitializeItemContexts(in List<ItemCollisionContext> contexts)
        {
            // 既存登録をすべて解除する
            _items.Clear();

            if (contexts == null)
            {
                return;
            }

            for (int i = 0; i < contexts.Count; i++)
            {
                ItemCollisionContext context = contexts[i];

                if (context == null || context.ItemSlot == null)
                {
                    continue;
                }

                _items.Add(context);
            }
        }
        
        /// <summary>
        /// アイテムコンテキストを衝突判定対象として追加する
        /// </summary>
        /// <param name="context">追加対象となるアイテム衝突コンテキスト</param>
        public void AddItemContext(in ItemCollisionContext context)
        {
            if (context == null || context.ItemSlot == null)
            {
                return;
            }

            // 既に登録済みの場合は追加しない
            if (_items.Contains(context))
            {
                return;
            }

            // 衝突判定対象として登録する
            _items.Add(context);
        }

        /// <summary>
        /// アイテムコンテキストを衝突判定対象から削除する
        /// </summary>
        /// <param name="context">削除対象となるアイテム衝突コンテキスト</param>
        public void RemoveItemContext(in ItemCollisionContext context)
        {
            if (context == null)
            {
                return;
            }

            // 登録されていない場合は何もしない
            if (_items.Contains(context) == false)
            {
                return;
            }

            // 衝突判定対象から除外する
            _items.Remove(context);
        }
    }
}