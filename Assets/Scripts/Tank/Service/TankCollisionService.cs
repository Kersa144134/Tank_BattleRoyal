// ======================================================
// TankCollisionService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-10
// 更新日時 : 2025-12-10
// 概要     : 戦車と障害物の AABB 衝突判定を専任で担当するサービスクラス
//            障害物 AABB をキャッシュし、戦車の AABB を動的生成して判定を行う
// ======================================================

using UnityEngine;
using TankSystem.Data;
using TankSystem.Utility;
using TankSystem.Controller;

namespace TankSystem.Service
{
    /// <summary>
    /// 戦車の AABB と障害物 AABB の衝突判定を行うサービスクラス
    /// </summary>
    public class TankCollisionService
    {
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

        /// <summary>障害物の AABB をキャッシュして保持する配列</summary>
        private readonly AABBData[] _obstacleAABBs;

        /// <summary>AABB を生成するためのファクトリークラス</summary>
        private readonly AABBFactory _aabbFactory;

        /// <summary>AABB 同士の衝突判定を行うコントローラ</summary>
        private readonly AABBCollisionController _collisionController;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 衝突判定サービスを初期化し、障害物 AABB のキャッシュを作成する
        /// </summary>
        public TankCollisionService(
            Transform tankTransform,
            Vector3 hitboxCenter,
            Vector3 hitboxSize,
            Transform[] obstacles,
            AABBFactory aabbFactory,
            AABBCollisionController collisionController
        )
        {
            // 戦車 Transform の参照を保持する
            _tankTransform = tankTransform;

            // 戦車ローカル中心を保持する
            _hitboxCenter = hitboxCenter;

            // 戦車ローカルサイズを保持する
            _hitboxSize = hitboxSize;

            // 障害物配列を保持する
            _obstacles = obstacles;

            // AABB 生成用のファクトリーを保持する
            _aabbFactory = aabbFactory;

            // 衝突判定コントローラを保持する
            _collisionController = collisionController;

            // 障害物 AABB のキャッシュ配列を初期化する
            _obstacleAABBs = new AABBData[_obstacles.Length];

            // --------------------------------------------------
            // 障害物 AABB を Awake 代わりにキャッシュ生成
            // --------------------------------------------------
            for (int i = 0; i < _obstacles.Length; i++)
            {
                // Transform が null の場合は空 AABB を設定する
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
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 衝突が発生した場合、その障害物の中心座標を返す衝突判定メソッド
        /// </summary>
        /// <param name="hitPosition">衝突した障害物の中心座標</param>
        /// <returns>衝突していれば true</returns>
        public bool TryGetCollision(out Vector3 hitPosition)
        {
            // 衝突無しの場合の初期値を設定する
            hitPosition = Vector3.zero;

            if (_obstacles == null || _obstacles.Length == 0)
            {
                return false;
            }

            // 戦車の AABB を現在の位置・回転から生成する
            AABBData tankAABB = _aabbFactory.CreateAABB(
                _tankTransform,
                _hitboxCenter,
                _hitboxSize
            );

            // 障害物 AABB と戦車 AABB を順に比較して衝突判定する
            for (int i = 0; i < _obstacles.Length; i++)
            {
                if (_obstacles[i] == null)
                {
                    continue;
                }

                // 衝突していれば障害物の AABB 中心を返す
                if (_collisionController.IsColliding(tankAABB, _obstacleAABBs[i]))
                {
                    // 障害物 AABB のワールド座標を返却する
                    hitPosition = _obstacleAABBs[i].Center;
                    return true;
                }
            }

            // 衝突が無ければ false を返す
            return false;
        }
    }
}