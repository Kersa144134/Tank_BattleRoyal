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
    /// アイテム取得でパラメーターを上昇させることを想定
    /// </summary>
    [System.Serializable]
    public class TankStatus
    {
        // ======================================================
        // パラメーター定義
        // ======================================================

        // --------------------------------------------------
        // エネルギー関連
        // --------------------------------------------------
        [Header("エネルギー関連")]
        /// <summary>燃料　本体の移動や攻撃に消費するエネルギー</summary>
        [SerializeField, Range(0, 30), Tooltip("燃料　本体の移動や攻撃に消費するエネルギー")]
        private int _fuel;

        /// <summary>弾薬　本体の攻撃可能回数</summary>
        [SerializeField, Range(0, 30), Tooltip("弾薬　本体の攻撃可能回数")]
        private int _ammo;

        // --------------------------------------------------
        // 防御力関連
        // --------------------------------------------------
        [Header("防御力関連")]
        /// <summary>耐久　本体の耐久力</summary>
        [SerializeField, Range(0, 30), Tooltip("耐久　本体の耐久力")]
        private int _durability;

        /// <summary>装甲　本体の防御力</summary>
        [SerializeField, Range(0, 30), Tooltip("装甲　本体の防御力")]
        private int _armor;

        // --------------------------------------------------
        // 機動力関連
        // --------------------------------------------------
        [Header("機動力関連")]
        /// <summary>馬力　本体の移動速度</summary>
        [SerializeField, Range(0, 30), Tooltip("馬力　本体の移動速度")]
        private int _horsePower;

        /// <summary>変速　本体の加速度</summary>
        [SerializeField, Range(0, 30), Tooltip("変速　本体の加速度")]
        private int _acceleration;

        // --------------------------------------------------
        // 攻撃力関連
        // --------------------------------------------------
        [Header("攻撃力関連")]
        /// <summary>砲身　砲身のスケール</summary>
        [SerializeField, Range(0, 30), Tooltip("砲身　砲身のスケール")]
        private int _barrelScale;

        /// <summary>質量　攻撃弾の質量</summary>
        [SerializeField, Range(0, 30), Tooltip("質量　攻撃弾の質量")]
        private int _projectileMass;

        /// <summary>装填　攻撃間隔</summary>
        [SerializeField, Range(0, 30), Tooltip("装填　攻撃間隔")]
        private int _reloadTime;

        // ======================================================
        // プロパティ定義
        // ======================================================

        // --------------------------------------------------
        // エネルギー関連
        // --------------------------------------------------
        /// <summary>燃料　本体の移動や攻撃に消費するエネルギー</summary>
        public int Fuel
        {
            get => _fuel;
            private set => _fuel = Mathf.Clamp(value, 0, 30);
        }

        /// <summary>弾薬　本体の攻撃可能回数</summary>
        public int Ammo
        {
            get => _ammo;
            private set => _ammo = Mathf.Clamp(value, 0, 30);
        }

        // --------------------------------------------------
        // 防御力関連
        // --------------------------------------------------
        /// <summary>耐久　本体の耐久力</summary>
        public int Durability
        {
            get => _durability;
            private set => _durability = Mathf.Clamp(value, 0, 30);
        }

        /// <summary>装甲　本体の防御力</summary>
        public int Armor
        {
            get => _armor;
            private set => _armor = Mathf.Clamp(value, 0, 30);
        }

        // --------------------------------------------------
        // 機動力関連
        // --------------------------------------------------
        /// <summary>馬力　本体の移動速度</summary>
        public int HorsePower
        {
            get => _horsePower;
            private set => _horsePower = Mathf.Clamp(value, 0, 30);
        }

        /// <summary>変速　本体の移動最高速度に達するまでの加速度</summary>
        public int Acceleration
        {
            get => _acceleration;
            private set => _acceleration = Mathf.Clamp(value, 0, 30);
        }

        // --------------------------------------------------
        // 攻撃力関連
        // --------------------------------------------------
        /// <summary>砲身　砲身のスケール</summary>
        public int BarrelScale
        {
            get => _barrelScale;
            private set => _barrelScale = Mathf.Clamp(value, 0, 30);
        }

        /// <summary>質量　攻撃弾の質量</summary>
        public int ProjectileMass
        {
            get => _projectileMass;
            private set => _projectileMass = Mathf.Clamp(value, 0, 30);
        }

        /// <summary>装填　攻撃間隔</summary>
        public int ReloadTime
        {
            get => _reloadTime;
            private set => _reloadTime = Mathf.Clamp(value, 0, 30);
        }

        // ======================================================
        // 辞書
        // ======================================================
        /// <summary>TankParam 列挙型の値をキーに、対応する TankStatus のプロパティ更新アクションを格納する辞書</summary>
        private readonly Dictionary<TankParam, Action<int>> _paramMap;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 全パラメーターを初期値0で初期化
        /// </summary>
        public TankStatus()
        {
            _fuel = 0; _ammo = 0; _durability = 0; _armor = 0;
            _horsePower = 0; _acceleration = 0; _barrelScale = 0; _projectileMass = 0; _reloadTime = 0;

            // TankParam とプロパティ更新アクションを紐付け
            _paramMap = new Dictionary<TankParam, Action<int>>
            {
                { TankParam.Fuel,           amount => Fuel += amount },
                { TankParam.Ammo,           amount => Ammo += amount },
                { TankParam.Durability,     amount => Durability += amount },
                { TankParam.Armor,          amount => Armor += amount },
                { TankParam.HorsePower,     amount => HorsePower += amount },
                { TankParam.Acceleration,   amount => Acceleration += amount },
                { TankParam.BarrelScale,    amount => BarrelScale += amount },
                { TankParam.ProjectileMass, amount => ProjectileMass += amount },
                { TankParam.ReloadTime,     amount => ReloadTime += amount },
            };
        }

        // ======================================================
        // パラメーター上昇処理
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