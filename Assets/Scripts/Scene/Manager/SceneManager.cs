// ======================================================
// SceneManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2025-12-08
// 概要     : シーン遷移、フェーズ管理、Update 管理を統括する
// ======================================================

using UnityEngine;
using CameraSystem.Manager;
using InputSystem.Manager;
using SceneSystem.Controller;
using SceneSystem.Data;
using SceneSystem.Interface;
using TankSystem.Manager;

public class SceneManager : MonoBehaviour
{
    // ======================================================
    // インスペクタ設定
    // ======================================================

    [Header("コンポーネント参照")]
    /// <summary>InputManager</summary>
    [SerializeField] private InputManager _inputManager;

    /// <summary>CameraManager</summary>
    [SerializeField] private CameraManager _cameraManager;

    /// <summary>TankRootManager</summary>
    [SerializeField] private TankRootManager _tankManager;

    [Header("フェーズ設定")]
    /// <summary>ScriptableObject で設定されたフェーズ情報</summary>
    [SerializeField] private PhaseData _phaseData;

    // ======================================================
    // コンポーネント参照
    // ======================================================

    /// <summary>IUpdatable を保持し毎フレーム OnUpdate を実行するコントローラ</summary>
    private UpdateController _updateController;

    /// <summary>フェーズ切替を管理し UpdateController に対象を割り当てるコントローラ</summary>
    private PhaseController _phaseController;

    /// <summary>フェーズデータを実行時形式で保持するランタイムデータ</summary>
    private PhaseRuntimeData _phaseRuntimeData;

    // ======================================================
    // フィールド
    // ======================================================

    /// <summary>現在ロードされているシーン名</summary>
    private string _currentScene = "";

    /// <summary>現在適用されているフェーズ</summary>
    private PhaseType _currentPhase = PhaseType.None;
    
    // ======================================================
    // Unityイベント
    // ======================================================

    private void Awake()
    {
        // UpdateController の生成
        _updateController = new UpdateController();

        // PhaseRuntimeData を生成（シーン上のコンポーネントを参照して IUpdatable を取得）
        _phaseRuntimeData = new PhaseRuntimeData(_phaseData, this.gameObject);

        // PhaseController の生成
        _phaseController = new PhaseController(_phaseRuntimeData, _updateController);

        // 初期シーン設定
        ChangeScene(_currentScene);
        
        // 初期フェーズ設定
        ChangePhase(_currentPhase);
    }

    private void Update()
    {
        // 各コンポーネントを UpdateController に登録
        RegisterSceneUpdatables();

        // UpdateController 実行
        _updateController.OnUpdate();
    }

    private void LateUpdate()
    {
        // UpdateController 実行
        _updateController.OnLateUpdate();
    }

    // ======================================================
    // プライベートメソッド
    // ======================================================

    /// <summary>
    /// シーン上の IUpdatable を UpdateController に登録
    /// </summary>
    private void RegisterSceneUpdatables()
    {
        // InputManager
        if (_inputManager is IUpdatable inputUpdatable)
        {
            _updateController.Add(inputUpdatable);
        }

        // CameraManager
        if (_cameraManager is IUpdatable cameraUpdatable)
        {
            _updateController.Add(cameraUpdatable);
        }

        // TankManager
        if (_tankManager is IUpdatable tankUpdatable)
        {
            _updateController.Add(tankUpdatable);
        }
    }

    // ======================================================
    // プライベートメソッド
    // ======================================================

    /// <summary>
    /// シーン遷移を実行する
    /// </summary>
    private void ChangeScene(string sceneName)
    {
        // 直前のシーンがあれば Exit を呼ぶ
        if (!string.IsNullOrEmpty(_currentScene))
        {
            _updateController.OnExit();
        }

        // 実際のシーン遷移
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);

        // 現在シーンを更新
        _currentScene = sceneName;

        // 新しいシーンの Enter を呼ぶ
        _updateController.OnEnter();
    }

    /// <summary>
    /// フェーズ切替を外部から実行
    /// </summary>
    private void ChangePhase(PhaseType nextPhase)
    {
        // 直前フェーズがあれば Exit を呼ぶ
        if (_currentPhase != PhaseType.None)
        {
            _updateController.OnPhaseExit();
        }

        // 現在フェーズを更新
        _currentPhase = nextPhase;

        // フェーズ変更
        _phaseController.ChangePhase(nextPhase);

        // 新しいフェーズの Enter を呼ぶ
        _updateController.OnPhaseEnter();
    }
}