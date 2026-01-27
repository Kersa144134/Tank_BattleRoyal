// ======================================================
// TankMovementBoundaryService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-12
// 更新日時 : 2025-12-12
// 概要     : タンク専用の移動範囲制限サービス
//            ステージ中心から一定距離を超えた場合、戦車を境界円周内まで押し戻す
// ======================================================

using UnityEngine;

namespace TankSystem.Service
{
    /// <summary>
    /// 戦車の移動がステージ範囲外に出ることを防止するサービス
    /// </summary>
    public class TankMovementBoundaryService
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>戦車が移動できる許容半径</summary>
        private readonly float _stageRadius;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// ステージ中心座標と許容半径を指定して初期化する
        /// </summary>
        /// <param name="radius">移動許容半径</param>
        public TankMovementBoundaryService(in float radius)
        {
            // 許容される移動半径を保持
            _stageRadius = radius;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 戦車の予定位置がステージ範囲外の場合、
        /// 半径内に収まるよう補正した位置を計算して返す
        /// </summary>
        /// <param name="plannedPosition">移動後に予定されているワールド座標</param>
        /// <param name="clampedPosition">範囲制限後のワールド座標</param>
        public void ClampPlannedPosition(
            in Vector3 plannedPosition,
            out Vector3 clampedPosition
        )
        {
            // 高さ成分を除いた平面位置を生成する
            Vector3 flatPosition = new Vector3(
                plannedPosition.x,
                0f,
                plannedPosition.z
            );

            // ステージ中心を設定
            Vector3 flatCenter = Vector3.zero;

            // ステージ中心からの距離を算出する
            float distanceFromCenter = Vector3.Distance(
                flatPosition,
                flatCenter
            );

            // --------------------------------------------------
            // 半径内判定
            // --------------------------------------------------
            // ステージ半径以内であれば補正なし
            if (distanceFromCenter <= _stageRadius)
            {
                clampedPosition = plannedPosition;
                return;
            }

            // --------------------------------------------------
            // 範囲外の場合の押し戻し計算
            // --------------------------------------------------
            // 中心から外向きの正規化方向ベクトルを算出する
            Vector3 outwardDirection = (flatPosition - flatCenter).normalized;

            // 円周上の補正後平面位置を算出する
            Vector3 correctedFlatPosition =
                flatCenter + outwardDirection * _stageRadius;

            // 元の Y 座標を維持した最終位置を生成する
            clampedPosition = new Vector3(
                correctedFlatPosition.x,
                plannedPosition.y,
                correctedFlatPosition.z
            );
        }
    }
}