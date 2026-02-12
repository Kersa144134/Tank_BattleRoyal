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

        /// <summary>汎用的な重なり量計算を担当する数学ユーティリティ</summary>
        private readonly OverlapMath _overlapMath;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 必要な数学ユーティリティを注入して初期化する
        /// </summary>
        public CircleOBBCollisionCalculator(
            in OBBMath obbMath,
            in OverlapMath overlapMath)
        {
            _obbMath = obbMath;
            _overlapMath = overlapMath;
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

            // 全 OBB を順番に検査
            for (int index = 0; index < obbArray.Length; index++)
            {
                // 現在対象 OBB を取得
                IOBBData obb = obbArray[index];

                if (obb == null)
                {
                    continue;
                }

                // 重なり量を算出
                float overlap = _overlapMath.CalculateCircleOBBHorizontalOverlap(circleCenter, circleRadius, obb);
                
                if (overlap > 0f)
                {
                    results.Add(obb);
                }
            }
        }
    }
}