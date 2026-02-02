// ======================================================
// MainUIManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-01-19
// 更新日時 : 2026-02-02
// 概要     : メインシーンで使用される UI 演出を管理するクラス
// ======================================================

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SceneSystem.Data;
using SceneSystem.Interface;
using TankSystem.Manager;
using UISystem.Controller;

namespace UISystem.Manager
{
    /// <summary>
    /// メインシーンにおける UI 演出およびゲーム連動 UI を管理するクラス
    /// </summary>
    public sealed class MainUIManager : BaseUIManager, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("メインシーン固有インスペクタ")]

        // --------------------------------------------------
        // 画面アニメーション
        // --------------------------------------------------
        [Header("画面アニメーション")]
        /// <summary>
        /// カメラ用アニメーター
        /// </summary>
        [SerializeField]
        private Animator _cameraAnimator;

        // --------------------------------------------------
        // 時間表示
        // --------------------------------------------------
        [Header("時間表示")]
        /// <summary>制限時間を表示するテキスト</summary>
        [SerializeField]
        private TextMeshProUGUI _limitTimeText;
        
        // --------------------------------------------------
        // 耐久値バー
        // --------------------------------------------------
        [Header("耐久値バー")]
        /// <summary>最大耐久値を表すバー Image</summary>
        [SerializeField]
        private Image _maxDurabilityBarImage;

        /// <summary>現在耐久値を表すバー Image</summary>
        [SerializeField]
        private Image _currentDurabilityBarImage;

        /// <summary>差分耐久値を表すバー Image</summary>
        [SerializeField]
        private Image _diffDurabilityBarImage;

        // --------------------------------------------------
        // 弾丸アイコン
        // --------------------------------------------------
        [Header("弾丸アイコン")]
        /// <summary>弾丸アイコン Image 配列</summary>
        [SerializeField]
        private Image[] _bulletIconImages;

        /// <summary>弾丸アイコンの配置方向</summary>
        [SerializeField]
        private SlotRotationUIController.LayoutDirection _bulletIconLayoutDirection;

        /// <summary>弾丸アイコンの回転方向の符号</summary>
        [SerializeField]
        private SlotRotationUIController.RotationSign _bulletIconRotationSign;

        // --------------------------------------------------
        // ログ
        // --------------------------------------------------
        [Header("ログ")]
        /// <summary>ログテキスト配列</summary>
        [SerializeField]
        private TextMeshProUGUI[] _logTexts;

        /// <summary>ログの縦方向表示方向</summary>
        [SerializeField]
        private LogRotationUIController.VerticalDirection _logVerticalDirection;

        /// <summary>ログの挿入方向</summary>
        [SerializeField]
        private LogRotationUIController.InsertDirection _logInsertDirection;

        // --------------------------------------------------
        // プレイヤー戦車
        // --------------------------------------------------
        [Header("プレイヤー戦車")]
        /// <summary>プレイヤー戦車のルートマネージャー</summary>
        [SerializeField]
        private BaseTankRootManager _playerTankRootManager;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        // --------------------------------------------------
        // UI
        // --------------------------------------------------
        /// <summary>ログ表示 UI コントローラー</summary>
        private LogRotationUIController _logRotationUIController;

        /// <summary>弾丸アイコンスロット UI コントローラー</summary>
        private SlotRotationUIController _bulletIconSlotRotationUIController;

        /// <summary>耐久値バー横幅 UI コントローラー</summary>
        private ValueBarWidthUIController _durabilityBarWidthUIController;

        // --------------------------------------------------
        // データ参照
        // --------------------------------------------------
        /// <summary>プレイヤー戦車の耐久力マネージャー</summary>
        private TankDurabilityManager _playerDurabilityManager;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>現在インゲーム状態かどうか</summary>
        private bool _isInGame;

        // ======================================================
        // 定数
        // ======================================================

        // --------------------------------------------------
        // アニメーション名
        // --------------------------------------------------
        /// <summary>開始前アニメーション名</summary>
        private const string READY_ANIMATION_NAME = "Ready";

        /// <summary>終了アニメーション名</summary>
        private const string FINISH_ANIMATION_NAME = "Finish";

        /// <summary>Show アニメーション名</summary>
        private const string SHOW_ANIMATION_NAME = "Show";

        /// <summary>Hide アニメーション名</summary>
        private const string HIDE_ANIMATION_NAME = "Hide";

        /// <summary>攻撃時アニメーション名</summary>
        private const string FIRE_ANIMATION_NAME = "Fire";

        /// <summary>エフェクト発火時アニメーション名</summary>
        private const string FLASH_ANIMATION_NAME = "Flash";

        /// <summary>死亡アニメーション名</summary>
        private const string DIE_ANIMATION_NAME = "Die";

        /// <summary>撃破アニメーション名</summary>
        private const string DESTROY_ANIMATION_NAME = "Destroy";

        // --------------------------------------------------
        // パラメーター
        // --------------------------------------------------
        /// <summary>通常時のタイムスケール</summary>
        private const float DEFAULT_TIME_SCALE = 1.0f;

        /// <summary>撃破時のタイムスケール</summary>
        private const float DESTROY_TIME_SCALE = 0.25f;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>Ready フェーズアニメーション終了時</summary>
        public event Action OnReadyPhaseAnimationFinished;

        /// <summary>Finish フェーズアニメーション終了時</summary>
        public event Action<float> OnFinishPhaseAnimationFinished;

        /// <summary>撃破アニメーション開始時</summary>
        public event Action<float> OnFlashAnimationStarted;

        /// <summary>撃破フェーズアニメーション終了時</summary>
        public event Action<float> OnFlashAnimationFinished;

        /// <summary>死亡アニメーション終了時</summary>
        public event Action<float> OnDieAnimationFinished;

        // ======================================================
        // IUpdatable 派生イベント
        // ======================================================

        protected override void OnEnterInternal()
        {
            base.OnEnterInternal();

            // カメラ用 Animator をタイムスケール非依存に設定する
            SetAnimatorUnscaledTime(_cameraAnimator);

            // プレイヤー耐久値 UI が使用可能かを確認する
            if (_playerTankRootManager != null &&
                _maxDurabilityBarImage != null &&
                _currentDurabilityBarImage != null &&
                _diffDurabilityBarImage != null)
            {
                // プレイヤー用耐久値管理クラスを取得する
                _playerDurabilityManager =
                    _playerTankRootManager.DurabilityManager;

                // 耐久値バー制御クラスを生成する
                _durabilityBarWidthUIController =
                    new ValueBarWidthUIController(
                        _maxDurabilityBarImage,
                        _currentDurabilityBarImage,
                        _diffDurabilityBarImage,
                        _playerDurabilityManager.MaxDurability,
                        _playerDurabilityManager.CurrentDurability
                    );
            }

            // 弾丸アイコン UI が設定されているかを確認する
            if (_bulletIconImages != null)
            {
                // 弾丸スロット回転制御クラスを生成する
                _bulletIconSlotRotationUIController =
                    new SlotRotationUIController(
                        _bulletIconImages,
                        _bulletIconLayoutDirection,
                        _bulletIconRotationSign
                    );
            }

            // ログ UI が設定されているかを確認する
            if (_logTexts != null)
            {
                // ログ回転制御クラスを生成する
                _logRotationUIController =
                    new LogRotationUIController(
                        _logTexts,
                        _logVerticalDirection,
                        _logInsertDirection
                    );
            }
        }

        protected override void OnLateUpdateInternal(in float unscaledDeltaTime)
        {
            base.OnLateUpdateInternal(unscaledDeltaTime);

            if (!_isInGame)
            {
                return;
            }

            // プレイヤー耐久値 UI を更新する
            if (_playerDurabilityManager != null)
            {
                _durabilityBarWidthUIController?.Update(unscaledDeltaTime);
            }

            // 弾丸アイコン UI を更新する
            _bulletIconSlotRotationUIController?.Update(unscaledDeltaTime);

            // ログ UI を更新する
            _logRotationUIController?.Update(unscaledDeltaTime);
        }

        // ======================================================
        // IUpdatable 派生イベント
        // ======================================================

        protected override void OnPhaseEnterInternal(in PhaseType phase)
        {
            // Play フェーズ開始時にインゲーム状態
            if (phase == PhaseType.Play)
            {
                _isInGame = true;
            }

            // Finish フェーズ開始時に Finish アニメーション再生
            if (phase == PhaseType.Finish)
            {
                _effectAnimator?.Play(FINISH_ANIMATION_NAME, 0, 0f);
            }
        }

        protected override void OnPhaseExitInternal(in PhaseType phase)
        {
            // Play フェーズ終了時にインゲーム状態解除
            if (phase == PhaseType.Play)
            {
                _isInGame = false;
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 経過時間と制限時間から残り時間を計算し、UI に表示する
        /// </summary>
        /// <param name="elapsedTime">現在までの経過時間（秒）</param>
        /// <param name="limitTime">制限時間（秒）</param>
        public void UpdateLimitTimeDisplay(in float elapsedTime, in float limitTime)
        {
            // 制限時間表示用テキストが未設定の場合は処理なし
            if (_limitTimeText == null)
            {
                return;
            }

            // 残り時間を計算する
            float remainingTime = limitTime - elapsedTime;

            // 残り時間が 0 未満にならないよう補正する
            if (remainingTime < 0.0f)
            {
                remainingTime = 0.0f;
            }

            // 分を算出する
            int minutes =
                Mathf.FloorToInt(remainingTime / 60.0f);

            // 秒を算出する
            int seconds =
                Mathf.FloorToInt(remainingTime % 60.0f);

            // MM:SS 形式でテキストに反映する
            _limitTimeText.text =
                $"{minutes:00}:{seconds:00}";
        }

        /// <summary>
        /// 耐久値変更時の処理を行う
        /// </summary>
        public void NotifyDurabilityChanged()
        {
            _durabilityBarWidthUIController?.NotifyValueChanged(
                _playerTankRootManager.DurabilityManager.MaxDurability,
                _playerTankRootManager.DurabilityManager.CurrentDurability
            );
        }

        /// <summary>
        /// 弾数に応じて弾丸アイコン表示を更新する
        /// </summary>
        public void UpdateBulletIcons()
        {
            _bulletIconSlotRotationUIController?.Rotate();
        }

        /// <summary>
        /// 攻撃時の処理を行う
        /// </summary>
        public void NotifyFireBullet()
        {
            _cameraAnimator?.Play(FIRE_ANIMATION_NAME, 0, 0f);
        }

        /// <summary>
        /// アイテム獲得時のログ表示を行う
        /// </summary>
        /// <param name="itemName">獲得したアイテム名</param>
        public void NotifyItemAcquired(in string itemName)
        {
            // ログに表示するメッセージを作成
            string logMessage = $"{itemName} を獲得";

            // ログ表示
            _logRotationUIController?.AddLog(logMessage);
        }

        /// <summary>
        /// 戦車撃破時のログ表示を行う
        /// </summary>
        /// <param name="tankId">撃破された戦車の ID</param>
        public void NotifyBrokenTanks(in int tankId)
        {
            int displayTankId;
            string logMessage;

            // 自身の戦車（ID = 1）の場合
            if (tankId == 1)
            {
                displayTankId = tankId;

                // ログに表示するメッセージを生成
                logMessage = "撃破された";

                // 先頭から再生
                _effectAnimator?.Play(DIE_ANIMATION_NAME, 0, 0f);
            }
            // 敵戦車の場合
            else
            {
                // 自身を除外するため -1
                displayTankId = tankId - 1;

                // ログに表示するメッセージを生成
                logMessage = $"戦車{displayTankId} を撃破";

                // 先頭から再生
                _effectAnimator?.Play(DESTROY_ANIMATION_NAME, 0, 0f);
            }

            // ログ UI に追加
            _logRotationUIController?.AddLog(logMessage);
        }
        
        // --------------------------------------------------
        // アニメーションイベント
        // --------------------------------------------------
        /// <summary>
        /// Ready フェーズアニメーション開始時に呼ばれる処理
        /// </summary>
        public void ReadyPhaseAnimationStart()
        {
            _notPlayAnimator?.Play(READY_ANIMATION_NAME, 0, 0f);

            _fade?.FadeOut(FADE_TIME);
        }

        /// <summary>
        /// Ready フェーズアニメーション終了時に呼ばれる処理
        /// </summary>
        public void ReadyPhaseAnimationFinish()
        {
            _playAnimator?.Play(SHOW_ANIMATION_NAME, 0, 0f);

            OnReadyPhaseAnimationFinished?.Invoke();
        }

        /// <summary>
        /// Finish フェーズアニメーション開始時に呼ばれる処理
        /// </summary>
        public void FinishPhaseAnimationStart()
        {
            _playAnimator?.Play(HIDE_ANIMATION_NAME, 0, 0f);
            _notPlayAnimator?.Play(FINISH_ANIMATION_NAME, 0, 0f);
        }

        /// <summary>
        /// Finish フェーズアニメーション終了時に呼ばれる処理
        /// </summary>
        public void FinishPhaseAnimationFinish()
        {
            _fade.FadeIn(FADE_TIME);

            OnFinishPhaseAnimationFinished?.Invoke(DEFAULT_TIME_SCALE);
        }

        /// <summary>
        /// フラッシュアニメーション開始時に呼ばれる処理
        /// </summary>
        public void FlashAnimationStart()
        {
            _playAnimator?.Play(HIDE_ANIMATION_NAME, 0, 0f);
            _cameraAnimator?.Play(FLASH_ANIMATION_NAME, 0, 0f);

            OnFlashAnimationStarted?.Invoke(DESTROY_TIME_SCALE);
        }
        
        /// <summary>
        /// フラッシュアニメーション終了時に呼ばれる処理
        /// </summary>
        public void FlashAnimationFinish()
        {
            _playAnimator?.Play(SHOW_ANIMATION_NAME, 0, 0f);

            OnFlashAnimationFinished?.Invoke(DEFAULT_TIME_SCALE);
        }

        /// <summary>
        /// 死亡アニメーション終了時に呼ばれる処理
        /// </summary>
        public void DieAnimationFinish()
        {
            OnDieAnimationFinished?.Invoke(DEFAULT_TIME_SCALE);
        }

        /// <summary>
        /// フェードインアニメーション開始時に呼ばれる処理
        /// </summary>
        public void FadeInAnimationStart()
        {
            _fade?.FadeIn(FADE_TIME);
        }

        /// <summary>
        /// フラッシュアニメーションのボリュームエフェクト開始時に呼ばれる処理
        /// </summary>
        public void VolumeFlashEffectStart()
        {
            _volumeAnimator?.Play(FLASH_ANIMATION_NAME, 0, 0f);
        }

        /// <summary>
        /// 死亡アニメーションのボリュームエフェクト開始時に呼ばれる処理
        /// </summary>
        public void VolumeDieEffectStart()
        {
            _volumeAnimator?.Play(DIE_ANIMATION_NAME, 0, 0f);
        }
    }
}