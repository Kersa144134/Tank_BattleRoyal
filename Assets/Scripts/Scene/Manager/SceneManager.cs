// ======================================================
// SceneManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2026-01-23
// 概要     : シーン遷移、フェーズ管理、Update 管理を統括する
// ======================================================

using UnityEngine;
using InputSystem.Manager;
using SceneSystem.Controller;
using SceneSystem.Data;
using SceneSystem.Interface;
using SceneSystem.Utility;

namespace SceneSystem.Manager
{
    /// <summary>
    /// シーン遷移・フェーズ遷移・Update 実行を統括する
    /// </summary>
    public sealed class SceneManager : MonoBehaviour
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("IUpdatable 保持オブジェクト")]
        /// <summary>IUpdatable を保持している GameObject 群</summary>
        [SerializeField] private GameObject[] _components;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>フェーズ切替制御クラス</summary>
        private PhaseController _phaseController;

        /// <summary>フェーズ遷移条件管理クラス</summary>
        private PhaseManager _phaseManager = new PhaseManager();

        /// <summary>フェーズ単位の IUpdatable を保持するクラス</summary>
        private PhaseRuntimeData _phaseRuntimeData = new PhaseRuntimeData();

        /// <summary>Update / LateUpdate / PhaseEnterExit を管理するクラス</summary>
        private UpdateManager _updateManager;

        /// <summary>IUpdatable の初期化と参照解決を行うクラス</summary>
        private UpdatableBootstrapper _bootstrapper = new UpdatableBootstrapper();

        /// <summary>シーン内イベントを仲介するクラス</summary>
        private SceneEventRouter _sceneEventRouter;

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

        /// <summary>遷移先フェーズ/summary>
        private PhaseType _targetPhase = PhaseType.None;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// ゲームの経過時間
        /// </summary>
        public float ElapsedTime => _updateManager.ElapsedTime;

        // ======================================================
        // Unityイベント
        // ======================================================

        private void Awake()
        {
            // 全フェーズデータを読み込む
            PhaseData[] phaseDataList = Resources.LoadAll<PhaseData>("Phase");

            // 各フェーズデータに対して初期化処理
            foreach (PhaseData phaseData in phaseDataList)
            {
                IUpdatable[] phaseUpdatables =
                    UpdatableCollector.Collect(
                        _components,
                        phaseData.GetUpdatableTypes()
                    );

                _phaseRuntimeData.RegisterPhase(
                    phaseData.Phase,
                    phaseUpdatables
                );
            }

            // Update 処理対象管理クラスの生成
            UpdateController updateController = new UpdateController();

            // フェーズ切替処理クラスの生成
            _phaseController = new PhaseController(
                _phaseRuntimeData,
                updateController
            );

            // Update 管理クラスの生成
            _updateManager = new UpdateManager(updateController);

            // Bootstrapper を通じてコンポーネント初期化
            UpdatableContext context = _bootstrapper.Initialize(_components);

            // シーンイベントクラスの生成
            _sceneEventRouter = new SceneEventRouter(context);

            // イベント購読
            _sceneEventRouter.Subscribe();

            // 初期フェーズを設定
            _targetPhase = PhaseType.Ready;
            ChangePhase(_targetPhase);
        }

        private void Update()
        {
            // シーン遷移判定
            if (_currentScene != _targetScene)
            {
                ChangeScene(_targetScene);
                return;
            }

            // フェーズ遷移判定
            if (_currentPhase != _targetPhase)
            {
                ChangePhase(_targetPhase);
            }

            // Update 実行
            _updateManager.Update();
        }

        private void LateUpdate()
        {
            float unscaledDeltaTime = Time.unscaledDeltaTime;

            // LateUpdate 実行
            _updateManager.LateUpdate();

            // オプションボタン押下判定
            if (InputManager.Instance.StartButton.Down)
            {
                _phaseManager.ToggleOptionPhaseChange(ref _currentPhase);
                _sceneEventRouter.HandleOptionButtonPressed();
            }

            // フェーズおよびシーン遷移条件の更新
            _phaseManager.Update(unscaledDeltaTime, ref _currentScene, ref _currentPhase);
        }

        private void OnDestroy()
        {
            // イベント購読解除
            _sceneEventRouter.Dispose();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// シーン遷移を行う
        /// </summary>
        private void ChangeScene(in string sceneName)
        {
            // 無効なシーン名は無視
            if (string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            // 現在シーンを更新
            _currentScene = sceneName;

            // シーンロード
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// フェーズ切替を行う
        /// </summary>
        private void ChangePhase(in PhaseType nextPhase)
        {
            // 現在フェーズを更新
            _currentPhase = nextPhase;

            // フェーズ変更
            _phaseController.ChangePhase(nextPhase);

            // UpdateManager 側へ通知
            _updateManager.ChangePhase(nextPhase);
        }
    }
}