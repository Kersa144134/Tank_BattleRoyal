// ======================================================
// PhaseManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-01-23
// 概要     : シーン及びフェーズ遷移条件を管理する
// ======================================================

using InputSystem.Manager;
using SceneSystem.Data;
using System;
using UnityEngine;
using WeaponSystem.Data;

namespace SceneSystem.Manager
{
    /// <summary>
    /// シーン及びフェーズ遷移条件を管理する
    /// </summary>
    public sealed class PhaseManager
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>
        /// Ready フェーズから Play フェーズへ遷移するまでの待機時間（秒）
        /// </summary>
        private const float READY_TO_PLAY_WAIT_TIME = 5.0f;

        /// <summary>
        /// Finish フェーズから Result フェーズへ遷移するまでの待機時間（秒）
        /// </summary>
        private const float FINISH_TO_RESULT_WAIT_TIME = 5.0f;

        /// <summary>
        /// Play フェーズから Finish フェーズへ遷移するまでのゲームプレイ時間（秒）
        /// </summary>
        private const float PLAY_TO_FINISH_WAIT_TIME = 60.0f;

        /// <summary>
        /// 条件成立時に Title フェーズから Ready フェーズへ遷移するまでの待機時間（秒）
        /// </summary>
        private const float TITLE_TO_READY_WAIT_TIME = 3.0f;

        /// <summary>
        /// 条件成立時に Pause フェーズから Title フェーズへ遷移するまでの待機時間（秒）
        /// </summary>
        private const float PAUSE_TO_TITLE_WAIT_TIME = 3.0f;

        /// <summary>
        /// 条件成立時に Result フェーズから Title フェーズへ遷移するまでの待機時間（秒）
        /// </summary>
        private const float RESULT_TO_TITLE_WAIT_TIME = 3.0f;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// 現在のフェーズに滞在している経過時間
        /// timeScale の影響を受けない経過時間で管理
        /// </summary>
        private float _phaseElapsedTime = 0.0f;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// オプションボタン押下時に発火するイベント
        /// </summary>
        public event Action OnOptionButtonPressed;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズおよびシーン遷移条件を毎フレーム評価する
        /// </summary>
        /// <param name="unscaledDeltaTime">timeScale の影響を受けない経過時間</param>
        /// <param name="elapsedTime">インゲームの経過時間</param>
        /// <param name="currentScene">現在のシーン名</param>
        /// <param name="currentPhase">現在のフェーズ</param>
        /// <param name="targetScene">遷移先シーン名</param>
        /// <param name="targetPhase">遷移先フェーズ</param>
        public void Update(
            in float unscaledDeltaTime,
            in float elapsedTime,
            in string currentScene,
            in PhaseType currentPhase,
            out string targetScene,
            out PhaseType targetPhase
        )
        {
            // 初期状態として遷移なしを明示する
            targetScene = currentScene;
            targetPhase = currentPhase;

            // フェーズ滞在時間を加算する
            _phaseElapsedTime += unscaledDeltaTime;

            switch (currentPhase)
            {
                case PhaseType.Ready:
                    UpdatePhaseByElapsedTime(
                        currentPhase,
                        out targetPhase,
                        READY_TO_PLAY_WAIT_TIME,
                        PhaseType.Play
                    );
                    break;

                case PhaseType.Play:
                    if (elapsedTime > PLAY_TO_FINISH_WAIT_TIME)
                    {
                        // 即時遷移
                        UpdatePhaseByElapsedTime(
                            currentPhase,
                            out targetPhase,
                            0f,
                            PhaseType.Finish
                        );
                    }

                    if (InputManager.Instance.StartButton.Down)
                    {
                        TogglePhaseChange(
                            currentPhase,
                            out targetPhase
                        );

                        OnOptionButtonPressed?.Invoke();
                    }
                    break;

                case PhaseType.Pause:
                    if (InputManager.Instance.StartButton.Down)
                    {
                        TogglePhaseChange(
                            currentPhase,
                            out targetPhase
                        );

                        OnOptionButtonPressed?.Invoke();
                    }
                    break;

                case PhaseType.Finish:
                    UpdatePhaseByElapsedTime(
                        currentPhase,
                        out targetPhase,
                        FINISH_TO_RESULT_WAIT_TIME,
                        PhaseType.Result
                    );
                    break;

                default:
                    break;
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 指定した待機時間経過後にフェーズ遷移を行う
        /// </summary>
        /// <param name="currentPhase">現在のフェーズ</param>
        /// <param name="targetPhase">遷移先フェーズ</param>
        /// <param name="waitTime">待機時間</param>
        /// <param name="nextPhase">指定フェーズ</param>
        private void UpdatePhaseByElapsedTime(
            in PhaseType currentPhase,
            out PhaseType targetPhase,
            in float waitTime,
            in PhaseType nextPhase
        )
        {
            // 初期状態では遷移を行わない
            targetPhase = currentPhase;

            // 指定された待機時間を超えているかを判定する
            if (_phaseElapsedTime < waitTime)
            {
                // 待機時間未満のため遷移を行わない
                return;
            }

            // フェーズ遷移
            targetPhase = nextPhase;

            // 経過時間を初期化
            _phaseElapsedTime = 0.0f;
        }

        /// <summary>
        /// オプションボタン押下時のフェーズ切り替え遷移を評価する
        /// </summary>
        /// <param name="currentPhase">現在のフェーズ</param>
        /// <param name="targetPhase">遷移先フェーズ</param>
        private void TogglePhaseChange(
            in PhaseType currentPhase,
            out PhaseType targetPhase
        )
        {
            targetPhase = currentPhase;

            switch (currentPhase)
            {
                case PhaseType.Play:
                    targetPhase = PhaseType.Pause;
                    break;

                case PhaseType.Pause:
                    targetPhase = PhaseType.Play;
                    break;

                default:
                    break;
            }

            // 経過時間を初期化
            if (targetPhase != currentPhase)
            {
                _phaseElapsedTime = 0.0f;
            }
        }
    }
}