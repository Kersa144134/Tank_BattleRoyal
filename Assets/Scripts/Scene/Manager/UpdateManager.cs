// ======================================================
// UpdateManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-01-23
// 概要     : Update 処理を管理する
// ======================================================

using SceneSystem.Controller;
using SceneSystem.Data;

namespace SceneSystem.Manager
{
    /// <summary>
    /// Update 処理の実行を担当する管理クラス
    /// </summary>
    public sealed class UpdateManager
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>毎フレーム更新対象を管理するコントローラ</summary>
        private readonly UpdateController _updateController;

        /// <summary>現在適用中のフェーズ</summary>
        private PhaseType _currentPhase = PhaseType.None;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// UpdateManager を生成する
        /// </summary>
        /// <param name="updateController">Update 実行用コントローラ</param>
        public UpdateManager(UpdateController updateController)
        {
            // UpdateController を保持
            _updateController = updateController;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // Update 管理
        // --------------------------------------------------
        /// <summary>
        /// 毎フレーム呼び出される Update 処理を実行する
        /// </summary>
        /// <param name="unscaledDeltaTime">timeScale の影響を受けない経過時間</param>
        /// <param name="elapsedTime">インゲームの経過時間</param>
        public void Update(in float unscaledDeltaTime, in float elapsedTime)
        {
            // 通常 Update を実行
            _updateController.OnUpdate(unscaledDeltaTime, elapsedTime);
        }

        /// <summary>
        /// LateUpdate タイミングで呼び出される処理を実行する
        /// </summary>
        /// <param name="unscaledDeltaTime">timeScale の影響を受けない経過時間</param>
        public void LateUpdate(in float unscaledDeltaTime)
        {
            // LateUpdate を実行
            _updateController.OnLateUpdate(unscaledDeltaTime);
        }

        // --------------------------------------------------
        // フェーズ 管理
        // --------------------------------------------------
        /// <summary>
        /// フェーズ変更時に Exit / Enter を実行する
        /// </summary>
        /// <param name="nextPhase">遷移先フェーズ</param>
        public void ChangePhase(in PhaseType nextPhase)
        {
            // 現在フェーズの Exit を呼ぶ
            if (_currentPhase != PhaseType.None)
            {
                _updateController.OnPhaseExit(_currentPhase);
            }

            // フェーズを更新
            _currentPhase = nextPhase;

            // 遷移先フェーズの Enter を呼ぶ
            _updateController.OnPhaseEnter(_currentPhase);
        }
    }
}