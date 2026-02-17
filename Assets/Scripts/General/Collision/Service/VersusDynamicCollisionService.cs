// ======================================================
// VersusDynamicCollisionService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-21
// 更新日時 : 2026-02-18
// 概要     : 動的コンテキスト同士の OBB 衝突検知を行うサービス
// ======================================================

using System;
using UnityEngine;
using CollisionSystem.Calculator;
using CollisionSystem.Data;
using CollisionSystem.Interface;

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

        /// <summary>OBB の衝突判定を計算するクラス</summary>
        private readonly CollisionCalculator _collisionCalculator;

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

        public VersusDynamicCollisionService(in CollisionCalculator collisionCalculator)
        {
            _collisionCalculator = collisionCalculator;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 判定ループ開始前の事前処理
        /// </summary>
        /// <param name="contextsA">動的衝突コンテキスト配列 A 配列</param>
        /// <param name="contextsB">動的衝突コンテキスト配列 B 配列</param>
        public void PreUpdate(in TDynamicA[] contextsA, in TDynamicB[] contextsB)
        {
            _contextsA = contextsA;
            _contextsB = contextsB;

            // A 配列の OBB 更新
            if (_contextsA != null)
            {
                for (int i = 0; i < _contextsA.Length; i++)
                {
                    _contextsA[i]?.UpdateOBB();
                }
            }

            // B 配列の OBB 更新
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

            // A と B が同一参照かを判定
            bool isSameArray =
                ReferenceEquals(_contextsA, _contextsB);

            // --------------------------------------------------
            // A 配列走査
            // --------------------------------------------------
            for (int i = 0; i < _contextsA.Length; i++)
            {
                TDynamicA contextA =
                    _contextsA[i];

                BaseOBBData obbA =
                    contextA.OBB;

                // 同一配列の場合は i+1 から開始し重複判定を防止する
                int startIndex =
                    isSameArray ? i + 1 : 0;

                // --------------------------------------------------
                // B 配列走査
                // --------------------------------------------------
                for (int j = startIndex; j < _contextsB.Length; j++)
                {
                    TDynamicB contextB =
                        _contextsB[j];

                    BaseOBBData obbB =
                        contextB.OBB;

                    // --------------------------------------------------
                    // ブロードフェーズ
                    // 距離判定
                    // 平方根回避のため二乗を算出
                    // --------------------------------------------------
                    // 中心間ベクトルを算出
                    Vector3 centerDelta =
                        obbA.Center - obbB.Center;

                    // 中心間距離の二乗を算出
                    float sqrDistance =
                        centerDelta.sqrMagnitude;

                    // 半径の合計値を算出
                    float radiusSum =
                        obbA.BoundingRadius + obbB.BoundingRadius;

                    // 半径合計の二乗値を算出
                    float sqrRadiusSum =
                        radiusSum * radiusSum;

                    // 半径範囲外であれば SAT を実行せずスキップ
                    if (sqrDistance > sqrRadiusSum)
                    {
                        continue;
                    }

                    // --------------------------------------------------
                    // ナローフェーズ
                    // SAT判定
                    // --------------------------------------------------
                    // OBB 同士が水平面上で衝突しているか判定する
                    bool isColliding =
                        _collisionCalculator.IsCollidingHorizontal(
                            obbA,
                            obbB
                        );

                    // 衝突していない場合はスキップ
                    if (!isColliding)
                    {
                        continue;
                    }

                    OnDynamicHit?.Invoke(
                        contextA,
                        contextB
                    );
                }
            }
        }
    }
}