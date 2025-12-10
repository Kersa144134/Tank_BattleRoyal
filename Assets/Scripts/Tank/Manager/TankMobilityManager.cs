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
using TankSystem.Utility;

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

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 移動処理および衝突判定に必要な外部参照を受け取って初期化する
        /// </summary>
        public TankMobilityManager(
            TankTrackController trackController,
            TankCollisionService collisionService,
            Transform transform,
            Vector3 hitboxCenter,
            Vector3 hitboxSize,
            Transform[] obstacles
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

            // 移動前の座標を保存
            Vector3 previousPosition = _tankTransform.position;

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

            // 移動後の衝突判定
            CheckCollision(previousPosition);
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
        /// 移動後の衝突をチェックし、必要に応じて位置を戻す
        /// </summary>
        /// <param name="previousPosition">移動前の座標</param>
        private void CheckCollision(in Vector3 previousPosition)
        {
            // 衝突対象のワールド位置を受け取る変数
            Vector3 hitPos;

            // 衝突した場合
            if (_collisionService.TryGetCollision(out hitPos))
            {
                // 現在位置と障害物の距離を算出
                float distance = Vector3.Distance(_tankTransform.position, hitPos);

                // 初回判定
                if (_lastHitDistance < 0f)
                {
                    _lastHitDistance = distance;
                    _tankTransform.position = previousPosition;
                    return;
                }

                if (distance < _lastHitDistance)
                {
                    // 障害物に近づく方向
                    // 部分的に移動をキャンセルして衝突回避を検証
                    ResolveCollision(previousPosition, hitPos);
                    return;
                }
                else
                {
                    // 障害物から離れる方向
                    // 基準距離を更新して移動を実行
                    _lastHitDistance = distance;
                    return;
                }
            }
            else
            {
                // 基準距離をリセット
                _lastHitDistance = -1f;
            }
        }

        /// <summary>
        /// 衝突した場合にX方向・Z方向のみの移動調整を検証し、
        /// 衝突が回避可能であれば部分的に移動を許可する
        /// </summary>
        /// <param name="previousPosition">移動前の座標</param>
        /// <param name="hitPos">衝突対象の座標</param>
        private void ResolveCollision(Vector3 previousPosition, Vector3 hitPos)
        {
            // 現在位置との差分
            Vector3 delta = _tankTransform.position - previousPosition;

            // --------------------------------------------------
            // X方向のみキャンセル
            // --------------------------------------------------
            Vector3 testPosX = new Vector3(previousPosition.x, _tankTransform.position.y, _tankTransform.position.z);
            _tankTransform.position = testPosX;
            if (!_collisionService.TryGetCollision(out _))
            {
                // X方向のみキャンセルで衝突回避成功
                return;
            }

            // --------------------------------------------------
            // Z方向のみキャンセル
            // --------------------------------------------------
            Vector3 testPosZ = new Vector3(_tankTransform.position.x, _tankTransform.position.y, previousPosition.z);
            _tankTransform.position = testPosZ;
            if (!_collisionService.TryGetCollision(out _))
            {
                // Z方向のみキャンセルで衝突回避成功
                return;
            }

            // --------------------------------------------------
            // 全体キャンセル
            // --------------------------------------------------
            _tankTransform.position = previousPosition;
        }
    }
}