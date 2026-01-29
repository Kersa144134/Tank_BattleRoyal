// ======================================================
// UIManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-01-19
// 更新日時 : 2026-01-21
// 概要     : 各種UIコントローラーを生成・更新する
// ======================================================

using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;
using SceneSystem.Data;
using SceneSystem.Interface;
using ShaderSystem.Controller;
using TankSystem.Manager;
using UISystem.Controller;

namespace UISystem.Manager
{
    /// <summary>
    /// UI 全体の生成および更新を管理するクラス
    /// </summary>
    public sealed class UIManager : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        // --------------------------------------------------
        // 画面アニメーション
        // --------------------------------------------------
        [Header("画面アニメーション")]
        /// <summary>画面フェードクラス</summary>
        [SerializeField]
        private Fade _fade;

        // --------------------------------------------------
        // 演出 <2 値化>
        // --------------------------------------------------
        [Header("演出 <2 値化>")]
        /// <summary>2 値化エフェクト用の制御対象となる Full Screen Pass Render Feature</summary>
        [SerializeField]
        private ScriptableRendererFeature _binarizationFullScreenPassFeature;

        /// <summary>2 値化エフェクト用の Full Screen Pass で使用するマテリアル</summary>
        [SerializeField]
        private Material _binarizationEffectMaterial;

        /// <summary>エフェクトが有効かどうか</summary>
        [SerializeField]
        private bool _isBinarizationEffectEnabled;

        /// <summary>歪みの中心座標（UV 空間）</summary>
        [SerializeField]
        private Vector2 _binarizationDistortionCenter;

        /// <summary>歪みエフェクトの強度</summary>
        [SerializeField]
        private float _binarizationDistortionStrength;

        /// <summary>ノイズエフェクトの強度</summary>
        [SerializeField]
        private float _binarizationNoiseStrength;

        /// <summary>ポスタライズ処理のしきい値</summary>
        [SerializeField]
        private float _binarizationPosterizationThreshold;

        /// <summary>ポスタライズ明部カラー</summary>
        [SerializeField, ColorUsage(false, true)]
        private Color _binarizationPosterizationLightColor;

        /// <summary>ポスタライズ暗部カラー</summary>
        [SerializeField, ColorUsage(false, true)]
        private Color _binarizationPosterizationDarkColor;

        // --------------------------------------------------
        // 演出 <グレースケール>
        // --------------------------------------------------
        [Header("演出 <グレースケール>")]
        /// <summary>グレースケール用の制御対象となる Full Screen Pass Render Feature</summary>
        [SerializeField]
        private ScriptableRendererFeature _greyScaleFullScreenPassFeature;

        /// <summary>グレースケール用の Full Screen Pass で使用するマテリアル</summary>
        [SerializeField]
        private Material _greyScaleEffectMaterial;

        /// <summary>エフェクトが有効かどうか</summary>
        [SerializeField]
        private bool _isGreyScaleEffectEnabled;

        /// <summary>グレースケールの強さ</summary>
        [SerializeField]
        private Vector3 _greyScaleStrength;

        /// <summary>歪みの中心座標（UV 空間）</summary>
        [SerializeField]
        private Vector2 _greyScaleDistortionCenter;

        /// <summary>歪みエフェクトの強度</summary>
        [SerializeField]
        private float _greyScaleDistortionStrength;

        /// <summary>ノイズエフェクトの強度</summary>
        [SerializeField]
        private float _greyScaleNoiseStrength;

        /// <summary>ポスタライズ明部カラー</summary>
        [SerializeField, ColorUsage(false, true)]
        private Color _greyScalePosterizationLightColor;

        /// <summary>ポスタライズ暗部カラー</summary>
        [SerializeField, ColorUsage(false, true)]
        private Color _greyScalePosterizationDarkColor;

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
        /// <summary>弾丸アイコン Image 配列</summary>
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
        /// <summary>2 値化エフェクト用のシェーダーパラメーターコントローラー</summary>
        private BinarizationPostProcessController _binarizationPostProcessController;

        /// <summary>グレースケール化エフェクト用のシェーダーパラメーターコントローラー</summary>
        private GreyScalePostProcessController _greyScalePostProcessController;

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

        /// <summary>UI アニメーション用アニメーター</summary>
        private Animator _uiAnimator;

        /// <summary>現在インゲーム状態かどうか</summary>
        private bool _isInGame;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>死亡アニメーション名</summary>
        private const string DIE_STATE_NAME = "Die";

        /// <summary>撃破アニメーション名</summary>
        private const string DESTROY_STATE_NAME = "Destroy";

        /// <summary>ゲーム終了アニメーション名</summary>
        private const string FINISH_STATE_NAME = "Finish";

        /// <summary>フェード演出の時間</summary>
        private const float FADE_TIME = 0.5f;

        /// <summary>通常時のタイムスケール</summary>
        private const float DEFAULT_TIME_SCALE = 1.0f;

        /// <summary>撃破時のタイムスケール</summary>
        private const float DESTROY_TIME_SCALE = 0.25f;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>Finish フェーズアニメーション終了時</summary>
        public event Action OnReadyPhaseAnimationFinished;

        /// <summary>Ready フェーズアニメーション終了時</summary>
        public event Action OnFinishPhaseAnimationFinished;

        /// <summary>撃破アニメーション開始時</summary>
        public event Action<float> OnFlashAnimationStarted;

        /// <summary>撃破フェーズアニメーション終了時</summary>
        public event Action<float> OnFlashAnimationFinished;

        /// <summary>死亡アニメーション終了時</summary>
        public event Action OnDieAnimationFinished;

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void OnEnter()
        {
            _uiAnimator = GetComponent<Animator>();
            // タイムスケールを無視する
            _uiAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

            _binarizationPostProcessController =
                new BinarizationPostProcessController(
                    _binarizationFullScreenPassFeature,
                    _binarizationEffectMaterial);

            _greyScalePostProcessController =
                new GreyScalePostProcessController(
                    _greyScaleFullScreenPassFeature,
                    _greyScaleEffectMaterial);

            if (_playerTankRootManager is PlayerTankRootManager)
            {
                _playerDurabilityManager =
                    _playerTankRootManager.DurabilityManager;

                _durabilityBarWidthUIController =
                    new ValueBarWidthUIController(
                        _maxDurabilityBarImage,
                        _currentDurabilityBarImage,
                        _diffDurabilityBarImage,
                        _playerTankRootManager.DurabilityManager.MaxDurability,
                        _playerTankRootManager.DurabilityManager.CurrentDurability
                    );
            }

            _bulletIconSlotRotationUIController =
                new SlotRotationUIController(
                    _bulletIconImages,
                    _bulletIconLayoutDirection,
                    _bulletIconRotationSign
                );

            _logRotationUIController =
                new LogRotationUIController(
                    _logTexts,
                    _logVerticalDirection,
                    _logInsertDirection
                );
        }

        public void OnLateUpdate(in float unscaledDeltaTime)
        {
            _binarizationPostProcessController.Update(
                _isBinarizationEffectEnabled,
                _binarizationDistortionCenter,
                _binarizationDistortionStrength,
                _binarizationNoiseStrength,
                _binarizationPosterizationThreshold,
                _binarizationPosterizationLightColor,
                _binarizationPosterizationDarkColor
            );

            _greyScalePostProcessController.Update(
                _isGreyScaleEffectEnabled,
                _greyScaleStrength,
                _greyScaleDistortionCenter,
                _greyScaleDistortionStrength,
                _greyScaleNoiseStrength,
                _greyScalePosterizationLightColor,
                _greyScalePosterizationDarkColor
            );

            if (!_isInGame)
            {
                return;
            }

            if (_playerDurabilityManager != null)
            {
                _durabilityBarWidthUIController.Update(unscaledDeltaTime);
            }

            _bulletIconSlotRotationUIController.Update(unscaledDeltaTime);
            _logRotationUIController.Update(unscaledDeltaTime);
        }

        public void OnPhaseEnter(in PhaseType phase)
        {
            // Play フェーズ開始時にインゲーム状態
            if (phase == PhaseType.Play)
            {
                _isInGame = true;
            }

            // Finish フェーズ開始時に Finish アニメーション再生
            if (phase == PhaseType.Finish)
            {
                // 先頭から再生
                _uiAnimator.Play(FINISH_STATE_NAME, 0, 0f);
            }
        }

        public void OnPhaseExit(in PhaseType phase)
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
        /// 弾数に応じて弾丸アイコン表示を更新する
        /// </summary>
        public void UpdateBulletIcons()
        {
            _bulletIconSlotRotationUIController.Rotate();
        }

        /// <summary>
        /// 耐久値変更時の処理を行うハンドラ
        /// </summary>
        public void NotifyDurabilityChanged()
        {
            _durabilityBarWidthUIController.NotifyValueChanged(
                _playerTankRootManager.DurabilityManager.MaxDurability,
                _playerTankRootManager.DurabilityManager.CurrentDurability
            );
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
            _logRotationUIController.AddLog(logMessage);
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
                _uiAnimator.Play(DIE_STATE_NAME, 0, 0f);
            }
            // 敵戦車の場合
            else
            {
                // 自身を除外するため -1
                displayTankId = tankId - 1;

                // ログに表示するメッセージを生成
                logMessage = $"戦車{displayTankId} を撃破";

                // 先頭から再生
                _uiAnimator.Play(DESTROY_STATE_NAME, 0, 0f);
            }

            // ログ UI に追加
            _logRotationUIController.AddLog(logMessage);
        }

        // --------------------------------------------------
        // アニメーションイベント
        // --------------------------------------------------
        /// <summary>
        /// Ready フェーズアニメーション開始時に呼ばれる処理
        /// </summary>
        public void ReadyPhaseAnimationStart()
        {
            _fade.FadeOut(FADE_TIME);
        }

        /// <summary>
        /// Ready フェーズアニメーション終了時に呼ばれる処理
        /// </summary>
        public void ReadyPhaseAnimationFinish()
        {
            OnReadyPhaseAnimationFinished?.Invoke();
        }

        /// <summary>
        /// Finish フェーズアニメーション開始時に呼ばれる処理
        /// </summary>
        public void FinishPhaseAnimationStart()
        {
        }

        /// <summary>
        /// Finish フェーズアニメーション終了時に呼ばれる処理
        /// </summary>
        public void FinishPhaseAnimationFinish()
        {
            _fade.FadeIn(FADE_TIME);
            OnFinishPhaseAnimationFinished?.Invoke();
        }

        /// <summary>
        /// フラッシュアニメーション開始時に呼ばれる処理
        /// </summary>
        public void FlashAnimationStart()
        {
            OnFlashAnimationStarted?.Invoke(DESTROY_TIME_SCALE);
        }
        
        /// <summary>
        /// フラッシュアニメーション終了時に呼ばれる処理
        /// </summary>
        public void FlashAnimationFinish()
        {
            OnFlashAnimationFinished?.Invoke(DEFAULT_TIME_SCALE);
        }
        
        /// <summary>
        /// 死亡アニメーション終了時に呼ばれる処理
        /// </summary>
        public void DieAnimationFinish()
        {
            OnDieAnimationFinished?.Invoke();
        }

        /// <summary>
        /// フェードインアニメーション開始時に呼ばれる処理
        /// </summary>
        public void FadeInAnimationStart()
        {
            _fade.FadeIn(FADE_TIME);
        }
    }
}