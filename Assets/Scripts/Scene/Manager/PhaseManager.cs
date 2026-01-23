// ======================================================
// PhaseManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-01-23
// 概要     : フェーズ・シーン遷移条件を管理する
// ======================================================

using SceneSystem.Data;

namespace SceneSystem.Manager
{
    /// <summary>
    /// フェーズおよびシーン遷移条件を集約して管理する
    /// </summary>
    public sealed class PhaseManager
    {
        // ======================================================
        // フィールド
        // ======================================================

        // --------------------------------------------------
        // フェーズ遷移判定用
        // --------------------------------------------------

        /// <summary>
        /// フェーズ経過時間計測用タイマー
        /// フェーズが継続している時間を秒単位で保持する
        /// </summary>
        private float _phaseElapsedTime = 0.0f;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズおよびシーン遷移条件を毎フレーム更新する
        /// </summary>
        /// <param name="unscaledDeltaTime">timeScaleに影響されない経過時間</param>
        /// <param name="currentScene">現在のシーン名</param>
        /// <param name="currentPhase">現在のフェーズ名</param>
        public void Update(
            float unscaledDeltaTime,
            ref string currentScene,
            ref PhaseType currentPhase
        )
        {
            // timeScaleに影響されない経過時間を加算する
            _phaseElapsedTime += unscaledDeltaTime;

            switch (currentPhase)
            {
                case PhaseType.Ready:
                    // Ready フェーズで一定時間経過したかを判定する
                    UpdateReadyPhase(ref currentPhase);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// オプションボタン押下時のフェーズ切り替え処理を行うハンドラ
        /// 現在のフェーズに応じてターゲットフェーズを切り替える
        /// </summary>
        public void ToggleOptionPhaseChange(
            ref PhaseType currentPhase
        )
        {
            switch (currentPhase)
            {
                case PhaseType.Play:
                    // Play フェーズ中は一時停止を行うため Pause に切り替える
                    currentPhase = PhaseType.Pause;
                    break;

                case PhaseType.Pause:
                    // Pause フェーズ中はゲーム再開を行うため Play に切り替える
                    currentPhase = PhaseType.Play;
                    break;

                default:
                    // 対象外フェーズでは何も行わない
                    break;
            }

            // フェーズが変更されたため経過時間を初期化する
            _phaseElapsedTime = 0.0f;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // Ready
        // --------------------------------------------------
        /// <summary>
        /// Ready フェーズ中の遷移条件を更新する
        /// </summary>
        /// <param name="currentPhase">
        /// 現在のフェーズ
        /// 遷移条件成立時に Play フェーズへ書き換える
        /// </param>
        private void UpdateReadyPhase(
            ref PhaseType currentPhase
        )
        {
            // Ready フェーズの待機時間として 5 秒を超えたかを判定する
            if (_phaseElapsedTime < 5.0f)
            {
                // 待機時間未満のためフェーズ遷移は行わない
                return;
            }

            // 待機時間を超えたためゲーム開始フェーズへ移行する
            currentPhase = PhaseType.Play;

            // フェーズが切り替わったため経過時間をリセットする
            _phaseElapsedTime = 0.0f;
        }
    }
}