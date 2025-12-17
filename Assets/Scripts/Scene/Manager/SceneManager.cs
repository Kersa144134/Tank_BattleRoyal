// ======================================================
// SceneManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2025-12-08
// 概要     : シーン遷移、フェーズ管理、Update 管理を統括する
// ======================================================

using CameraSystem.Manager;
using InputSystem.Manager;
using SceneSystem.Controller;
using SceneSystem.Data;
using SceneSystem.Interface;
using System;
using System.Collections.Generic;
using TankSystem.Data;
using TankSystem.Manager;
using TankSystem.Service;
using UnityEngine;
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
    private UpdateController _updateController = new UpdateController();

    /// <summary>フェーズ切替を管理し UpdateController に対象を割り当てるコントローラ</summary>
    private PhaseController _phaseController;

    /// <summary>フェーズデータを実行時形式で保持するランタイムデータ</summary>
    private PhaseRuntimeData _phaseRuntimeData;

    // --------------------------------------------------
    // 内部参照
    // --------------------------------------------------
    /// <summary>戦車同士の衝突判定を一元管理するサービス</summary>
    private TankVersusTankCollisionService
        _tankVersusTankCollisionService = new TankVersusTankCollisionService();

    // --------------------------------------------------
    // 外部参照
    // --------------------------------------------------
    /// <summary>弾丸オブジェクトとロジックの管理を行うプールクラス</summary>
    private BulletPool _bulletPool;

    /// <summary>カメラ管理クラス</summary>
    private CameraManager _cameraManager;

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
    // 辞書
    // ======================================================

    /// <summary>戦車IDと衝突判定エントリーの対応表</summary>
    private Dictionary<int, TankCollisionEntry> _tankEntries;

    // ======================================================
    // Unityイベント
    // ======================================================

    private void Awake()
    {
        // 戦車エントリーを生成
        _tankEntries = new Dictionary<int, TankCollisionEntry>();

        // PhaseData を読み込み
        PhaseData[] phaseDataList = LoadPhaseData();

        // PhaseRuntimeData を生成
        CreatePhaseRuntimeData(phaseDataList);

        // PhaseController を生成
        CreatePhaseController();

        // 初期フェーズを設定
        InitializePhase();

        // シーン内 IUpdatable を初期化・キャッシュ
        InitializeUpdatables();

        // 戦車 OBB を衝突判定サービスへ登録
        RegisterTankOBBs();
    }

    private void Update()
    {
        // --------------------------------------------------
        // シーン判定
        // --------------------------------------------------
        if (_currentScene != _targetScene)
        {
            ChangeScene(_targetScene);
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

        // --------------------------------------------------
        // 戦車衝突処理
        // --------------------------------------------------
        _tankVersusTankCollisionService.UpdateCollisionChecks();
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
            _playerTankRootManager.OnModeChangeButtonPressed += HandleModeChangeButtonPressed;
            _playerTankRootManager.OnOptionButtonPressed += HandleOptionButtonPressed;
            _playerTankRootManager.OnFireBullet += HandleFireBullet;
        }

        if (_enemyTankRootManagers == null)
        {
            return;
        }
        for (int i = 0; i < _enemyTankRootManagers.Length; i++)
        {
            _enemyTankRootManagers[i].OnFireBullet += HandleFireBullet;
        }

        _tankVersusTankCollisionService.OnTankVersusTankHit
            += HandleTankVersusTankHit;
    }

    private void OnDisable()
    {
        // イベント購読解除
        if (_playerTankRootManager != null)
        {
            _playerTankRootManager.OnModeChangeButtonPressed -= HandleModeChangeButtonPressed;
            _playerTankRootManager.OnOptionButtonPressed -= HandleOptionButtonPressed;
            _playerTankRootManager.OnFireBullet -= HandleFireBullet;
        }

        if (_enemyTankRootManagers == null)
        {
            return;
        }
        for (int i = 0; i < _enemyTankRootManagers.Length; i++)
        {
            _enemyTankRootManagers[i].OnFireBullet -= HandleFireBullet;
        }

        _tankVersusTankCollisionService.OnTankVersusTankHit
            -= HandleTankVersusTankHit;
    }

    // ======================================================
    // プライベートメソッド
    // ======================================================

    /// <summary>
    /// Resources から PhaseData を読み込む
    /// </summary>
    private PhaseData[] LoadPhaseData()
    {
        PhaseData[] phaseDataList = Resources.LoadAll<PhaseData>("Phase");

        if (phaseDataList == null || phaseDataList.Length == 0)
        {
            Debug.LogWarning("[SceneManager] PhaseData が Resources/Phase に存在しません");
        }

        return phaseDataList;
    }

    /// <summary>
    /// PhaseRuntimeData を生成し、フェーズごとの IUpdatable を登録する
    /// </summary>
    private void CreatePhaseRuntimeData(PhaseData[] phaseDataList)
    {
        // PhaseRuntimeData の生成
        _phaseRuntimeData = new PhaseRuntimeData();

        // シーン上すべての IUpdatable を取得
        _updatables = UpdatableCollector.Collect(_components);

        // フェーズ単位で IUpdatable を登録
        foreach (PhaseData phaseData in phaseDataList)
        {
            Type[] types = phaseData.GetUpdatableTypes();
            IUpdatable[] updatables = UpdatableCollector.Collect(_components, types);
            _phaseRuntimeData.RegisterPhase(phaseData.Phase, updatables);
        }
    }

    /// <summary>
    /// PhaseController を生成する
    /// </summary>
    private void CreatePhaseController()
    {
        _phaseController = new PhaseController(
            _phaseRuntimeData,
            _updateController
        );
    }

    /// <summary>
    /// 初期フェーズを設定する
    /// </summary>
    private void InitializePhase()
    {
        _targetPhase = PhaseType.Play;
        ChangePhase(_targetPhase);
    }

    /// <summary>
    /// IUpdatable の OnEnter を実行し、必要な参照をキャッシュする
    /// </summary>
    private void InitializeUpdatables()
    {
        // Enemy 一時収集用リスト
        List<EnemyTankRootManager> enemyList =
            new List<EnemyTankRootManager>();

        // OnEnter 実行と参照キャッシュ
        foreach (IUpdatable updatable in _updatables)
        {
            updatable.OnEnter();

            CacheUpdatable(updatable, enemyList);
        }

        // Enemy 配列を確定
        _enemyTankRootManagers = enemyList.ToArray();
    }

    /// <summary>
    /// IUpdatable の型に応じて参照をキャッシュする
    /// </summary>
    private void CacheUpdatable(
        IUpdatable updatable,
        List<EnemyTankRootManager> enemyList
    )
    {
        if (updatable is BulletPool bulletPool)
        {
            _bulletPool = bulletPool;
            return;
        }

        if (updatable is CameraManager cameraManager)
        {
            _cameraManager = cameraManager;
            return;
        }

        if (updatable is InputManager inputManager)
        {
            _inputManager = inputManager;
            return;
        }

        if (updatable is PlayerTankRootManager playerTankRootManager)
        {
            _playerTankRootManager = playerTankRootManager;
            return;
        }

        if (updatable is EnemyTankRootManager enemyTankRootManager)
        {
            enemyList.Add(enemyTankRootManager);
        }
    }

    /// <summary>
    /// 戦車の衝突判定エントリを登録する
    /// </summary>
    /// <param name="tankId">戦車固有ID</param>
    /// <param name="entry">衝突判定エントリ</param>
    private void RegisterTankEntry(
        in int tankId,
        in TankCollisionEntry entry
    )
    {
        // 既存キーがある場合は上書きしない
        if (_tankEntries.ContainsKey(tankId))
        {
            return;
        }

        _tankEntries.Add(tankId, entry);
    }

    /// <summary>
    /// シーン内に存在するすべての戦車の OBB 情報を TankVersusTankCollisionService に登録する
    /// </summary>
    private void RegisterTankOBBs()
    {
        // 戦車IDカウンタ
        int tankId = 0;

        // プレイヤー戦車が存在しない場合は処理しない
        if (_playerTankRootManager == null)
        {
            return;
        }

        // プレイヤー戦車を登録
        RegisterTankEntry(
            tankId,
            _playerTankRootManager.CollisionEntry
        );
        tankId++;

        // エネミー戦車が存在しない場合は終了
        if (_enemyTankRootManagers == null)
        {
            return;
        }

        // エネミー戦車を登録
        for (int i = 0; i < _enemyTankRootManagers.Length; i++)
        {
            RegisterTankEntry(
                tankId,
                _enemyTankRootManagers[i].CollisionEntry
            );
            tankId++;
        }

        // 一括で衝突判定サービスへ反映
        _tankVersusTankCollisionService.SetTankEntries(_tankEntries);
    }

    /// <summary>
    /// 戦車IDから直前フレームの前進移動量を取得する
    /// </summary>
    private float GetDeltaForwardByTankId(in int tankId)
    {
        // 対応する戦車エントリが存在しない場合は 0
        if (!_tankEntries.TryGetValue(tankId, out TankCollisionEntry entry))
        {
            return 0f;
        }

        // 戦車コンポーネントが未設定の場合は 0
        if (entry.TankRootManager == null)
        {
            return 0f;
        }

        // 戦車側で管理している直前前進量を取得
        return entry.TankRootManager.DeltaForward;
    }

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
    /// 現在の入力モードに応じて、次の入力モードへ切り替える
    /// </summary>
    private void HandleModeChangeButtonPressed()
    {
        // プレイヤー戦車のキャタピラ入力モードをトグル切替
        _playerTankRootManager.ChangeInputMode();

        // 現在の入力モードを取得
        TrackInputMode currentMode = _playerTankRootManager.InputMode;

        // 入力モードに応じてカメラターゲット用インデックスを決定
        int cameraTargetIndex =
            currentMode == TrackInputMode.Single
            ? 1
            : 0;

        // カメラの追従ターゲットを切り替え
        _cameraManager.SetTargetByIndex(cameraTargetIndex);
    }

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

    /// <summary>
    /// 戦車同士が接触した際に呼び出されるハンドラ
    /// </summary>
    /// <param name="tankIdA">戦車AのID</param>
    /// <param name="tankIdB">戦車BのID</param>
    private void HandleTankVersusTankHit(
        int tankIdA,
        int tankIdB
    )
    {
        float deltaForwardA =
        GetDeltaForwardByTankId(tankIdA);

        float deltaForwardB =
            GetDeltaForwardByTankId(tankIdB);

        _tankVersusTankCollisionService.ResolveTankVersusTank(
            tankIdA,
            tankIdB,
            deltaForwardA,
            deltaForwardB
        );
    }
}