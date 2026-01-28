// ======================================================
// TankTurretController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-10
// 更新日時 : 2026-01-28
// 概要     : 砲塔の回転制御を担当するクラス
// ======================================================

using TankSystem.Data;
using UnityEngine;

namespace TankSystem.Controller
{
    /// <summary>
    /// 戦車の砲塔制御を行うコントローラー
    /// </summary>
    public sealed class TankTurretController
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>砲塔回転入力に対する回転倍率</summary>
        private const float ROTATION_INPUT_MULTIPLIER = 20.0f;

        /// <summary>砲塔の最大回転許容角度（左右共通）</summary>
        private const float MAX_ROTATION_ANGLE = 60.0f;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>回転対象となる砲塔の Transform</summary>
        private readonly Transform _turretTransform;

        /// <summary>砲塔の初期ローカル Y 回転角</summary>
        private readonly float _defaultLocalYAngle;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 砲塔回転コントローラーを生成する
        /// </summary>
        /// <param name="turretTransform">砲塔の Transform</param>
        public TankTurretController(in Transform turretTransform)
        {
            // 砲塔 Transform を保持
            _turretTransform = turretTransform;

            // 初期状態のローカル Y 回転角を保存
            _defaultLocalYAngle = _turretTransform.localEulerAngles.y;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// BarrelScale ステータスを元に砲塔スケールを再計算する
        /// </summary>
        /// <param name="tankStatus">砲塔スケールに使用する戦車ステータス</param>
        public void UpdateTurretParameter(in TankStatus tankStatus)
        {
        }

        /// <summary>
        /// 入力値を元に砲塔を回転させる
        /// </summary>
        /// <param name="deltaTime">フレーム時間</param>
        /// <param name="rotationInput">
        /// 回転入力値  
        /// -1 ～ 1 を想定（左回転 / 右回転）
        /// </param>
        public void Rotate(
            in float deltaTime,
            in float rotationInput
        )
        {
            // 入力が無い場合は処理しない
            if (Mathf.Approximately(rotationInput, 0f))
            {
                return;
            }

            // 入力値に回転倍率を適用
            float scaledInput = rotationInput * ROTATION_INPUT_MULTIPLIER;

            // フレーム内の回転量を算出
            float rotationDelta = scaledInput * deltaTime;

            // 現在のローカル Y 回転角を取得
            float currentLocalYAngle = NormalizeAngle(_turretTransform.localEulerAngles.y);

            // 回転後の角度を算出
            float targetLocalYAngle = currentLocalYAngle + rotationDelta;

            // 初期角度からの差分を算出
            float deltaFromDefault = targetLocalYAngle - _defaultLocalYAngle;

            // 許容角度内に調整
            deltaFromDefault = Mathf.Clamp(
                deltaFromDefault,
                -MAX_ROTATION_ANGLE,
                MAX_ROTATION_ANGLE
            );

            // 最終角度を算出
            float clampedLocalYAngle = _defaultLocalYAngle + deltaFromDefault;

            // ローカル回転として適用
            _turretTransform.localRotation = Quaternion.Euler(0f, clampedLocalYAngle, 0f);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 0 ～ 360 度の角度を -180 ～ +180 に正規化する
        /// </summary>
        /// <param name="angle">正規化前の角度</param>
        /// <returns>正規化後の角度</returns>
        private float NormalizeAngle(in float angle)
        {
            float normalizedAngle = angle;

            if (normalizedAngle > 180f)
            {
                normalizedAngle -= 360f;
            }

            return normalizedAngle;
        }
    }
}