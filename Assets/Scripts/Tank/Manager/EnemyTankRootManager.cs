// ======================================================
// EnemyTankRootManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-14
// 更新日時 : 2025-12-14
// 概要     : 敵AI用戦車マネージャー
//            BaseTankRootManager を継承し、入力処理をAI制御に差し替える
// ======================================================

using InputSystem.Data;
using TankSystem.Data;
using UnityEngine;

namespace TankSystem.Manager
{
    /// <summary>
    /// 敵戦車用マネージャー
    /// 入力処理を AI 制御に置き換え、プレイヤー入力を使用しない
    /// </summary>
    public sealed class EnemyTankRootManager : BaseTankRootManager
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>プレイヤー入力管理クラス</summary>
        private readonly TankInputManager _inputManager = new TankInputManager();

        // ======================================================
        // 入力処理
        // ======================================================

        /// <summary>
        /// 毎フレーム呼び出される AI 入力更新処理
        /// BaseTankRootManager の抽象メソッドをオーバーライド
        /// </summary>
        /// <param name="leftMobility">左キャタピラ入力から算出される前進/後退量</param>
        /// <param name="rightMobility">右キャタピラ入力から算出される前進/後退量</param>
        /// <param name="optionPressed">オプションボタン押下フラグ（AIは使用しない）</param>
        /// <param name="leftFire">左攻撃ボタンの状態（AIは自動制御）</param>
        /// <param name="rightFire">右攻撃ボタンの状態（AIは自動制御）</param>
        protected override void UpdateInput(
            out float leftMobility,
            out float rightMobility,
            out bool optionPressed,
            out ButtonState leftFire,
            out ButtonState rightFire
        )
        {
            // 入力の更新
            _inputManager.UpdateInput();

            // --------------------------------------------------
            // AI制御の簡易例
            // 前進と旋回の固定値を出力
            // --------------------------------------------------
            leftMobility = 1f;
            rightMobility = 0f;

            optionPressed = false;

            leftFire = new ButtonState();
            rightFire = _inputManager.GetButtonState(TankInputKeys.INPUT_RIGHT_FIRE);
        }
    }
}