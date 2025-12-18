// ======================================================
// ITankCollisionService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : 戦車衝突判定処理の共通フローを抽象化するインターフェース
// ======================================================

using TankSystem.Data;

namespace TankSystem.Interface
{
    /// <summary>
    /// 戦車衝突判定における「判定処理単位」を表す共通インターフェース
    /// </summary>
    public interface ITankCollisionService
    {
        /// <summary>
        /// 判定前処理
        /// OBB 更新やキャッシュ初期化を行う
        /// </summary>
        void PreUpdate();

        /// <summary>
        /// 衝突判定処理を 1 フレーム分実行する
        /// 判定構造は各 Service に委ねる
        /// </summary>
        void Execute();
    }
}