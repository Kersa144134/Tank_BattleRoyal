// ======================================================
// TankAttackManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2026-01-28
// 概要     : 戦車の攻撃処理を管理するクラス
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using CollisionSystem.Interface;
using InputSystem.Data;
using TankSystem.Data;
using VisionSystem.Calculator;
using WeaponSystem.Data;

namespace TankSystem.Manager
{
    /// <summary>
    /// 戦車の攻撃処理を管理するクラス
    /// </summary>
    public class TankAttackManager
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>視界判定のユースケースクラス</summary>
        private readonly FieldOfViewCalculator _fieldOfViewCalculator;

        // ======================================================
        // フィールド
        // ======================================================

        // --------------------------------------------------
        // 攻撃関連
        // --------------------------------------------------
        /// <summary>弾丸タイプの順序</summary>
        private readonly BulletType[] _bulletCycle = new BulletType[]
        {
            BulletType.Explosive,
            BulletType.Penetration,
            BulletType.Homing
        };
        
        // 現在の弾丸インデックス
        private int _currentBulletIndex = 0;

        /// <summary>攻撃クールタイムの残り時間（秒）</summary>
        private float _cooldownTime = 0f;

        /// <summary>現在の攻撃間隔倍率</summary>
        private float _reloadTimeMultiplier = BASE_RELOAD_TIME_MULTIPLIER;

        /// <summary>左攻撃ボタンが押されてからの経過時間（同時押し判定用、未押下時は -1）</summary>
        private float _leftInputTimer = -1f;

        /// <summary>右攻撃ボタンが押されてからの経過時間（同時押し判定用、未押下時は -1）</summary>
        private float _rightInputTimer = -1f;

        // --------------------------------------------------
        // 視界判定関連
        // --------------------------------------------------
        /// <summary>自身の戦車 Transform</summary>
        private Transform _transform;

        /// <summary>自身の砲塔 Transform</summary>
        private Transform _turretTransform;

        /// <summary>ターゲットの Transform 配列</summary>
        private Transform[] _targetTransforms = new Transform[0];

        /// <summary>遮蔽物の OBB 配列</summary>
        private IOBBData[] _shieldOBBs = new IOBBData[0];

        /// <summary>現在のターゲットとして保持する戦車マネージャー </summary>
        private BaseTankRootManager _targetTankManager;

        // ======================================================
        // 定数
        // ======================================================

        // --------------------------------------------------
        // 基準値
        // --------------------------------------------------
        /// <summary>基準となる攻撃間隔倍率</summary>
        private const float BASE_RELOAD_TIME_MULTIPLIER = 1.0f;

        // --------------------------------------------------
        // パラメーター
        // --------------------------------------------------
        /// <summary>装填 1 あたりの攻撃間隔倍率減算値</summary>
        private const float RELOAD_TIME_MULTIPLIER = 0.025f;

        // --------------------------------------------------
        // 攻撃関連
        // --------------------------------------------------
        /// <summary>入力を受けて攻撃を決定するまでの待機時間（秒）</summary>
        private const float INPUT_DECISION_DELAY = 0.1f;

        /// <summary>攻撃クールタイム（秒）</summary>
        private const float ATTACK_COOLDOWN = 1.0f;

        // --------------------------------------------------
        // 視界判定関連
        // --------------------------------------------------
        /// <summary>視界角</summary>
        private const float FOV_ANGLE = 30f;

        /// <summary>視界距離</summary>
        private const float VIEW_DISTANCE = 100f;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// 弾丸発射時に発火するイベント。引数で弾丸タイプを通知する
        /// </summary>
        public event Action<BulletType, Transform> OnFireBullet;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 視界判定コンポーネントを注入して初期化する
        /// </summary>
        /// <param name="tankStatus">攻撃間隔算出に使用する戦車ステータス</param>
        /// <param name="fieldOfViewCalculator">視界判定のユースケースクラス</param>
        /// <param name="turretTransform">本体の Transform</param>
        /// <param name="turretTransform">砲塔の Transform</param>
        public TankAttackManager(
            in TankStatus tankStatus,
            in FieldOfViewCalculator fieldOfViewCalculator,
            in Transform transform,
            in Transform turretTransform)
        {
            _fieldOfViewCalculator = fieldOfViewCalculator;
            _transform = transform;
            _turretTransform = turretTransform;

            UpdateAttackParameter(tankStatus);
        }

        // ======================================================
        // セッター
        // ======================================================

        /// <summary>
        /// ターゲットと遮蔽物の Transform 配列と OBB 配列を AttackManager に送る
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
        /// ReloadTime ステータスを元に攻撃間隔を再計算する
        /// </summary>
        /// <param name="tankStatus">攻撃間隔に使用する戦車ステータス</param>
        public void UpdateAttackParameter(in TankStatus tankStatus)
        {
            // 攻撃クールタイムに使用する攻撃間隔倍率を算出
            _reloadTimeMultiplier =
                BASE_RELOAD_TIME_MULTIPLIER
                - tankStatus.ReloadTime * RELOAD_TIME_MULTIPLIER;
        }

        /// <summary>
        /// 毎フレーム呼び出し、攻撃入力を処理する
        /// </summary>
        /// <param name="unscaledDeltaTime">timeScaleに影響されない経過時間</param>
        /// <param name="leftInput">榴弾ボタン入力</param>
        /// <param name="rightInput">徹甲弾ボタン入力</param>
        public void UpdateAttack(
            in float unscaledDeltaTime,
            in ButtonState leftInput,
            in ButtonState rightInput)
        {
            if (leftInput == null || rightInput == null)
            {
                return;
            }

            // 視界内ターゲットを距離順に取得
            List<Transform> visibleTargets = _fieldOfViewCalculator.GetVisibleTargets(
                _turretTransform,
                _targetTransforms,
                _shieldOBBs,
                FOV_ANGLE,
                VIEW_DISTANCE
            );

            // 自身以外で最も近いターゲットを取得
            Transform closestTarget = GetClosestTarget(visibleTargets);

            // クールタイム中は何もしない
            if (_cooldownTime > 0f)
            {
                _cooldownTime -= unscaledDeltaTime;
                return;
            }

            // 新規入力があればタイマー開始
            if (!leftInput.IsPressed)
            {
                _leftInputTimer = -1f;
            }
            if (!rightInput.IsPressed)
            {
                _rightInputTimer = -1f;
            }

            // 新規入力があればタイマー開始
            if (leftInput.IsPressed && _leftInputTimer < 0f)
            {
                _leftInputTimer = 0f;
            }
            if (rightInput.IsPressed && _rightInputTimer < 0f)
            {
                _rightInputTimer = 0f;
            }

            // タイマーを進める
            if (_leftInputTimer >= 0f)
            {
                _leftInputTimer += unscaledDeltaTime;
            }
            if (_rightInputTimer >= 0f)
            {
                _rightInputTimer += unscaledDeltaTime;
            }

            // 両方押されていれば特殊攻撃判定
            if (_leftInputTimer >= 0f && _rightInputTimer >= 0f)
            {
                if (Mathf.Abs(_leftInputTimer - _rightInputTimer) <= INPUT_DECISION_DELAY)
                {
                    FireSpecial(closestTarget);
                    ResetInputTimers();
                    _cooldownTime = ATTACK_COOLDOWN * _reloadTimeMultiplier;
                    return;
                }
            }

            // 個別攻撃はタイマーが入力受付遅延を超えた場合に実行
            if (_leftInputTimer >= 0f && _leftInputTimer > INPUT_DECISION_DELAY)
            {
                FireCurrentBullet(closestTarget);
                ResetInputTimers();
                _cooldownTime = ATTACK_COOLDOWN * _reloadTimeMultiplier;
            }

            if (_rightInputTimer >= 0f && _rightInputTimer > INPUT_DECISION_DELAY)
            {
                FireCurrentBullet(closestTarget);
                ResetInputTimers();
                _cooldownTime = ATTACK_COOLDOWN * _reloadTimeMultiplier;
            }
        }

        /// <summary>
        /// 次に発射する弾丸タイプを切り替える
        /// </summary>
        public void NextBulletType()
        {
            _currentBulletIndex = (_currentBulletIndex + 1) % _bulletCycle.Length;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 視界内ターゲットの中から自身以外で最も近いターゲットを取得し、
        /// ターゲットアイコン表示を切り替える
        /// </summary>
        /// <param name="visibleTargets">視界内ターゲットのリスト（距離順）</param>
        /// <returns>最も近いターゲット Transform</returns>
        private Transform GetClosestTarget(List<Transform> visibleTargets)
        {
            if (visibleTargets == null || visibleTargets.Count == 0)
            {
                UpdateCachedTarget(null);

                return null;
            }

            Transform closestTarget = null;
            BaseTankRootManager targetManager = null;

            // 距離順に走査
            for (int i = 0; i < visibleTargets.Count; i++)
            {
                Transform candidate = visibleTargets[i];

                if (candidate == null)
                {
                    continue;
                }

                // 自身は除外
                if (candidate == _transform || candidate == _turretTransform)
                {
                    continue;
                }

                // BaseTankRootManager 取得
                BaseTankRootManager manager = candidate.GetComponent<BaseTankRootManager>();

                if (manager == null)
                {
                    continue;
                }

                // 破壊済みなら除外
                if (manager.IsBroken)
                {
                    continue;
                }

                closestTarget = candidate;
                targetManager = manager;

                break;
            }

            // ターゲット状態更新
            UpdateCachedTarget(targetManager);

            return closestTarget;
        }

        /// <summary>
        /// ターゲットを更新し、ターゲットアイコンの表示を切り替える
        /// </summary>
        /// <param name="newTarget">新しいターゲットの BaseTankRootManager。null の場合はアイコンオフ</param>
        private void UpdateCachedTarget(BaseTankRootManager newTarget)
        {
            if (_targetTankManager == newTarget)
            {
                return;
            }

            // 既存のターゲットがある場合はアイコンオフ
            if (_targetTankManager != null)
            {
                _targetTankManager.ChangeTargetIcon(false);
            }

            // 新しいターゲットが存在すればアイコンオン
            if (newTarget != null)
            {
                newTarget.ChangeTargetIcon(true);
            }

            // ターゲットを更新
            _targetTankManager = newTarget;
        }

        /// <summary>
        /// 現在の弾丸タイプで発射する
        /// </summary>
        /// <param name="target">弾丸の回転方向に指定するターゲット Transform</param>
        private void FireCurrentBullet(Transform target = null)
        {
            BulletType typeToFire = _bulletCycle[_currentBulletIndex];
            OnFireBullet?.Invoke(typeToFire, target);
        }

        /// <summary>
        /// 特殊攻撃を実行する
        /// </summary>
        private void FireSpecial(Transform target = null)
        {
            FireCurrentBullet(target);
        }

        /// <summary>
        /// 入力タイマーをリセットし、次の入力受付を初期化する
        /// </summary>
        private void ResetInputTimers()
        {
            _leftInputTimer = -1f;
            _rightInputTimer = -1f;
        }
    }
}