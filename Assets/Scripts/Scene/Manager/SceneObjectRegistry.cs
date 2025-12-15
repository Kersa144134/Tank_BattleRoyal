// ======================================================
// SceneObjectRegistry.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-12
// 更新日時 : 2025-12-12
// 概要     : シーン上の戦車・障害物・アイテムを一元管理するレジストリクラス
// ======================================================

using SceneSystem.Interface;
using System;
using System.Collections.Generic;
using TankSystem.Data;
using UnityEngine;
using WeaponSystem.Data;
using WeaponSystem.Manager;

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

        [Header("メインカメラ")]
        /// <summary>メインカメラ</summary>
        [SerializeField] private Camera _mainCamera;

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
        // フィールド
        // ======================================================

        /// <summary>更新対象の弾丸リスト</summary>
        private readonly List<BulletBase> _updatableBullets = new List<BulletBase>();

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
            _itemManager = new ItemManager(_itemSlots, OnItemListChanged, _mainCamera.transform);

            // アイテムスロットをすべて有効化して登録
            foreach (ItemSlot slot in _itemSlots)
            {
                if (slot.ItemTransform != null)
                {
                    _itemManager.AddItem(slot);
                }
            }
        }

        public void OnUpdate()
        {
            // 登録された弾丸の更新
            float deltaTime = Time.deltaTime;

            for (int i = _updatableBullets.Count - 1; i >= 0; i--)
            {
                BulletBase bullet = _updatableBullets[i];

                // 無効な弾丸はスキップ
                if (bullet == null || !bullet.IsEnabled)
                {
                    continue;
                }

                // 弾丸更新
                bullet.OnUpdate(deltaTime);
            }

            // アイテム回転処理
            _itemManager.UpdateItemRotations();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>弾丸を更新対象として登録する</summary>
        /// <param name="bullet">登録する弾丸</param>
        public void RegisterBullet(BulletBase bullet)
        {
            if (!_updatableBullets.Contains(bullet))
            {
                _updatableBullets.Add(bullet);
            }
        }

        /// <summary>弾丸を更新対象から解除する</summary>
        /// <param name="bullet">解除する弾丸</param>
        public void UnregisterBullet(BulletBase bullet)
        {
            _updatableBullets.Remove(bullet);
        }
        
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