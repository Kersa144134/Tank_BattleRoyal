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
        [SerializeField] private Transform _mainCamera;

        [Header("戦車")]
        /// <summary>戦車の Transform 配列</summary>
        [SerializeField] private Transform[] _tanks;

        [Header("障害物")]
        /// <summary>障害物の親 Transform</summary>
        [SerializeField] private Transform _obstacleRoot;

        [Header("アイテム")]
        /// <summary>アイテムの Transform とデータを併せ持つスロットリスト</summary>
        [SerializeField] private List<ItemSlot> _itemSlots;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>アイテム管理担当マネージャー</summary>
        private ItemManager _itemManager;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>障害物オブジェクトの Transform 配列</summary>
         private Transform[] _obstacles;
        
        /// <summary>更新対象の弾丸リスト</summary>
        private readonly List<BulletBase> _updatableBullets = new List<BulletBase>();

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// プレイヤー戦車 Transform を返す
        /// </summary>
        public Transform[] Tanks => _tanks;

        /// <summary>
        /// 障害物 Transform 配列を返す
        /// </summary>
        public Transform[] Obstacles => _obstacles;

        /// <summary>
        /// アイテムスロットリストを返す
        /// </summary>
        public List<ItemSlot> ItemSlots => _itemSlots;

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
            _itemManager = new ItemManager(_itemSlots, OnItemListChanged, _mainCamera);

            // 障害物オブジェクトを取得して登録
            InitializeObstacles();

            // アイテムスロットを有効化して登録
            InitializeItemSlots();
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

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// シーン内の障害物を親オブジェクトから取得して配列に格納
        /// </summary>
        private void InitializeObstacles()
        {
            if (_obstacleRoot == null)
            {
                _obstacles = Array.Empty<Transform>();
                Debug.LogWarning("[SceneObjectRegistry] 障害物親オブジェクトが未設定です。");
                return;
            }

            // 親オブジェクトの子すべてを取得
            int count = _obstacleRoot.childCount;
            _obstacles = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                _obstacles[i] = _obstacleRoot.GetChild(i);
            }
        }

        /// <summary>
        /// 登録済みアイテムスロットを初期化して ItemManager に追加
        /// </summary>
        private void InitializeItemSlots()
        {
            foreach (ItemSlot slot in _itemSlots)
            {
                if (slot.ItemTransform != null)
                {
                    _itemManager.AddItem(slot);
                }
            }
        }
    }
}