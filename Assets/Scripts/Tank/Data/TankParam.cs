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
        Fuel,           // 燃料
        Ammo,           // 弾薬
        Durability,     // 耐久
        Armor,          // 装甲
        HorsePower,     // 馬力
        Transmission,   // 変速
        Barrel,         // 砲身
        ProjectileMass, // 質量
        ReloadTime      // 装填
    }
}