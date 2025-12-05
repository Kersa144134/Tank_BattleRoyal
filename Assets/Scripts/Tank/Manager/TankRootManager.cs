// ======================================================
// TankRootManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-04
// 更新日時 : 2025-12-05
// 概要     : 戦車の各種制御を統合管理する
// ======================================================

using UnityEngine;
using InputSystem.Manager;
using TankSystem.Controller;

namespace TankSystem.Manager
{
    /// <summary>
    /// 戦車の各種制御を統括するクラス
    /// </summary>
    public class TankRootManager : MonoBehaviour
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        // ======================================================
        // コンポーネント参照
        // ======================================================

        // --------------------------------------------------
        // パラメーター
        // --------------------------------------------------

        // --------------------------------------------------
        // 攻撃力
        // --------------------------------------------------

        // --------------------------------------------------
        // 防御力
        // --------------------------------------------------

        // --------------------------------------------------
        // 機動力
        // --------------------------------------------------
        /// <summary>戦車の機動力管理クラス</summary>
        private TankMobilityManager _mobilityManager;

        /// <summary>左右キャタピラ入力から前進量・旋回量を算出するコントローラ</summary>
        private TankTrackController _trackController = new TankTrackController();

        // ======================================================
        // Unityイベント
        // ======================================================

        private void Start()
        {
            // TankMobilityManager の生成
            _mobilityManager = new TankMobilityManager(_trackController, transform);
        }

        private void Update()
        {
            // 左右スティック入力取得
            float leftInput = InputManager.Instance.LeftStick.y;
            float rightInput = InputManager.Instance.RightStick.y;

            // 前進・旋回処理
            _mobilityManager.ApplyMobility(leftInput, rightInput);
        }
    }
}