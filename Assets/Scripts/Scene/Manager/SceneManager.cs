// ======================================================
// SceneManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2025-12-08
// 概要     : シーン遷移、フェーズ管理、Update 管理を統括する
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using InputSystem.Manager;
using SceneSystem.Controller;
using SceneSystem.Data;
using SceneSystem.Interface;
using TankSystem.Manager;
using WeaponSystem.Data;
using WeaponSystem.Manager;

public class SceneManager : MonoBehaviour
{
    // ======================================================
    // インスペクタ設定
    // ======================================================

    [Header("コンポーネント参照")]
    /// <summary>IUpdatable インターフェースを実装しているコンポーネントを取得するためのシーン上の GameObject 配列</summary>
    [SerializeField] private GameObject[] _components;

    // ======================================================
    // コンポーネント参照
    // ======================================================

    // --------------------------------------------------
    // シーン・フェーズ制御
    // --------------------------------------------------
    /// <summary>IUpdatable を保持し毎フレーム OnUpdate を実行するコントローラ</summary>
    private UpdateController _updateController;

    /// <summary>フェーズ切替を管理し UpdateController に対象を割り当てるコントローラ</summary>
    private PhaseController _phaseController;

    /// <summary>フェーズデータを実行時形式で保持するランタイムデータ</summary>
    private PhaseRuntimeData _phaseRuntimeData;

    // --------------------------------------------------
    // その他
    // --------------------------------------------------
    /// <summary>弾丸オブジェクトとロジックの管理を行うプールクラス</summary>
    private BulletPool _bulletPool;

    /// <summary>入力管理クラス</summary>
    private InputManager _inputManager;

    /// <summary>プレイヤー戦車の各種制御を統括するクラス</summary>
    private PlayerTankRootManager _playerTankRootManager;

    /// <summary>シーン上に存在するすべてのエネミー戦車の各種制御を統括するクラス配列</summary>
    private EnemyTankRootManager[] _enemyTankRootManagers;

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

    private IUpdatable[] _updatables;

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

        // PhaseRuntimeData の生成
        _phaseRuntimeData = new PhaseRuntimeData();

        // シーン上すべての IUpdatable を取得して登録
        _updatables = UpdatableCollector.Collect(_components);

        // フェーズごとに IUpdatable を取得して登録
        foreach (PhaseData phaseData in phaseDataList)
        {
            Type[] types = phaseData.GetUpdatableTypes();
            IUpdatable[] updatables = UpdatableCollector.Collect(_components, types);
            _phaseRuntimeData.RegisterPhase(phaseData.Phase, updatables);
        }

        // PhaseController の生成
        _phaseController = new PhaseController(_phaseRuntimeData, _updateController);

        // 初期フェーズ設定
        _targetPhase = PhaseType.Play;
        ChangePhase(_targetPhase);

        // 一時的にエネミーを収集するためのリスト
        List<EnemyTankRootManager> enemyList = new List<EnemyTankRootManager>();

        // 現在シーンの OnEnter を実行
        foreach (IUpdatable updatable in _updatables)
        {
            updatable.OnEnter();

            // コンポーネントのキャッシュ登録
            if (updatable is BulletPool bulletPool)
            {
                _bulletPool = bulletPool;
            }
            if (updatable is InputManager inputManager)
            {
                _inputManager = inputManager;
            }
            if (updatable is PlayerTankRootManager playerTankRootManager)
            {
                _playerTankRootManager = playerTankRootManager;
            }

            // Enemy 戦車は一時リストに収集
            if (updatable is EnemyTankRootManager enemyTankRootManager)
            {
                enemyList.Add(enemyTankRootManager);
            }
        }

        // List → 配列へ確定
        _enemyTankRootManagers = enemyList.ToArray();
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

    private void OnEnable()
    {
        // イベント購読
        if (_playerTankRootManager != null)
        {
            _playerTankRootManager.OnFireBullet += HandleFireBullet;
            _playerTankRootManager.OnOptionButtonPressed += HandleOptionButtonPressed;
        }

        if (_enemyTankRootManagers == null)
        {
            return;
        }
        for (int i = 0; i < _enemyTankRootManagers.Length; i++)
        {
            _enemyTankRootManagers[i].OnFireBullet += HandleFireBullet;
        }
    }

    private void OnDisable()
    {
        // イベント購読解除
        if (_playerTankRootManager != null)
        {
            _playerTankRootManager.OnFireBullet -= HandleFireBullet;
            _playerTankRootManager.OnOptionButtonPressed -= HandleOptionButtonPressed;
        }

        if (_enemyTankRootManagers == null)
        {
            return;
        }
        for (int i = 0; i < _enemyTankRootManagers.Length; i++)
        {
            _enemyTankRootManagers[i].OnFireBullet -= HandleFireBullet;
        }
    }

    // ======================================================
    // プライベートメソッド
    // ======================================================

    /// <summary>
    /// シーン遷移を実行する
    /// </summary>
    /// <param name="sceneName">遷移先のシーン名</param>
    private void ChangeScene(in string sceneName)
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
            // 現在シーンの OnExit を実行
            foreach (IUpdatable updatable in _updatables)
            {
                updatable.OnExit();
            }

            // 現在シーン情報を更新
            _currentScene = sceneName;

            // シーン遷移を実行
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }

    /// <summary>
    /// フェーズ切替を実行する
    /// </summary>
    private void ChangePhase(in PhaseType nextPhase)
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

    // ======================================================
    // イベントハンドラ
    // ======================================================

    /// <summary>
    /// オプションボタン押下時の処理を行うハンドラ
    /// 現在のフェーズに応じて入力マッピングとターゲットフェーズを切り替える
    /// </summary>
    private void HandleOptionButtonPressed()
    {
        switch (_currentPhase)
        {
            case PhaseType.Play:
                // Play フェーズなら Pause に切替
                _targetPhase = PhaseType.Pause;

                // UI用入力マッピング
                _inputManager.SwitchInputMapping(1);
                break;

            case PhaseType.Pause:
                // Pause フェーズなら Play に切替
                _targetPhase = PhaseType.Play;

                // インゲーム用入力マッピング
                _inputManager.SwitchInputMapping(0);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// TankRootManager から発射イベントを受け取り
    /// BulletPool で弾丸を生成・発射する
    /// </summary>
    /// <param name="tank">発射元の戦車</param>
    /// <param name="type">発射する弾丸の種類</param>
    private void HandleFireBullet(BaseTankRootManager tank, BulletType type)
    {
        // 必須参照が欠けている場合は処理しない
        if (_bulletPool == null || tank == null)
        {
            Debug.LogWarning("[SceneManager] BulletPool または TankRootManager が未設定です。");
            return;
        }

        // 発射位置を発射元戦車から取得
        Vector3 firePosition = tank.FirePoint.position;

        // 発射方向を発射元戦車の前方から取得
        Vector3 fireDirection = tank.transform.forward;

        // BulletPool で弾丸を生成・発射
        _bulletPool.Spawn(type, tank.TankStatus, firePosition, fireDirection);
    }
}