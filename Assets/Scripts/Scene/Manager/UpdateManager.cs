// ======================================================
// UpdateManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-01-07
// 概要     : Update / LateUpdate / フェーズEnterExit の実行を管理する
// ======================================================

using UnityEngine;
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

        /// <summary>Play フェーズ中のみ進行するゲームの経過時間</summary>
        private float _elapsedTime;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// ゲームの経過時間
        /// </summary>
        public float ElapsedTime => _elapsedTime;

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

            // タイマーを初期化
            _elapsedTime = 0.0f;
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
            // Play フェーズ中のみタイマーを進行
            if (_currentPhase == PhaseType.Play)
            {
                // 非スケール時間で加算
                _elapsedTime += Time.unscaledDeltaTime;
            }

            // 通常 Update を実行
            _updateController.OnUpdate(_elapsedTime);
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

            // Initialize フェーズ開始時にタイマーをリセット
            if (_currentPhase == PhaseType.Initialize)
            {
                _elapsedTime = 0.0f;
            }

            // 新フェーズの Enter を呼ぶ
            _updateController.OnPhaseEnter();
        }
    }
}