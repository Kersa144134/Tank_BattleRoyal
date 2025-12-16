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
using TankSystem.Data;
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

        /// <summary>キャタピラ入力モード</summary>
        private readonly TrackInputMode _inputMode;

        /// <summary>戦車本体の Transform</summary>
        private readonly Transform _tankTransform;

        /// <summary>現在の機動倍率（前進・旋回に適用）</summary>
        private float _mobilityMultiplier = BASE_MOBILITY_MULTIPLIER;

        /// <summary>現在の前進加減速倍率</summary>
        private float _forwardAccelerationMultiplier = BASE_FORWARD_ACCELERATION_MULTIPLIER;

        /// <summary>現在の旋回加減速倍率</summary>
        private float _turnAccelerationMultiplier = BASE_TURN_ACCELERATION_MULTIPLIER;

        /// <summary>現在フレームでの前進量</summary>
        private float _currentForward;

        /// <summary>現在フレームでの旋回量</summary>
        private float _currentTurn;

        // ======================================================
        // 定数
        // ======================================================

        // --------------------------------------------------
        // 基準値
        // --------------------------------------------------
        /// <summary>基準となる機動倍率</summary>
        private const float BASE_MOBILITY_MULTIPLIER = 15.0f;

        /// <summary>基準となる前進加減速倍率</summary>
        private const float BASE_FORWARD_ACCELERATION_MULTIPLIER = 7.5f;

        /// <summary>基準となる旋回加減速倍率</summary>
        private const float BASE_TURN_ACCELERATION_MULTIPLIER = 120.0f;

        // --------------------------------------------------
        // パラメーター
        // --------------------------------------------------
        /// <summary>馬力 1 あたりの機動倍率加算値</summary>
        private const float HORSEPOWER_MULTIPLIER = 1.75f;

        /// <summary>変速 1 あたりの前進倍率加算値</summary>
        private const float TRANSMISSION_FORWARD_ACCELERATION_MULTIPLIER = 24.625f;

        /// <summary>変速 1 あたりの旋回倍率加算値</summary>
        private const float TRANSMISSION_TURN_ACCELERATION_MULTIPLIER = 394.0f;

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
            in TrackInputMode inputMode,
            in Transform transform,
            in Vector3 hitboxCenter,
            in Vector3 hitboxSize,
            in Transform[] obstacles
        )
        {
            _trackController = trackController;
            _collisionService = collisionService;
            _boundaryService = boundaryService;
            _inputMode = inputMode;

            // 操作対象の戦車 Transform を保持する
            _tankTransform = transform;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 前進・旋回処理を適用し、移動後に境界制御を行う
        /// </summary>
        /// <param name="tankStatus">戦車のステータス</param>
        /// <param name="left">左キャタピラ入力値（-1～1）</param>
        /// <param name="right">右キャタピラ入力値（-1～1）</param>
        public void ApplyMobility(in TankStatus tankStatus, in Vector2 left, in Vector2 right)
        {
            // 機動力関連の倍率を更新
            UpdateMobilityParameters(tankStatus);

            // 入力から目標移動量を算出
            CalculateTargetMovement(left, right, out float targetForward, out float targetTurn);

            // 加減速を適用して現在値を更新
            ApplyAcceleration(targetForward, targetTurn);

            // 前進を適用
            _tankTransform.Translate(
                Vector3.forward * _currentForward * Time.deltaTime,
                Space.Self
            );

            // 旋回を適用
            _tankTransform.Rotate(
                0f,
                _currentTurn * Time.deltaTime,
                0f,
                Space.Self
            );

            // 移動範囲を制限
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

        /// <summary>
        /// キャタピラの入力モードを切り替える
        /// </summary>
        public void ChangeInputMode()
        {
            _trackController.ChangeInputMode();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// HorsePower・Transmission を元に
        /// 最高速倍率と前進・旋回の加減速倍率を算出する
        /// </summary>
        private void UpdateMobilityParameters(in TankStatus tankStatus)
        {
            // 最高速度・旋回速度に影響する機動倍率を算出
            _mobilityMultiplier =
                BASE_MOBILITY_MULTIPLIER
                + tankStatus.HorsePower * HORSEPOWER_MULTIPLIER;

            // 前進加減速性能に影響する倍率を算出
            _forwardAccelerationMultiplier =
                BASE_FORWARD_ACCELERATION_MULTIPLIER
                + tankStatus.Transmission * TRANSMISSION_FORWARD_ACCELERATION_MULTIPLIER;

            // 旋回加減速性能に影響する倍率を算出
            _turnAccelerationMultiplier =
                BASE_TURN_ACCELERATION_MULTIPLIER
                + tankStatus.Transmission * TRANSMISSION_TURN_ACCELERATION_MULTIPLIER;
        }

        /// <summary>
        /// キャタピラ入力から前進・旋回の目標値を算出する
        /// </summary>
        private void CalculateTargetMovement(
            in Vector2 left,
            in Vector2 right,
            out float targetForward,
            out float targetTurn)
        {
            // キャタピラ入力を前進量・旋回量に変換
            _trackController.UpdateTrack(_inputMode, left, right, out float forwardInput, out float turnInput);

            // 機動力倍率を掛けて最終的な目標値を算出
            targetForward = forwardInput * _mobilityMultiplier;
            targetTurn = turnInput * _mobilityMultiplier;
        }

        /// <summary>
        /// 目標移動量に向かって現在値を加減速させる
        /// </summary>
        private void ApplyAcceleration(in float targetForward, in float targetTurn)
        {
            // 前進用：1フレームあたりの最大変化量を算出
            float forwardMaxDelta =
                _forwardAccelerationMultiplier * Time.deltaTime;

            // 旋回用：1フレームあたりの最大変化量を算出
            float turnMaxDelta =
                _turnAccelerationMultiplier * Time.deltaTime;

            // 前進量を加減速
            _currentForward = Mathf.MoveTowards(
                _currentForward,
                targetForward,
                forwardMaxDelta
            );

            // 旋回量を加減速
            _currentTurn = Mathf.MoveTowards(
                _currentTurn,
                targetTurn,
                turnMaxDelta
            );
        }
    }
}