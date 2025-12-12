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
using static TankSystem.Data.TankInputKeys;

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
        public Dictionary<string, List<ButtonState>> ButtonMap { get; private set; } = new Dictionary<string, List<ButtonState>>();

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
            ButtonMap[INPUT_OPTION] = new List<ButtonState>() { InputManager.Instance.StartButton };

            if (currentMapping == 0)
            {
                // --------------------------------------------------
                // インゲームマッピング
                // --------------------------------------------------
                LeftStick = InputManager.Instance.LeftStick;
                RightStick = InputManager.Instance.RightStick;

                // 攻撃ボタン
                // 榴弾発射は L/R トリガー両方に対応
                ButtonMap[INPUT_EXPLOSIVE_FIRE] = new List<ButtonState>()
                {
                    InputManager.Instance.LeftTrigger,
                    InputManager.Instance.RightTrigger
                };

                // 従来通り単一ボタンはリスト化して登録
                ButtonMap[INPUT_PENETRATION_FIRE] = new List<ButtonState>() { null };
                ButtonMap[INPUT_HOMING_FIRE] = new List<ButtonState>() { null };
            }
            else
            {
                // --------------------------------------------------
                // UIマッピング時は攻撃ボタン無効化
                // --------------------------------------------------
                LeftStick = Vector2.zero;
                RightStick = Vector2.zero;

                ButtonMap[INPUT_EXPLOSIVE_FIRE] = null;
                ButtonMap[INPUT_PENETRATION_FIRE] = null;
                ButtonMap[INPUT_HOMING_FIRE] = null;
            }
        }

        /// <summary>
        /// 指定キーに対応するボタンリストを取得
        /// 存在しない場合は空リストを返す
        /// </summary>
        public List<ButtonState> GetButtonStates(in string key)
        {
            if (ButtonMap.TryGetValue(key, out List<ButtonState> states))
            {
                return states;
            }
            return new List<ButtonState>();
        }
    }
}