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

        /// <summary>現在フレームでの正方向の前進量</summary>
        private float _currentForwardPositive;

        /// <summary>現在フレームでの負方向の前進量</summary>
        private float _currentForwardNegative;

        /// <summary>現在フレームでの旋回量</summary>
        private float _currentTurn;

        /// <summary>現在フレームでの正方向の旋回量</summary>
        private float _currentTurnPositive;

        /// <summary>現在フレームでの負方向の旋回量</summary>
        private float _currentTurnNegative;

        /// <summary>前フレームの前進量を保持し、移動量計算に使用する</summary>
        private float _previousForward;

        /// <summary>移動予定ワールド座標</summary>
        private Vector3 _nextPosition;

        /// <summary>移動予定ワールド回転</summary>
        private Quaternion _nextRotation;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>前フレームからの移動量</summary>
        public float DeltaForward => _currentForward - _previousForward;
        
        // ======================================================
        // 定数
        // ======================================================

        // --------------------------------------------------
        // 基準値
        // --------------------------------------------------
        /// <summary>基準となる機動倍率</summary>
        private const float BASE_MOBILITY_MULTIPLIER = 15.0f;

        /// <summary>基準となる前進加減速倍率</summary>
        private const float BASE_FORWARD_ACCELERATION_MULTIPLIER = 5.0f;

        /// <summary>基準となる旋回加減速倍率</summary>
        private const float BASE_TURN_ACCELERATION_MULTIPLIER = 80.0f;

        // --------------------------------------------------
        // パラメーター
        // --------------------------------------------------
        /// <summary>馬力 1 あたりの機動倍率加算値</summary>
        private const float HORSEPOWER_MULTIPLIER = 1.75f;

        /// <summary>変速 1 あたりの前進倍率加算値</summary>
        private const float TRANSMISSION_FORWARD_ACCELERATION_MULTIPLIER = 4.75f;

        /// <summary>変速 1 あたりの旋回倍率加算値</summary>
        private const float TRANSMISSION_TURN_ACCELERATION_MULTIPLIER = 76.0f;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 移動処理および衝突判定に必要な外部参照を受け取って初期化する
        /// </summary>
        /// <param name="trackController">キャタピラ入力を管理し、前進量・旋回量を算出するコントローラ</param>
        /// <param name="boundaryService">戦車の移動可能範囲を制御する境界判定サービス</param>
        /// <param name="transform">操作対象となる戦車本体の Transform</param>
        public TankMobilityManager(
            in TankTrackController trackController,
            in TankMovementBoundaryService boundaryService,
            in Transform transform
        )
        {
            _trackController = trackController;
            _boundaryService = boundaryService;

            // 操作対象の戦車 Transform を保持する
            _tankTransform = transform;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 前進・旋回処理を適用した場合の移動結果を計算し、
        /// 実際には Transform を変更せず、予定位置と回転を返す
        /// </summary>
        /// <param name="tankStatus">戦車のステータス</param>
        /// <param name="leftInput">左キャタピラ入力値（-1～1）</param>
        /// <param name="rightInput">右キャタピラ入力値（-1～1）</param>
        /// <param name="nextPosition">次フレームでの予定ワールド座標</param>
        /// <param name="nextRotation">次フレームでの予定回転</param>
        public void CalculateMobility(
            in TankStatus tankStatus,
            in Vector2 leftInput,
            in Vector2 rightInput,
            out Vector3 nextPosition,
            out Quaternion nextRotation
        )
        {
            // --------------------------------------------------
            // 機動力パラメータ更新
            // --------------------------------------------------
            UpdateMobilityParameters(tankStatus);

            // --------------------------------------------------
            // 目標移動量を算出
            // --------------------------------------------------
            CalculateTargetMovement(
                leftInput,
                rightInput,
                out float targetForward,
                out float targetTurn
            );

            // --------------------------------------------------
            // 加減速処理
            // --------------------------------------------------
            // 前フレーム値を元に、現在の前進・旋回速度を更新する
            ApplyAcceleration(targetForward, targetTurn);

            // --------------------------------------------------
            // 前進予定位置計算
            // --------------------------------------------------
            // 現在の Transform 位置を基準にする
            Vector3 basePosition = _tankTransform.position;

            // 現在の Transform 回転を基準にする
            Quaternion baseRotation = _tankTransform.rotation;

            // 前進方向（ローカル forward）をワールド方向に変換する
            Vector3 forwardDirection = baseRotation * Vector3.forward;

            // 今フレームで進む予定距離を算出する
            float forwardDistance = _currentForward * Time.deltaTime;

            // 前進後の予定位置を計算する
            Vector3 movedPosition = basePosition + forwardDirection * forwardDistance;

            // --------------------------------------------------
            // 旋回予定回転計算
            // --------------------------------------------------
            // 今フレームで回転する Y 軸角度を算出する
            float turnAngle = _currentTurn * Time.deltaTime;

            // Y 軸回転のみを表すクォータニオンを生成する
            Quaternion turnRotation = Quaternion.Euler(0f, turnAngle, 0f);

            // 基準回転に旋回分を合成する
            Quaternion rotatedRotation = baseRotation * turnRotation;

            // --------------------------------------------------
            // 境界制御
            // --------------------------------------------------
            // 境界制御後の位置を受け取る
            Vector3 clampedPosition;

            // 予定位置に対して移動範囲制限を適用する
            _boundaryService.ClampPlannedPosition(
                movedPosition,
                out clampedPosition
            );

            // --------------------------------------------------
            // 出力
            // --------------------------------------------------
            nextPosition = clampedPosition;
            nextRotation = rotatedRotation;

            // --------------------------------------------------
            // キャッシュ
            // --------------------------------------------------
            _nextPosition = nextPosition;
            _nextRotation = nextRotation;
        }

        /// <summary>
        /// 計算済みの予定座標・回転を Transform に反映する
        /// </summary>
        /// <param name="nextPosition">反映する予定ワールド座標</param>
        /// <param name="nextRotation">反映する予定回転</param>
        public void ApplyMobility(
            in Vector3 nextPosition,
            in Quaternion nextRotation,
            bool collisionResolve
        )
        {
            // --------------------------------------------------
            // 座標反映
            // --------------------------------------------------
            _tankTransform.position = _nextPosition;

            // --------------------------------------------------
            // 回転反映
            // --------------------------------------------------
            _tankTransform.rotation = nextRotation;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// HorsePower・Transmission を元に
        /// 最高速倍率と前進・旋回の加減速倍率を算出する
        /// </summary>
        /// <param name="tankStatus">機動力算出に使用する戦車ステータス情報</param>
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
        /// <param name="leftInput">左キャタピラ入力値</param>
        /// <param name="rightInput">右キャタピラ入力値</param>
        /// <param name="targetForward">算出された前進・後退の目標値</param>
        /// <param name="targetTurn">算出された旋回の目標値</param>
        private void CalculateTargetMovement(
            in Vector2 leftInput,
            in Vector2 rightInput,
            out float targetForward,
            out float targetTurn)
        {
            // キャタピラ入力を前進量・旋回量に変換
            _trackController.UpdateTrack(_inputMode, leftInput, rightInput, out float forwardInput, out float turnInput);

            // 機動力倍率を掛けて最終的な目標値を算出
            targetForward = forwardInput * _mobilityMultiplier;
            targetTurn = turnInput * _mobilityMultiplier;
        }

        /// <summary>
        /// 目標移動量に向かって現在値を加減速させる（正負方向分離版）
        /// </summary>
        /// <param name="targetForward">前進・後退の目標移動量</param>
        /// <param name="targetTurn">旋回の目標移動量</param>
        private void ApplyAcceleration(in float targetForward, in float targetTurn)
        {
            float dt = Time.deltaTime;

            // --------------------------------------------------
            // 前進・後退の加減速
            // --------------------------------------------------
            float forwardMaxDelta = _forwardAccelerationMultiplier * dt;

            // 正方向（前進）の減速および加速
            if (targetForward >= 0f)
            {
                _currentForwardPositive = Mathf.MoveTowards(
                    _currentForwardPositive,
                    targetForward,
                    forwardMaxDelta
                );
                _currentForwardNegative = Mathf.MoveTowards(
                    _currentForwardNegative,
                    0f,
                    forwardMaxDelta
                );
            }
            else // 負方向（後退）の減速および加速
            {
                _currentForwardNegative = Mathf.MoveTowards(
                    _currentForwardNegative,
                    -targetForward,
                    forwardMaxDelta
                );
                _currentForwardPositive = Mathf.MoveTowards(
                    _currentForwardPositive,
                    0f,
                    forwardMaxDelta
                );
            }

            // 正負の合算で現在前進量を決定
            _currentForward = _currentForwardPositive - _currentForwardNegative;

            // --------------------------------------------------
            // 旋回の加減速
            // --------------------------------------------------
            float turnMaxDelta = _turnAccelerationMultiplier * dt;

            // 正方向（右旋回）の減速および加速
            if (targetTurn >= 0f)
            {
                _currentTurnPositive = Mathf.MoveTowards(
                    _currentTurnPositive,
                    targetTurn,
                    turnMaxDelta
                );
                _currentTurnNegative = Mathf.MoveTowards(
                    _currentTurnNegative,
                    0f,
                    turnMaxDelta
                );
            }
            else // 負方向（左旋回）の減速および加速
            {
                _currentTurnNegative = Mathf.MoveTowards(
                    _currentTurnNegative,
                    -targetTurn,
                    turnMaxDelta
                );
                _currentTurnPositive = Mathf.MoveTowards(
                    _currentTurnPositive,
                    0f,
                    turnMaxDelta
                );
            }

            // 正負の合算で現在旋回量を決定
            _currentTurn = _currentTurnPositive - _currentTurnNegative;
        }
    }
}