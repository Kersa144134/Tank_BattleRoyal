// ======================================================
// TankEnergyManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-15
// 更新日時 : 2026-02-15
// 概要     : 戦車の燃料と弾薬を管理するクラス
// ======================================================

using System;
using UnityEngine;
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

        /// <summary>エリア侵入経過時間</summary>
        private float _areaIntrusionTimer;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>現在の燃料量</summary>
        public float CurrentFuel => _currentFuel;

        /// <summary>燃料の最大値</summary>
        public float MaxFuel => _maxFuel;

        /// <summary>現在の弾薬量</summary>
        public int CurrentAmmo => _currentAmmo;

        /// <summary>弾薬の最大値</summary>
        public int MaxAmmo => _maxAmmo;

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
        private const int BASE_AMMO_MAX_VALUE = 20;

        /// <summary>
        /// エリア侵入時の燃料回復量
        /// </summary>
        private const float REFILL_FUEL_VALUE_BY_AREA = 0.1f;

        /// <summary>
        /// エリア内で弾薬が回復するまでに必要な経過秒数の基準値
        /// </summary>
        private const float BASE_AREA_AMMO_REFILL_INTERVAL = 0.5f;

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
        private const int AMMO_MAX_MULTIPLIER = 2;

        /// <summary>
        /// エリア内で弾薬が回復するまでに必要な経過秒数の倍率加算量
        /// </summary>
        private const float AREA_AMMO_REFILL_INTERVAL_MULTIPLIER = 0.1f;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>燃料が変更された瞬間に発火するイベント</summary>
        public event Action OnFuelChanged;

        /// <summary>弾薬が変更された瞬間に発火するイベント</summary>
        public event Action OnAmmoChanged;

        /// <summary>燃料が空になった瞬間に発火するイベント</summary>
        public event Action OnFuelEmptied;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 戦車ステータスを元にパラメーターを初期化
        /// </summary>
        /// <param name="tankStatus">燃料・弾薬算出に使用する戦車ステータス</param>
        public void InitialEnergyParameter(in TankStatus tankStatus)
        {
            // 最大値を計算
            _maxFuel = BASE_FUEL_MAX_VALUE + tankStatus.Fuel * FUEL_MAX_MULTIPLIER;
            _maxAmmo = BASE_AMMO_MAX_VALUE + tankStatus.Ammo * AMMO_MAX_MULTIPLIER;

            _currentFuel = _maxFuel;
            _currentAmmo = _maxAmmo;

            OnFuelChanged?.Invoke();
            OnAmmoChanged?.Invoke();
        }
        
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

            // 上限補正
            if (_currentFuel > _maxFuel)
            {
                _currentFuel = _maxFuel;
                OnFuelChanged?.Invoke();
            }
            if (_currentAmmo > _maxAmmo)
            {
                _currentAmmo = _maxAmmo;
                OnAmmoChanged?.Invoke();
            }
        }

        /// <summary>
        /// 燃料を消費
        /// </summary>
        /// <param name="amount">消費量</param>
        public void ConsumeFuel(in float amount = 0f)
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

                // 燃料が空になった時の専用通知
                OnFuelEmptied?.Invoke();
                return;
            }

            OnFuelChanged?.Invoke();
        }

        /// <summary>
        /// 弾薬を消費
        /// </summary>
        /// <param name="amount">消費量</param>
        public void ConsumeAmmo(in int amount = 1)
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

        /// <summary>
        /// エリア侵入時の燃料回復
        /// </summary>
        /// <param name="amount">回復量</param>
        public void RefillFuelByArea()
        {
            RefillFuel(REFILL_FUEL_VALUE_BY_AREA);
        }

        /// <summary>
        /// エリア侵入時の弾薬回復
        /// </summary>
        /// <param name="amount">回復量</param>
        public void RefillAmmoByArea()
        {
            // 不正値の場合は処理なし
            if (_currentAmmo < 0 || _currentAmmo >= _maxAmmo)
            {
                return;
            }

            // 経過時間を加算
            _areaIntrusionTimer += Time.deltaTime;

            // 回復インターバルを算出
            float refillInterval = BASE_AREA_AMMO_REFILL_INTERVAL
                + BASE_AREA_AMMO_REFILL_INTERVAL * AREA_AMMO_REFILL_INTERVAL_MULTIPLIER * (_currentAmmo - 1);

            if (_areaIntrusionTimer < refillInterval)
            {
                return;
            }

            // タイマーをリセット
            _areaIntrusionTimer = 0f;

            // 弾薬を 1 つ補充
            RefillAmmo(1);
        }
    }
}