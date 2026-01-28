// ======================================================
// EnemyTankRootManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-14
// 更新日時 : 2025-12-14
// 概要     : 敵AI用戦車マネージャー
//            BaseTankRootManager を継承し、入力処理をAI制御に差し替える
// ======================================================

using UnityEngine;
using InputSystem.Data;

namespace TankSystem.Manager
{
    /// <summary>
    /// 敵戦車用マネージャー
    /// 入力処理を AI 制御に置き換え、プレイヤー入力を使用しない
    /// </summary>
    public sealed class EnemyTankRootManager : BaseTankRootManager
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>無入力ボタン</summary>
        private readonly ButtonState _none = new ButtonState();

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
            leftStick = Vector2.zero;
            rightStick = Vector2.zero;

            turretRotation = 0f;

            inputModeChange = false;
            fireModeChange = false;

            leftFire = _none;
            rightFire = _none;
        }
    }
}