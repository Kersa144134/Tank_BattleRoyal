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
        // シーン管理
        // --------------------------------------------------
        /// <summary>現在のシーン名</summary>
        private string _currentScene = string.Empty;

        /// <summary>遷移先シーン名</summary>
        private string _targetScene = string.Empty;

        // --------------------------------------------------
        // フェーズ管理
        // --------------------------------------------------
        /// <summary>現在のフェーズ</summary>
        private PhaseType _currentPhase = PhaseType.None;

        /// <summary>遷移先フェーズ</summary>
        private PhaseType _targetPhase = PhaseType.None;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>遷移先フェーズ</summary>
        public PhaseType TargetPhase => _targetPhase;

        /// <summary>遷移先シーン名</summary>
        public string TargetScene => _targetScene;

        // ======================================================
        // 初期化
        // ======================================================

        /// <summary>
        /// 初期フェーズとシーンを設定する
        /// </summary>
        public void Initialize(in PhaseType startPhase, in string startScene)
        {
            // 現在フェーズを設定
            _currentPhase = startPhase;

            // 遷移先フェーズを設定
            _targetPhase = startPhase;

            // 現在シーンを設定
            _currentScene = startScene;

            // 遷移先シーンを設定
            _targetScene = startScene;
        }

        // ======================================================
        // フェーズ遷移判定
        // ======================================================

        /// <summary>
        /// オプションボタン押下時のフェーズ切替判定
        /// </summary>
        public void EvaluateOptionToggle()
        {
            switch (_currentPhase)
            {
                case PhaseType.Play:
                    // Play 中なら Pause へ
                    _targetPhase = PhaseType.Pause;
                    break;

                case PhaseType.Pause:
                    // Pause 中なら Play へ
                    _targetPhase = PhaseType.Play;
                    break;
            }
        }

        /// <summary>
        /// フェーズ変更完了通知
        /// </summary>
        public void NotifyPhaseChanged(in PhaseType newPhase)
        {
            // 現在フェーズを更新
            _currentPhase = newPhase;
        }

        // ======================================================
        // シーン遷移判定
        // ======================================================

        /// <summary>
        /// シーン遷移要求を設定する
        /// </summary>
        public void RequestSceneChange(in string nextScene)
        {
            // 無効なシーン名は無視
            if (string.IsNullOrEmpty(nextScene))
            {
                return;
            }

            // 遷移先シーンを設定
            _targetScene = nextScene;
        }

        /// <summary>
        /// シーン遷移完了通知
        /// </summary>
        public void NotifySceneChanged(in string sceneName)
        {
            // 現在シーンを更新
            _currentScene = sceneName;
        }
    }
}