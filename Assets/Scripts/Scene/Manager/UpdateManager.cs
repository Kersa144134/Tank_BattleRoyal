// ======================================================
// UpdateManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : Update / LateUpdate / フェーズEnterExit の実行を管理する
// ======================================================

using SceneSystem.Controller;
using SceneSystem.Data;

namespace SceneSystem.Manager
{
    /// <summary>
    /// UpdateController をラップし、
    /// Update / LateUpdate / フェーズ EnterExit の実行のみを担当する管理クラス
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
        public void Update()
        {
            // 通常 Update を実行
            _updateController.OnUpdate();
        }

        /// <summary>
        /// LateUpdate タイミングで呼び出される処理を実行する
        /// </summary>
        public void LateUpdate()
        {
            // LateUpdate を実行
            _updateController.OnLateUpdate();
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
            // 現在フェーズが存在する場合は Exit を呼ぶ
            if (_currentPhase != PhaseType.None)
            {
                _updateController.OnPhaseExit();
            }

            // フェーズを更新
            _currentPhase = nextPhase;

            // 新フェーズの Enter を呼ぶ
            _updateController.OnPhaseEnter();
        }
    }
}