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

        /// <summary>
        /// シーン上に存在するアイテムコンテキスト一覧
        /// </summary>
        private List<ItemCollisionContext> _items;

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
                context.UpdateLockAxis(_tanks[i].TankRootManager.CurrentFrameLockAxis);
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
        /// アイテムコンテキストを設定する
        /// ゲーム中にアイテムの追加・削除・更新があった場合に呼び出す
        /// </summary>
        /// <param name="itemContexts">新しいアイテムコンテキスト一覧</param>
        public void SetItemContexts(in List<ItemCollisionContext> itemContexts)
        {
            if (itemContexts == null)
            {
                // null が渡された場合は内部データをクリア
                _items = null;
                return;
            }

            // アイテムコンテキスト一覧を生成
            _items = new List<ItemCollisionContext>(itemContexts.Count);

            for (int i = 0; i < itemContexts.Count; i++)
            {
                ItemCollisionContext context = itemContexts[i];

                if (context == null || context.ItemSlot == null)
                {
                    continue;
                }

                // アイテムコンテキストを追加
                _items.Add(context);
            }
        }
    }
}