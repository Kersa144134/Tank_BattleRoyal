// ======================================================
// TankMobilityManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-05
// 更新日時 : 2025-12-10
// 概要     : 戦車の前進・旋回処理を担当する機動力管理クラス。
//            TrackController による前進量・旋回量を Transform に反映し、
//            TankCollisionService により移動後の衝突判定を行う。
// ======================================================

using UnityEngine;
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

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>戦車本体の Transform</summary>
        private readonly Transform _tankTransform;

        /// <summary>現在の機動力倍率（前進・旋回に適用）</summary>
        private float _mobilityMultiplier = BASE_MOBILITY;

        /// <summary>接触した戦車と障害物の距離</summary>
        private float _lastHitDistance = -1f;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>基準機動力倍率</summary>
        private const float BASE_MOBILITY = 5.0f;

        /// <summary>馬力1あたりの倍率加算値</summary>
        private const float HORSEPOWER_MULTIPLIER = 1.5f;

        /// <summary>衝突回避時のXZ平面での移動ステップ量</summary>
        private const float COLLISION_RETREAT_STEP = 0.05f;

        /// <summary>衝突回避時の最大ステップ回数</summary>
        private const int COLLISION_MAX_STEPS = 100;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 移動処理および衝突判定に必要な外部参照を受け取って初期化する
        /// </summary>
        public TankMobilityManager(
            in TankTrackController trackController,
            in TankCollisionService collisionService,
            in Transform transform,
            in Vector3 hitboxCenter,
            in Vector3 hitboxSize,
            in Transform[] obstacles
        )
        {
            _trackController = trackController;
            _collisionService = collisionService;

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
        }

        /// <summary>
        /// 移動後の衝突をチェックし、必要に応じて位置を戻す
        /// </summary>
        public void CheckObstaclesCollision(Transform obstacle)
        {
            // 現在位置と障害物の距離を算出
            float distance = Vector3.Distance(_tankTransform.position, obstacle.position);

            // 初回判定
            if (_lastHitDistance < 0f)
            {
                _lastHitDistance = distance;
                return;
            }

            if (distance < _lastHitDistance)
            {
                // 障害物に近づく方向
                ResolveCollision(obstacle.position);
                return;
            }
            else
            {
                // 障害物から離れる方向
                _lastHitDistance = distance;
                return;
            }
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

        /// <summary>
        /// 衝突した場合に、障害物との方向に沿ってXZ平面のみで戦車を少しずつ後退させ、
        /// 衝突が解消されるまで移動を調整する
        /// </summary>
        /// <param name="hitPos">接触対象の座標</param>
        private void ResolveCollision(Vector3 hitPos)
        {
            // 戦車から障害物への方向ベクトルを計算
            Vector3 toObstacle = hitPos - _tankTransform.position;

            // Y成分を無視
            toObstacle.y = 0f;

            // 長さ0ベクトルは処理不要
            if (toObstacle.sqrMagnitude < 1e-6f)
            {
                return;
            }

            // 衝突回避用の後退ベクトル
            Vector3 retreatDirection = -toObstacle.normalized;

            int stepCount = 0;

            // 衝突が解消されるまでループ
            while (_collisionService.IsCollidingWithObstacleAtPosition(_tankTransform.position) && stepCount < COLLISION_MAX_STEPS)
            {
                // XZ平面方向に少しずつ退避
                _tankTransform.position += retreatDirection * COLLISION_RETREAT_STEP;
                stepCount++;
            }
        }
    }
}