// ======================================================
// TankDurabilityManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-05
// 更新日時 : 2026-01-09
// 概要     : 戦車の耐久力を管理する
// ======================================================

using System;
using TankSystem.Data;

namespace TankSystem.Manager
{
    /// <summary>
    /// 戦車の耐久力を管理するクラス
    /// </summary>
    public sealed class TankDurabilityManager
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>現在の耐久力</summary>
        private float _currentDurability;

        /// <summary>最大耐久力</summary>
        private readonly float _maxDurability;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>現在の耐久力</summary>
        public float CurrentDurability => _currentDurability;

        /// <summary>耐久力が 0 以下かどうか</summary>
        public bool IsBroken => _currentDurability <= 0f;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>耐久力が 0 になった際に発火するイベント</summary>
        public event Action OnBroken;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 耐久力管理クラスを生成する
        /// </summary>
        /// <param name="tankStatus">戦車のステータス</param>
        public TankDurabilityManager(in TankStatus tankStatus)
        {
            // TankStatus から耐久を取得
            _maxDurability = tankStatus.Durability;

            // 初期耐久力を最大値に設定
            _currentDurability = _maxDurability;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ダメージを適用する
        /// </summary>
        /// <param name="damage">受けるダメージ量</param>
        public void ApplyDamage(in float damage)
        {
            // 無効なダメージは処理しない
            if (damage <= 0f)
            {
                return;
            }

            // すでに破壊されている場合は処理しない
            if (IsBroken)
            {
                return;
            }

            // 耐久力を減算
            _currentDurability -= damage;

            // 0 未満にならないよう補正
            if (_currentDurability < 0f)
            {
                _currentDurability = 0f;
            }

            // 耐久力が 0 になった場合
            if (IsBroken)
            {
                OnBroken?.Invoke();
            }
        }
    }
}
