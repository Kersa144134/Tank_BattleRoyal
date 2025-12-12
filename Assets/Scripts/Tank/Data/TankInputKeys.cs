// ======================================================
// TankInputKeys.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2025-12-08
// 概要     : ゲーム内で使用する入力キー名を一元管理する定数クラス
// ======================================================

using WeaponSystem.Data;

namespace TankSystem.Data
{
    /// <summary>
    /// 入力キー名を定数で管理
    /// </summary>
    public class TankInputKeys
    {
        /// <summary>オプションボタンキー</summary>
        public const string INPUT_OPTION = "Option";

        // 榴弾（Explosive）用ボタンキー
        public const string INPUT_EXPLOSIVE_FIRE = nameof(BulletType.Explosive) + "Fire";

        // 徹甲弾（Penetration）用ボタンキー
        public const string INPUT_PENETRATION_FIRE = nameof(BulletType.Penetration) + "Fire";

        // 誘導弾（Homing）用ボタンキー
        public const string INPUT_HOMING_FIRE = nameof(BulletType.Homing) + "Fire";
    }
}