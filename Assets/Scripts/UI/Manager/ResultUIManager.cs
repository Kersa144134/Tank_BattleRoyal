// ======================================================
// ResultUIManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-01-19
// 更新日時 : 2026-02-02
// 概要     : リザルトシーンで使用される UI 演出を管理するクラス
// ======================================================

using System;
using InputSystem.Manager;
using SceneSystem.Interface;
using ScoreSystem.Manager;
using SoundSystem.Manager;
using TMPro;
using UISystem.Service;
using UnityEngine;

namespace UISystem.Manager
{
    /// <summary>
    /// リザルトシーンにおける UI 演出およびゲーム連動 UI を管理するクラス
    /// </summary>
    public sealed class ResultUIManager : BaseUIManager, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("リザルトシーン固有インスペクタ")]

        [Header("スコア")]
        /// <summary>スコアを表示するテキスト</summary>
        [SerializeField]
        private TextMeshProUGUI _scoreText;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>スコア表示フォーマットサービス</summary>
        private TextFormatService _scoreFormatService;

        // ======================================================
        // 定数
        // ======================================================

        // --------------------------------------------------
        // スコア
        // --------------------------------------------------
        // <summary>
        /// スコア表示フォーマット
        /// </summary>
        private const string SCORE_FORMAT = "SCORE {0}";

        /// <summary>
        /// スコア表示桁数
        /// </summary>
        private static readonly int[] SCORE_DIGITS = { 8 };
        
        // --------------------------------------------------
        // アニメーション名
        // --------------------------------------------------
        /// <summary>リザルト終了アニメーション名</summary>
        private const string END_ANIMATION_NAME = "End";

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>Result フェーズアニメーション終了時</summary>
        public event Action OnResultPhaseAnimationFinished;

        // ======================================================
        // IUpdatable 派生イベント
        // ======================================================

        protected override void OnEnterInternal()
        {
            base.OnEnterInternal();
            
            if (_scoreText != null)
            {
                // スコア表示フォーマットクラスを生成する
                _scoreFormatService = new TextFormatService(_scoreText, SCORE_FORMAT, SCORE_DIGITS);

                // スコア表示
                NotifyScoreChanged(ScoreManager.Instance.TotalScore);
            }
        }

        protected override void OnLateUpdateInternal(in float unscaledDeltaTime)
        {
            base.OnLateUpdateInternal(unscaledDeltaTime);

            if (InputManager.Instance.ButtonA.Down)
            {
                _effectAnimator?.Play(END_ANIMATION_NAME, 0, 0f);
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // アニメーションイベント
        // --------------------------------------------------
        /// <summary>
        /// Result フェーズアニメーション開始時に呼ばれる処理
        /// </summary>
        public void ResultPhaseAnimationStart()
        {
            _fade?.FadeOut(FADE_TIME);
        }

        /// <summary>
        /// Result 終了アニメーション終了時に呼ばれる処理
        /// </summary>
        public void ResultPhaseAnimationFinish()
        {
            OnResultPhaseAnimationFinished?.Invoke();
            SoundManager.Instance?.StopBGM(0);
        }

        /// <summary>
        /// フェードインアニメーション開始時に呼ばれる処理
        /// </summary>
        public void FadeInAnimationStart()
        {
            _fade?.FadeIn(FADE_TIME);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// スコア変更時の処理を行う
        /// </summary>
        /// <param name="score">加算されるスコア値</param>
        private void NotifyScoreChanged(int score)
        {
            if (_scoreText == null)
            {
                return;
            }

            // 現在スコア取得
            int totalScore = ScoreManager.Instance?.TotalScore ?? 0;

            // フォーマットを使用して UI に反映
            _scoreFormatService.SetNumberText(totalScore);
        }
    }
}