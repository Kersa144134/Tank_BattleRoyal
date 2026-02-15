// ======================================================
// TankEnergyManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-15
// 更新日時 : 2026-02-15
// 概要     : 戦車の燃料と弾薬を管理するクラス
// ======================================================

using System;
using TankSystem.Data;

namespace TankSystem.Manager
{
    /// <summary>
    /// 戦車の燃料と弾薬を管理するクラス
    /// </summary>
    public sealed class TankEnergyManager
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>現在の燃料量</summary>
        private float _currentFuel;

        /// <summary>燃料の最大値</summary>
        private float _maxFuel;

        /// <summary>現在の弾薬量</summary>
        private int _currentAmmo;

        /// <summary>弾薬の最大値</summary>
        private int _maxAmmo;

        /// <summary>燃料が 0 以下かどうか</summary>
        private bool _isFuelEmpty => _currentFuel <= 0f;

        /// <summary>弾薬が 0 以下かどうか</summary>
        private bool _isAmmoEmpty => _currentAmmo <= 0;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>現在の燃料量</summary>
        public float CurrentFuel => _currentFuel;

        /// <summary>燃料の最大値</summary>
        public float MaxFuel => _maxFuel;

        /// <summary>現在の弾薬量</summary>
        public int CurrentAmmo => _currentAmmo;

        // ======================================================
        // 定数
        // ======================================================

        // --------------------------------------------------
        // 基準値
        // --------------------------------------------------
        /// <summary>
        /// Fuel ステータスが 0 のときの燃料最大値の基準値
        /// </summary>
        private const float BASE_FUEL_MAX_VALUE = 100f;

        /// <summary>
        /// Ammo ステータスが 0 のときの弾薬最大値の基準値
        /// </summary>
        private const int BASE_AMMO_MAX_VALUE = 10;

        // --------------------------------------------------
        // パラメーター
        // --------------------------------------------------
        /// <summary>
        /// Fuel ステータス 1 あたりの燃料最大値加算量
        /// </summary>
        private const float FUEL_MAX_MULTIPLIER = 20f;

        /// <summary>
        /// Ammo ステータス 1 あたりの弾薬最大値加算量
        /// </summary>
        /// 
        private const int AMMO_MAX_MULTIPLIER = 1;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>燃料が変更された瞬間に発火するイベント</summary>
        public event Action OnFuelChanged;

        /// <summary>弾薬が変更された瞬間に発火するイベント</summary>
        public event Action OnAmmoChanged;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 戦車のエネルギー管理クラスを生成
        /// </summary>
        /// <param name="tankStatus">戦車ステータス</param>
        public TankEnergyManager(in TankStatus tankStatus)
        {
            // 初回はステータスから燃料と弾薬の最大値を計算
            UpdateEnergyParameter(tankStatus);

            // 初期値を最大値に設定
            _currentFuel = _maxFuel;
            _currentAmmo = _maxAmmo;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// Fuel / Ammo ステータスを元に最大値を再計算
        /// </summary>
        /// <param name="tankStatus">燃料・弾薬算出に使用する戦車ステータス</param>
        public void UpdateEnergyParameter(in TankStatus tankStatus)
        {
            // 現在の最大値を保持
            float previousMaxFuel = _maxFuel;
            int previousMaxAmmo = _maxAmmo;

            // 最大値を計算
            _maxFuel = BASE_FUEL_MAX_VALUE + tankStatus.Fuel * FUEL_MAX_MULTIPLIER;
            _maxAmmo = BASE_AMMO_MAX_VALUE + tankStatus.Ammo * AMMO_MAX_MULTIPLIER;

            // 燃料が増えた場合は現在値を補正
            float fuelIncrease = _maxFuel - previousMaxFuel;
            if (fuelIncrease > 0f)
            {
                _currentFuel += fuelIncrease;

                // 上限補正
                if (_currentFuel > _maxFuel)
                {
                    _currentFuel = _maxFuel;
                }

                OnFuelChanged?.Invoke();
            }

            // 弾薬が増えた場合は現在値を補正
            int ammoIncrease = _maxAmmo - previousMaxAmmo;
            if (ammoIncrease > 0)
            {
                _currentAmmo += ammoIncrease;

                // 上限補正
                if (_currentAmmo > _maxAmmo)
                {
                    _currentAmmo = _maxAmmo;
                }
                
                OnAmmoChanged?.Invoke();
            }
        }

        /// <summary>
        /// 燃料を消費
        /// </summary>
        /// <param name="amount">消費量</param>
        public void ConsumeFuel(in float amount = FUEL_MAX_MULTIPLIER)
        {
            if (amount <= 0f || _isFuelEmpty)
            {
                return;
            }

            _currentFuel -= amount;

            // 下限補正
            if (_currentFuel < 0f)
            {
                _currentFuel = 0f;
            }

            OnFuelChanged?.Invoke();
        }

        /// <summary>
        /// 弾薬を消費
        /// </summary>
        /// <param name="amount">消費量</param>
        public void ConsumeAmmo(in int amount = AMMO_MAX_MULTIPLIER)
        {
            if (amount <= 0 || _isAmmoEmpty)
            {
                return;
            }

            _currentAmmo -= amount;

            // 下限補正
            if (_currentAmmo < 0)
            {
                _currentAmmo = 0;
            }

            OnAmmoChanged?.Invoke();
        }

        /// <summary>
        /// 燃料を回復
        /// </summary>
        /// <param name="amount">回復量</param>
        public void RefillFuel(in float amount = FUEL_MAX_MULTIPLIER)
        {
            if (amount <= 0f)
            {
                return;
            }

            _currentFuel += amount;

            // 上限補正
            if (_currentFuel > _maxFuel)
            {
                _currentFuel = _maxFuel;
            }

            OnFuelChanged?.Invoke();
        }

        /// <summary>
        /// 弾薬を補充
        /// </summary>
        /// <param name="amount">補充量</param>
        public void RefillAmmo(in int amount = AMMO_MAX_MULTIPLIER)
        {
            if (amount <= 0)
            {
                return;
            }

            _currentAmmo += amount;

            // 上限補正
            if (_currentAmmo > _maxAmmo)
            {
                _currentAmmo = _maxAmmo;
            }

            OnAmmoChanged?.Invoke();
        }
    }
}