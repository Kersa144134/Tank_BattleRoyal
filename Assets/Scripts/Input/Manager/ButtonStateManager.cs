// ======================================================
// ButtonStateManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-11-11
// 更新日時 : 2025-11-11
// 概要     : ボタン入力状態の管理クラス
//            InputManager.ButtonState を内部で保持し、ボタン押下状態を更新する
// ======================================================

using System;
using System.Collections.Generic;
using System.Linq;
using InputSystem.Data;

namespace InputSystem.Manager
{
    /// <summary>
    /// ボタン状態を管理するクラス
    /// サブクラス ButtonState で押下/離上状態を更新する
    /// </summary>
    public class ButtonStateManager
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>ボタン種別ごとの状態を保持する辞書</summary>
        private readonly Dictionary<GamepadInputType, ButtonState> _buttonStates;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>ButtonStateManager のコンストラクタ。全ボタンを初期化</summary>
        public ButtonStateManager()
        {
            // 辞書を初期化
            _buttonStates = new Dictionary<GamepadInputType, ButtonState>();

            // 全 GamepadInputType を列挙して初期状態を作成
            foreach (GamepadInputType type in Enum.GetValues(typeof(GamepadInputType)))
            {
                _buttonStates[type] = new ButtonState();
            }
        }

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>ボタンAの状態</summary>
        public ButtonState ButtonA => _buttonStates[GamepadInputType.ButtonA];

        /// <summary>ボタンBの状態</summary>
        public ButtonState ButtonB => _buttonStates[GamepadInputType.ButtonB];

        /// <summary>ボタンXの状態</summary>
        public ButtonState ButtonX => _buttonStates[GamepadInputType.ButtonX];

        /// <summary>ボタンYの状態</summary>
        public ButtonState ButtonY => _buttonStates[GamepadInputType.ButtonY];

        /// <summary>左ショルダーの状態</summary>
        public ButtonState LeftShoulder => _buttonStates[GamepadInputType.LeftShoulder];

        /// <summary>右ショルダーの状態</summary>
        public ButtonState RightShoulder => _buttonStates[GamepadInputType.RightShoulder];

        /// <summary>左トリガーの状態</summary>
        public ButtonState LeftTrigger => _buttonStates[GamepadInputType.LeftTrigger];

        /// <summary>右トリガーの状態</summary>
        public ButtonState RightTrigger => _buttonStates[GamepadInputType.RightTrigger];

        /// <summary>Startボタンの状態</summary>
        public ButtonState StartButton => _buttonStates[GamepadInputType.Start];

        /// <summary>Selectボタンの状態</summary>
        public ButtonState SelectButton => _buttonStates[GamepadInputType.Select];

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// コントローラ入力からボタン状態を更新する（列挙中例外を回避）
        /// </summary>
        /// <param name="controller">IGamepadInputSource を実装した入力コントローラ</param>
        public void UpdateButtonStates(IGamepadInputSource controller)
        {
            // 辞書のキーを配列化して列挙中変更エラーを防止
            GamepadInputType[] keys = _buttonStates.Keys.ToArray();

            for (int i = 0; i < keys.Length; i++)
            {
                GamepadInputType key = keys[i];

                // 現在の押下状態を取得
                bool pressed = key switch
                {
                    GamepadInputType.ButtonA => controller.ButtonA,
                    GamepadInputType.ButtonB => controller.ButtonB,
                    GamepadInputType.ButtonX => controller.ButtonX,
                    GamepadInputType.ButtonY => controller.ButtonY,
                    GamepadInputType.LeftShoulder => controller.LeftShoulder,
                    GamepadInputType.RightShoulder => controller.RightShoulder,
                    GamepadInputType.LeftTrigger => controller.LeftTrigger,
                    GamepadInputType.RightTrigger => controller.RightTrigger,
                    GamepadInputType.Start => controller.StartButton,
                    GamepadInputType.Select => controller.SelectButton,
                    _ => false
                };

                // クラスの更新
                ButtonState state = _buttonStates[key];
                state.Update(pressed);
            }
        }
    }
}