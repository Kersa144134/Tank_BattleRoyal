// ==============================================================================
// TankTrackController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-04
// 更新日時 : 2025-12-16
// 概要     : キャタピラ入力モードを管理し、移動・旋回量を算出するロジッククラス
// ==============================================================================

using UnityEngine;
using InputSystem.Data;
using InputSystem.Utility;

namespace TankSystem.Controller
{
    /// <summary>
    /// キャタピラ入力モードを管理し、計算処理を委譲する制御クラス
    /// </summary>
    public class TankTrackController
    {
        // ======================================================================
        // コンポーネント参照
        // ======================================================================

        /// <summary>シングルスティック入力モード</summary>
        private readonly SingleStickMode _singleStickMode;

        /// <summary>デュアルスティック入力モード</summary>
        private readonly DualStickMode _dualStickMode;

        /// <summary>キャタピラ入力の補正クラス</summary>
        private readonly InputCorrectionUtility _correctionUtility = new InputCorrectionUtility();

        // ======================================================================
        // 定数
        // ======================================================================

        /// <summary>基準前進後退速度定数</summary>
        private const float FORWARD_SPEED = 1.0f;

        /// <summary>基準旋回速度定数</summary>
        private const float TURN_SPEED = 5.0f;

        /// <summary>入力値の指数補正値</summary>
        private const float INPUT_EXPONENT = 2.0f;

        // ======================================================================
        // コンストラクタ
        // ======================================================================

        /// <summary>
        /// 入力モードを生成し初期モードを設定する
        /// </summary>

        public TankTrackController()
        {
            _singleStickMode = new SingleStickMode(this);
            _dualStickMode = new DualStickMode(this);
        }

        // ======================================================================
        // パブリックメソッド
        // ======================================================================

        /// <summary>
        /// 指定された入力モードで前進量と旋回量を算出する
        /// </summary>
        /// <param name="inputMode">キャタピラ入力モード</param>
        /// <param name="leftInput">左スティックの入力値</param>
        /// <param name="rightInput">右スティックの入力値</param>
        /// <param name="forwardAmount">算出された前進／後退量</param>
        /// <param name="turnAmount">算出された旋回量</param>
        public void UpdateTrack(
            in TrackInputMode inputMode,
            in Vector2 leftInput,
            in Vector2 rightInput,
            out float forwardAmount,
            out float turnAmount
        )
        {
            // 現在の入力モードへ計算処理を委譲
            if (inputMode == TrackInputMode.Single)
            {
                _singleStickMode.Calculate(
                    leftInput,
                    out forwardAmount,
                    out turnAmount
                );
            }
            else
            {
                _dualStickMode.Calculate(
                    leftInput,
                    rightInput,
                    out forwardAmount,
                    out turnAmount
                );
            }
        }

        // ======================================================================
        // ユーティリティ
        // ======================================================================

        /// <summary>
        /// 入力値を小数第1位に丸めた後、段階補正テーブルに基づいて補正値へ変換する
        /// </summary>
        /// <param name="value">補正対象となる入力値</param>
        /// <returns>段階補正後の入力値</returns>
        public float ConvertRoundedInputToCorrectedValue(in float value)
        {
            return _correctionUtility.ConvertRoundedInputToCorrectedValue(value);
        }

        /// <summary>
        /// 左右入力平均から前進／後退量を算出
        /// </summary>
        /// <param name="leftInput">左側キャタピラの入力値</param>
        /// <param name="rightInput">右側キャタピラの入力値</param>
        /// <returns>前進または後退の移動量</returns>
        public float CalculateForwardAmount(in float leftInput, in float rightInput)
        {
            // 左右入力の平均値を算出し、前後移動の基準値を作成
            float average = (leftInput + rightInput) * 0.5f;

            // 入力方向を取り除く
            float abs = Mathf.Abs(average);

            // 入力感度を指数カーブで補正
            float curved = Mathf.Pow(abs, INPUT_EXPONENT);

            // 入力方向を復元
            float curvedSigned = curved * Mathf.Sign(average);

            // 基準移動速度を掛けて最終的な前進量を算出
            return curvedSigned * FORWARD_SPEED;
        }

        /// <summary>
        /// 単一軸入力から前進／後退量を算出
        /// </summary>
        /// <param name="input">単一軸の入力値</param>
        /// <returns>前進または後退の移動量</returns>
        public float CalculateForwardFromSingleAxis(in float input)
        {
            // 入力方向を取り除く
            float abs = Mathf.Abs(input);

            // 入力感度を指数カーブで補正
            float curved = Mathf.Pow(abs, INPUT_EXPONENT);

            // 入力方向を復元
            float curvedSigned = curved * Mathf.Sign(input);

            // 基準移動速度を掛けて最終的な前進量を算出
            return curvedSigned * FORWARD_SPEED;
        }

        /// <summary>
        /// 左右差分から旋回量を算出
        /// </summary>
        /// <param name="leftInput">左側キャタピラの入力値</param>
        /// <param name="rightInput">右側キャタピラの入力値</param>
        /// <returns>旋回の強さを表す回転量</returns>
        public float CalculateTurnAmount(in float leftInput, in float rightInput)
        {
            // 左右入力の差分を取り除く
            float diff = leftInput - rightInput;

            // 差分入力値の大きさのみを取得
            float abs = Mathf.Abs(diff);

            // 入力感度を逆指数カーブで補正
            float curved = Mathf.Pow(abs, 1f / INPUT_EXPONENT);

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
        public float CalculateTurnFromSingleAxis(in float input)
        {
            // 入力値の大きさを取得
            float abs = Mathf.Abs(input);

            // 入力感度を逆指数カーブで補正
            float curved = Mathf.Pow(abs, 1f / INPUT_EXPONENT);

            // 入力方向を反映して旋回方向を決定
            float curvedSigned = curved * Mathf.Sign(input);

            // 基準旋回速度を掛けて最終的な旋回量を算出
            return curvedSigned * TURN_SPEED;
        }
    }
}