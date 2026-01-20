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

        /// <summary>無入力ボタン</summary>
        private readonly ButtonState _none = new ButtonState();

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>現在の耐久値</summary>
        public float CurrentDurability => DurabilityManager.CurrentDurability;

        /// <summary>ステータスから算出される耐久の最大値</summary>
        public float MaxDurability => DurabilityManager.MaxDurability;

        // ======================================================
        // 抽象メソッド
        // ======================================================

        /// <summary>
        /// 毎フレーム呼び出される入力更新処理
        /// BaseTankRootManager の抽象メソッドをオーバーライド
        /// </summary>
        /// <param name="leftMobility">左キャタピラ入力から算出される前進/旋回量</param>
        /// <param name="rightMobility">右キャタピラ入力から算出される前進/旋回量</param>
        /// <param name="inputModeChange">入力モード切替ボタン押下フラグ</param>
        /// <param name="fireModeChange">攻撃モード切替ボタン押下フラグ</param>
        /// <param name="option">オプションボタン押下フラグ</param>
        /// <param name="leftFire">左攻撃ボタンの状態</param>
        /// <param name="rightFire">右攻撃ボタンの状態</param>
        protected override void UpdateInput(
            out Vector2 leftMobility,
            out Vector2 rightMobility,
            out bool inputModeChange,
            out bool fireModeChange,
            out bool option,
            out ButtonState leftFire,
            out ButtonState rightFire
        )
        {
            // 入力の更新
            _inputManager.UpdateInput();

            // 左右キャタピラ入力を取得
            leftMobility = _inputManager.LeftStick;
            rightMobility = _inputManager.RightStick;

            // 入力切替ボタン押下判定
            inputModeChange = _inputManager.GetButtonState(TankInputKeys.INPUT_MODE_CHANGE).Down;
            fireModeChange = _inputManager.GetButtonState(TankInputKeys.FIRE_MODE_CHANGE).Down;

            // オプションボタン押下判定
            option = _inputManager.GetButtonState(TankInputKeys.INPUT_OPTION).Down;

            // 攻撃ボタン状態を取得
            leftFire = _inputManager.GetButtonState(TankInputKeys.INPUT_LEFT_FIRE);
            rightFire = _none;
            // rightFire = _inputManager.GetButtonState(TankInputKeys.INPUT_RIGHT_FIRE);
        }
    }
}