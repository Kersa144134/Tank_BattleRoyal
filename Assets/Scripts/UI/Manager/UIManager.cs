// ======================================================
// UIManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-01-19
// 更新日時 : 2026-01-19
// 概要     : 各種UIコントローラーを生成・更新する
// ======================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SceneSystem.Interface;
using TankSystem.Manager;
using UISystem.Controller;

namespace UISystem.Manager
{
    /// <summary>
    /// UI 全体の生成および更新を管理するクラス
    /// </summary>
    public sealed class UIManager : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        // --------------------------------------------------
        // プレイヤー参照
        // --------------------------------------------------
        [Header("プレイヤー戦車")]
        /// <summary>プレイヤー戦車のルートマネージャー</summary>
        [SerializeField]
        private BaseTankRootManager _playerTankRootManager;

        // --------------------------------------------------
        // 耐久値バー
        // --------------------------------------------------
        [Header("耐久値バー")]
        /// <summary>最大耐久値を表すバー Image</summary>
        [SerializeField]
        private Image _maxDurabilityBarImage;

        /// <summary>現在耐久値を表すバー Image</summary>
        [SerializeField]
        private Image _currentDurabilityBarImage;

        /// <summary>差分耐久値を表すバー Image</summary>
        [SerializeField]
        private Image _diffDurabilityBarImage;

        // --------------------------------------------------
        // 弾丸アイコン
        // --------------------------------------------------
        [Header("弾丸アイコン")]
        /// <summary>弾丸アイコン Image 配列</summary>
        [SerializeField]
        private Image[] _bulletIconImages;

        /// <summary>弾丸アイコンの配置方向</summary>
        [SerializeField]
        private SlotRotationUIController.LayoutDirection _bulletIconLayoutDirection;

        /// <summary>弾丸アイコンの回転方向の符号</summary>
        [SerializeField]
        private SlotRotationUIController.RotationSign _bulletIconRotationSign;

        // --------------------------------------------------
        // ログ
        // --------------------------------------------------
        [Header("ログ")]
        /// <summary>弾丸アイコン Image 配列</summary>
        [SerializeField]
        private TextMeshProUGUI[] _logTexts;
        
        /// <summary>ログの縦方向表示方向</summary>
        [SerializeField]
        private LogRotationUIController.VerticalDirection _logVerticalDirection;

        /// <summary>ログの挿入方向</summary>
        [SerializeField]
        private LogRotationUIController.InsertDirection _logInsertDirection;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        // --------------------------------------------------
        // UI
        // --------------------------------------------------
        /// <summary>ログ表示 UI コントローラー</summary>
        private LogRotationUIController _logRotationUIController;

        /// <summary>弾丸アイコンスロット UI コントローラー</summary>
        private SlotRotationUIController _bulletIconSlotRotationUIController;

        /// <summary>耐久値バー横幅 UI コントローラー</summary>
        private ValueBarWidthUIController _durabilityBarWidthUIController;

        // --------------------------------------------------
        // データ参照
        // --------------------------------------------------
        /// <summary>プレイヤー戦車の耐久力マネージャー</summary>
        private TankDurabilityManager _playerDurabilityManager;

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void OnEnter()
        {
            if (_playerTankRootManager is PlayerTankRootManager)
            {
                // プレイヤー戦車から耐久力マネージャーを取得
                _playerDurabilityManager =
                _playerDurabilityManager =
                    _playerTankRootManager.DurabilityManager;

                _durabilityBarWidthUIController =
                    new ValueBarWidthUIController(
                        _maxDurabilityBarImage,
                        _currentDurabilityBarImage,
                        _diffDurabilityBarImage,
                        _playerTankRootManager.DurabilityManager.MaxDurability,
                        _playerTankRootManager.DurabilityManager.CurrentDurability
                    );
            }

            _bulletIconSlotRotationUIController =
                new SlotRotationUIController(
                    _bulletIconImages,
                    _bulletIconLayoutDirection,
                    _bulletIconRotationSign
                );

            _logRotationUIController =
                new LogRotationUIController(
                    _logTexts,
                    _logVerticalDirection,
                    _logInsertDirection
                );
        }

        public void OnLateUpdate()
        {
            float deltaTime = Time.deltaTime;
            
            // 耐久力マネージャーが未取得の場合は処理しない
            if (_playerDurabilityManager != null)
            {
                _durabilityBarWidthUIController.Update(deltaTime);
            }

            _bulletIconSlotRotationUIController.Update(deltaTime);
            _logRotationUIController.Update(deltaTime);

            // --------------------------------------------------
            // デバッグ用（いずれ削除予定）
            // --------------------------------------------------
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _bulletIconSlotRotationUIController.StartRouletteRotation();
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                _bulletIconSlotRotationUIController.StopRouletteRotation();
            }
            if (Input.GetKeyUp(KeyCode.Return))
            {
                _logRotationUIController.AddLog("デバッグ");
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 弾数に応じて弾丸アイコン表示を更新する
        /// </summary>
        public void UpdateBulletIcons()
        {
            _bulletIconSlotRotationUIController.Rotate();
        }

        /// <summary>
        /// 耐久値変更時の処理を行うハンドラ
        /// </summary>
        public void NotifyDurabilityChanged()
        {
            _durabilityBarWidthUIController.NotifyValueChanged(
                _playerTankRootManager.DurabilityManager.MaxDurability,
                _playerTankRootManager.DurabilityManager.CurrentDurability
            );
        }

        /// <summary>
        /// アイテム獲得時のログ表示を行う
        /// </summary>
        /// <param name="itemName">獲得したアイテム名</param>
        public void NotifyItemAcquired(in string itemName)
        {
            // ログに表示するメッセージを作成
            string logMessage = $"{itemName}を獲得";

            // ログ表示
            _logRotationUIController.AddLog(logMessage);
        }
    }
}