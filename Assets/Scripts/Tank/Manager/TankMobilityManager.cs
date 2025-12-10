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

        /// <summary>接触した戦車と障害物の距離</summary>
        private float _lastHitDistance = -1f;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>機動力倍率（前進・旋回の両方に適用）</summary>
        private const float MOBILITY = 5.5f;

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
        public void ApplyMobility(in float left, in float right)
        {
            // 移動前の座標を保存する
            Vector3 previousPosition = _tankTransform.position;

            // キャタピラ入力から前進量と旋回量を取得する
            _trackController.UpdateTrack(left, right, out float forward, out float turn);

            // 前方向への移動量を計算し Transform に適用する
            _tankTransform.Translate(
                Vector3.forward * forward * MOBILITY * Time.deltaTime,
                Space.Self
            );

            // Y 軸周りの旋回量を Transform に適用する
            _tankTransform.Rotate(
                0f,
                turn * MOBILITY * Time.deltaTime,
                0f,
                Space.Self
            );

            // 衝突対象のワールド位置を受け取る変数
            Vector3 hitPos;

            // 衝突した場合
            if (_collisionService.TryGetCollision(out hitPos))
            {
                // 現在位置と障害物の距離を算出
                float distance = Vector3.Distance(_tankTransform.position, hitPos);

                // 衝突距離を登録
                if (_lastHitDistance < 0f)
                {
                    _lastHitDistance = distance;
                    _tankTransform.position = previousPosition;
                    return;
                }

                // --------------------------------------------------
                // 2回目以降：距離比較で処理を分岐
                // --------------------------------------------------

                // さらにめり込む方向（距離が基準より小さい）
                if (distance < _lastHitDistance)
                {
                    _tankTransform.position = previousPosition;
                    return;
                }

                // 障害物から離れる方向（距離が基準より大きい）
                if (distance >= _lastHitDistance)
                {
                    // → 戻さない（移動許可）

                    // → 離れる方向なので、この距離を新たな基準値とする
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
    }
}