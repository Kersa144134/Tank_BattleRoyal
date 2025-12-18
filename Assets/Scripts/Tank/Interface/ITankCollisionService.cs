namespace TankSystem.Interface
{
    /// <summary>
    /// 戦車衝突処理サービス共通インターフェース
    /// </summary>
    public interface ITankCollisionService
    {
        /// <summary>
        /// 毎フレームの衝突更新処理
        /// </summary>
        void UpdateCollisionChecks();
    }
}