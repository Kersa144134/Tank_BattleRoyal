// ======================================================
// TankCollisionService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-10
// 更新日時 : 2025-12-13
// 概要     : 戦車と障害物の OBB 衝突判定を専任で担当するサービスクラス
//            障害物 OBB をキャッシュし、戦車の OBB を動的生成して判定を行う
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
    /// 戦車の OBB と障害物 OBB の衝突判定を行うサービスクラス
    /// </summary>
    public class TankCollisionService
    {
        // ======================================================
        // 構造体
        // ======================================================

        /// <summary>
        /// OBB 衝突解決に必要な最小移動量情報
        /// </summary>
        public struct CollisionResolveInfo
        {
            /// <summary>押し戻し方向</summary>
            public Vector3 ResolveDirection;

            /// <summary>押し戻し距離</summary>
            public float ResolveDistance;

            /// <summary>有効な解決情報か</summary>
            public bool IsValid;
        }
        
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>OBB を生成するためのファクトリークラス</summary>
        private readonly OBBFactory _obbFactory;

        /// <summary>OBB / OBB の距離計算および衝突判定を行うコントローラー</summary>
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

        /// <summary>障害物の OBB をキャッシュして保持する構造体配列</summary>
        private readonly OBBData[] _obstacleOBBs;

        /// <summary>アイテムの OBB をキャッシュして保持する構造体配列</summary>
        private OBBData[] _itemOBBs;

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
        /// 衝突判定サービスを初期化し、障害物 OBB のキャッシュを作成する
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

            // 障害物 OBB のキャッシュ配列を初期化する
            _obstacleOBBs = new OBBData[_obstacles.Length];

            // 障害物 OBB を生成
            for (int i = 0; i < _obstacles.Length; i++)
            {
                _obstacleOBBs[i] = CreateOBBFromTransform(_obstacles[i]);
            }
        }

        // ======================================================
        // セッター
        // ======================================================

        /// <summary>
        /// アイテム OBB 配列を生成する
        /// </summary>
        public void SetItemOBBs(in List<ItemSlot> items)
        {
            if (items == null || items.Count == 0)
            {
                _itemOBBs = new OBBData[0];
                return;
            }

            _items = items;

            // アイテム OBB のキャッシュ配列を初期化する
            _itemOBBs = new OBBData[items.Count];

            // アイテム OBB を生成
            for (int i = 0; i < items.Count; i++)
            {
                _itemOBBs[i] = CreateOBBFromTransform(items[i].ItemTransform);
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

            // --------------------------------------------------
            // 障害物衝突チェック
            // --------------------------------------------------
            for (int i = 0; i < _obstacles.Length; i++)
            {
                // 無効な障害物は無視
                if (_obstacles[i] == null)
                {
                    continue;
                }

                // 衝突していれば毎フレーム通知
                if (_boxCollisionController.IsColliding(
                        _tankOBB,
                        _obstacleOBBs[i]))
                {
                    OnObstacleHit?.Invoke(_obstacles[i]);
                }
            }

            // --------------------------------------------------
            // アイテムチェック
            // --------------------------------------------------
            for (int i = 0; i < _items.Count; i++)
            {
                if (!_items[i].IsEnabled || _items[i].ItemTransform == null)
                {
                    continue;
                }

                if (_boxCollisionController.IsColliding(_tankOBB, _itemOBBs[i]))
                {
                    OnItemHit?.Invoke(_items[i]);
                }
            }
        }

        /// <summary>
        /// 戦車 OBB と指定した障害物 OBB の侵入量を計算し、
        /// 解消に必要な最小移動量を返す
        /// </summary>
        public CollisionResolveInfo CalculateObstacleResolveInfo(in Transform obstacle)
        {
            // null 安全チェック
            if (obstacle == null)
            {
                return default;
            }

            // BoxCollider を持たない場合は無効
            if (!obstacle.TryGetComponent(out BoxCollider boxCollider))
            {
                return default;
            }

            // --------------------------------------------------
            // OBB 再生成（戦車）
            // --------------------------------------------------

            OBBData tankOBB = _obbFactory.CreateOBB(
                _tankTransform,
                _hitboxCenter,
                _hitboxSize
            );

            // --------------------------------------------------
            // OBB 生成（障害物）
            // --------------------------------------------------

            OBBData obstacleOBB = CreateOBBFromTransform(obstacle);

            // --------------------------------------------------
            // MTV 算出（SAT）
            // --------------------------------------------------

            if (!_boxCollisionController.TryCalculateMTV(
                tankOBB,
                obstacleOBB,
                out Vector3 resolveAxis,
                out float resolveDistance
            ))
            {
                return default;
            }

            // --------------------------------------------------
            // 押し戻し方向補正
            // --------------------------------------------------

            Vector3 centerDelta = tankOBB.Center - obstacleOBB.Center;
            centerDelta.y = 0f;

            if (Vector3.Dot(resolveAxis, centerDelta) < 0f)
            {
                resolveAxis = -resolveAxis;
            }

            return new CollisionResolveInfo
            {
                ResolveDirection = resolveAxis,
                ResolveDistance = resolveDistance,
                IsValid = true
            };
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// Transform から中心・半サイズ・回転を取得して OBBData を生成する
        /// null の場合は空 OBB を返す
        /// </summary>
        /// <param name="tf">対象 Transform</param>
        /// <returns>生成された OBBData</returns>
        private OBBData CreateOBBFromTransform(Transform tf)
        {
            if (tf == null)
            {
                return new OBBData(Vector3.zero, Vector3.zero, Quaternion.identity);
            }

            // 中心座標を取得
            Vector3 center = tf.position;

            // 半サイズ（lossyScaleの半分）
            Vector3 half = tf.lossyScale * 0.5f;

            // 回転（ワールド回転）
            Quaternion rotation = tf.rotation;

            return new OBBData(center, half, rotation);
        }

        /// <summary>
        /// 現在生成されている戦車 OBB が、
        /// 指定したインデックスの障害物 OBB と衝突しているか判定する
        /// </summary>
        /// <param name="obstacleIndex">障害物インデックス</param>
        /// <returns>衝突していれば true</returns>
        private bool IsCollidingWithObstacleAtIndex(in int obstacleIndex)
        {
            // インデックス範囲外は衝突なし
            if (obstacleIndex < 0 || obstacleIndex >= _obstacleOBBs.Length)
            {
                return false;
            }

            // 対象障害物が無効なら衝突なし
            if (_obstacles[obstacleIndex] == null)
            {
                return false;
            }

            // OBB 同士の衝突判定
            return _boxCollisionController.IsColliding(
                _tankOBB,
                _obstacleOBBs[obstacleIndex]
            );
        }
    }
}