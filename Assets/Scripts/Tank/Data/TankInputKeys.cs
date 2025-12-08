// ======================================================
// TankInputKeys.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2025-12-08
// 概要     : ゲーム内で使用する入力キー名を一元管理する定数クラス
// ======================================================

namespace TankSystem.Data
{
    /// <summary>
    /// 入力キー名を定数で管理
    /// </summary>
    public class TankInputKeys
    {
        /// <summary>オプションボタンキー</summary>
        public const string INPUT_OPTION = "Option";

        /// <summary>榴弾攻撃ボタンキー</summary>
        public const string INPUT_HE_FIRE = "HEFire";

        /// <summary>徹甲弾攻撃ボタンキー</summary>
        public const string INPUT_AP_FIRE = "APFire";
    }
}