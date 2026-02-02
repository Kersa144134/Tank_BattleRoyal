// ======================================================
// TitleUIManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-01-19
// 更新日時 : 2026-02-02
// 概要     : タイトルシーンで使用される UI 演出を管理するクラス
// ======================================================

using System;
using InputSystem.Manager;
using SceneSystem.Interface;

namespace UISystem.Manager
{
    /// <summary>
    /// タイトルシーンにおける UI 演出およびゲーム連動 UI を管理するクラス
    /// </summary>
    public sealed class TitleUIManager : BaseUIManager, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        // [Header("タイトルシーン固有インスペクタ")]

        // ======================================================
        // コンポーネント参照
        // ======================================================

        // ======================================================
        // フィールド
        // ======================================================

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>タイトル終了アニメーション名</summary>
        private const string End_ANIMATION_NAME = "End";

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>Title フェーズアニメーション終了時</summary>
        public event Action OnTitlePhaseAnimationFinished;

        // ======================================================
        // IUpdatable 派生イベント
        // ======================================================

        protected override void OnEnterInternal()
        {
            base.OnEnterInternal();
        }

        protected override void OnLateUpdateInternal(in float unscaledDeltaTime)
        {
            base.OnLateUpdateInternal(unscaledDeltaTime);

            if (InputManager.Instance.ButtonA.Down)
            {
                _effectAnimator?.Play(End_ANIMATION_NAME, 0, 0f);
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // アニメーションイベント
        // --------------------------------------------------
        /// <summary>
        /// Title フェーズアニメーション開始時に呼ばれる処理
        /// </summary>
        public void TitlePhaseAnimationStart()
        {
            _fade?.FadeOut(FADE_TIME);
        }

        /// <summary>
        /// Title 終了アニメーション終了時に呼ばれる処理
        /// </summary>
        public void TitlePhaseAnimationFinish()
        {
            OnTitlePhaseAnimationFinished?.Invoke();
        }

        /// <summary>
        /// フェードインアニメーション開始時に呼ばれる処理
        /// </summary>
        public void FadeInAnimationStart()
        {
            _fade?.FadeIn(FADE_TIME);
        }
    }
}