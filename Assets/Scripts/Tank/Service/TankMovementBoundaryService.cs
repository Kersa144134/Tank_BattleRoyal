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
        /// 戦車がステージ範囲外へ出た場合、半径内へ押し戻す
        /// </summary>
        /// <param name="target">移動対象の戦車 Transform</param>
        public void ClampPosition(Transform target)
        {
            // 現在位置を取得する
            Vector3 pos = target.position;

            // XZ 平面上での位置に変換する
            Vector3 flatPos = new Vector3(pos.x, 0f, pos.z);
            Vector3 flatCenter = Vector3.zero;

            // ステージ中心からの距離を計算する
            float distance = Vector3.Distance(flatPos, flatCenter);

            // 半径以内なら処理不要
            if (distance <= _stageRadius)
            {
                return;
            }

            // 範囲外の場合は円周上に押し戻すため方向ベクトルを計算
            Vector3 direction = (flatPos - flatCenter).normalized;

            // 円周位置を算出する
            Vector3 clampedPos = flatCenter + direction * _stageRadius;

            // 元のY座標を維持する
            clampedPos.y = pos.y;

            // 新しい位置を適用する
            target.position = clampedPos;
        }
    }
}