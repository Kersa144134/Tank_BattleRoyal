//// ======================================================
//// TankCollisionService.cs
//// 作成者   : 高橋一翔
//// 作成日時 : 2025-12-17
//// 更新日時 : 2025-12-17
//// 概要     : 戦車同士の衝突判定と解決を統括する
//// ======================================================

//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using CollisionSystem.Calculator;
//using CollisionSystem.Data;
//using CollisionSystem.Interface;
//using TankSystem.Data;
//using TankSystem.Utility;

//namespace TankSystem.Service
//{
//    /// <summary>
//    /// 戦車の衝突判定登録・更新・解決を一元管理するクラス
//    /// </summary>
//    public sealed class TankCollisionService
//    {
//        // ======================================================
//        // コンポーネント参照
//        // ======================================================

//        /// <summary>OBB を生成するためのファクトリークラス</summary>
//        private readonly OBBFactory _obbFactory = new OBBFactory();

//        /// <summary>OBB / OBB の距離計算および衝突判定を行う計算器</summary>
//        private readonly BoundingBoxCollisionCalculator _boxCollisionCalculator = new BoundingBoxCollisionCalculator();

//        /// <summary>戦車衝突判定サービス</summary>
//        private readonly TankVersusTankCollisionService _service;

//        // ======================================================
//        // フィールド
//        // ======================================================

//        // --------------------------------------------------
//        // 戦車
//        // --------------------------------------------------
//        /// <summary>戦車の Transform 配列</summary>
//        private readonly Transform[] _tankTransforms;

//        /// <summary>戦車の OBBData 配列</summary>
//        private IOBBData[] _tankOBBs;

//        // --------------------------------------------------
//        // 障害物
//        // --------------------------------------------------
//        /// <summary>障害物の Transform 配列</summary>
//        private readonly Transform[] _obstacles;

//        /// <summary>障害物の OBBData 配列</summary>
//        private readonly IOBBData[] _obstacleOBBs;

//        // --------------------------------------------------
//        // アイテム
//        // --------------------------------------------------
//        /// <summary>アイテムの構造体リスト</summary>
//        private List<ItemSlot> _items;

//        /// <summary>アイテムの OBBData 配列</summary>
//        private IOBBData[] _itemOBBs;

//        // ======================================================
//        // パブリック
//        // ======================================================

//        /// <summary>戦車の OBBData 配列</summary>
//        public IOBBData[] TankOBBs => _tankOBBs;

//        // ======================================================
//        // 辞書
//        // ======================================================

//        /// <summary>戦車IDと衝突エントリーの対応表</summary>
//        private readonly Dictionary<int, TankCollisionEntry> _entries;

//        /// <summary>障害物 Transform からインデックスを引くための対応表</summary>
//        private readonly Dictionary<Transform, int> _obstacleIndexMap;

//        // ======================================================
//        // イベント
//        // ======================================================

//        /// <summary>障害物衝突時</summary>
//        public event Action<Transform> OnObstacleHit;

//        /// <summary>アイテム取得時</summary>
//        public event Action<ItemSlot> OnItemHit;

//        /// <summary>通知した軸方向の移動を制限する</summary>
//        public event Action<MovementLockAxis> OnMovementLockAxisHit;

//        // ======================================================
//        // コンストラクタ
//        // ======================================================

//        /// <summary>
//        /// 衝突判定サービスを初期化し、戦車用パラメータおよび障害物 OBB のキャッシュを作成する
//        /// </summary>
//        /// <param name="tankTransforms">戦車の Transform 配列</param>
//        /// <param name="obstacles">障害物の Transform 配列</param>
//        public TankCollisionService(
//            in Transform[] tankTransforms,
//            in Transform[] obstacles
//        )
//        {
//            _tankTransforms = tankTransforms;
//            _obstacles = obstacles;

//            // サービス生成
//            _service = new TankVersusTankCollisionService();

//            // 辞書生成
//            _entries = new Dictionary<int, TankCollisionEntry>();

//            // --------------------------------------------------
//            // 戦車 OBB 生成
//            // --------------------------------------------------
//            // 登録されている戦車 Transform を順に処理する
//            for (int i = 0; i < _tankTransforms.Length; i++)
//            {
//                // 現在処理中の Transform を取得する
//                Transform tankTransform = _tankTransforms[i];

//                // Transform が未設定の場合は処理しない
//                if (tankTransform == null)
//                {
//                    continue;
//                }

//                // BoxCollider を持たない場合は無効
//                if (!_tankTransforms[i].TryGetComponent(out BoxCollider boxCollider))
//                {
//                    continue;
//                }

//                // BoxCollider 情報を元に動的 OBB を生成する
//                _tankOBBs[i] = _obbFactory.CreateDynamicOBB(
//                    tankTransform,
//                    boxCollider.center,
//                    boxCollider.size
//                );
//            }

//            // 障害物のキャッシュデータ配列を初期化する
//            _obstacleOBBs = new IOBBData[_obstacles.Length];

//            // 障害物インデックス対応表を登録
//            _obstacleIndexMap = new Dictionary<Transform, int>(_obstacles.Length);

//            for (int i = 0; i < _obstacles.Length; i++)
//            {
//                Transform obstacle = _obstacles[i];

//                // null は登録しない
//                if (obstacle == null)
//                {
//                    continue;
//                }

//                _obstacleIndexMap.Add(obstacle, i);
//            }

//            // 障害物 OBB を生成
//            for (int i = 0; i < _obstacles.Length; i++)
//            {
//                // BoxCollider を持たない場合は無効
//                if (!_obstacles[i].TryGetComponent(out BoxCollider boxCollider))
//                {
//                    continue;
//                }

//                _obstacleOBBs[i] = _obbFactory.CreateStaticOBB(
//                    _obstacles[i],
//                    boxCollider.center,
//                    boxCollider.size
//                );
//            }

//            // 衝突イベント購読
//            _service.OnTankVersusTankHit += HandleTankHit;
//        }

//        // ======================================================
//        // セッター
//        // ======================================================

//        /// <summary>
//        /// アイテムリストから OBB 配列を生成する
//        /// </summary>
//        /// <param name="items">OBB を生成する対象のアイテムリスト</param>
//        public void SetItemOBBs(in List<ItemSlot> items)
//        {
//            if (items == null || items.Count == 0)
//            {
//                _itemOBBs = new IOBBData[0];
//                return;
//            }

//            _items = items;

//            // アイテム OBB のキャッシュ配列を初期化する
//            _itemOBBs = new IOBBData[items.Count];

//            // アイテム OBB を生成
//            for (int i = 0; i < items.Count; i++)
//            {
//                // アイテムは Transform 原点を中心とし、Transform のスケールと一致する OBB として扱う
//                _itemOBBs[i] = _obbFactory.CreateStaticOBB(
//                items[i].ItemTransform,
//                    Vector3.zero,
//                    Vector3.one
//                );
//            }
//        }

//        // ======================================================
//        // パブリックメソッド
//        // ======================================================

//        /// <summary>
//        /// 毎フレーム呼び出すことで戦車と障害物／アイテムの衝突をチェックし、
//        /// ヒットした対象に応じてイベントを発火する
//        /// </summary>
//        public void UpdateCollisionChecks()
//        {
//            if (_obstacles == null || _obstacleOBBs == null
//                || _items == null || _itemOBBs == null
//            )
//            {
//                return;
//            }

//            // 登録されている戦車 Transform を順に処理する
//            for (int i = 0; i < _tankOBBs.Length; i++)
//            {
//                // --------------------------------------------------
//                // 戦車 OBB 更新
//                // --------------------------------------------------
//                _tankOBBs[i].Update();

//                // --------------------------------------------------
//                // 障害物衝突チェック
//                // --------------------------------------------------
//                for (int j = 0; j < _obstacles.Length; j++)
//                {
//                    // 無効な障害物は無視
//                    if (_obstacles[j] == null)
//                    {
//                        continue;
//                    }

//                    // 衝突していれば毎フレーム通知
//                    if (_boxCollisionCalculator.IsCollidingHorizontal(
//                            _tankOBBs[i],
//                            _obstacleOBBs[j]))
//                    {
//                        OnObstacleHit?.Invoke(_obstacles[j]);
//                    }
//                }

//                // --------------------------------------------------
//                // アイテムチェック
//                // --------------------------------------------------
//                for (int j = 0; i < _items.Count; j++)
//                {
//                    if (!_items[j].IsEnabled || _items[j].ItemTransform == null)
//                    {
//                        continue;
//                    }

//                    if (_boxCollisionCalculator.IsCollidingHorizontal(_tankOBBs[i], _itemOBBs[j]))
//                    {
//                        OnItemHit?.Invoke(_items[j]);
//                    }
//                }
//            }

//            // --------------------------------------------------
//            // 戦車チェック
//            // --------------------------------------------------
//            _service.UpdateCollisionChecks();

//        }

//        /// <summary>
//        /// 指定した障害物インデックスに対応する戦車 OBB との侵入量を計算し、
//        /// 押し戻しに必要な最小移動量（MTV）を返す
//        /// </summary>
//        /// <param name="obstacle">障害物の Transform</param>
//        /// <returns>有効な衝突が存在する場合は true、存在しなければ false</returns>
//        public CollisionResolveInfo CalculateObstacleResolveInfo(in Transform obstacle)
//        {
//            // Transform から障害物インデックスを取得できなければ無効扱い
//            if (!TryGetObstacleIndex(obstacle, out int obstacleIndex))
//            {
//                return default;
//            }

//            // インデックス指定版の衝突解消計算に委譲
//            return CalculateObstacleResolveInfo(obstacleIndex);
//        }

//        // ======================================================
//        // プライベートメソッド
//        // ======================================================

//        /// <summary>
//        /// 指定した障害物 Transform から配列インデックスを取得する。
//        /// 存在しない場合は false を返し、out パラメータには -1 を設定する。
//        /// </summary>
//        /// <param name="obstacle">検索対象の障害物 Transform</param>
//        /// <param name="obstacleIndex">見つかった場合は障害物の配列インデックスを格納</param>
//        /// <returns>障害物が存在すれば true、存在しなければ false</returns>
//        private bool TryGetObstacleIndex(
//            in Transform obstacle,
//            out int obstacleIndex
//        )
//        {
//            if (obstacle == null)
//            {
//                obstacleIndex = -1;
//                return false;
//            }

//            return _obstacleIndexMap.TryGetValue(obstacle, out obstacleIndex);
//        }

//        /// <summary>
//        /// 指定した障害物インデックスに対応する戦車 OBB との侵入量を計算し、
//        /// 押し戻しに必要な最小移動量（MTV）を返す
//        /// </summary>
//        /// <param name="obstacleIndex">障害物の配列インデックス</param>
//        /// <returns>衝突解消情報を格納した CollisionResolveInfo</returns>
//        private CollisionResolveInfo CalculateObstacleResolveInfo(in int obstacleIndex)
//        {
//            //// 登録されている戦車 Transform を順に処理する
//            //for (int i = 0; i < _tankOBBs.Length; i++)
//            //{
//            //    // --------------------------------------------------
//            //    // 戦車 OBB 更新
//            //    // --------------------------------------------------
//            //    _tankOBBs[i].Update();
//            //}

//            //// MTV 算出
//            //if (!_boxCollisionCalculator.TryCalculateHorizontalMTV(
//            //    _tankOBB,
//            //    _obstacleOBBs[obstacleIndex],
//            //    out Vector3 resolveAxis,
//            //    out float resolveDistance
//            //))
//            //{
//            //    return default;
//            //}

//            //// 押し戻し方向補正
//            //Vector3 centerDelta =
//            //    _tankOBB.Center - _obstacleOBBs[obstacleIndex].Center;

//            //centerDelta.y = 0f;

//            //if (Vector3.Dot(resolveAxis, centerDelta) < 0f)
//            //{
//            //    resolveAxis = -resolveAxis;
//            //}

//            //// --------------------------------------------------
//            //// 移動ロック軸判定
//            //// --------------------------------------------------
//            //MovementLockAxis lockAxis = MovementLockAxis.None;

//            //// X 成分が支配的なら X 軸をロック
//            //if (Mathf.Abs(resolveAxis.x) > Mathf.Abs(resolveAxis.z))
//            //{
//            //    lockAxis |= MovementLockAxis.X;
//            //}
//            //// Z 成分が支配的なら Z 軸をロック
//            //else
//            //{
//            //    lockAxis |= MovementLockAxis.Z;
//            //}

//            //OnMovementLockAxisHit?.Invoke(lockAxis);

//            //return new CollisionResolveInfo
//            //{
//            //    ResolveDirection = resolveAxis,
//            //    ResolveDistance = resolveDistance,
//            //    IsValid = true
//            //};

//            return default;
//        }
        
//        // ======================================================
//        // プライベートメソッド
//        // ======================================================

//        /// <summary>
//        /// 戦車同士が接触した際の解決処理
//        /// </summary>
//        /// <param name="tankIdA">戦車AのID</param>
//        /// <param name="tankIdB">戦車BのID</param>
//        private void HandleTankHit(int tankIdA, int tankIdB)
//        {
//            // ID からエントリを取得
//            if (!_entries.TryGetValue(tankIdA, out TankCollisionEntry entryA) ||
//                !_entries.TryGetValue(tankIdB, out TankCollisionEntry entryB))
//            {
//                return;
//            }

//            // 各戦車の前進量を取得
//            float deltaA = entryA.TankRootManager.DeltaForward;
//            float deltaB = entryB.TankRootManager.DeltaForward;

//            // 押し戻し計算処理を実行し、各戦車の押し戻し情報を out で受け取る
//            CollisionResolveInfo resolveA;
//            CollisionResolveInfo resolveB;
//            _service.CalculateTankVersusTankResolveInfo(
//                entryA,
//                entryB,
//                deltaA,
//                deltaB,
//                out resolveA,
//                out resolveB
//            );

//            // 押し戻し処理を実行
//            entryA.TankRootManager.ApplyCollisionResolve(resolveA);
//            entryB.TankRootManager.ApplyCollisionResolve(resolveB);
//        }
//    }
//}