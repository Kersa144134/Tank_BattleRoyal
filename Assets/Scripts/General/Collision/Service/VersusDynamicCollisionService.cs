// ======================================================
// VersusDynamicCollisionService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-21
// 更新日時 : 2025-12-21
// 概要     : 動的コンテキスト同士の OBB 衝突検知を行うサービス
// ======================================================

using System;
using System.Diagnostics;
using CollisionSystem.Calculator;
using CollisionSystem.Interface;
using WeaponSystem.Data;

namespace CollisionSystem.Service
{
    /// <summary>
    /// 動的コンテキスト同士の OBB 衝突検知を行うサービス
    /// 重複判定を防ぎつつ、検知結果のみを通知する
    /// </summary>
    public sealed class VersusDynamicCollisionService<TDynamicA, TDynamicB> : ICollisionService
        where TDynamicA : IDynamicCollisionContext
        where TDynamicB : IDynamicCollisionContext
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        private readonly BoundingBoxCollisionCalculator _boxCollisionCalculator;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>判定対象Aの動的コンテキスト配列</summary>
        private TDynamicA[] _contextsA;

        /// <summary>判定対象Bの動的コンテキスト配列</summary>
        private TDynamicB[] _contextsB;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// 動的コンテキスト同士が接触した際に通知されるイベント
        /// </summary>
        public event Action<TDynamicA, TDynamicB> OnDynamicHit;

        // ======================================================
        // コンストラクタ
        // ======================================================

        public VersusDynamicCollisionService(in BoundingBoxCollisionCalculator boxCollisionCalculator)
        {
            _boxCollisionCalculator = boxCollisionCalculator;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 判定前処理
        /// 動的コンテキスト配列を受け取り OBB 更新を行う
        /// </summary>
        /// <param name="contexts">判定対象となる動的コンテキスト配列</param>
        public void PreUpdate(in TDynamicA[] contextsA, in TDynamicB[] contextsB)
        {
            _contextsA = contextsA;
            _contextsB = contextsB;

            // 全ての動的コンテキストの OBB を更新
            if (_contextsA != null)
            {
                for (int i = 0; i < _contextsA.Length; i++)
                {
                    _contextsA[i]?.UpdateOBB();
                }
            }

            if (_contextsB != null)
            {
                for (int i = 0; i < _contextsB.Length; i++)
                {
                    _contextsB[i]?.UpdateOBB();
                }
            }
        }

        /// <summary>
        /// 動的コンテキスト同士の衝突判定を実行する
        /// </summary>
        public void Execute()
        {
            if (_contextsA == null || _contextsB == null)
            {
                return;
            }

            // --------------------------------------------------
            // 動的 × 動的 判定ループ
            // --------------------------------------------------
            // A と B が同じ配列なら重複防止のため i+1 からループ
            bool sameArray = ReferenceEquals(_contextsA, _contextsB);

            for (int i = 0; i < _contextsA.Length; i++)
            {
                TDynamicA a = _contextsA[i];

                int startJ = sameArray ? i + 1 : 0;
                for (int j = startJ; j < _contextsB.Length; j++)
                {
                    TDynamicB b = _contextsB[j];

                    if (!_boxCollisionCalculator.IsCollidingHorizontal(a.OBB, b.OBB))
                    {
                        continue;
                    }

                    OnDynamicHit?.Invoke(a, b);
                }
            }
        }
    }
}