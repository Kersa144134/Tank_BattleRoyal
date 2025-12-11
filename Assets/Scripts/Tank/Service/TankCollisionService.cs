// ======================================================
// TankCollisionService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-10
// 更新日時 : 2025-12-10
// 概要     : 戦車と障害物の AABB 衝突判定を専任で担当するサービスクラス
//            障害物 AABB をキャッシュし、戦車の AABB を動的生成して判定を行う
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using TankSystem.Controller;
using TankSystem.Data;
using TankSystem.Utility;

namespace TankSystem.Service
{
    /// <summary>
    /// 戦車の AABB と障害物 AABB の衝突判定を行うサービスクラス
    /// </summary>
    public class TankCollisionService
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>OBB を生成するためのファクトリークラス</summary>
        private readonly OBBFactory _obbFactory;

        /// <summary>AABB / OBB の距離計算および衝突判定を行うコントローラー</summary>
        private readonly BoundingBoxCollisionController _boxCollisionController;

        // ======================================================
        // フィールド
        // ======================================================
        
        /// <summary>戦車本体の Transform</summary>
        private readonly Transform _tankTransform;

        /// <summary>戦車の当たり判定ローカル中心</summary>
        private readonly Vector3 _hitboxCenter;

        /// <summary>戦車の当たり判定ローカルサイズ</summary>
        private readonly Vector3 _hitboxSize;

        /// <summary>障害物の Transform 配列</summary>
        private readonly Transform[] _obstacles;

        /// <summary>アイテムの Transform リスト</summary>
        private List<Transform> _items;

        /// <summary>障害物の AABB をキャッシュして保持する構造体配列</summary>
        private readonly AABBData[] _obstacleAABBs;

        /// <summary>アイテムの AABB をキャッシュして保持する構造体配列</summary>
        private AABBData[] _itemAABBs;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 衝突判定サービスを初期化し、障害物 AABB のキャッシュを作成する
        /// </summary>
        public TankCollisionService(
            in OBBFactory obbFactory,
            in BoundingBoxCollisionController boxCollisionController,
            in Transform tankTransform,
            in Vector3 hitboxCenter,
            in Vector3 hitboxSize,
            in Transform[] obstacles
        )
        {
            _obbFactory = obbFactory;
            _boxCollisionController = boxCollisionController;
            _tankTransform = tankTransform;
            _hitboxCenter = hitboxCenter;
            _hitboxSize = hitboxSize;
            _obstacles = obstacles;

            // 障害物 AABB のキャッシュ配列を初期化する
            _obstacleAABBs = new AABBData[_obstacles.Length];

            // --------------------------------------------------
            // 障害物 AABB を生成
            // --------------------------------------------------
            for (int i = 0; i < _obstacles.Length; i++)
            {
                Transform obs = _obstacles[i];
                if (obs == null)
                {
                    _obstacleAABBs[i] = new AABBData(Vector3.zero, Vector3.zero);
                    continue;
                }

                // 障害物中心座標を取得する
                Vector3 center = obs.position;

                // 障害物のワールドサイズを lossyScale から取得する
                Vector3 worldSize = obs.lossyScale;

                // 半径（半サイズ）を計算する
                Vector3 half = worldSize * 0.5f;

                // キャッシュ用 AABB を生成し配列に格納する
                _obstacleAABBs[i] = new AABBData(center, half);
            }
        }

        // ======================================================
        // セッター
        // ======================================================

        /// <summary>
        /// アイテム AABB 配列を生成する
        /// </summary>
        public void SetItemAABBs(in List<Transform> items)
        {
            if (items == null || items.Count == 0)
            {
                _itemAABBs = new AABBData[0];
                return;
            }

            _items = items;

            // アイテム AABB のキャッシュ配列を初期化する
            _itemAABBs = new AABBData[items.Count];
            
            for (int i = 0; i < items.Count; i++)
            {
                Transform item = items[i];
                if (item == null)
                {
                    _itemAABBs[i] = new AABBData(Vector3.zero, Vector3.zero);
                    continue;
                }

                // 障害物中心座標を取得する
                Vector3 center = item.position;

                // 障害物のワールドサイズを lossyScale から取得する
                Vector3 worldSize = item.lossyScale;

                // 半径（半サイズ）を計算する
                Vector3 half = worldSize * 0.5f;

                // キャッシュ用 AABB を生成し配列に格納する
                _itemAABBs[i] = new AABBData(center, half);
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 戦車と障害物の衝突判定
        /// </summary>
        /// <param name="hitPosition">衝突した障害物の中心座標</param>
        /// <returns>衝突していれば true</returns>
        public bool TryGetObstacleCollision(out Vector3 hitPosition)
        {
            hitPosition = Vector3.zero;

            if (_obstacles == null || _obstacles.Length == 0)
            {
                return false;
            }

            // 戦車の OBB を現在の位置・回転から生成する
            OBBData tankOBB = _obbFactory.CreateOBB(
                _tankTransform,
                _hitboxCenter,
                _hitboxSize
            );

            // 障害物 AABB と戦車 OBB を順に比較して衝突判定する
            for (int i = 0; i < _obstacles.Length; i++)
            {
                if (_obstacles[i] == null)
                {
                    continue;
                }

                // 衝突していれば障害物の AABB のワールド座標を返す
                if (_boxCollisionController.IsColliding(tankOBB, _obstacleAABBs[i]))
                {
                    hitPosition = _obstacleAABBs[i].Center;
                    return true;
                }
            }

            // 衝突が無ければ false を返す
            return false;
        }

        /// <summary>
        /// 戦車とアイテムの衝突判定
        /// </summary>
        /// <param name="hitTransform">衝突したアイテムの Transform</param>
        public bool TryGetItemCollision(out Transform hitTransform)
        {
            hitTransform = null;

            if (_items == null || _items.Count == 0)
            {
                return false;
            }

            // AABB 配列が未生成または長さ不一致の場合
            if (_itemAABBs == null || _itemAABBs.Length != _items.Count)
            {
                return false;
            }

            // 戦車の OBB を現在の位置・回転から生成する
            OBBData tankOBB = _obbFactory.CreateOBB(
                _tankTransform,
                _hitboxCenter,
                _hitboxSize
            );

            // アイテム AABB と戦車 OBB を順に比較して衝突判定する
            for (int i = 0; i < _items.Count; i++)
            {
                if (_obstacles[i] == null)
                {
                    continue;
                }

                bool isColliding = _boxCollisionController.IsColliding(tankOBB, _itemAABBs[i]);
                
                // 衝突していればアイテムの OBB Transformを返す
                if (_boxCollisionController.IsColliding(tankOBB, _itemAABBs[i]))
                {
                    hitTransform = _items[i];
                    return true;
                }
            }

            // 衝突が無ければ false を返す
            return false;
        }
    }
}