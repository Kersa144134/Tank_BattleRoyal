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
        // Unityイベント
        // ======================================================

        private void Start()
        {
            // TankTrackController の生成
            trackController = new TankTrackController();
        }

        private void Update()
        {
            // 左キャタピラ入力値（左スティックの上下）
            float leftInput = InputManager.Instance.LeftStick.y;

            // 右キャタピラ入力値（右スティックの上下）
            float rightInput = InputManager.Instance.RightStick.y;

            trackController.UpdateTrack(leftInput, rightInput, out float forward, out float turn);
            
            // 本体移動
            transform.Translate(Vector3.forward * forward * Time.deltaTime, Space.Self);

            // 回転
            transform.Rotate(0f, turn * Time.deltaTime, 0f, Space.Self);
        }
    }
}