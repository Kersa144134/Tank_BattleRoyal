// ======================================================
// TankRootManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-04
// 更新日時 : 2025-12-05
// 概要     : 戦車の各種制御を統合管理する
// ======================================================

using InputSystem.Data;
using InputSystem.Manager;
using SceneSystem.Interface;
using TankSystem.Controller;
using UnityEngine;

namespace TankSystem.Manager
{
    /// <summary>
    /// 戦車の各種制御を統括するクラス
    /// </summary>
    public class TankRootManager : MonoBehaviour, IUpdatable
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
        /// <summary>戦車の攻撃管理クラス</summary>
        private TankAttackManager _attackManager;

        // --------------------------------------------------
        // 防御力
        // --------------------------------------------------

        // --------------------------------------------------
        // 機動力
        // --------------------------------------------------
        /// <summary>戦車の機動管理クラス</summary>
        private TankMobilityManager _mobilityManager;

        /// <summary>左右キャタピラ入力から前進量・旋回量を算出するコントローラ</summary>
        private TankTrackController _trackController = new TankTrackController();

        // --------------------------------------------------
        // 入力
        // --------------------------------------------------
        /// <summary>戦車の入力管理クラス</summary>
        private TankInputManager _inputManager = new TankInputManager();

        // ======================================================
        // IUpdatableイベント
        // ======================================================

        public void OnEnter()
        {
            // TankMobilityManager の生成
            _attackManager = new TankAttackManager(transform);
            _mobilityManager = new TankMobilityManager(_trackController, transform);
        }

        public void OnUpdate()
        {
            // --------------------------------------------------
            // 入力
            // --------------------------------------------------
            // 入力取得
            _inputManager.UpdateInput();

            // 入力変換
            float leftMobility = _inputManager.LeftStick.y;
            float rightMobility = _inputManager.RightStick.y;

            // --------------------------------------------------
            // 攻撃
            // --------------------------------------------------
            // 攻撃処理
            _attackManager.UpdateAttack(_inputManager.HEFireButton, _inputManager.APFireButton);

            // --------------------------------------------------
            // 機動
            // --------------------------------------------------
            // 前進・旋回処理
            _mobilityManager.ApplyMobility(leftMobility, rightMobility);
        }

        public void OnLateUpdate()
        {

        }

        public void OnExit()
        {

        }

        public void OnPhaseEnter()
        {

        }

        public void OnPhaseExit()
        {

        }
    }
}