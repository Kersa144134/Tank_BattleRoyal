// ======================================================
// TankVisibilityController.cs
// 作成者   : 高橋一翔
// 作成日   : 2025-12-22
// 更新日   : 2026-03-10
// 概要     : 戦車の視界判定とターゲット決定を担当するクラス
// ======================================================

using System;
using System.Collections.Generic;
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

        /// <summary>戦車 Transform</summary>
        private readonly Transform _transform;

        /// <summary>砲塔 Transform</summary>
        private readonly Transform _turretTransform;

        /// <summary>遮蔽物 OBB 配列</summary>
        private BaseOBBData[] _shieldOBBs = new BaseOBBData[0];

        /// <summary>FieldOfViewCalculator に渡す参照配列</summary>
        private BaseCollisionContext[] _visibleTargetsBuffer
            = new BaseCollisionContext[MAX_TARGETS];

        /// <summary>ターゲット中の戦車</summary>
        private BaseTankRootManager _cachedTargetTank;

        /// <summary>最後に視界更新を行ったフレーム</summary>
        private int _lastUpdateFrame = -1;

        /// <summary>次回ターゲット更新フレーム</summary>
        private int _nextUpdateFrame;

        /// <summary>初回更新完了フラグ</summary>
        private bool _isFirstUpdateDone;

        /// <summary>ランダムフレームオフセット</summary>
        private readonly int _randomFrameOffset;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>視界判定対象の最大数</summary>
        private const int MAX_TARGETS = 64;

        /// <summary>ターゲット更新フレーム間隔</summary>
        private const int TARGET_UPDATE_INTERVAL_FRAME = 60;

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
        public TankVisibilityController(
            in FieldOfViewCalculator fieldOfViewCalculator,
            in Transform transform,
            in Transform turretTransform)
        {
            // 視界判定計算クラスを保持
            _fieldOfViewCalculator = fieldOfViewCalculator;

            // 戦車本体 Transform を保持
            _transform = transform;

            // 砲塔 Transform を保持
            _turretTransform = turretTransform;

            // 視界更新タイミング分散のためのランダムオフセットを生成
            _randomFrameOffset = UnityEngine.Random.Range(0, TARGET_UPDATE_INTERVAL_FRAME);

            // 初回フレームで必ず視界更新を行うため現在フレームを設定
            _nextUpdateFrame = Time.frameCount;
        }

        // ======================================================
        // セッター
        // ======================================================

        /// <summary>
        /// 遮蔽物の OBB 配列を受け取る
        /// </summary>
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
        public bool TryGetClosestTarget(
            in float fovAngle,
            in float viewDistance,
            in BaseCollisionContext[] targetContexts,
            ref Transform resultTarget,
            in bool switchIcon = false)
        {
            // 現在フレーム取得
            int currentFrame = Time.frameCount;

            // 同一フレーム内の2回目以降の呼び出しは許可
            if (_lastUpdateFrame != currentFrame)
            {
                // 次回更新フレームに達していない場合はスキップ
                if (currentFrame < _nextUpdateFrame)
                {
                    return false;
                }

                // 初回更新
                if (!_isFirstUpdateDone)
                {
                    _nextUpdateFrame =
                        currentFrame +
                        TARGET_UPDATE_INTERVAL_FRAME +
                        _randomFrameOffset;

                    _isFirstUpdateDone = true;
                }
                else
                {
                    _nextUpdateFrame =
                        currentFrame +
                        TARGET_UPDATE_INTERVAL_FRAME;
                }

                // このフレームで更新済み記録
                _lastUpdateFrame = currentFrame;
            }

            // ターゲット配列が存在しない場合は処理終了
            if (targetContexts == null || targetContexts.Length == 0)
            {
                UpdateTankIcon(null);
                return false;
            }

            // 視界計算クラスから視界内ターゲットを取得
            int visibleCount =
                _fieldOfViewCalculator.GetVisibleTargets(
                    _turretTransform,
                    targetContexts,
                    _shieldOBBs,
                    fovAngle,
                    viewDistance,
                    ref _visibleTargetsBuffer);

            // 視界内ターゲットが存在しない場合は処理終了
            if (visibleCount == 0)
            {
                UpdateTankIcon(null);
                return false;
            }

            // 視界内ターゲットから最短距離ターゲットを選択
            BaseCollisionContext closestTarget =
                SelectClosestTarget(_visibleTargetsBuffer, visibleCount, switchIcon);

            if (closestTarget != null)
            {
                // 呼び出し元へ結果ターゲットを返す
                resultTarget = closestTarget.Transform;
            }

            return true;
        }

        /// <summary>
        /// ターゲットのアイコン更新
        /// </summary>
        public void UpdateTankIcon(in BaseTankRootManager newTarget)
        {
            // 同一ターゲットの場合は処理なし
            if (_cachedTargetTank == newTarget)
            {
                return;
            }

            if (newTarget != null)
            {
                // 旧ターゲットが存在する場合はアイコンを非表示
                if (_cachedTargetTank != null)
                {
                    _cachedTargetTank.ChangeTargetIcon(false);
                }

                // 新ターゲットのアイコンを表示
                newTarget.ChangeTargetIcon(true);
            }
            else
            {
                // ターゲット解除時は旧ターゲットのアイコンを非表示
                if (_cachedTargetTank != null)
                {
                    _cachedTargetTank.ChangeTargetIcon(false);
                }
            }

            // ターゲットキャッシュを更新
            _cachedTargetTank = newTarget;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 最短ターゲット選択
        /// </summary>
        private BaseCollisionContext SelectClosestTarget(
            in BaseCollisionContext[] visibleTargets,
            in int count,
            in bool switchIcon = false)
        {
            // 有効ターゲットが存在しない場合、ターゲット解除
            if (visibleTargets == null || count == 0)
            {
                UpdateTankIcon(null);
                return null;
            }

            BaseCollisionContext closest = null;

            // 視界内ターゲット配列を先頭から探索
            for (int i = 0; i < count; i++)
            {
                BaseCollisionContext candidate = visibleTargets[i];

                // 自身または砲塔または null の場合はスキップ
                if (candidate == null ||
                    candidate.Transform == _transform ||
                    candidate.Transform == _turretTransform)
                {
                    continue;
                }

                // 最初に見つかった有効ターゲットを最短ターゲットとする
                closest = candidate;

                break;
            }

            // アイコン更新フラグが true かつ、最短ターゲットが存在する場合
            if (switchIcon)
            {
                // Transform から BaseTankRootManager をキャッシュ取得
                BaseTankRootManager manager = GetTankManager(closest);

                // ターゲットキャッシュ更新
                UpdateTankIcon(manager);

                // ターゲット取得イベントを通知
                OnTargetAcquired?.Invoke(manager);
            }

            // 最終的なターゲット Transform を返す
            return closest;
        }

        /// <summary>
        /// Transform から BaseTankRootManager を取得
        /// </summary>
        private BaseTankRootManager GetTankManager(BaseCollisionContext target)
        {
            if (target == null)
            {
                return null;
            }

            if (target is not TankCollisionContext tank)
            {
                return null;
            }

            return tank.TankRootManager;
        }
    }
}