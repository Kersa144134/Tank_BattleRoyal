// ======================================================
// InputManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-09-24
// 更新日時 : 2025-11-11
// 概要     : 物理ゲームパッドおよびキーボード・マウス入力を統合管理
// ======================================================

using UnityEngine;
using InputSystem.Data;

namespace InputSystem.Manager
{
    /// <summary>
    /// 入力管理クラス
    /// 物理ゲームパッドとキーボード・マウス入力を統合
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        // ======================================================
        // シングルトンインスタンス
        // ======================================================

        /// <summary>InputManagerのグローバルインスタンス</summary>
        public static InputManager Instance { get; private set; }

        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("入力マッピング設定")]
        /// <summary>入力マッピング設定（InputMappingConfig）</summary>
        [SerializeField] private InputMappingConfig inputMappingConfig;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>入力デバイス切替を管理するマネージャ</summary>
        private DeviceManager _deviceManager;

        /// <summary>ボタン状態を管理するマネージャ</summary>
        private ButtonStateManager _buttonStateManager;

        /// <summary>スティック/D-Pad状態を管理するマネージャ</summary>
        private StickStateManager _stickStateManager;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>ボタンAの状態</summary>
        public ButtonState ButtonA => _buttonStateManager.ButtonA;

        /// <summary>ボタンBの状態</summary>
        public ButtonState ButtonB => _buttonStateManager.ButtonB;

        /// <summary>ボタンXの状態</summary>
        public ButtonState ButtonX => _buttonStateManager.ButtonX;

        /// <summary>ボタンYの状態</summary>
        public ButtonState ButtonY => _buttonStateManager.ButtonY;

        /// <summary>左ショルダーの状態</summary>
        public ButtonState LeftShoulder => _buttonStateManager.LeftShoulder;

        /// <summary>右ショルダーの状態</summary>
        public ButtonState RightShoulder => _buttonStateManager.RightShoulder;

        /// <summary>左トリガーの状態</summary>
        public ButtonState LeftTrigger => _buttonStateManager.LeftTrigger;

        /// <summary>右トリガーの状態</summary>
        public ButtonState RightTrigger => _buttonStateManager.RightTrigger;

        /// <summary>左スティックボタンの状態</summary>
        public ButtonState LeftStickButton => _buttonStateManager.LeftStickButton;

        /// <summary>右スティックボタンの状態</summary>
        public ButtonState RightStickButton => _buttonStateManager.RightStickButton;

        /// <summary>左スティックの入力ベクトル</summary>
        public Vector2 LeftStick => _stickStateManager.LeftStick;

        /// <summary>右スティックの入力ベクトル</summary>
        public Vector2 RightStick => _stickStateManager.RightStick;

        /// <summary>D-Pad の入力ベクトル</summary>
        public Vector2 DPad => _stickStateManager.DPad;

        /// <summary>Startボタンの状態</summary>
        public ButtonState StartButton => _buttonStateManager.StartButton;

        /// <summary>Selectボタンの状態</summary>
        public ButtonState SelectButton => _buttonStateManager.SelectButton;

        // ======================================================
        // Unityイベント関数
        // ======================================================

        private void Awake()
        {
            // シングルトン制御
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 入力マッピング設定のチェック
            if (inputMappingConfig == null)
            {
                // エラーログを出力してゲームを終了
                Debug.LogError("[InputManager] InputMappingConfig が設定されていません。アプリケーションを終了します。");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                return;
            }

            // サブマネージャ初期化
            _deviceManager = new DeviceManager(inputMappingConfig);
            _buttonStateManager = new ButtonStateManager();
            _stickStateManager = new StickStateManager();
        }

        private void Update()
        {
            // デバイス入力更新
            _deviceManager.UpdateDevices();

            // ボタン状態更新
            _buttonStateManager.UpdateButtonStates(_deviceManager.ActiveController);

            // スティック状態更新
            _stickStateManager.UpdateStickStates(_deviceManager.ActiveController);
        }
    }
}