// ======================================================
// ICollisionService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : 衝突判定処理の共通フローを抽象化するインターフェース
// ======================================================

using CollisionSystem.Data;

namespace CollisionSystem.Interface
{
    /// <summary>
    /// 戦車衝突判定における「判定処理単位」を表す共通インターフェース
    /// </summary>
    public interface ICollisionService
    {
        /// <summary>
        /// 判定前処理
        /// 動的コンテキストは必ず OBB 更新、静的コンテキストは初期化のみ
        /// </summary>
        /// <typeparam name="TDynamic">動的衝突コンテキスト型</typeparam>
        /// <typeparam name="TStatic">静的衝突コンテキスト型</typeparam>
        /// <param name="dynamics">動的コンテキスト配列</param>
        /// <param name="statics">静的コンテキスト配列</param>
        public static void PreUpdate<TDynamic, TStatic>(
            TDynamic[] dynamics,
            TStatic[] statics = null
        )
            where TDynamic : IDynamicCollisionContext
            where TStatic : IStaticCollisionContext
        {
            // --------------------------------------------------
            // 動的コンテキスト OBB 更新
            // --------------------------------------------------
            if (dynamics != null)
            {
                for (int i = 0; i < dynamics.Length; i++)
                {
                    dynamics[i]?.UpdateOBB();
                }
            }

            // --------------------------------------------------
            // 静的コンテキストの初期化
            // --------------------------------------------------
            if (statics != null)
            {
                for (int i = 0; i < statics.Length; i++)
                {
                    TStatic staticContext = statics[i];
                    if (staticContext == null)
                    {
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// 衝突判定処理を 1 フレーム分実行する
        /// 判定構造は各 Service に委ねる
        /// </summary>
        void Execute();
    }
}