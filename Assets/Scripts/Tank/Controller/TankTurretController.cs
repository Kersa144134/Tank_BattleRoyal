// ======================================================
// TankTurretController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-10
// 更新日時 : 2026-01-28
// 概要     : 砲塔の回転制御を担当するクラス
// ======================================================

using UnityEngine;
using TankSystem.Data;

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

        // --------------------------------------------------
        // 基準値
        // --------------------------------------------------
        /// <summary>基準となる砲塔スケール倍率</summary>
        private const float BASE_BARREL_TURRET_SCALE_MULTIPLIER = 0.8f;

        /// <summary>基準となる砲塔スケール倍率</summary>
        private const float BASE_BARREL_TURRET_ROTATION_MULTIPLIER = 60f;

        /// <summary>砲塔の最大回転許容角度（左右共通）</summary>
        private const float MAX_ROTATION_ANGLE = 60.0f;

        // --------------------------------------------------
        // パラメーター
        // --------------------------------------------------
        /// <summary>砲身 1 あたりの砲塔スケール倍率加算値</summary>
        private const float BARREL_TURRET_SCALE_MULTIPLIER = 0.02f;

        /// <summary>砲身 1 あたりの砲塔回転倍率加算値</summary>
        private const float BARREL_TURRET_ROTATION_MULTIPLIER = 1.5f;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>回転対象となる砲塔の Transform</summary>
        private readonly Transform _turretTransform;

        /// <summary>現在の砲塔スケール倍率</summary>
        private float _turretScaleMultiplier = BASE_BARREL_TURRET_SCALE_MULTIPLIER;

        /// <summary>現在の砲塔回転倍率</summary>
        private float _turretRotationMultiplier = BASE_BARREL_TURRET_ROTATION_MULTIPLIER;

        /// <summary>砲塔の初期ローカル Y 回転角</summary>
        private readonly float _defaultLocalYAngle;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 砲塔回転コントローラーを生成する
        /// </summary>
        /// <param name="tankStatus">砲塔スケール算出に使用する戦車ステータス</param>
        /// <param name="turretTransform">砲塔の Transform</param>
        public TankTurretController(
            in TankStatus tankstatus,
            in Transform turretTransform
        )
        {
            // 砲塔 Transform を保持
            _turretTransform = turretTransform;

            // 初期状態のローカル Y 回転角を保持
            _defaultLocalYAngle = _turretTransform.localEulerAngles.y;

            UpdateTurretParameter(tankstatus);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// BarrelScale ステータスを元に砲塔スケールを再計算する
        /// </summary>
        /// <param name="tankStatus">砲塔スケール算出に使用する戦車ステータス</param>
        public void UpdateTurretParameter(in TankStatus tankStatus)
        {
            // 砲塔スケール倍率を算出
            _turretScaleMultiplier =
                BASE_BARREL_TURRET_SCALE_MULTIPLIER
                + tankStatus.Barrel * BARREL_TURRET_SCALE_MULTIPLIER;

            // 砲塔回転倍率を算出
            _turretRotationMultiplier =
                BASE_BARREL_TURRET_ROTATION_MULTIPLIER
                - tankStatus.Barrel * BARREL_TURRET_ROTATION_MULTIPLIER;
        }

        /// <summary>
        /// 砲塔 Transform にスケール倍率を適用する
        /// </summary>
        public void ApplyTurretScale()
        {
            if (_turretTransform == null)
            {
                return;
            }

            // 現在のローカルスケールを取得
            Vector3 localScale = _turretTransform.localScale;

            localScale.x = _turretScaleMultiplier;
            localScale.y = _turretScaleMultiplier;
            localScale.z = _turretScaleMultiplier;

            _turretTransform.localScale = localScale;
        }

        /// <summary>
        /// 入力値を元に砲塔を回転させる
        /// </summary>
        /// <param name="deltaTime">フレーム時間</param>
        /// <param name="rotationInput">
        /// 回転入力値  
        /// -1 ～ 1 を想定（左回転 / 右回転）
        /// </param>
        public void ApplyTurretRotate(
            in float deltaTime,
            in float rotationInput
        )
        {
            // 入力が無い場合は処理なし
            if (Mathf.Approximately(rotationInput, 0f))
            {
                return;
            }

            // 入力値に回転倍率を適用
            float scaledInput = rotationInput * _turretRotationMultiplier;

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