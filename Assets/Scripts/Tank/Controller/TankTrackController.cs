// ==============================================================================
// TankTrackController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-04
// 更新日時 : 2025-12-16
// 概要     : キャタピラ入力モードを管理し、移動・旋回量を算出するロジッククラス
// ==============================================================================

using System.Collections.Generic;
using UnityEngine;
using TankSystem.Data;
using TankSystem.Interface;
using TankSystem.Manager;

namespace TankSystem.Controller
{
    /// <summary>
    /// キャタピラ入力モードを管理し、計算処理を委譲する制御クラス
    /// </summary>
    public class TankTrackController
    {
        // ======================================================================
        // 定数
        // ======================================================================

        /// <summary>基準前後移動速度定数</summary>
        private const float MOVE_SPEED = 1f;

        /// <summary>前進量の指数補正値</summary>
        private const float FORWARD_EXPONENT = 1.5f;

        /// <summary>基準旋回速度定数</summary>
        private const float TURN_SPEED = 7.5f;

        /// <summary>旋回量の指数補正値</summary>
        private const float TURN_EXPONENT = 1.5f;

        // ======================================================================
        // 辞書
        // ======================================================================

        /// <summary>入力モード管理用辞書</summary>
        private readonly Dictionary<BaseTankRootManager.TrackInputMode, ITrackInputMode> _modeTable;

        // ======================================================================
        // コンストラクタ
        // ======================================================================

        /// <summary>
        /// 各入力モードを生成し管理テーブルへ登録する
        /// </summary>
        public TankTrackController()
        {
            // モード管理テーブルを生成
            _modeTable = new Dictionary<BaseTankRootManager.TrackInputMode, ITrackInputMode>
            {
                // 左右スティック独立操作モードを登録
                {
                    BaseTankRootManager.TrackInputMode.Dual,
                    new DualStickMode(this)
                },

                // 左スティック単独操作モードを登録
                {
                    BaseTankRootManager.TrackInputMode.Single,
                    new SingleStickMode(this)
                }
            };
        }

        // ======================================================================
        // パブリックメソッド
        // ======================================================================

        /// <summary>
        /// 指定された入力モードで前進量と旋回量を算出する
        /// </summary>
        /// <param name="inputMode">使用するキャタピラ入力モード</param>
        /// <param name="leftInput">左スティックの入力値</param>
        /// <param name="rightInput">右スティックの入力値</param>
        /// <param name="forwardAmount">算出された前進／後退量</param>
        /// <param name="turnAmount">算出された旋回量</param>
        public void UpdateTrack(
            in BaseTankRootManager.TrackInputMode inputMode,
            in Vector2 leftInput,
            in Vector2 rightInput,
            out float forwardAmount,
            out float turnAmount
        )
        {
            // 指定モードが未登録の場合は停止
            if (!_modeTable.TryGetValue(inputMode, out ITrackInputMode mode))
            {
                forwardAmount = 0f;
                turnAmount = 0f;
                return;
            }

            // 入力モードに処理を委譲
            mode.Calculate(
                leftInput,
                rightInput,
                out forwardAmount,
                out turnAmount
            );
        }

        // ======================================================================
        // ユーティリティ
        // ======================================================================

        /// <summary>
        /// 入力値を小数第1位に丸める
        /// </summary>
        /// <param name="value">丸め対象となる入力値</param>
        /// <returns>小数第1位に丸められた値</returns>
        internal float RoundValue(in float value)
        {
            return Mathf.Round(value * 10f) * 0.1f;
        }

        /// <summary>
        /// 左右入力平均から前進／後退量を算出
        /// </summary>
        /// <param name="leftInput">左側キャタピラの入力値</param>
        /// <param name="rightInput">右側キャタピラの入力値</param>
        /// <returns>前進または後退の移動量</returns>
        internal float CalculateForwardAmount(in float leftInput, in float rightInput)
        {
            // 左右入力の平均値を算出し、前後移動の基準値を作成
            float average = (leftInput + rightInput) * 0.5f;

            // 入力方向を取り除く
            float abs = Mathf.Abs(average);

            // 入力感度を指数カーブで補正
            float curved = Mathf.Pow(abs, FORWARD_EXPONENT);

            // 入力方向を復元
            float curvedSigned = curved * Mathf.Sign(average);

            // 基準移動速度を掛けて最終的な前進量を算出
            return curvedSigned * MOVE_SPEED;
        }

        /// <summary>
        /// 単一軸入力から前進／後退量を算出
        /// </summary>
        /// <param name="input">単一軸の入力値</param>
        /// <returns>前進または後退の移動量</returns>
        internal float CalculateForwardFromSingleAxis(in float input)
        {
            // 入力方向を取り除く
            float abs = Mathf.Abs(input);

            // 入力感度を指数カーブで補正
            float curved = Mathf.Pow(abs, FORWARD_EXPONENT);

            // 入力方向を復元
            float curvedSigned = curved * Mathf.Sign(input);

            // 基準移動速度を掛けて最終的な前進量を算出
            return curvedSigned * MOVE_SPEED;
        }

        /// <summary>
        /// 左右差分から旋回量を算出
        /// </summary>
        /// <param name="leftInput">左側キャタピラの入力値</param>
        /// <param name="rightInput">右側キャタピラの入力値</param>
        /// <returns>旋回の強さを表す回転量</returns>
        internal float CalculateTurnAmount(in float leftInput, in float rightInput)
        {
            // 左右入力の差分を取り除く
            float diff = leftInput - rightInput;

            // 差分入力値の大きさのみを取得
            float abs = Mathf.Abs(diff);

            // 入力感度を逆指数カーブで補正
            float curved = Mathf.Pow(abs, 1f / TURN_EXPONENT);

            // 入力方向を反映して旋回方向を決定
            float curvedSigned = curved * Mathf.Sign(diff);

            // 基準旋回速度を掛けて最終的な旋回量を算出
            return curvedSigned * TURN_SPEED;
        }

        /// <summary>
        /// 単一軸入力から旋回量を算出
        /// </summary>
        /// <param name="input">単一軸の入力値</param>
        /// <returns>旋回の強さを表す回転量</returns>
        internal float CalculateTurnFromSingleAxis(in float input)
        {
            // 入力値の大きさを取得
            float abs = Mathf.Abs(input);

            // 入力感度を逆指数カーブで補正
            float curved = Mathf.Pow(abs, 1f / TURN_EXPONENT);

            // 入力方向を反映して旋回方向を決定
            float curvedSigned = curved * Mathf.Sign(input);

            // 基準旋回速度を掛けて最終的な旋回量を算出
            return curvedSigned * TURN_SPEED;
        }
    }
}