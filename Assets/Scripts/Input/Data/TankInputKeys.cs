// ======================================================
// TankInputKeys.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2025-12-08
// 概要     : ゲーム内で使用する入力キー名を一元管理する定数クラス
// ======================================================

namespace InputSystem.Data
{
    /// <summary>
    /// 入力キー名を定数で管理
    /// </summary>
    public class TankInputKeys
    {
        /// <summary>入力切り替えボタンキー</summary>
        public const string INPUT_MODE_CHANGE = "InputModeChange";

        /// <summary>攻撃切り替えボタンキー</summary>
        public const string FIRE_MODE_CHANGE = "FireModeChange";
        
        /// <summary>オプションボタンキー</summary>
        public const string INPUT_OPTION = "Option";

        // 左攻撃用ボタンキー
        public const string INPUT_LEFT_FIRE = "LeftFire";

        // 右攻撃用ボタンキー
        public const string INPUT_RIGHT_FIRE = "RightFire";
    }
}