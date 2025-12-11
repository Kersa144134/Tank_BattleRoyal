// ======================================================
// TankCollisionService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-10
// 更新日時 : 2025-12-10
// 概要     : 戦車と障害物の AABB 衝突判定を専任で担当するサービスクラス
//            障害物 AABB をキャッシュし、戦車の AABB を動的生成して判定を行う
// ======================================================

using System;
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

        /// <summary>アイテムの構造体リスト</summary>
        private List<ItemSlot> _items;

        /// <summary>戦車 OBB</summary>
        private OBBData _tankOBB;
        
        /// <summary>障害物の AABB をキャッシュして保持する構造体配列</summary>
        private readonly AABBData[] _obstacleAABBs;

        /// <summary>アイテムの AABB をキャッシュして保持する構造体配列</summary>
        private AABBData[] _itemAABBs;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>障害物衝突時</summary>
        public event Action<Transform> OnObstacleHit;

        /// <summary>アイテム取得時</summary>
        public event Action<ItemSlot> OnItemHit;

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
        public void SetItemAABBs(in List<ItemSlot> items)
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
                Transform itemTransform = items[i].ItemTransform;
                if (itemTransform == null)
                {
                    _itemAABBs[i] = new AABBData(Vector3.zero, Vector3.zero);
                    continue;
                }

                // 障害物中心座標を取得する
                Vector3 center = itemTransform.position;

                // 障害物のワールドサイズを lossyScale から取得する
                Vector3 worldSize = itemTransform.lossyScale;

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
        /// 毎フレーム呼び出すことで戦車と障害物／アイテムの衝突をチェックし、
        /// ヒットした対象に応じてイベントを発火する
        /// </summary>
        public void UpdateCollisionChecks()
        {
            // 戦車 OBB を生成
            _tankOBB = _obbFactory.CreateOBB(
                _tankTransform,
                _hitboxCenter,
                _hitboxSize
            );

            // 障害物チェック
            for (int i = 0; i < _obstacles.Length; i++)
            {
                if (_obstacles[i] == null) continue;

                if (_boxCollisionController.IsColliding(_tankOBB, _obstacleAABBs[i]))
                {
                    OnObstacleHit?.Invoke(_obstacles[i]);
                }
            }

            // アイテムチェック
            for (int i = 0; i < _items.Count; i++)
            {
                if (!_items[i].IsEnabled || _items[i].ItemTransform == null)
                {
                    continue;
                }

                if (_boxCollisionController.IsColliding(_tankOBB, _itemAABBs[i]))
                {
                    OnItemHit?.Invoke(_items[i]);
                }
            }
        }

        /// <summary>
        /// 指定した座標に戦車を移動させた場合、障害物と衝突するか判定する
        /// </summary>
        /// <param name="tankPos">判定対象となる戦車の座標</param>
        /// <returns>
        /// 衝突していれば <c>true</c>、衝突していなければ <c>false</c> を返す
        /// </returns>
        public bool IsCollidingWithObstacleAtPosition(Vector3 tankPos)
        {
            // 戦車 OBB を生成
            _tankOBB = _obbFactory.CreateOBB(
                _tankTransform,
                _hitboxCenter,
                _hitboxSize
            );

            // 障害物チェック
            for (int i = 0; i < _obstacleAABBs.Length; i++)
            {
                if (_obstacles[i] == null)
                {
                    continue;
                }

                if (_boxCollisionController.IsColliding(_tankOBB, _obstacleAABBs[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}