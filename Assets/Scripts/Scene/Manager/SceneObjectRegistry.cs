// ======================================================
// SceneObjectRegistry.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-12
// 更新日時 : 2025-12-12
// 概要     : シーン上の戦車・障害物・アイテムを一元管理するレジストリクラス
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using SceneSystem.Interface;
using TankSystem.Data;

namespace TankSystem.Manager
{
    /// <summary>
    /// シーン内の当たり判定対象オブジェクトを一元管理するレジストリ
    /// </summary>
    public class SceneObjectRegistry : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("プレイヤー戦車")]
        /// <summary>プレイヤー戦車の Transform</summary>
        [SerializeField] private Transform _playerTankTransform;

        [Header("障害物リスト")]
        /// <summary>障害物オブジェクトの Transform 配列</summary>
        [SerializeField] private Transform[] _obstacleTransforms;

        [Header("アイテムリスト")]
        /// <summary>アイテムオブジェクトの Transform とデータを併せ持つスロットリスト</summary>
        [SerializeField] private List<ItemSlot> _itemSlots;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>アイテム管理担当マネージャー</summary>
        private ItemManager _itemManager;
        
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// プレイヤー戦車 Transform を返す
        /// </summary>
        public Transform PlayerTankTransform
        {
            get { return _playerTankTransform; }
        }

        /// <summary>
        /// 障害物 Transform 配列を返す
        /// </summary>
        public Transform[] Obstacles
        {
            get { return _obstacleTransforms; }
        }

        /// <summary>
        /// アイテムスロットリストを返す
        /// </summary>
        public List<ItemSlot> ItemSlots
        {
            get { return _itemSlots; }
        }

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// アイテムスロットの数が変化したときに発火するイベント
        /// </summary>
        public event Action<List<ItemSlot>> OnItemListChanged;

        // ======================================================
        // IUpdatableイベント
        // ======================================================

        public void OnEnter()
        {
            // ItemManager を生成して保持
            _itemManager = new ItemManager(_itemSlots, OnItemListChanged);
        }

        public void OnUpdate()
        {

        }

        public void OnLateUpdate()
        {

        }

        public void OnExit()
        {
            
        }

        public void OnPhaseEnter()
        {

        }

        public void OnPhaseExit()
        {

        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// アイテムスロットを追加しイベントを発火する
        /// </summary>
        /// <param name="slot">追加するスロット</param>
        public void AddItem(ItemSlot slot)
        {
            // スロット追加処理
            _itemManager.AddItem(slot);
        }

        /// <summary>
        /// アイテムスロットを削除しイベントを発火する
        /// </summary>
        /// <param name="slot">削除するスロット</param>
        public void RemoveItem(ItemSlot slot)
        {
            // スロット削除処理
            _itemManager.RemoveItem(slot);
        }
    }
}