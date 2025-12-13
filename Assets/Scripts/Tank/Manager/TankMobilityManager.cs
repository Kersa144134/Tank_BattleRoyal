// ======================================================
// TankMobilityManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-05
// 更新日時 : 2025-12-13
// 概要     : 戦車の前進・旋回処理を担当する機動力管理クラス。
//            TrackController による前進量・旋回量を Transform に反映し、
//            TankCollisionService により移動後の衝突判定を行う。
// ======================================================

using UnityEngine;
using CollisionSystem.Data;
using TankSystem.Controller;
using TankSystem.Service;

namespace TankSystem.Manager
{
    /// <summary>
    /// 戦車の移動・旋回処理を専任で受け持つマネージャ
    /// </summary>
    public class TankMobilityManager
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>左右キャタピラ入力から前進量・旋回量を算出するコントローラ</summary>
        private readonly TankTrackController _trackController;

        /// <summary>衝突判定サービス</summary>
        private readonly TankCollisionService _collisionService;

        /// <summary>戦車移動範囲制限サービス</summary>
        private readonly TankMovementBoundaryService _boundaryService;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>戦車本体の Transform</summary>
        private readonly Transform _tankTransform;

        /// <summary>戦車本体の当たり判定中心位置</summary>
        private readonly Vector3 _hitboxCenter;

        /// <summary>戦車本体の当たり判定スケール</summary>
        private readonly Vector3 _hitboxSize;

        /// <summary>現在の機動力倍率（前進・旋回に適用）</summary>
        private float _mobilityMultiplier = BASE_MOBILITY;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>基準機動力倍率</summary>
        private const float BASE_MOBILITY = 5.0f;

        /// <summary>馬力1あたりの倍率加算値</summary>
        private const float HORSEPOWER_MULTIPLIER = 1.5f;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 移動処理および衝突判定に必要な外部参照を受け取って初期化する
        /// </summary>
        public TankMobilityManager(
            in TankTrackController trackController,
            in TankCollisionService collisionService,
            in TankMovementBoundaryService boundaryService,
            in Transform transform,
            in Vector3 hitboxCenter,
            in Vector3 hitboxSize,
            in Transform[] obstacles
        )
        {
            _trackController = trackController;
            _collisionService = collisionService;
            _boundaryService = boundaryService;

            // 操作対象の戦車 Transform を保持する
            _tankTransform = transform;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 前進・旋回処理を適用し、移動後に衝突判定を行う
        /// </summary>
        /// <param name="horsePower">戦車の馬力パラメーター</param>
        /// <param name="left">左キャタピラ入力値（-1～1）</param>
        /// <param name="right">右キャタピラ入力値（-1～1）</param>
        public void ApplyMobility(in int horsePower, in float left, in float right)
        {
            // 馬力に応じた倍率を更新
            UpdateMobilityMultiplier(horsePower);

            // キャタピラ入力から前進量と旋回量を取得
            _trackController.UpdateTrack(left, right, out float forward, out float turn);

            // 前進量・旋回量に倍率を掛けて適用
            _tankTransform.Translate(
                Vector3.forward * forward * _mobilityMultiplier * Time.deltaTime,
                Space.Self
            );

            _tankTransform.Rotate(
                0f,
                turn * _mobilityMultiplier * Time.deltaTime,
                0f,
                Space.Self
            );

            // 移動範囲チェック
            _boundaryService.ClampPosition(_tankTransform);
        }

        /// <summary>
        /// TankCollisionService からの衝突通知を受けて、
        /// 戦車と障害物のめり込みを解消する
        /// </summary>
        public void CheckObstaclesCollision(in Transform obstacle)
        {
            // 侵入量を計算
            CollisionResolveInfo resolveInfo =
                _collisionService.CalculateObstacleResolveInfo(obstacle);

            // 有効でなければ何もしない
            if (!resolveInfo.IsValid)
            {
                return;
            }

            // 押し戻し適用
            const float COLLISION_EPSILON = 0.001f;

            _tankTransform.position +=
                resolveInfo.ResolveDirection *
                (resolveInfo.ResolveDistance + COLLISION_EPSILON);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 現在の馬力パラメーターに応じて機動力倍率を計算し更新する
        /// </summary>
        /// <param name="horsePower">戦車の馬力</param>
        private void UpdateMobilityMultiplier(in int horsePower)
        {
            // 馬力を反映した倍率計算
            _mobilityMultiplier = BASE_MOBILITY + horsePower * HORSEPOWER_MULTIPLIER;

            // 最大値を50に制限
            if (_mobilityMultiplier > 50f)
            {
                _mobilityMultiplier = 50f;
            }
        }
    }
}