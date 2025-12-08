// ======================================================
// TankInputManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2025-12-08
// 概要     : 戦車操作用の入力を管理するクラス（辞書登録対応）
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using InputSystem.Data;
using InputSystem.Manager;
using TankSystem.Data;

namespace TankSystem.Manager
{
    /// <summary>
    /// 戦車操作用入力管理クラス
    /// ボタンやスティックを文字列キーで辞書登録してアクセス可能
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

        /// <summary>ボタン名と ButtonState の辞書</summary>
        public Dictionary<string, ButtonState> ButtonMap { get; private set; } = new Dictionary<string, ButtonState>();

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 毎フレーム呼び出して入力を更新する
        /// </summary>
        public void UpdateInput()
        {
            // 現在の入力マッピングインデックスを取得
            int currentMapping = InputManager.Instance.CurrentMappingIndex;

            // --------------------------------------------------
            // 常時有効ボタン
            // --------------------------------------------------
            ButtonMap[TankInputKeys.INPUT_OPTION] = InputManager.Instance.StartButton;

            if (currentMapping == 0)
            {
                // --------------------------------------------------
                // インゲーム
                // --------------------------------------------------
                LeftStick = InputManager.Instance.LeftStick;
                RightStick = InputManager.Instance.RightStick;

                // 攻撃ボタン
                ButtonMap[TankInputKeys.INPUT_HE_FIRE] = InputManager.Instance.RightTrigger;
                ButtonMap[TankInputKeys.INPUT_AP_FIRE] = InputManager.Instance.LeftTrigger;
            }
            else
            {
                // --------------------------------------------------
                // UI マッピング時は攻撃ボタン無効化
                // --------------------------------------------------
                LeftStick = Vector2.zero;
                RightStick = Vector2.zero;

                ButtonMap[TankInputKeys.INPUT_HE_FIRE] = null;
                ButtonMap[TankInputKeys.INPUT_AP_FIRE] = null;
            }
        }

        /// <summary>
        /// 指定の文字列キーでボタン状態を取得
        /// 存在しない場合は null を返す
        /// </summary>
        public ButtonState GetButton(in string key)
        {
            if (ButtonMap.TryGetValue(key, out ButtonState state))
            {
                return state;
            }

            return null;
        }
    }
}
