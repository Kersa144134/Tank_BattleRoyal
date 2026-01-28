// ======================================================
// TankInputManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2026-01-23
// 概要     : 戦車操作の入力を管理するクラス
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using InputSystem.Data;

namespace InputSystem.Manager
{
    /// <summary>
    /// 戦車操作用入力管理クラス
    /// ボタンやスティックを文字列キーで辞書登録してアクセス可能
    /// </summary>
    public class TankInputManager
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>キャタピラ入力モード</summary>
        private TrackInputMode _inputMode = TrackInputMode.Dual;

        /// <summary>無入力ボタン</summary>
        private readonly ButtonState _none = new ButtonState();

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>キャタピラ入力モード</summary>
        public TrackInputMode InputMode => _inputMode;

        /// <summary>左スティック入力</summary>
        public Vector2 LeftStick { get; private set; }

        /// <summary>右スティック入力</summary>
        public Vector2 RightStick { get; private set; }

        /// <summary>砲塔回転入力</summary>
        public float TurretRotation { get; private set; }

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>ボタン名と ButtonState の辞書</summary>
        public Dictionary<string, ButtonState> ButtonMap { get; private set; } = new Dictionary<string, ButtonState>();

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 戦車入力管理クラスの生成
        /// </summary>
        public TankInputManager()
        {
            // 初期入力モードを設定
            _inputMode = TrackInputMode.Dual;
        }
        
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 毎フレーム呼び出して入力を更新する
        /// </summary>
        public void UpdateInput()
        {
            int currentMapping = InputManager.Instance.CurrentMappingIndex;

            if (currentMapping == 0)
            {
                // --------------------------------------------------
                // インゲームマッピング
                // --------------------------------------------------
                if (_inputMode == TrackInputMode.Single)
                {
                    LeftStick = InputManager.Instance.LeftStick;
                    RightStick = Vector2.zero;

                    TurretRotation = InputManager.Instance.RightStick.x;
                }
                else
                {
                    LeftStick = InputManager.Instance.LeftStick;
                    RightStick = InputManager.Instance.RightStick;

                    // 左ショルダー入力
                    float leftShoulder =
                        InputManager.Instance.LeftShoulder.IsPressed ? 1f : 0f;

                    // 右ショルダー入力
                    float rightShoulder =
                        InputManager.Instance.RightShoulder.IsPressed ? 1f : 0f;

                    // 左右の入力差分から砲塔回転量を算出
                    TurretRotation = -leftShoulder + rightShoulder;
                }

                ButtonMap[TankInputKeys.INPUT_MODE_CHANGE] = InputManager.Instance.LeftStickButton;
                ButtonMap[TankInputKeys.FIRE_MODE_CHANGE] = InputManager.Instance.RightStickButton;

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

                TurretRotation = 0f;

                ButtonMap[TankInputKeys.INPUT_MODE_CHANGE] = _none;
                ButtonMap[TankInputKeys.FIRE_MODE_CHANGE] = _none;

                ButtonMap[TankInputKeys.INPUT_LEFT_FIRE] = _none;
                ButtonMap[TankInputKeys.INPUT_RIGHT_FIRE] = _none;
            }
        }

        /// <summary>
        /// キャタピラの入力モードを切り替える
        /// </summary>
        public void ChangeInputMode()
        {
            // 現在の入力モードに応じて次のモードを決定
            TrackInputMode nextMode;

            // 現在有効なモードを基準に判定
            if (_inputMode == TrackInputMode.Single)
            {
                // シングル操作中の場合はデュアル操作へ切り替える
                nextMode = TrackInputMode.Dual;
            }
            else
            {
                // それ以外の場合はシングル操作へ切り替える
                nextMode = TrackInputMode.Single;
            }

            _inputMode = nextMode;
        }

        /// <summary>
        /// 指定の文字列キーでボタン状態を取得
        /// 存在しない場合は none を返す
        /// </summary>
        public ButtonState GetButtonState(in string key)
        {
            if (ButtonMap.TryGetValue(key, out ButtonState state))
            {
                return state;
            }

            return _none;
        }
    }
}