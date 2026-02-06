// ======================================================
// CircleOBBCollisionCalculator.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-01-31
// 更新日時 : 2026-01-31
// 概要     : 円と OBB の水平重なり判定を担当する計算クラス
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using CollisionSystem.Interface;
using CollisionSystem.Utility;

namespace CollisionSystem.Calculator
{
    /// <summary>
    /// 円と OBB の水平重なり判定を行う計算クラス
    /// </summary>
    public sealed class CircleOBBCollisionCalculator
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>OBB の軸情報および射影計算を担当するユーティリティ</summary>
        private readonly OBBMath _obbMath;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 必要な数学ユーティリティを注入して初期化する
        /// </summary>
        public CircleOBBCollisionCalculator(in OBBMath obbMath)
        {
            _obbMath = obbMath;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定円と水平面上で重なっている OBB をすべて取得し、結果リストへ書き込む
        /// </summary>
        public void CollectOverlappingHorizontal(
            in Vector3 circleCenter,
            in float circleRadius,
            in IOBBData[] obbArray,
            ref List<IOBBData> results
        )
        {
            // 結果リストが存在しない場合は処理不能
            if (results == null)
            {
                return;
            }

            // 前回結果をクリア
            results.Clear();

            // 入力配列が存在しない場合は終了
            if (obbArray == null)
            {
                return;
            }

            // 半径二乗を事前計算
            float radiusSqr = circleRadius * circleRadius;

            // 全 OBB を順番に検査
            for (int index = 0; index < obbArray.Length; index++)
            {
                // 現在対象 OBB を取得
                IOBBData obb = obbArray[index];

                // Null 安全対策
                if (obb == null)
                {
                    continue;
                }

                // 円と OBB の重なり検出のみ実行
                if (IsOverlappingHorizontal(
                        circleCenter,
                        radiusSqr,
                        obb
                    ))
                {
                    // 重なっている OBB を結果へ追加
                    results.Add(obb);
                }
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 円と単体 OBB の水平重なり判定
        /// </summary>
        private bool IsOverlappingHorizontal(
            in Vector3 circleCenter,
            in float radiusSqr,
            in IOBBData obb
        )
        {
            // OBB のローカル軸をワールド空間で取得
            _obbMath.GetAxes(
                obb,
                out Vector3 rightAxis,
                out _,
                out Vector3 forwardAxis
            );

            // OBB 中心を取得
            // 水平判定のため、Y 座標を円中心と揃える
            Vector3 obbCenter = obb.Center;
            obbCenter.y = circleCenter.y;

            // 円中心から OBB 中心への差分ベクトル
            Vector3 delta = circleCenter - obbCenter;

            // 念のため水平成分のみに制限
            delta.y = 0.0f;

            // OBB ローカル空間での距離（Right 方向）
            float localX = Vector3.Dot(delta, rightAxis);

            // OBB ローカル空間での距離（Forward 方向）
            float localZ = Vector3.Dot(delta, forwardAxis);

            // OBB の半サイズ
            Vector3 half = obb.HalfSize;

            // OBB 内での最近点（ローカル）
            float closestX = Mathf.Clamp(localX, -half.x, half.x);
            float closestZ = Mathf.Clamp(localZ, -half.z, half.z);

            // ワールド空間での最近点
            Vector3 closestPoint =
                obbCenter +
                rightAxis * closestX +
                forwardAxis * closestZ;

            // 円中心から最近点への差分
            Vector3 diff = circleCenter - closestPoint;

            // 水平成分のみで距離判定
            diff.y = 0.0f;

            // 距離二乗
            float distSqr = diff.sqrMagnitude;

            // 半径以内なら重なり
            return distSqr <= radiusSqr;
        }
    }
}