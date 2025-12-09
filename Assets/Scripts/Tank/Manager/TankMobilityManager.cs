// ======================================================
// TankMobilityManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-05
// 更新日時 : 2025-12-05
// 概要     : 戦車の前進・旋回処理を担当する機動力管理クラス
//            TrackController から得た前進量・旋回量に基づき
//            Transform を更新する責務を持つ
// ======================================================

using UnityEngine;
using UnityEngine.AI;
using TankSystem.Controller;

namespace TankSystem.Manager
{
    /// <summary>
    /// 戦車の移動・旋回処理を専任で受け持つマネージャ
    /// </summary>
    public class TankMobilityManager
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>左右キャタピラ入力から前進量・旋回量を算出するコントローラ</summary>
        private readonly TankTrackController _trackController;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>戦車本体の Transform</summary>
        private Transform _tankTransform;

        /// <summary>戦車本体の Collider</summary>
        private Collider _tankCollider;

        private Vector3 _previousPosition;

        private Vector3[] _localColliderPoints;

        private Vector3 _lastInvalidMoveDir;

        private bool _isOutOfNavMesh = false; // 範囲外フラグ

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>機動力倍率（前進・旋回の両方に適用）</summary>
        private const float MOBILITY = 5.5f;

        /// <summary>NavMesh 判定時の基準 Y 座標</summary>
        private const float NAVMESH_BASE_Y = 0.08333f;

        /// <summary>NavMesh上に有効位置をサンプルする距離</summary>
        private const float NAVMESH_SAMPLE_DISTANCE = 0.01f;
        // 許容角度
        private const float ALLOWED_ANGLE = 120f;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 機動力管理に必要な外部参照を受け取り初期化する
        /// </summary>
        /// <param name="trackController">左右キャタピラ入力から前進量・旋回量を算出するコントローラ</param>
        /// <param name="transform">戦車本体の Transform</param>
        public TankMobilityManager(TankTrackController trackController, Transform transform)
        {
            _trackController = trackController;
            _tankTransform = transform;

            // Collider を取得
            _tankCollider = transform.GetComponent<Collider>();
            if (_tankCollider == null)
            {
                Debug.LogError("[TankMobilityManager] Collider がアタッチされていません。");
            }

            // 初期化時に一度だけキャッシュ
            CacheColliderPoints();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 入力値を元に前進・旋回処理を実行する
        /// </summary>
        /// <param name="left">左キャタピラ入力</param>
        /// <param name="right">右キャタピラ入力</param>
        public void ApplyMobility(in float left, in float right)
        {
            // 現在位置を退避
            _previousPosition = _tankTransform.position;

            // キャタピラ入力から前進量と旋回量を計算
            _trackController.UpdateTrack(left, right, out float forward, out float turn);

            // 移動処理（前進・後退）
            _tankTransform.Translate(
                Vector3.forward * forward * MOBILITY * Time.deltaTime,
                Space.Self
            );

            // 旋回処理（左右回転）
            _tankTransform.Rotate(
                0f,
                turn * MOBILITY * Time.deltaTime,
                0f,
                Space.Self
            );

            // ---------------------
            // NavMesh範囲外チェック
            // ---------------------
            bool isOnNavMesh = IsPositionOnNavMesh(_tankTransform);

            if (!isOnNavMesh)
            {
                if (!_isOutOfNavMesh)
                {
                    // 範囲外になった瞬間のみキャッシュ
                    Vector3 moveVector = _tankTransform.position - _previousPosition;

                    // XZ平面で正規化してベクトルを保存
                    _lastInvalidMoveDir = new Vector3(moveVector.x, 0, moveVector.z).normalized;

                    // もしベクトルがゼロになってしまった場合は、前向きに補正
                    if (_lastInvalidMoveDir.sqrMagnitude < 0.001f)
                    {
                        _lastInvalidMoveDir = _tankTransform.forward;
                        _lastInvalidMoveDir.y = 0f;
                        _lastInvalidMoveDir.Normalize();
                    }

                    _isOutOfNavMesh = true;
                }

                // ---------------------
                // 角度判定用ベクトル（入力方向代わり）
                // ---------------------
                Vector3 attemptedMoveDir = _tankTransform.position - _previousPosition;

                // XZ平面のみで正規化
                attemptedMoveDir = new Vector3(attemptedMoveDir.x, 0, attemptedMoveDir.z);
                attemptedMoveDir.Normalize();

                // 角度判定
                if (ShouldRevertPosition(_lastInvalidMoveDir, attemptedMoveDir, ALLOWED_ANGLE))
                {
                    _tankTransform.position = _previousPosition;
                }
            }
            else
            {
                // 範囲内に戻ったらフラグをリセット
                _isOutOfNavMesh = false;
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// BoxCollider の四隅をローカル座標でキャッシュ
        /// </summary>
        private void CacheColliderPoints()
        {
            if (!(_tankCollider is BoxCollider box))
                return;

            // ワールド座標の中心
            Vector3 worldCenter = _tankTransform.position + box.center;

            // Collider の半幅・半長を取得
            float halfX = box.size.x * 0.5f;
            float halfZ = box.size.z * 0.5f;

            // Y は元の Transform の Y を使用
            float y = _tankTransform.position.y;

            _localColliderPoints = new Vector3[4];
            _localColliderPoints[0] = new Vector3(worldCenter.x + halfX, y, worldCenter.z + halfZ);
            _localColliderPoints[1] = new Vector3(worldCenter.x + halfX, y, worldCenter.z - halfZ);
            _localColliderPoints[2] = new Vector3(worldCenter.x - halfX, y, worldCenter.z + halfZ);
            _localColliderPoints[3] = new Vector3(worldCenter.x - halfX, y, worldCenter.z - halfZ);
        }

        /// <summary>
        /// 指定TransformがNavMesh上に存在するか確認（XZ平面のみ）
        /// </summary>
        private bool IsPositionOnNavMesh(Transform transform)
        {
            if (_localColliderPoints == null || _localColliderPoints.Length != 4)
                return true;

            float sampleDistance = NAVMESH_SAMPLE_DISTANCE;

            foreach (Vector3 localPoint in _localColliderPoints)
            {
                // ローカル座標をワールド座標に変換
                Vector3 worldPoint = transform.TransformPoint(localPoint);

                // Yは無視してNavMesh基準でサンプル
                Vector3 samplePoint = new Vector3(worldPoint.x, NAVMESH_BASE_Y, worldPoint.z);

                if (!NavMesh.SamplePosition(samplePoint, out NavMeshHit hit, sampleDistance, NavMesh.AllAreas))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 範囲外になった方向と移動ベクトルの角度を判定し、戻す処理を行うか決定
        /// </summary>
        /// <param name="invalidDir">NavMesh外になった方向</param>
        /// <param name="inputDir">移動方向</param>
        /// <param name="allowAngle">許容角度（度数法、0～180）</param>
        /// <returns>trueなら戻す処理を行う</returns>
        private bool ShouldRevertPosition(Vector3 invalidDir, Vector3 inputDir, float allowAngle)
        {
            // 移動ベクトルがほぼゼロなら元に戻す
            if (inputDir.sqrMagnitude < 0.001f)
            {
                return true;
            }

            // XZ平面のみで計算
            Vector3 invalidDirXZ = new Vector3(invalidDir.x, 0, invalidDir.z).normalized;
            Vector3 inputDirXZ = new Vector3(inputDir.x, 0, inputDir.z).normalized;

            // 内積から角度を取得
            float angle = Vector3.Angle(invalidDirXZ, inputDirXZ);

            // ログ出力
            Debug.Log($"[NavMeshCheck] 範囲外方向: {invalidDirXZ}, 移動方向: {inputDirXZ}, 角度差: {angle}, 許容角度: {allowAngle}");

            // 角度が許容範囲より小さい場合は戻す処理を実行
            bool shouldRevert = angle < allowAngle;
            Debug.Log($"[NavMeshCheck] 戻す処理判定: {shouldRevert}");

            return shouldRevert;
        }
    }
}