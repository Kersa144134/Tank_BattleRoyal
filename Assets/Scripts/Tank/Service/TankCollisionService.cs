// ======================================================
// TankCollisionService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-10
// 更新日時 : 2025-12-15
// 概要     : 戦車と障害物の OBB 衝突判定を専任で担当するサービスクラス
//            障害物 OBB をキャッシュし、戦車の OBB を動的生成して判定を行う
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using CollisionSystem.Calculator;
using CollisionSystem.Data;
using CollisionSystem.Interface;
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
        // コンポーネント参照
        // ======================================================

        /// <summary>OBB を生成するためのファクトリークラス</summary>
        private readonly OBBFactory _obbFactory;

        /// <summary>OBB / OBB の距離計算および衝突判定を行うコントローラー</summary>
        private readonly BoundingBoxCollisionCalculator _boxCollisionCalculator;

        // ======================================================
        // フィールド
        // ======================================================

        // --------------------------------------------------
        // 戦車
        // --------------------------------------------------
        /// <summary>戦車本体の Transform</summary>
        private readonly Transform _tankTransform;

        /// <summary>戦車本体の BoxCollider </summary>
        private readonly BoxCollider _tankCollider;

        /// <summary>戦車の OBBData</summary>
        private IOBBData _tankOBB;

        // --------------------------------------------------
        // 障害物
        // --------------------------------------------------
        /// <summary>障害物の Transform 配列</summary>
        private readonly Transform[] _obstacles;

        /// <summary>障害物の BoxCollider 配列</summary>
        private readonly BoxCollider[] _obstacleColliders;

        /// <summary>障害物の OBBData 配列</summary>
        private readonly IOBBData[] _obstacleOBBs;

        // --------------------------------------------------
        // アイテム
        // --------------------------------------------------
        /// <summary>アイテムの構造体リスト</summary>
        private List<ItemSlot> _items;

        /// <summary>アイテムの OBBData 配列</summary>
        private IOBBData[] _itemOBBs;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>障害物 Transform からインデックスを引くための対応表</summary>
        private readonly Dictionary<Transform, int> _obstacleIndexMap;
        
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
        /// 衝突判定サービスを初期化し、戦車用パラメータおよび障害物 OBB のキャッシュを作成する
        /// </summary>
        /// <param name="obbFactory">OBB を生成するファクトリークラス</param>
        /// <param name="boxCollisionCalculator">OBB 衝突判定と MTV 算出を行う計算クラス</param>
        /// <param name="tankTransform">戦車本体の Transform</param>
        /// <param name="tankCollider">戦車本体の Collider</param>
        /// <param name="obstacles">障害物の Transform 配列</param>
        public TankCollisionService(
            in OBBFactory obbFactory,
            in BoundingBoxCollisionCalculator boxCollisionCalculator,
            in Transform tankTransform,
            in BoxCollider tankCollider,
            in Transform[] obstacles
        )
        {
            _obbFactory = obbFactory;
            _boxCollisionCalculator = boxCollisionCalculator;
            _tankTransform = tankTransform;
            _tankCollider = tankCollider;
            _obstacles = obstacles;

            // 戦車 OBB を生成
            _tankOBB = _obbFactory.CreateDynamicOBB(
                _tankTransform,
                _tankCollider.center,
                _tankCollider.size
            );

            // 障害物のキャッシュデータ配列を初期化する
            _obstacleColliders = new BoxCollider[_obstacles.Length];
            _obstacleOBBs = new IOBBData[_obstacles.Length];

            // 障害物インデックス対応表を登録
            _obstacleIndexMap = new Dictionary<Transform, int>(_obstacles.Length);

            for (int i = 0; i < _obstacles.Length; i++)
            {
                Transform obstacle = _obstacles[i];

                // null は登録しない
                if (obstacle == null)
                {
                    continue;
                }

                _obstacleIndexMap.Add(obstacle, i);
            }
            
            // 障害物 OBB を生成
            for (int i = 0; i < _obstacles.Length; i++)
            {
                // BoxCollider を持たない場合は無効
                if (!_obstacles[i].TryGetComponent(out BoxCollider boxCollider))
                {
                    continue;
                }

                _obstacleColliders[i] = boxCollider;

                _obstacleOBBs[i] = _obbFactory.CreateStaticOBB(
                    _obstacles[i],
                    boxCollider.center,
                    boxCollider.size
                );
            }
        }

        // ======================================================
        // セッター
        // ======================================================

        /// <summary>
        /// アイテムリストから OBB 配列を生成する
        /// </summary>
        /// <param name="items">OBB を生成する対象のアイテムリスト</param>
        public void SetItemOBBs(in List<ItemSlot> items)
        {
            if (items == null || items.Count == 0)
            {
                _itemOBBs = new IOBBData[0];
                return;
            }

            _items = items;

            // アイテム OBB のキャッシュ配列を初期化する
            _itemOBBs = new IOBBData[items.Count];

            // アイテム OBB を生成
            for (int i = 0; i < items.Count; i++)
            {
                // アイテムは Transform 原点を中心とし、Transform のスケールと一致する OBB として扱う
                _itemOBBs[i] = _obbFactory.CreateStaticOBB(
                items[i].ItemTransform,
                    Vector3.zero,
                    Vector3.one
                );
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
            if (_obstacles == null || _obstacleOBBs == null
                || _items == null || _itemOBBs == null
            )
            {
                return;
            }

            // --------------------------------------------------
            // 戦車 OBB 更新
            // --------------------------------------------------

            _tankOBB.Update();

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
                if (_boxCollisionCalculator.IsCollidingHorizontal(
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

                if (_boxCollisionCalculator.IsCollidingHorizontal(_tankOBB, _itemOBBs[i]))
                {
                    OnItemHit?.Invoke(_items[i]);
                }
            }
        }

        /// <summary>
        /// 指定した障害物インデックスに対応する戦車 OBB との侵入量を計算し、
        /// 押し戻しに必要な最小移動量（MTV）を返す
        /// </summary>
        /// <param name="obstacle">障害物の Transform</param>
        /// <returns>有効な衝突が存在する場合は true、存在しなければ false</returns>
        public CollisionResolveInfo CalculateObstacleResolveInfo(in Transform obstacle)
        {
            // Transform から障害物インデックスを取得できなければ無効扱い
            if (!TryGetObstacleIndex(obstacle, out int obstacleIndex))
            {
                return default;
            }

            // インデックス指定版の衝突解消計算に委譲
            return CalculateObstacleResolveInfo(obstacleIndex);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 指定した障害物 Transform から配列インデックスを取得する。
        /// 存在しない場合は false を返し、out パラメータには -1 を設定する。
        /// </summary>
        /// <param name="obstacle">検索対象の障害物 Transform</param>
        /// <param name="obstacleIndex">見つかった場合は障害物の配列インデックスを格納</param>
        /// <returns>障害物が存在すれば true、存在しなければ false</returns>
        private bool TryGetObstacleIndex(
            in Transform obstacle,
            out int obstacleIndex
        )
        {
            if (obstacle == null)
            {
                obstacleIndex = -1;
                return false;
            }

            return _obstacleIndexMap.TryGetValue(obstacle, out obstacleIndex);
        }

        /// <summary>
        /// 指定した障害物インデックスに対応する戦車 OBB との侵入量を計算し、
        /// 押し戻しに必要な最小移動量（MTV）を返す
        /// </summary>
        /// <param name="obstacleIndex">障害物の配列インデックス</param>
        /// <returns>衝突解消情報を格納した CollisionResolveInfo</returns>
        private CollisionResolveInfo CalculateObstacleResolveInfo(in int obstacleIndex)
        {
            BoxCollider boxCollider = _obstacleColliders[obstacleIndex];

            if (boxCollider == null)
            {
                return default;
            }

            // --------------------------------------------------
            // 戦車 OBB 更新
            // --------------------------------------------------

            _tankOBB.Update();

            // --------------------------------------------------
            // MTV 算出（SAT）
            // --------------------------------------------------

            if (!_boxCollisionCalculator.TryCalculateHorizontalMTV(
                _tankOBB,
                _obstacleOBBs[obstacleIndex],
                out Vector3 resolveAxis,
                out float resolveDistance
            ))
            {
                return default;
            }

            // --------------------------------------------------
            // 押し戻し方向補正
            // --------------------------------------------------

            Vector3 centerDelta = _tankOBB.Center - _obstacleOBBs[obstacleIndex].Center;
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
    }
}