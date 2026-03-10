// ======================================================
// TankStatus.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-10
// 更新日時 : 2025-12-10
// 概要     : 戦車のゲーム中に変動する各種パラメーターを管理するクラス
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TankSystem.Data
{
    /// <summary>
    /// 戦車のゲーム中に変動するパラメーターを管理するクラス
    /// </summary>
    [Serializable]
    public class TankStatus
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        // --------------------------------------------------
        // エネルギー関連
        // --------------------------------------------------
        [Header("エネルギー関連")]
        /// <summary>燃料　本体の移動や攻撃に消費するエネルギー</summary>
        [SerializeField, Range(MIN_PARAMETER_VALUE, MAX_PARAMETER_VALUE), Tooltip("燃料　本体の移動や攻撃に消費するエネルギー")]
        private int _fuel;

        /// <summary>弾薬　本体の攻撃可能回数</summary>
        [SerializeField, Range(MIN_PARAMETER_VALUE, MAX_PARAMETER_VALUE), Tooltip("弾薬　本体の攻撃可能回数")]
        private int _ammo;

        // --------------------------------------------------
        // 防御力関連
        // --------------------------------------------------
        [Header("防御力関連")]
        /// <summary>耐久　本体の耐久力</summary>
        [SerializeField, Range(MIN_PARAMETER_VALUE, MAX_PARAMETER_VALUE), Tooltip("耐久　本体の耐久力")]
        private int _durability;

        /// <summary>装甲　本体の防御力</summary>
        [SerializeField, Range(MIN_PARAMETER_VALUE, MAX_PARAMETER_VALUE), Tooltip("装甲　本体の防御力")]
        private int _armor;

        // --------------------------------------------------
        // 機動力関連
        // --------------------------------------------------
        [Header("機動力関連")]
        /// <summary>馬力　本体の移動速度</summary>
        [SerializeField, Range(MIN_PARAMETER_VALUE, MAX_PARAMETER_VALUE), Tooltip("馬力　本体の移動速度")]
        private int _horsePower;

        /// <summary>変速　本体の加減速度</summary>
        [SerializeField, Range(MIN_PARAMETER_VALUE, MAX_PARAMETER_VALUE), Tooltip("変速　本体の加減速度")]
        private int _transmission;

        // --------------------------------------------------
        // 攻撃力関連
        // --------------------------------------------------
        [Header("攻撃力関連")]
        /// <summary>砲身　砲身のスケール</summary>
        [SerializeField, Range(MIN_PARAMETER_VALUE, MAX_PARAMETER_VALUE), Tooltip("砲身　砲身のスケール")]
        private int _barrel;

        /// <summary>質量　攻撃弾の質量</summary>
        [SerializeField, Range(MIN_PARAMETER_VALUE, MAX_PARAMETER_VALUE), Tooltip("質量　攻撃弾の質量")]
        private int _projectileMass;

        /// <summary>装填　攻撃間隔</summary>
        [SerializeField, Range(MIN_PARAMETER_VALUE, MAX_PARAMETER_VALUE), Tooltip("装填　攻撃間隔")]
        private int _reloadTime;

        // ======================================================
        // フィールド
        // ======================================================

        // ======================================================
        // プロパティ
        // ======================================================

        // --------------------------------------------------
        // エネルギー関連
        // --------------------------------------------------
        /// <summary>燃料　本体の移動や攻撃に消費するエネルギー</summary>
        public int Fuel
        {
            get => _fuel;
            private set => _fuel = Mathf.Clamp(value, MIN_PARAMETER_VALUE, MAX_PARAMETER_VALUE);
        }

        /// <summary>弾薬　本体の攻撃可能回数</summary>
        public int Ammo
        {
            get => _ammo;
            private set => _ammo = Mathf.Clamp(value, MIN_PARAMETER_VALUE, MAX_PARAMETER_VALUE);
        }

        // --------------------------------------------------
        // 防御力関連
        // --------------------------------------------------
        /// <summary>耐久　本体の耐久力</summary>
        public int Durability
        {
            get => _durability;
            private set => _durability = Mathf.Clamp(value, MIN_PARAMETER_VALUE, MAX_PARAMETER_VALUE);
        }

        /// <summary>装甲　本体の防御力</summary>
        public int Armor
        {
            get => _armor;
            private set => _armor = Mathf.Clamp(value, MIN_PARAMETER_VALUE, MAX_PARAMETER_VALUE);
        }

        // --------------------------------------------------
        // 機動力関連
        // --------------------------------------------------
        /// <summary>馬力　本体の移動速度</summary>
        public int HorsePower
        {
            get => _horsePower;
            private set => _horsePower = Mathf.Clamp(value, MIN_PARAMETER_VALUE, MAX_PARAMETER_VALUE);
        }

        /// <summary>変速　本体の加減速度</summary>
        public int Transmission
        {
            get => _transmission;
            private set => _transmission = Mathf.Clamp(value, MIN_PARAMETER_VALUE, MAX_PARAMETER_VALUE);
        }

        // --------------------------------------------------
        // 攻撃力関連
        // --------------------------------------------------
        /// <summary>砲身　砲身のスケール</summary>
        public int Barrel
        {
            get => _barrel;
            private set => _barrel = Mathf.Clamp(value, MIN_PARAMETER_VALUE, MAX_PARAMETER_VALUE);
        }

        /// <summary>質量　攻撃弾の質量</summary>
        public int ProjectileMass
        {
            get => _projectileMass;
            private set => _projectileMass = Mathf.Clamp(value, MIN_PARAMETER_VALUE, MAX_PARAMETER_VALUE);
        }

        /// <summary>装填　攻撃間隔</summary>
        public int ReloadTime
        {
            get => _reloadTime;
            private set => _reloadTime = Mathf.Clamp(value, MIN_PARAMETER_VALUE, MAX_PARAMETER_VALUE);
        }

        // --------------------------------------------------
        // 内部パラメーター
        // --------------------------------------------------
        /// <summary>戦車本体の重量</summary>
        public float Weight
        {
            get
            {
                return BASE_WEIGHT
                    + (_fuel * FUEL_WEIGHT)
                    + (_ammo * AMMO_WEIGHT)
                    + (_durability * DURABILITY_WEIGHT)
                    + (_armor * ARMOR_WEIGHT)
                    + (_horsePower * HORSEPOWER_WEIGHT)
                    + (_transmission * TRANSMISSION_WEIGHT)
                    + (_barrel * BARREL_WEIGHT)
                    + (_projectileMass * PROJECTILE_MASS_WEIGHT)
                    + (_reloadTime * RELOAD_TIME_WEIGHT);
            }
        }

        // ======================================================
        // 定数
        // ======================================================

        // --------------------------------------------------
        // パラメーター
        // --------------------------------------------------
        /// <summary>パラメーター最小値</summary>
        private const int MIN_PARAMETER_VALUE = 0;

        /// <summary>パラメーター最大値</summary>
        private const int MAX_PARAMETER_VALUE = 20;

        // --------------------------------------------------
        // 重量
        // --------------------------------------------------
        /// 戦車の基準重量
        private const float BASE_WEIGHT = 50f;

        /// 燃料 1 あたりの重量
        private const float FUEL_WEIGHT = 1.0f;

        /// 弾薬 1 あたりの重量
        private const float AMMO_WEIGHT = 1.0f;

        /// 耐久 1 あたりの重量
        private const float DURABILITY_WEIGHT = 1.5f;

        /// 装甲 1 あたりの重量
        private const float ARMOR_WEIGHT = 1.5f;

        /// 馬力 1 あたりの重量
        private const float HORSEPOWER_WEIGHT = 0.5f;

        /// 変速 1 あたりの重量
        private const float TRANSMISSION_WEIGHT = 0.5f;

        /// 砲身 1 あたりの重量
        private const float BARREL_WEIGHT = 2.0f;

        /// 質量 1 あたりの重量
        private const float PROJECTILE_MASS_WEIGHT = 2.5f;

        /// 装填 1 あたりの重量
        private const float RELOAD_TIME_WEIGHT = 2.0f;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>TankParam 列挙型の値をキーに、対応する TankStatus のプロパティ更新アクションを格納する辞書</summary>
        private readonly Dictionary<TankParam, Action<int>> _paramMap;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 全パラメーターを初期値 0 で初期化
        /// </summary>
        public TankStatus()
        {
            _fuel = 0;
            _ammo = 0;
            _durability = 0;
            _armor = 0;
            _horsePower = 0;
            _transmission = 0;
            _barrel = 0;
            _projectileMass = 0;
            _reloadTime = 0;

            // TankParam とパラメーター更新を紐付け
            _paramMap = new Dictionary<TankParam, Action<int>>
            {
                { TankParam.Fuel,           amount => Fuel += amount },
                { TankParam.Ammo,           amount => Ammo += amount },
                { TankParam.Durability,     amount => Durability += amount },
                { TankParam.Armor,          amount => Armor += amount },
                { TankParam.HorsePower,     amount => HorsePower += amount },
                { TankParam.Transmission,   amount => Transmission += amount },
                { TankParam.Barrel,         amount => Barrel += amount },
                { TankParam.ProjectileMass, amount => ProjectileMass += amount },
                { TankParam.ReloadTime,     amount => ReloadTime += amount },
            };
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 任意のパラメーターを指定量だけ増加
        /// </summary>
        /// <param name="param">増加させるパラメーター</param>
        /// <param name="amount">増加量</param>
        public void IncreaseParameter(in TankParam param, in int amount)
        {
            if (_paramMap.TryGetValue(param, out Action<int> setter))
            {
                setter(amount);
            }
        }
    }
}