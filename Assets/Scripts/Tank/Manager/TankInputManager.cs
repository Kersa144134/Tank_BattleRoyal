// ======================================================
// TankInputManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2025-12-12
// 概要     : 戦車操作用の入力を管理するクラス
//            基本は単一ボタンを辞書で管理し、複数ボタン対応は必要時のみ処理
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

        /// <summary>左スティック入力</summary>
        public Vector2 LeftStick { get; private set; }

        /// <summary>右スティック入力</summary>
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
            int currentMapping = InputManager.Instance.CurrentMappingIndex;

            // --------------------------------------------------
            // 常時有効ボタン
            // --------------------------------------------------
            ButtonMap[TankInputKeys.INPUT_OPTION] = InputManager.Instance.StartButton;

            if (currentMapping == 0)
            {
                // --------------------------------------------------
                // インゲームマッピング
                // --------------------------------------------------
                LeftStick = InputManager.Instance.LeftStick;
                RightStick = InputManager.Instance.RightStick;

                ButtonMap[TankInputKeys.INPUT_MODE_CHANGE] = InputManager.Instance.LeftStickButton;

                ButtonMap[TankInputKeys.INPUT_LEFT_FIRE] = InputManager.Instance.LeftTrigger;
                ButtonMap[TankInputKeys.INPUT_RIGHT_FIRE] = InputManager.Instance.RightTrigger;
            }
            else
            {
                // --------------------------------------------------
                // UIマッピング時は攻撃ボタン無効化
                // --------------------------------------------------
                LeftStick = Vector2.zero;
                RightStick = Vector2.zero;

                ButtonMap[TankInputKeys.INPUT_MODE_CHANGE] = null;

                ButtonMap[TankInputKeys.INPUT_LEFT_FIRE] = null;
                ButtonMap[TankInputKeys.INPUT_RIGHT_FIRE] = null;
            }
        }

        /// <summary>
        /// 指定の文字列キーでボタン状態を取得
        /// 存在しない場合は null を返す
        /// </summary>
        public ButtonState GetButtonState(in string key)
        {
            if (ButtonMap.TryGetValue(key, out ButtonState state))
            {
                return state;
            }

            return null;
        }
    }
}