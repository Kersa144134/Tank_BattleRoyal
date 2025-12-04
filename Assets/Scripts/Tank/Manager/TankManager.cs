// ======================================================
// TankManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-04
// 更新日時 : 2025-12-04
// 概要     : 戦車の入力取得と制御ロジックの統合管理
// ======================================================

using UnityEngine;
using InputSystem.Manager;
using TankSystem.Controller;

namespace TankSystem.Manager
{
    /// <summary>
    /// 戦車の入力管理および移動制御を統括するクラス
    /// InputManager を介して入力を取得し、TankTrackController へ渡す
    /// </summary>
    public class TankManager : MonoBehaviour
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>キャタピラ制御ロジック本体</summary>
        private TankTrackController trackController;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>戦車の機動力倍率</summary>
        private const float MOBILITY = 7f;
        
        // ======================================================
        // Unityイベント
        // ======================================================

        private void Start()
        {
            // TankTrackController の生成
            trackController = new TankTrackController();
        }

        private void Update()
        {
            // 左右スティック入力取得
            float leftInput = InputManager.Instance.LeftStick.y;
            float rightInput = InputManager.Instance.RightStick.y;

            // キャタピラ入力から前進量・旋回量を計算
            trackController.UpdateTrack(leftInput, rightInput, out float forward, out float turn);

            // 機動力倍率を掛けて移動・回転
            transform.Translate(Vector3.forward * forward * MOBILITY * Time.deltaTime, Space.Self);
            transform.Rotate(0f, turn * MOBILITY * Time.deltaTime, 0f, Space.Self);
        }
    }
}