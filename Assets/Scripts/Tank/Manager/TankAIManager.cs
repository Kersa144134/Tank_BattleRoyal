// ======================================================
// TankAIManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-15
// 更新日時 : 2026-02-15
// 概要     : 戦車 AI 制御クラス
//            索敵およびターゲット決定処理を担当する
// ======================================================

using UnityEngine;
using CollisionSystem.Data;
using TankSystem.Controller;

/// <summary>
/// 戦車 AI 制御クラス
/// 索敵処理およびターゲット選定処理を管理する純C#クラス
/// </summary>
public sealed class TankAIManager
{
    // ======================================================
    // フィールド
    // ======================================================

    /// <summary>視界制御クラス</summary>
    private readonly TankVisibilityController _visibilityController;

    /// <summary>
    /// プレイヤー戦車コンテキスト
    /// GetClosestTarget が複数要素の Transform 配列を想定しているため、
    /// 単要素の配列として扱う
    /// </summary>
    private TankCollisionContext[] _player = new TankCollisionContext[1];

    /// <summary>アイテムコンテキスト配列</summary>
    private BaseCollisionContext[] _items;

    /// <summary>現在のターゲット Transform</summary>
    private Transform _currentTarget;

    // ======================================================
    // 定数
    // ======================================================

    // --------------------------------------------------
    // 視界関連
    // --------------------------------------------------
    /// <summary>対プレイヤー視界判定に使用する視野角（度）</summary>
    private const float PLAYER_FOV_ANGLE = 120f;

    /// <summary>対プレイヤー視界判定に使用する最大索敵距離</summary>
    private const float PLAYER_VIEW_DISTANCE = 80f;

    /// <summary>対アイテム視界判定に使用する視野角（度）</summary>
    private const float ITEM_FOV_ANGLE = 360f;

    /// <summary>対アイテム視界判定に使用する最大索敵距離</summary>
    private const float ITEM_VIEW_DISTANCE = 150f;

    // --------------------------------------------------
    // 移動関連
    // --------------------------------------------------
    /// <summary>入力を開始する閾値</summary>
    private const float INPUT_THRESHOLD = 0.1f;

    /// <summary>スティックの最大入力値</summary>
    private const float MAX_INPUT = 0.5f;
    
    /// <summary>角度差を正規化するための最大角度（度）</summary>
    private const float ROTATION_ANGLE_FULL = 180f;

    /// <summary>回転入力の角度減衰に使用する指数</summary>
    private const float ROTATION_INPUT_EXPONENT = 0.25f;

    /// <summary>プレイヤーに近づきすぎた場合、前進を停止する距離</summary>
    private const float PLAYER_STOP_DISTANCE = 30f;

    // ======================================================
    // コンストラクタ
    // ======================================================

    /// <summary>
    /// TankAIManager を初期化する
    /// </summary>
    /// <param name="visibilityController">視界制御クラス</param>
    public TankAIManager(in TankVisibilityController visibilityController)
    {
        _visibilityController = visibilityController;
    }

    // ======================================================
    // セッター
    // ======================================================

    /// <summary>
    /// 戦車（プレイヤー）とアイテムのコンテキスト配列を受け取り、
    /// 内部ターゲット配列を構築する
    /// </summary>
    /// <param name="tankContexts">戦車 Transform 配列</param>
    /// <param name="itemContexts">アイテム Transform 配列</param>
    public void SetTargetData(
        in TankCollisionContext[] tankContexts,
        in ItemCollisionContext[] itemContexts
    )
    {
        if (tankContexts == null || itemContexts.Length == 0)
        {
            return;
        }

        _player[0] = tankContexts[0];
        _items = itemContexts;
    }

    // ======================================================
    // パブリックメソッド
    // ======================================================

    /// <summary>
    /// 現在のターゲットに向かってキャタピラ入力を計算する
    /// </summary>
    /// <param name="currentTransform">自身の Transform</param>
    /// <param name="leftStick">出力: 左スティック入力</param>
    public void GetMovementInputTowardsTarget(
        in Transform currentTransform,
        out Vector2 leftStick
    )
    {
        leftStick = Vector2.zero;

        // ターゲットと方向・距離を取得
        if (!CalculateTargetDirection(currentTransform, out Transform target, out Vector3 localTargetDir))
        {
            return;
        }

        // 回転入力
        leftStick.x = CalculateRotationInput(localTargetDir);

        // 前進入力
        leftStick.y = CalculateForwardInput(target, localTargetDir);
    }

    // ======================================================
    // プライベートメソッド
    // ======================================================

    /// <summary>
    /// ターゲットと自身の相対ベクトルを計算する
    /// </summary>
    /// <param name="currentTransform">自身の Transform</param>
    /// <param name="target">出力: ターゲット Transform</param>
    /// <param name="localTargetDir">出力: ローカル座標系のターゲット方向</param>
    /// <returns>ターゲットが存在する場合 true</returns>
    private bool CalculateTargetDirection(
        in Transform currentTransform,
        out Transform target,
        out Vector3 localTargetDir)
    {
        // --------------------------------------------------
        // ターゲット決定
        // --------------------------------------------------
        // プレイヤーを優先して探索
        bool found =
            GetTarget(
                PLAYER_FOV_ANGLE,
                PLAYER_VIEW_DISTANCE,
                _player,
                ref _currentTarget);

        // プレイヤーが見つからない場合のみアイテム探索
        if (!found)
        {
            GetTarget(
                ITEM_FOV_ANGLE,
                ITEM_VIEW_DISTANCE,
                _items,
                ref _currentTarget);
        }

        if (_currentTarget == null)
        {
            target = null;
            localTargetDir = Vector3.zero;
            return false;
        }

        // 出力ターゲット設定
        target = _currentTarget;

        // --------------------------------------------------
        // ターゲット方向計算
        // --------------------------------------------------
        // ターゲットまでのベクトル計算
        Vector3 toTarget =
            target.position - currentTransform.position;

        // ローカル座標へ変換
        localTargetDir =
            currentTransform.InverseTransformDirection(toTarget);

        return true;
    }

    /// <summary>
    /// 指定配列を対象としてターゲットを取得する
    /// </summary>
    /// <param name="fovAngle">視野角（度）</param>
    /// <param name="viewDistance">最大索敵距離</param>
    /// <param name="targets">ターゲット対象の配列</param>
    /// <param name="resultTarget">結果ターゲット</param>
    /// <returns>更新成功した場合 true</returns>
    private bool GetTarget(
        in float fovAngle,
        in float viewDistance,
        in BaseCollisionContext[] targets,
        ref Transform resultTarget)
    {
        if (_visibilityController == null ||
            targets == null ||
            targets.Length == 0)
        {
            return false;
        }

        // プレイヤーを優先して最も近いターゲットを取得
        return _visibilityController.TryGetClosestTarget(
            fovAngle,
            viewDistance,
            targets,
            ref resultTarget
        );
    }

    /// <summary>
    /// 左右回転のキャタピラ入力を計算する
    /// 水平方向の角度差に基づき、角度差が小さいほど入力値を弱める
    /// </summary>
    /// <param name="localTargetDir">ターゲット方向のローカル座標ベクトル</param>
    /// <returns>左右回転入力値（-MAX_INPUT ～ +MAX_INPUT）</returns>
    private float CalculateRotationInput(in Vector3 localTargetDir)
    {
        if (Mathf.Abs(localTargetDir.x) < INPUT_THRESHOLD)
        {
            return 0f;
        }

        // 水平面の角度差を計算
        float angle = Vector3.SignedAngle(Vector3.forward, localTargetDir, Vector3.up);

        // 角度差に応じて入力を減衰
        float input = Mathf.Sign(angle) * Mathf.Pow(Mathf.Abs(angle) / ROTATION_ANGLE_FULL, ROTATION_INPUT_EXPONENT);

        // 最大回転入力を制限
        return Mathf.Clamp(input, -MAX_INPUT, MAX_INPUT);
    }

    /// <summary>
    /// 前進のキャタピラ入力を計算する
    /// ターゲットまでの距離が一定以下なら停止、それ以外は全力前進
    /// </summary>
    /// <param name="target">ターゲット Transform</param>
    /// <param name="localTargetDir">ターゲット方向のローカル座標ベクトル</param>
    /// <returns>前進入力値（0 または MAX_INPUT）</returns>
    private float CalculateForwardInput(Transform target, in Vector3 localTargetDir)
    {
        if (localTargetDir.z <= INPUT_THRESHOLD)
        {
            return 0f;
        }

        // ターゲットがプレイヤーなら停止距離以下で停止
        if (target == _player[0].Transform && localTargetDir.magnitude <= PLAYER_STOP_DISTANCE)
        {
            return 0f;
        }

        return MAX_INPUT;
    }
}