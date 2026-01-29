// ======================================================
// TankForceFieldController.cs
// 作成者   : 高橋一翔
// 作成日   : 2026-01-29
// 更新日   : 2026-01-29
// 概要     : 戦車のターゲット ForceField　制御を担当するクラス
// ======================================================

using UnityEngine;

namespace TankSystem.Controller
{
    /// <summary>
    /// 戦車 ForceField 制御クラス
    /// </summary>
    public sealed class TankForceFieldController
    {
        // ======================================================
        // Field
        // ======================================================

        /// <summary>ForceField 参照</summary>
        private readonly ForceFieldController _forceFieldController;

        /// <summary>ForceField の補間アニメーションに使用する目標値</summary>
        private float _targetValue;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>ForceField OFF 時の目標値</summary>
        private const float TARGET_OFF_VALUE = -1f;

        /// <summary>ForceField ON 時の目標値</summary>
        private const float TARGET_ON_VALUE = 1f;

        /// <summary>ForceField の補間速度</summary>
        private const float TARGET_LERP_SPEED = 10f;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// TankForceFieldController
        /// </summary>
        public TankForceFieldController(in Transform rootTransform)
        {
            _forceFieldController = rootTransform.GetComponentInChildren<ForceFieldController>(true);
            _targetValue = TARGET_OFF_VALUE;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ForceField エフェクト切り替え
        /// </summary>
        public void SetForceField(in bool isOn)
        {
            if (isOn)
            {
                _targetValue = TARGET_ON_VALUE;
                return;
            }

            _targetValue = TARGET_OFF_VALUE;
        }

        /// <summary>
        /// ForceField エフェクト更新
        /// </summary>
        public void UpdateForceField(in float deltaTime)
        {
            if (_forceFieldController == null)
            {
                return;
            }

            float currentValue = _forceFieldController.openCloseProgress;

            float nextValue = Mathf.MoveTowards(
                currentValue,
                _targetValue,
                TARGET_LERP_SPEED * deltaTime
            );

            _forceFieldController.openCloseProgress = nextValue;
        }
    }
}