// ======================================================
// PlayerTankRootManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-14
// 更新日時 : 2025-12-14
// 概要     : プレイヤー操作用戦車マネージャー
//            BaseTankRootManager を継承し、入力処理をプレイヤー操作に差し替える
// ======================================================

using UnityEngine;
using InputSystem.Data;
using InputSystem.Manager;

namespace TankSystem.Manager
{
    /// <summary>
    /// プレイヤー戦車用マネージャー
    /// 入力処理を TankInputManager によるプレイヤー操作に対応させる
    /// </summary>
    public sealed class PlayerTankRootManager : BaseTankRootManager
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>プレイヤー入力管理クラス</summary>
        private readonly TankInputManager _inputManager = new TankInputManager();

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>プレイヤー入力管理クラス</summary>
        public TankInputManager InputManager => _inputManager;

        /// <summary>キャタピラ入力モード</summary>
        public TrackInputMode InputMode => _inputManager.InputMode;

        // ======================================================
        // 抽象メソッド
        // ======================================================

        /// <summary>
        /// 毎フレーム呼び出される入力更新処理
        /// </summary>
        /// <param name="leftStick">左キャタピラ入力</param>
        /// <param name="rightStick">右キャタピラ入力</param>
        /// <param name="turretRotation">砲塔回転入力</param>
        /// <param name="inputModeChange">入力モード切替ボタン押下フラグ</param>
        /// <param name="fireModeChange">攻撃モード切替ボタン押下フラグ</param>
        /// <param name="leftFire">左攻撃ボタンの状態</param>
        /// <param name="rightFire">右攻撃ボタンの状態</param>
        protected override void UpdateInput(
            out Vector2 leftStick,
            out Vector2 rightStick,
            out float turretRotation,
            out bool inputModeChange,
            out bool fireModeChange,
            out ButtonState leftFire,
            out ButtonState rightFire
        )
        {
            // 入力の更新
            _inputManager.UpdateInput();

            // 左右キャタピラ入力を取得
            leftStick = _inputManager.LeftStick;
            rightStick = _inputManager.RightStick;

            // 砲塔回転入力を取得
            turretRotation = _inputManager.TurretRotation;

            // 入力切替ボタン押下判定
            inputModeChange = _inputManager.GetButtonState(TankInputKeys.INPUT_MODE_CHANGE).Down;
            fireModeChange = _inputManager.GetButtonState(TankInputKeys.FIRE_MODE_CHANGE).Down;

            // 攻撃ボタン状態を取得
            leftFire = _inputManager.GetButtonState(TankInputKeys.INPUT_LEFT_FIRE);
            rightFire = _inputManager.GetButtonState(TankInputKeys.INPUT_RIGHT_FIRE);
        }
    }
}