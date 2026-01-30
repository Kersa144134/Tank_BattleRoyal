// ======================================================
// TankVisibilityController.cs
// 作成者   : 高橋一翔
// 作成日   : 2025-12-22
// 更新日   : 2026-01-30
// 概要     : 戦車の視界判定とターゲット決定を担当するクラス
// ======================================================

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
        // 参照
        // ======================================================

        /// <summary>視界計算ユースケース</summary>
        private readonly FieldOfViewCalculator _fieldOfViewCalculator;

        /// <summary>自身Transform</summary>
        private readonly Transform _transform;

        /// <summary>砲塔Transform</summary>
        private readonly Transform _turretTransform;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>ターゲット候補</summary>
        private Transform[] _targetTransforms = new Transform[0];

        /// <summary>遮蔽物OBB</summary>
        private IOBBData[] _shieldOBBs = new IOBBData[0];

        /// <summary>現在ターゲット</summary>
        private BaseTankRootManager _cachedTarget;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>視界判定に使用する視野角（度）</summary>
        private const float FOV_ANGLE = 30f;

        /// <summary>視界判定に使用する最大索敵距離</summary>
        private const float VIEW_DISTANCE = 100f;
        
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
        /// ターゲットと遮蔽物の Transform 配列と OBB 配列を受け取る
        /// </summary>
        /// <param name="targetTransforms">ターゲットの Transform 配列</param>
        /// <param name="shieldOBBs">遮蔽物の OBB 配列</param>
        public void SetContextData(
            in Transform[] targetTransforms,
            in IOBBData[] shieldOBBs
        )
        {
            _targetTransforms = targetTransforms;
            _shieldOBBs = shieldOBBs;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 現在の最適ターゲット取得
        /// </summary>
        public Transform GetClosestTarget()
        {
            // 視界内ターゲット取得
            List<Transform> visibleTargets = _fieldOfViewCalculator.GetVisibleTargets(
                _turretTransform,
                _targetTransforms,
                _shieldOBBs,
                FOV_ANGLE,
                VIEW_DISTANCE
            );

            // 最短ターゲット取得
            Transform result = SelectClosestTarget(visibleTargets);

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

            // キャッシュターゲット更新
            _cachedTarget = newTarget;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 最短ターゲット選択
        /// </summary>
        private Transform SelectClosestTarget(in List<Transform> visibleTargets)
        {
            // ターゲットが存在しない場合はターゲット解除
            if (visibleTargets == null || visibleTargets.Count == 0)
            {
                UpdateCachedTarget(null);
                return null;
            }

            // 最短距離のターゲット
            Transform closest = null;
            BaseTankRootManager manager = null;

            // visibleTargets は距離順
            for (int i = 0; i < visibleTargets.Count; i++)
            {
                Transform candidate = visibleTargets[i];

                if (candidate == null)
                {
                    continue;
                }

                // 自身の本体・砲塔はターゲット対象外
                if (candidate == _transform || candidate == _turretTransform)
                {
                    continue;
                }

                BaseTankRootManager tankManager =
                    candidate.GetComponent<BaseTankRootManager>();

                if (tankManager == null)
                {
                    continue;
                }

                // 破壊後の戦車はターゲット不可
                if (tankManager.IsBroken)
                {
                    continue;
                }

                closest = candidate;
                manager = tankManager;
                break;
            }

            // ターゲットキャッシュ更新
            UpdateCachedTarget(manager);
            return closest;
        }
    }
}