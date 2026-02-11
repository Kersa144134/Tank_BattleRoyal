// ======================================================
// TankParam.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-11
// 更新日時 : 2025-12-11
// 概要     : 戦車のパラメーター種別
// ======================================================

namespace TankSystem.Data
{
    /// <summary>
    /// 戦車のパラメーター種別
    /// </summary>
    public enum TankParam
    {
        // 燃料
        Fuel,
        // 弾薬
        Ammo,
        // 耐久
        Durability,
        // 装甲
        Armor,
        // 馬力
        HorsePower,
        // 変速
        Transmission,
        // 砲身
        Barrel,
        // 質量
        ProjectileMass,
        // 装填
        ReloadTime
    }
}