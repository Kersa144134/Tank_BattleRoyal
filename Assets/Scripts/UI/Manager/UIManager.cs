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

        /// <summary>プレイヤー戦車のルートマネージャー</summary>
        [SerializeField]
        private BaseTankRootManager _playerTankRootManager;

        /// <summary>最大体力を表すバー Image</summary>
        [SerializeField]
        private Image _maxHealthBarImage;

        /// <summary>現在体力を表すバー Image</summary>
        [SerializeField]
        private Image _currentHealthBarImage;

        /// <summary>差分体力を表すバー Image</summary>
        [SerializeField]
        private Image _diffHealthBarImage;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>体力バー横幅制御コントローラー</summary>
        private ValueBarWidthUIController _healthBarWidthUIController;

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

                // 体力バー横幅制御コントローラーを生成
                _healthBarWidthUIController =
                    new ValueBarWidthUIController(
                        _maxHealthBarImage,
                        _currentHealthBarImage,
                        _diffHealthBarImage,
                        _playerTankRootManager.DurabilityManager.MaxDurability,
                        _playerTankRootManager.DurabilityManager.CurrentDurability
                    );

                // イベント購読
                _playerTankRootManager.DurabilityManager.OnDurabilityChanged += HandleDurabilityChanged;
            }
        }

        public void OnLateUpdate()
        {
            float deltaTime = Time.deltaTime;
            
            // 耐久力マネージャーが未取得の場合は処理しない
            if (_playerDurabilityManager != null)
            {
                // 現在体力・最大体力をUIに反映
                _healthBarWidthUIController.Update(deltaTime);
            }
        }

        public void OnExit()
        {
            // イベント購読の解除
            _playerTankRootManager.DurabilityManager.OnDurabilityChanged -= HandleDurabilityChanged;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        private void HandleDurabilityChanged()
        {
            _healthBarWidthUIController.NotifyValueChanged(
                _playerTankRootManager.DurabilityManager.MaxDurability,
                _playerTankRootManager.DurabilityManager.CurrentDurability
            );
        }
    }
}