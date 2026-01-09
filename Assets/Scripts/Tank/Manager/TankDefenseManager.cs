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

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>現在の装甲値</summary>
        public float CurrentDefense => _currentDefense;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 防御力管理クラスを生成する
        /// </summary>
        /// <param name="tankStatus">戦車のステータス</param>
        public TankDefenseManager(in TankStatus tankStatus)
        {
            // TankStatus から装甲値を取得
            _currentDefense = tankStatus.Armor;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 装甲値を考慮してダメージを軽減する
        /// </summary>
        /// <param name="rawDamage">元のダメージ量</param>
        /// <returns>軽減後のダメージ量</returns>
        public float CalculateReducedDamage(in float rawDamage)
        {
            // 無効なダメージはそのまま返す
            if (rawDamage <= 0f)
            {
                return 0f;
            }

            // 装甲が存在しない場合は軽減しない
            if (_currentDefense <= 0f)
            {
                return rawDamage;
            }

            // 装甲値による軽減率を算出
            float reductionRate = _currentDefense / (_currentDefense + rawDamage);

            // 軽減後ダメージを算出
            float reducedDamage = rawDamage * (1f - reductionRate);

            // 最低 1 ダメージ保証（必要なければ削除可）
            if (reducedDamage < 1f)
            {
                reducedDamage = 1f;
            }

            return reducedDamage;
        }
    }
}