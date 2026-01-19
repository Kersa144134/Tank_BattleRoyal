// ======================================================
// UIManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-01-19
// 更新日時 : 2026-01-19
// 概要     : 各種UIコントローラーを生成・更新する
// ======================================================

using UnityEngine;
using UnityEngine.UI;
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

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>耐久値バー横幅制御コントローラー</summary>
        private ValueBarWidthUIController _durabilityBarWidthUIController;

        /// <summary>弾丸アイコンスロット回転 UI コントローラー</summary>
        private SlotRotationUIController _bulletIconSlotRotationUIController;

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

                // 耐久値バー横幅制御コントローラーを生成
                _durabilityBarWidthUIController =
                    new ValueBarWidthUIController(
                        _maxDurabilityBarImage,
                        _currentDurabilityBarImage,
                        _diffDurabilityBarImage,
                        _playerTankRootManager.DurabilityManager.MaxDurability,
                        _playerTankRootManager.DurabilityManager.CurrentDurability
                    );
            }

            // スロット回転 UI コントローラーを生成する
            _bulletIconSlotRotationUIController =
                new SlotRotationUIController(
                    _bulletIconImages,
                    _bulletIconLayoutDirection,
                    _bulletIconRotationSign
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
        public void HandleDurabilityChanged()
        {
            _durabilityBarWidthUIController.NotifyValueChanged(
                _playerTankRootManager.DurabilityManager.MaxDurability,
                _playerTankRootManager.DurabilityManager.CurrentDurability
            );
        }
    }
}