// ======================================================
// TankDefenseManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-05
// 更新日時 : 2026-01-09
// 概要     : 戦車の防御力を管理する
// ======================================================

using TankSystem.Data;

namespace TankSystem.Manager
{
    /// <summary>
    /// 戦車の防御力を管理するクラス
    /// </summary>
    public sealed class TankDefenseManager
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>現在の装甲値</summary>
        private float _currentDefense;

        /// <summary>ステータスから算出される装甲の最大値</summary>
        private float _maxDefense;

        // ======================================================
        // 定数
        // ======================================================

        // --------------------------------------------------
        // 基準値
        // --------------------------------------------------
        /// <summary>装甲ステータス 0 のときの基準装甲最大値</summary>
        private const float BASE_DEFENSE_MAX_VALUE = 20.0f;

        // --------------------------------------------------
        // パラメーター
        // --------------------------------------------------
        /// <summary>装甲ステータス 1 あたりの装甲最大値加算量</summary>
        private const float DEFENSE_MAX_VALUE_PER_STATUS = 9.0f;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 防御力管理クラスを生成する
        /// </summary>
        /// <param name="tankStatus">戦車のステータス</param>
        public TankDefenseManager(in TankStatus tankStatus)
        {
            // 初回はステータスから装甲最大値を算出
            UpdateDefenseParameter(tankStatus);

            // 初期装甲値は最大値に合わせる
            _currentDefense = _maxDefense;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// Armor ステータスを元に装甲の最大値を再計算する
        /// </summary>
        /// <param name="tankStatus">装甲算出に使用する戦車ステータス</param>
        public void UpdateDefenseParameter(in TankStatus tankStatus)
        {
            // TankStatus から装甲ステータス値を取得
            int armorStatus = tankStatus.Armor;

            // 変更前の最大装甲値を保持
            float previousMaxDefense = _maxDefense;

            // 基準値 + ステータス加算で装甲最大値を算出
            _maxDefense =
                BASE_DEFENSE_MAX_VALUE
                + armorStatus * DEFENSE_MAX_VALUE_PER_STATUS;

            // 最大値が増加した分だけ現在装甲も加算
            float increasedValue = _maxDefense - previousMaxDefense;

            // 強化による増加時のみ現在値を補正
            if (increasedValue > 0f)
            {
                _currentDefense += increasedValue;
            }

            // 現在装甲が最大値を超えないよう補正
            if (_currentDefense > _maxDefense)
            {
                _currentDefense = _maxDefense;
            }
        }

        /// <summary>
        /// 装甲値を考慮してダメージを軽減する
        /// </summary>
        /// <param name="rawDamage">元のダメージ量</param>
        /// <returns>軽減後のダメージ量</returns>
        public float CalculateReducedDamage(in float rawDamage)
        {
            // 無効なダメージは処理なし
            if (rawDamage <= 0f)
            {
                return 0f;
            }

            // 装甲が存在しない場合はそのまま返す
            if (_currentDefense <= 0f)
            {
                return rawDamage;
            }

            // 装甲値による軽減率を算出
            float reductionRate = _currentDefense / (_currentDefense + rawDamage);

            // 軽減後ダメージを算出
            float reducedDamage = rawDamage * (1f - reductionRate);

            return reducedDamage;
        }

        /// <summary>
        /// 装甲に直接ダメージを与える
        /// 装甲が少なくなるほど実際の減少量が小さくなる
        /// </summary>
        /// <param name="damage">装甲への基礎ダメージ量</param>
        public void ApplyArmorDamage(in float damage)
        {
            // 無効な値は処理なし
            if (damage <= 0f)
            {
                return;
            }

            // 装甲が存在しない場合は処理なし
            if (_currentDefense <= 0f)
            {
                return;
            }

            // 現在装甲の割合を算出
            float armorRatio = _currentDefense / _maxDefense;

            // 装甲割合に応じて実際の装甲ダメージを減衰
            float actualDamage = damage * armorRatio;

            // 装甲値を減算
            _currentDefense -= actualDamage;

            // 0 未満にならないよう補正
            if (_currentDefense < 0f)
            {
                _currentDefense = 0f;
            }
        }
    }
}