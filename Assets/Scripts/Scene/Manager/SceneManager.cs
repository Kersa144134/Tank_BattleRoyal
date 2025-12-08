// ======================================================
// SceneManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2025-12-08
// 概要     : シーン遷移、フェーズ管理、Update 管理を統括する
// ======================================================

using UnityEngine;
using SceneSystem.Controller;
using SceneSystem.Data;

public class SceneManager : MonoBehaviour
{
    // ======================================================
    // インスペクタ設定
    // ======================================================

    [Header("コンポーネント参照")]
    /// <summary>InputManager</summary>
    [SerializeField] private GameObject[] _components;

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

    /// <summary>遷移先ターゲットシーン名</summary>
    private string _targetScene = "";
    
    /// <summary>現在適用されているフェーズ</summary>
    private PhaseType _currentPhase = PhaseType.None;

    /// <summary>遷移先ターゲットフェーズ</summary>
    private PhaseType _targetPhase = PhaseType.None;

    // ======================================================
    // Unityイベント
    // ======================================================

    private void Awake()
    {
        // UpdateController の生成
        _updateController = new UpdateController();

        // Resources/Phase フォルダ内のすべての PhaseData を取得
        PhaseData[] phaseDataList = Resources.LoadAll<PhaseData>("Phase");
        if (phaseDataList == null || phaseDataList.Length == 0)
        {
            Debug.LogWarning("[SceneManager] PhaseData が Resources/Phase に存在しません");
        }

        // PhaseRuntimeData の生成（複数フェーズ対応版）
        _phaseRuntimeData = new PhaseRuntimeData(phaseDataList, _components);

        // PhaseController の生成
        _phaseController = new PhaseController(_phaseRuntimeData, _updateController);

        // 初期フェーズ設定
        _targetPhase = PhaseType.Play;
        ChangePhase(_targetPhase);

        // 現在シーンの Enter を呼ぶ
        _updateController.OnEnter();
    }

    private void Update()
    {
        // --------------------------------------------------
        // シーン判定
        // --------------------------------------------------
        if (_currentScene != _targetScene)
        {
            ChangeScene(_targetScene);

            // シーン切替中は以降処理をスキップ
            return;
        }

        // --------------------------------------------------
        // フェーズ判定
        // --------------------------------------------------
        if (_currentPhase != _targetPhase)
        {
            ChangePhase(_targetPhase);
        }

        // --------------------------------------------------
        // 更新処理
        // --------------------------------------------------
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
    /// シーン遷移を実行する
    /// </summary>
    /// <param name="sceneName">遷移先のシーン名</param>
    private void ChangeScene(string sceneName)
    {
        // 空文字なら何もしない
        if (string.IsNullOrEmpty(sceneName))
        {
            return;
        }

        // 現在アクティブなシーン名を取得
        string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // 遷移先が現在のシーンと異なる場合のみ処理
        if (activeScene != sceneName)
        {
            // 直前のシーンの Exit を呼ぶ
            _updateController.OnExit();

            // 現在シーン情報を更新
            _currentScene = sceneName;

            // シーン遷移を実行
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }

    /// <summary>
    /// フェーズ切替を実行する
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