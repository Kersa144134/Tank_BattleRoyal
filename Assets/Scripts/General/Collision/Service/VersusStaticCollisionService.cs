// ======================================================
// VersusStaticCollisionService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-21
// 更新日時 : 2026-02-18
// 概要     : 動的オブジェクトと静的オブジェクトの
//            OBB 衝突検知を担当するサービス
// ======================================================

using System;
using UnityEngine;
using CollisionSystem.Calculator;
using CollisionSystem.Data;
using CollisionSystem.Interface;

namespace CollisionSystem.Service
{
    /// <summary>
    /// 動的オブジェクトと静的オブジェクトの
    /// OBB 衝突検知を専門に行うサービス
    /// </summary>
    public sealed class VersusStaticCollisionService<TDynamic, TStatic> : ICollisionService
        where TDynamic : IDynamicCollisionContext
        where TStatic : IStaticCollisionContext
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>OBB の衝突判定を計算するクラス</summary>
        private readonly CollisionCalculator _collisionCalculator;

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

        public VersusStaticCollisionService(in CollisionCalculator collisionCalculator)
        {
            _collisionCalculator = collisionCalculator;
        }

        // ======================================================
        // ICollisionService イベント
        // ======================================================

        /// <summary>
        /// 判定ループ開始前の事前処理
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
        public void Execute()
        {
            if (_dynamics == null || _statics == null)
            {
                return;
            }

            // --------------------------------------------------
            // 動的 OBB 配列走査
            // --------------------------------------------------
            for (int i = 0; i < _dynamics.Length; i++)
            {
                TDynamic dynamicContext =
                    _dynamics[i];

                BaseOBBData obbDynamic =
                    dynamicContext.OBB;

                // --------------------------------------------------
                // 静的 OBB 配列走査
                // --------------------------------------------------
                for (int s = 0; s < _statics.Length; s++)
                {
                    TStatic staticContext =
                        _statics[s];

                    if (staticContext == null)
                    {
                        continue;
                    }

                    BaseOBBData obbStatic =
                        staticContext.OBB;

                    // --------------------------------------------------
                    // ブロードフェーズ
                    // 距離判定
                    // --------------------------------------------------
                    // 中心間ベクトルを算出
                    Vector3 centerDelta =
                        obbDynamic.Center - obbStatic.Center;

                    // 中心間距離の二乗を算出
                    float sqrDistance =
                        centerDelta.sqrMagnitude;

                    // 半径の合計値を算出
                    float radiusSum =
                        obbDynamic.BoundingRadius + obbStatic.BoundingRadius;

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
                            obbDynamic,
                            obbStatic
                        );

                    // 衝突していない場合はスキップ
                    if (!isColliding)
                    {
                        continue;
                    }

                    // 衝突検知イベントを発行
                    OnStaticHit?.Invoke(
                        dynamicContext,
                        staticContext
                    );
                }
            }
        }
    }
}