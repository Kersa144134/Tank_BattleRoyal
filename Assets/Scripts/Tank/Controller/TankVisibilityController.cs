// ======================================================
// TankVisibilityController.cs
// 作成者   : 高橋一翔
// 作成日   : 2025-12-22
// 更新日   : 2026-01-30
// 概要     : 戦車の視界判定とターゲット決定を担当するクラス
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using CollisionSystem.Interface;
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

        /// <summary>視界計算ユースケース</summary>
        private readonly FieldOfViewCalculator _fieldOfViewCalculator;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>自身Transform</summary>
        private readonly Transform _transform;

        /// <summary>砲塔Transform</summary>
        private readonly Transform _turretTransform;

        /// <summary>遮蔽物OBB</summary>
        private IOBBData[] _shieldOBBs = new IOBBData[0];

        /// <summary>現在ターゲット</summary>
        private BaseTankRootManager _cachedTarget;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>ターゲットを見つけたときに通知するイベント</summary>
        public event Action<BaseTankRootManager> OnTargetAcquired;
        
        // ======================================================
        // コンストラクタ
        // ======================================================

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
        public void SetObstacleData(in IOBBData[] shieldOBBs)
        {
            _shieldOBBs = shieldOBBs;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 現在の最短ターゲット取得
        /// </summary>
        /// <param name="changeIcon">アイコンを更新するかどうか</param>
        /// <param name="fovAngle">視野角（度）</param>
        /// <param name="viewDistance">最大索敵距離</param>
        /// <param name="targetTransforms">対象 Transform 配列</param>
        /// <param name="filterTargets">対象を限定する Transform 配列、null の場合は全ターゲットを対象とする</param>
        public Transform GetClosestTarget(
            in bool changeIcon,
            in float fovAngle,
            in float viewDistance,
            in Transform[] targetTransforms,
            in Transform[] filterTargets = null)
        {
            // ----------------------------
            // 優先ターゲットを視界内から取得
            // ----------------------------
            Transform[] targets = (filterTargets != null && filterTargets.Length > 0)
                ? filterTargets
                : targetTransforms;

            // 対象が存在しない場合は処理なし
            if (targets == null || targets.Length == 0)
            {
                return null;
            }

            // 視界内ターゲット取得
            List<Transform> visibleTargets = _fieldOfViewCalculator.GetVisibleTargets(
                _turretTransform,
                targets,
                _shieldOBBs,
                fovAngle,
                viewDistance
            );

            // ----------------------------
            // 優先ターゲットが視界にいなければ全体ターゲットで再取得
            // ----------------------------
            if ((filterTargets != null && filterTargets.Length > 0) &&
                (visibleTargets == null || visibleTargets.Count == 0))
            {
                // 全ターゲットで再取得
                visibleTargets = _fieldOfViewCalculator.GetVisibleTargets(
                    _turretTransform,
                    targetTransforms,
                    _shieldOBBs,
                    fovAngle,
                    viewDistance
                );
            }

            // ----------------------------
            // 最短ターゲットを取得
            // ----------------------------
            Transform result = SelectClosestTarget(changeIcon, visibleTargets);

            return result;
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

            // イベント発火
            OnTargetAcquired?.Invoke(newTarget);

            // キャッシュターゲット更新
            _cachedTarget = newTarget;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 最短ターゲット選択
        /// </summary>
        /// <param name="changeIcon">アイコンを更新するかどうか</param>
        /// <param name="visibleTargets">対象 Transform リスト</param>
        private Transform SelectClosestTarget(in bool changeIcon, in List<Transform> visibleTargets)
        {
            if (visibleTargets == null || visibleTargets.Count == 0)
            {
                UpdateCachedTarget(null);
                return null;
            }

            // 最初に見つかった有効なターゲットを取得
            Transform closest = null;
            for (int i = 0; i < visibleTargets.Count; i++)
            {
                Transform candidate = visibleTargets[i];

                if (candidate == null || candidate == _transform || candidate == _turretTransform)
                {
                    continue;
                }

                closest = candidate;
                break;
            }

            // アイコン更新
            if (changeIcon && closest != null)
            {
                BaseTankRootManager manager = closest.GetComponent<BaseTankRootManager>();
                if (manager != null)
                {
                    UpdateCachedTarget(manager);
                }
                else
                {

                    UpdateCachedTarget(null);
                }
            }

            return closest;
        }
    }
}