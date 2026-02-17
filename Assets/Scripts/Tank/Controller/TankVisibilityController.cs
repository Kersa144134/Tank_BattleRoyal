// ======================================================
// TankVisibilityController.cs
// 作成者   : 高橋一翔
// 作成日   : 2025-12-22
// 更新日   : 2026-02-18
// 概要     : 戦車の視界判定とターゲット決定を担当するクラス
// ======================================================

using System;
using UnityEngine;
using CollisionSystem.Data;
using TankSystem.Manager;
using VisionSystem.Calculator;

namespace TankSystem.Controller
{
    /// <summary>
    /// 視界ターゲット管理クラス
    /// </summary>
    public sealed class TankVisibilityController
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>視界判定計算クラス</summary>
        private readonly FieldOfViewCalculator _fieldOfViewCalculator;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>フレーム間隔カウンター</summary>
        private int _frameCounter;

        /// <summary>現在取得した最短ターゲット</summary>
        private Transform _resultTarget;

        /// <summary>戦車 Transform</summary>
        private readonly Transform _transform;

        /// <summary>砲塔 Transform</summary>
        private readonly Transform _turretTransform;

        /// <summary>遮蔽物 OBB 配列</summary>
        private BaseOBBData[] _shieldOBBs = new BaseOBBData[0];

        /// <summary>キャッシュされた現在ターゲット</summary>
        private BaseTankRootManager _cachedTarget;

        /// <summary>FieldOfViewCalculator に渡す参照配列</summary>
        private Transform[] _visibleTargetsBuffer = new Transform[MAX_TARGETS];

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>視界判定対象の最大数</summary>
        private const int MAX_TARGETS = 64;

        /// <summary>ターゲット更新フレーム間隔</summary>
        private const int TARGET_UPDATE_INTERVAL_FRAME = 30;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>ターゲット取得時に通知されるイベント</summary>
        public event Action<BaseTankRootManager> OnTargetAcquired;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// TankVisibilityController クラスのコンストラクタ
        /// </summary>
        /// <param name="fieldOfViewCalculator">視界計算ユースケース</param>
        /// <param name="transform">戦車本体 Transform</param>
        /// <param name="turretTransform">砲塔 Transform</param>
        public TankVisibilityController(
            in FieldOfViewCalculator fieldOfViewCalculator,
            in Transform transform,
            in Transform turretTransform)
        {
            _fieldOfViewCalculator = fieldOfViewCalculator;
            _transform = transform;
            _turretTransform = turretTransform;
        }

        // ======================================================
        // セッター
        // ======================================================
        /// <summary>
        /// 遮蔽物の OBB 配列を受け取る
        /// </summary>
        /// <param name="shieldOBBs">遮蔽物の OBB 配列</param>
        public void SetObstacleData(in BaseOBBData[] shieldOBBs)
        {
            _shieldOBBs = shieldOBBs;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================
        /// <summary>
        /// 現在の最短ターゲット取得
        /// </summary>
        /// <param name="fovAngle">視野角（度）</param>
        /// <param name="viewDistance">最大索敵距離</param>
        /// <param name="targetContexts">対象コンテキスト配列</param>
        /// <param name="filterTargets">優先ターゲット配列、null の場合は全ターゲット対象</param>
        /// <returns>最短ターゲット Transform</returns>
        public Transform GetClosestTarget(
            in float fovAngle,
            in float viewDistance,
            in BaseCollisionContext[] targetContexts,
            in BaseCollisionContext[] filterTargets = null)
        {
            // --------------------------------------------------
            // フレーム間隔制御
            // --------------------------------------------------
            // カウンターが 0 以外の場合は処理をスキップ
            if (_frameCounter != 0)
            {
                _frameCounter++;

                // 指定フレーム間隔に達したらカウンターを 0 に戻す
                if (_frameCounter >= TARGET_UPDATE_INTERVAL_FRAME)
                    _frameCounter = 0;

                // 前回計算したターゲットを返す
                return _resultTarget;
            }

            _frameCounter++;

            // --------------------------------------------------
            // 対象配列の決定
            // --------------------------------------------------
            // filterTargets が指定されていれば優先ターゲット配列、なければ全ターゲット配列を使用
            BaseCollisionContext[] targets = (filterTargets != null && filterTargets.Length > 0)
                ? filterTargets
                : targetContexts;

            if (targets == null || targets.Length == 0)
            {
                return null;
            }

            // --------------------------------------------------
            // 視界内ターゲット取得
            // --------------------------------------------------
            // 優先ターゲットで取得
            int visibleCount = _fieldOfViewCalculator.GetVisibleTargets(
                _turretTransform,
                targets,
                _shieldOBBs,
                fovAngle,
                viewDistance,
                ref _visibleTargetsBuffer);

            // 優先ターゲットが視界に存在しない場合、全体ターゲットで再取得
            if ((filterTargets != null && filterTargets.Length > 0) && visibleCount == 0)
            {
                visibleCount = _fieldOfViewCalculator.GetVisibleTargets(
                    _turretTransform,
                    targetContexts,
                    _shieldOBBs,
                    fovAngle,
                    viewDistance,
                    ref _visibleTargetsBuffer);
            }

            // --------------------------------------------------
            // 最短ターゲット選択
            // --------------------------------------------------
            // visibleCount の範囲内で最短距離のターゲットを選択
            _resultTarget = SelectClosestTarget(_visibleTargetsBuffer, visibleCount);

            return _resultTarget;
        }

        /// <summary>
        /// ターゲットのキャッシュ更新
        /// </summary>
        public void UpdateCachedTarget(in BaseTankRootManager newTarget)
        {
            if (_cachedTarget == newTarget)
            {
                return;
            }

            // 旧ターゲットアイコン非表示
            if (_cachedTarget != null)
            {
                _cachedTarget.ChangeTargetIcon(false);
            }

            // 新ターゲットアイコン表示
            if (newTarget != null)
            {
                newTarget.ChangeTargetIcon(true);
            }

            // キャッシュターゲット更新
            _cachedTarget = newTarget;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================
        /// <summary>
        /// 最短ターゲット選択
        /// </summary>
        /// <param name="visibleTargets">視界内対象配列（固定バッファ）</param>
        /// <param name="count">配列内有効要素数</param>
        /// <returns>最短ターゲット Transform（存在しなければ null）</returns>
        private Transform SelectClosestTarget(Transform[] visibleTargets, int count)
        {
            if (visibleTargets == null || count == 0)
            {
                UpdateCachedTarget(null);
                return null;
            }

            // 最初に見つかった有効なターゲットを取得
            Transform closest = null;
            for (int i = 0; i < count; i++)
            {
                Transform candidate = visibleTargets[i];

                // 自身または砲塔を除外
                if (candidate == null || candidate == _transform || candidate == _turretTransform)
                {
                    continue;
                }

                closest = candidate;
                break;
            }

            // アイコン更新
            if (closest != null)
            {
                BaseTankRootManager manager = closest.GetComponent<BaseTankRootManager>();

                UpdateCachedTarget(manager);

                // イベント発火
                OnTargetAcquired?.Invoke(manager);
            }

            return closest;
        }
    }
}