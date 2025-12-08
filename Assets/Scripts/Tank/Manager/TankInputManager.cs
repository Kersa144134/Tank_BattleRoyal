// ======================================================
// TankInputManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2025-12-08
// 概要     : 戦車操作用の入力を管理するロジッククラス
// ======================================================

using UnityEngine;
using InputSystem.Manager;

namespace TankSystem.Manager
{
    /// <summary>
    /// 戦車操作用入力管理クラス
    /// </summary>
    public class TankInputManager
    {
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>左スティック入力 (前後)</summary>
        public Vector2 LeftStick { get; private set; }

        /// <summary>右スティック入力 (前後)</summary>
        public Vector2 RightStick { get; private set; }

        /// <summary>榴弾攻撃ボタン入力 (Rトリガー)</summary>
        public bool HEFireButton { get; private set; }

        /// <summary>徹甲弾攻撃ボタン入力 (Lトリガー)</summary>
        public bool APFireButton { get; private set; }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 毎フレーム呼び出して入力を更新する
        /// </summary>
        public void UpdateInput()
        {
            // スティック入力取得
            LeftStick = InputManager.Instance.LeftStick;
            RightStick = InputManager.Instance.RightStick;

            // 攻撃ボタン取得
            HEFireButton = InputManager.Instance.RightTrigger.Down;
            APFireButton = InputManager.Instance.LeftTrigger.Down;
        }
    }
}
