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

        /// <summary>現在の耐久値</summary>
        private float _currentDurability;

        /// <summary>ステータスから算出される耐久の最大値</summary>
        private float _maxDurability;

        /// <summary>耐久力が 0 以下かどうか</summary>
        private bool _isBroken => _currentDurability <= 0f;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>現在の耐久値</summary>
        public float CurrentDurability => _currentDurability;

        /// <summary>ステータスから算出される耐久の最大値</summary>
        public float MaxDurability => _maxDurability;

        // ======================================================
        // 定数
        // ======================================================

        // --------------------------------------------------
        // 基準値
        // --------------------------------------------------
        /// <summary>耐久ステータス 0 のときの基準耐久最大値</summary>
        private const float BASE_DURABILITY_MAX_VALUE = 40.0f;

        // --------------------------------------------------
        // パラメーター
        // --------------------------------------------------
        /// <summary>耐久ステータス 1 あたりの耐久最大値加算量</summary>
        private const float DURABILITY_MAX_VALUE_PER_STATUS = 8f;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>耐久力が変更された瞬間に発火するイベント</summary>
        public event Action OnDurabilityChanged;
        
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
            // 初回はステータスから耐久最大値を算出
            UpdateDurabilityParameter(tankStatus);
            
            // 初期耐久力を最大値に設定
            _currentDurability = _maxDurability;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// Durability ステータスを元に
        /// 耐久の最大値を再計算する
        /// </summary>
        /// <param name="tankStatus">耐久算出に使用する戦車ステータス</param>
        public void UpdateDurabilityParameter(in TankStatus tankStatus)
        {
            // TankStatus から耐久ステータス値を取得
            int durabilityStatus = tankStatus.Durability;

            // 変更前の最大耐久値を保持
            float previousMaxDurability = _maxDurability;

            // 基準値 + ステータス加算で耐久最大値を算出
            _maxDurability =
                BASE_DURABILITY_MAX_VALUE
                + durabilityStatus * DURABILITY_MAX_VALUE_PER_STATUS;

            // 最大値が増加した分だけ現在耐久も加算する
            float increasedValue = _maxDurability - previousMaxDurability;

            // 最大値が増えている場合のみ現在値を補正
            if (increasedValue > 0f)
            {
                _currentDurability += increasedValue;
            }

            // 現在耐久が最大値を超えないよう補正
            if (_currentDurability > _maxDurability)
            {
                _currentDurability = _maxDurability;
            }

            // 耐久力変更イベント発火
            OnDurabilityChanged?.Invoke();
        }
        
        /// <summary>
        /// ダメージを適用する
        /// </summary>
        /// <param name="damage">受けるダメージ量</param>
        public void ApplyDamage(in float damage)
        {
            // 無効な値は処理なし
            if (damage <= 0f)
            {
                return;
            }

            // すでに破壊されている場合は処理なし
            if (_isBroken)
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

            // 耐久力変更イベント発火
            OnDurabilityChanged?.Invoke();
            
            // 耐久力が 0 になった場合
            if (_isBroken)
            {
                OnBroken?.Invoke();
            }
        }
    }
}
