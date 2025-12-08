// ======================================================
// TankMobilityManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-05
// 更新日時 : 2025-12-05
// 概要     : 戦車の前進・旋回処理を担当する機動力管理クラス
//            TrackController から得た前進量・旋回量に基づき
//            Transform を更新する責務を持つ
// ======================================================

using UnityEngine;
using TankSystem.Controller;

namespace TankSystem.Manager
{
    /// <summary>
    /// 戦車の移動・旋回処理を専任で受け持つマネージャ
    /// </summary>
    public class TankMobilityManager
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>左右キャタピラ入力から前進量・旋回量を算出するコントローラ</summary>
        private TankTrackController _trackController;

        /// <summary>戦車本体の Transform</summary>
        private Transform _tankTransform;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>機動力倍率（前進・旋回の両方に適用）</summary>
        private const float MOBILITY = 5.5f;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 機動力管理に必要な外部参照を受け取り初期化する
        /// </summary>
        /// <param name="trackController">左右キャタピラ入力から前進量・旋回量を算出するコントローラ</param>
        /// <param name="transform">戦車本体の Transform</param>
        public TankMobilityManager(TankTrackController trackController, Transform transform)
        {
            _trackController = trackController;
            _tankTransform = transform;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 入力値を元に前進・旋回処理を実行する
        /// </summary>
        /// <param name="left">左キャタピラ入力</param>
        /// <param name="right">右キャタピラ入力</param>
        public void ApplyMobility(in float left, in float right)
        {
            // キャタピラ入力から前進量と旋回量を計算
            _trackController.UpdateTrack(left, right, out float forward, out float turn);

            // 移動処理（前進・後退）
            _tankTransform.Translate(
                Vector3.forward * forward * MOBILITY * Time.deltaTime,
                Space.Self
            );

            // 旋回処理（左右回転）
            _tankTransform.Rotate(
                0f,
                turn * MOBILITY * Time.deltaTime,
                0f,
                Space.Self
            );
        }
    }
}