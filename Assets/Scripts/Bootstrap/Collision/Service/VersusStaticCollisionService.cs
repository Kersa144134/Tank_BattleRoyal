// ======================================================
// VersusStaticCollisionService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-21
// 概要     : 動的オブジェクトと静的オブジェクトの
//            OBB 衝突検知を担当するサービス
// ======================================================

using System;
using CollisionSystem.Calculator;
using CollisionSystem.Interface;

namespace CollisionSystem.Service
{
    /// <summary>
    /// 動的オブジェクトと静的オブジェクトの
    /// OBB 衝突検知を専門に行うサービス
    /// </summary>
    public sealed class VersusStaticCollisionService<TDynamic, TStatic>
        : ICollisionService
        where TDynamic : IDynamicCollisionContext
        where TStatic : IStaticCollisionContext
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>
        /// OBB 同士の水平方向衝突判定を行う計算器
        /// </summary>
        private readonly BoundingBoxCollisionCalculator _boxCollisionCalculator;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>動的衝突コンテキスト配列</summary>
        private TDynamic[] _dynamics;

        /// <summary>静的衝突コンテキスト配列</summary>
        private TStatic[] _statics;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// 動的オブジェクトが静的オブジェクトと接触した際に通知されるイベント
        /// </summary>
        public event Action<TDynamic, TStatic> OnStaticHit;

        // ======================================================
        // コンストラクタ
        // ======================================================

        public VersusStaticCollisionService(
            in BoundingBoxCollisionCalculator boxCollisionCalculator
        )
        {
            _boxCollisionCalculator = boxCollisionCalculator;
        }

        // ======================================================
        // ICollisionService イベント
        // ======================================================

        /// <summary>
        /// 判定ループ開始前の事前処理
        /// 動的コンテキストと静的コンテキストを更新する
        /// </summary>
        /// <param name="dynamics">動的衝突コンテキスト配列</param>
        /// <param name="statics">静的衝突コンテキスト配列</param>
        public void PreUpdate(in TDynamic[] dynamics, in TStatic[] statics)
        {
            _dynamics = dynamics;
            _statics = statics;

            if (_dynamics == null)
            {
                return;
            }

            // 動的オブジェクトの OBB を更新
            for (int i = 0; i < _dynamics.Length; i++)
            {
                _dynamics[i].UpdateOBB();
            }
        }

        /// <summary>
        /// 動的オブジェクトと静的オブジェクトの衝突判定を実行する
        /// </summary>
        /// <param name="statics">判定対象となる静的オブジェクト群</param>
        public void Execute()
        {
            if (_dynamics == null || _statics == null)
            {
                return;
            }

            for (int i = 0; i < _dynamics.Length; i++)
            {
                TDynamic dynamicContext = _dynamics[i];

                for (int s = 0; s < _statics.Length; s++)
                {
                    TStatic staticContext = _statics[s];

                    if (staticContext == null)
                    {
                        continue;
                    }

                    if (!_boxCollisionCalculator.IsCollidingHorizontal(
                        dynamicContext.OBB,
                        staticContext.OBB
                    ))
                    {
                        continue;
                    }

                    OnStaticHit?.Invoke(
                        dynamicContext,
                        staticContext
                    );
                }
            }
        }
    }
}