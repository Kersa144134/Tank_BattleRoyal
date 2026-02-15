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
using ItemSystem.Data;
using SceneSystem.Data;
using SceneSystem.Interface;
using SceneSystem.Manager;
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
        // タイマー
        // --------------------------------------------------
        [Header("タイマー")]
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

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>シーン上オブジェクトの Transform を一元管理するレジストリー</summary>
        private SceneObjectRegistry _sceneRegistry;

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

        /// <summary>直前に表示した残り秒数を保持する</summary>
        private int _previousDisplayTotalSeconds = -1;

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
        // タイマー
        // --------------------------------------------------
        /// <summary>
        /// 制限時間表示フォーマット
        /// </summary>
        private const string LIMIT_TIME_FORMAT = "{0:00}:{1:00}";
        
        // --------------------------------------------------
        // ログ
        // --------------------------------------------------
        /// <summary>自身撃破時にログへ表示する文</summary>
        private const string SELF_DESTROYED_TEXT = "撃破された";

        /// <summary>敵撃破時にログへ表示する文のフォーマット</summary>
        private const string ENEMY_DESTROYED_FORMAT = "戦車{0} を撃破";

        /// <summary>パラメータ増加時にログへ追加する文/// </summary>
        private const string INCREASE_SUFFIX = " が増加";

        /// <summary>パラメータ減少時にログへ追加する文</summary>
        private const string DECREASE_SUFFIX = " が減少";

        // --------------------------------------------------
        // その他
        // --------------------------------------------------
        /// <summary>プレイヤーの戦車ID</summary>
        private const int PLAYER_TANK_ID = 1;

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
        // セッター
        // ======================================================

        /// <summary>
        /// シーン内オブジェクト管理用のレジストリー参照を設定する
        /// </summary>
        /// <param name="sceneRegistry">シーンに存在する各種オブジェクト情報を一元管理するレジストリー</param>
        public void SetSceneRegistry(SceneObjectRegistry sceneRegistry)
        {
            _sceneRegistry = sceneRegistry;
        }

        // ======================================================
        // IUpdatable 派生イベント
        // ======================================================

        protected override void OnEnterInternal()
        {
            base.OnEnterInternal();

            // カメラ用 Animator をタイムスケール非依存に設定する
            SetAnimatorUnscaledTime(_cameraAnimator);

            BaseTankRootManager playerTankRootManager = _sceneRegistry.Tanks[0].GetComponent<BaseTankRootManager>();

            // プレイヤー耐久値 UI が使用可能かを確認する
            if (playerTankRootManager != null &&
                _maxDurabilityBarImage != null &&
                _currentDurabilityBarImage != null &&
                _diffDurabilityBarImage != null)
            {
                // プレイヤー用耐久値管理クラスを取得する
                _playerDurabilityManager =
                    playerTankRootManager.DurabilityManager;

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
            if (_limitTimeText == null)
            {
                return;
            }

            // 残り時間を算出する
            float remainingTime = limitTime - elapsedTime;

            // 残り時間が負数にならないよう補正する
            if (remainingTime < 0.0f)
            {
                remainingTime = 0.0f;
            }

            // 残り時間を整数秒へ変換する（小数切り捨て）
            int totalSeconds = Mathf.FloorToInt(remainingTime);

            // 前回表示秒と同一の場合は処理なし
            if (totalSeconds == _previousDisplayTotalSeconds)
            {
                return;
            }

            // 現在の表示秒をキャッシュへ保存する
            _previousDisplayTotalSeconds = totalSeconds;

            // 分を算出する
            int minutes = totalSeconds / 60;

            // 秒を算出する
            int seconds = totalSeconds % 60;

            // フォーマット文字列を使用して描画する
            _limitTimeText.SetText(LIMIT_TIME_FORMAT, minutes, seconds);
        }

        /// <summary>
        /// 耐久値変更時の処理を行う
        /// </summary>
        public void NotifyDurabilityChanged()
        {
            if (_playerDurabilityManager == null)
            {
                return;
            }

            _durabilityBarWidthUIController?.NotifyValueChanged(
                _playerDurabilityManager.MaxDurability,
                _playerDurabilityManager.CurrentDurability
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
        /// <param name="type">アイテム種別</param>
        public void NotifyItemAcquired(in string itemName, in ItemType type)
        {
            // アイテム種別に応じたログ文を生成
            string logMessage = type switch
            {
                ItemType.ParamIncrease => string.Concat(itemName, INCREASE_SUFFIX),
                ItemType.ParamDecrease => string.Concat(itemName, DECREASE_SUFFIX),
                _ => null
            };

            if (string.IsNullOrEmpty(logMessage))
            {
                return;
            }

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

            // 自身の戦車の場合
            if (tankId == PLAYER_TANK_ID)
            {
                displayTankId = tankId;

                // ログ文言を設定
                logMessage = SELF_DESTROYED_TEXT;

                // アニメーションを先頭から再生
                _effectAnimator?.Play(DIE_ANIMATION_NAME, 0, 0f);
            }
            else
            {
                // 自身を除外するためIDを補正
                displayTankId = tankId - 1;

                // フォーマットを使用してログ文を生成
                logMessage = string.Format(ENEMY_DESTROYED_FORMAT, displayTankId);

                // アニメーションを先頭から再生
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