// ======================================================
// EnemyTankRootManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-14
// 更新日時 : 2025-12-14
// 概要     : 敵AI用戦車マネージャー
//            BaseTankRootManager を継承し、入力処理をAI制御に差し替える
// ======================================================

using InputSystem.Data;
using UnityEngine;

namespace TankSystem.Manager
{
    /// <summary>
    /// 敵戦車用マネージャー
    /// 入力処理を AI 制御に置き換え、プレイヤー入力を使用しない
    /// </summary>
    public sealed class EnemyTankRootManager : BaseTankRootManager
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>戦車 AI 制御クラス</summary>
        private TankAIManager _tankAIManager;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>無入力ボタン</summary>
        private readonly ButtonState _none = new ButtonState();

        /// <summary>ターゲットを発見しているかどうか</summary>
        private bool _hasTarget;

        // ======================================================
        // IUpdatable 派生イベント
        // ======================================================

        protected override void OnEnterInternal()
        {
            base.OnEnterInternal();

            if (_visibilityController != null)
            {
                _tankAIManager = new TankAIManager(_visibilityController);

                _visibilityController.OnTargetAcquired += HandleAttackInput;
            }
        }

        protected override void OnExitInternal()
        {
            base.OnExitInternal();

            if (_visibilityController != null)
            {
                _visibilityController.OnTargetAcquired -= HandleAttackInput;
            }
        }

        // ======================================================
        // 抽象メソッド
        // ======================================================

        /// <summary>
        /// 毎フレーム呼び出される入力更新処理
        /// </summary>
        /// <param name="leftStick">左キャタピラ入力</param>
        /// <param name="rightStick">右キャタピラ入力</param>
        /// <param name="turretRotation">砲塔回転入力</param>
        /// <param name="inputModeChange">入力モード切替ボタン押下フラグ</param>
        /// <param name="fireModeChange">攻撃モード切替ボタン押下フラグ</param>
        /// <param name="leftFire">左攻撃ボタンの状態</param>
        /// <param name="rightFire">右攻撃ボタンの状態</param>
        protected override void UpdateInput(
            out Vector2 leftStick,
            out Vector2 rightStick,
            out float turretRotation,
            out bool inputModeChange,
            out bool fireModeChange,
            out ButtonState leftFire,
            out ButtonState rightFire
        )
        {
            // AI による移動入力計算を委譲
            _tankAIManager.GetMovementInputTowardsTarget(transform, out leftStick);
            rightStick = Vector2.zero;

            turretRotation = 0f;

            inputModeChange = false;
            fireModeChange = false;

            leftFire = _none;
            rightFire = _none;
            rightFire.Update(_hasTarget);
        }

        /// <summary>
        /// ターゲット Transform 配列を送る
        /// </summary>
        /// <param name="tankTransforms">戦車 Transform 配列</param>
        /// <param name="itemTransforms">アイテム Transform 配列</param>
        public override void SetTargetData(
            in Transform[] tankTransforms,
            in Transform[] itemTransforms
        )
        {
            Tanks = tankTransforms;

            _tankAIManager.SetTargetData(tankTransforms, itemTransforms);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// ターゲット取得イベント受け取り
        /// </summary>
        /// <param name="target">ターゲット</param>
        private void HandleAttackInput(BaseTankRootManager target)
        {
            if (target is PlayerTankRootManager)
            {
                _hasTarget = true;
            }
            else
            {
                _hasTarget = false;
            }
        }
    }
}