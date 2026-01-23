// ======================================================
// PhaseManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-01-23
// 概要     : シーン及びフェーズ遷移条件を管理する
// ======================================================

using SceneSystem.Data;
using System.Diagnostics;

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

        /// <summary>Ready フェーズから Play フェーズへ遷移するまでの待機時間（秒）</summary>
        private const float READY_TO_PLAY_WAIT_TIME = 3.0f;

        /// <summary>Finish フェーズから Result フェーズへ遷移するまでの待機時間（秒）</summary>
        private const float FINISH_TO_RESULT_WAIT_TIME = 5.0f;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// 現在のフェーズに滞在している経過時間
        /// timeScale の影響を受けない経過時間で管理
        /// </summary>
        private float _phaseElapsedTime = 0.0f;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズおよびシーン遷移条件を毎フレーム評価する
        /// </summary>
        /// <param name="unscaledDeltaTime">timeScale の影響を受けない経過時間</param>
        /// <param name="currentScene">現在のシーン名（参照専用）</param>
        /// <param name="currentPhase">現在のフェーズ（参照専用）</param>
        /// <param name="targetScene">遷移先シーン名</param>
        /// <param name="targetPhase">遷移先フェーズ</param>
        public void Update(
            in float unscaledDeltaTime,
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
                    UpdateReadyPhase(
                        currentPhase,
                        out targetPhase
                    );
                    break;

                case PhaseType.Finish:
                    UpdateFinishPhase(
                        currentPhase,
                        out targetPhase
                    );
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// オプションボタン押下時のフェーズ切り替え遷移を評価する
        /// </summary>
        /// <param name="currentPhase">現在のフェーズ（参照専用）</param>
        /// <param name="targetPhase">遷移先フェーズ</param>
        public void ToggleOptionPhaseChange(
            in PhaseType currentPhase,
            out PhaseType targetPhase
        )
        {
            // 初期状態として遷移なしを明示する
            targetPhase = currentPhase;

            switch (currentPhase)
            {
                case PhaseType.Play:
                    // プレイ中は一時停止へ遷移させる
                    targetPhase = PhaseType.Pause;
                    break;

                case PhaseType.Pause:
                    // 一時停止中はプレイへ戻す
                    targetPhase = PhaseType.Play;
                    break;

                default:
                    // 対象外フェーズでは遷移を行わない
                    break;
            }

            // フェーズ遷移が確定したため経過時間を初期化する
            if (targetPhase != currentPhase)
            {
                _phaseElapsedTime = 0.0f;
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // Ready
        // --------------------------------------------------
        /// <summary>
        /// Ready フェーズ中の遷移条件を評価する
        /// </summary>
        /// <param name="currentPhase">現在のフェーズ</param>
        /// <param name="targetPhase">遷移先フェーズ</param>
        private void UpdateReadyPhase(
            in PhaseType currentPhase,
            out PhaseType targetPhase
        )
        {
            // 初期状態では遷移を行わない
            targetPhase = currentPhase;

            // Ready フェーズの待機時間を超えているかを判定する
            if (_phaseElapsedTime < READY_TO_PLAY_WAIT_TIME)
            {
                // 待機時間未満のため遷移を行わない
                return;
            }

            // 待機時間を超えたためプレイフェーズへ遷移させる
            targetPhase = PhaseType.Play;

            // フェーズ切り替えに伴い経過時間を初期化する
            _phaseElapsedTime = 0.0f;
        }

        // --------------------------------------------------
        // Finish
        // --------------------------------------------------
        /// <summary>
        /// Finish フェーズ中の遷移条件を評価する
        /// </summary>
        /// <param name="currentPhase">現在のフェーズ</param>
        /// <param name="targetPhase">遷移先フェーズ</param>
        private void UpdateFinishPhase(
            in PhaseType currentPhase,
            out PhaseType targetPhase
        )
        {
            // 初期状態では遷移を行わない
            targetPhase = currentPhase;
            
            // Finish フェーズの待機時間を超えているかを判定する
            if (_phaseElapsedTime < FINISH_TO_RESULT_WAIT_TIME)
            {
                // 待機時間未満のため遷移を行わない
                return;
            }

            // 待機時間を超えたため結果表示フェーズへ遷移させる
            targetPhase = PhaseType.Result;

            // フェーズ切り替えに伴い経過時間を初期化する
            _phaseElapsedTime = 0.0f;
        }
    }
}