// ======================================================
// ICollisionService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : 衝突判定処理の共通フローを抽象化するインターフェース
// ======================================================

namespace CollisionSystem.Interface
{
    /// <summary>
    /// 戦車衝突判定における「判定処理単位」を表す共通インターフェース
    /// </summary>
    public interface ICollisionService
    {
        /// <summary>
        /// 衝突判定処理を 1 フレーム分実行する
        /// 判定構造は各 Service に委ねる
        /// </summary>
        void Execute();
    }
}