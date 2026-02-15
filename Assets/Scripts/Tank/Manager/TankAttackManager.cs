// ======================================================
// TankAttackManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2026-01-30
// 概要     : 戦車の攻撃処理を管理するクラス
// ======================================================

using System;
using UnityEngine;
using InputSystem.Data;
using TankSystem.Controller;
using TankSystem.Data;
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

        /// <summary>視界・ターゲット管理コントローラー</summary>
        private readonly TankVisibilityController _visibilityController;

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

        /// <summary>現在の弾丸インデックス</summary>
        private int _currentBulletIndex = 0;

        /// <summary>攻撃クールタイムの残り時間（秒）</summary>
        private float _cooldownTime = 0f;

        /// <summary>現在の攻撃間隔倍率</summary>
        private float _reloadTimeMultiplier = BASE_RELOAD_TIME_MULTIPLIER;

        /// <summary>左攻撃ボタン入力経過時間（未押下時は -1）</summary>
        private float _leftInputTimer = -1f;

        /// <summary>右攻撃ボタン入力経過時間（未押下時は -1）</summary>
        private float _rightInputTimer = -1f;

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
        /// <summary>入力確定待機時間（秒）</summary>
        private const float INPUT_DECISION_DELAY = 0.1f;

        /// <summary>攻撃クールタイム（秒）</summary>
        private const float ATTACK_COOLDOWN = 1.0f;

        // --------------------------------------------------
        // 視界関連
        // --------------------------------------------------
        /// <summary>視界判定に使用する視野角（度）</summary>
        private const float FOV_ANGLE = 30f;
        
        /// <summary>視界判定に使用する最大索敵距離</summary>
        private const float VIEW_DISTANCE = 100f;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// 弾丸発射イベント
        /// </summary>
        public event Action<BulletType, Transform> OnFireBullet;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// AttackManager 初期化
        /// </summary>
        public TankAttackManager(
            in TankStatus tankStatus,
            in TankVisibilityController visibilityController)
        {
            // 視界コントローラー注入
            _visibilityController = visibilityController;

            // 攻撃パラメーター初期化
            UpdateAttackParameter(tankStatus);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 攻撃間隔再計算
        /// </summary>
        public void UpdateAttackParameter(in TankStatus tankStatus)
        {
            // ReloadTime に応じて攻撃間隔を短縮
            _reloadTimeMultiplier = BASE_RELOAD_TIME_MULTIPLIER - tankStatus.ReloadTime * RELOAD_TIME_MULTIPLIER;
        }

        /// <summary>
        /// 攻撃更新処理
        /// </summary>
        public void UpdateAttack(
            in float unscaledDeltaTime,
            in ButtonState leftInput,
            in ButtonState rightInput,
            in Transform[] tanks)
        {
            // 入力無効防止
            if (leftInput == null || rightInput == null)
            {
                return;
            }

            // ターゲット取得 
            Transform target = _visibilityController.GetClosestTarget(
                true,
                FOV_ANGLE,
                VIEW_DISTANCE,
                tanks
            );

            // クールタイム中
            if (_cooldownTime > 0f)
            {
                _cooldownTime -= unscaledDeltaTime;
                return;
            }

            // 入力解除時タイマーリセット
            if (!leftInput.IsPressed)
            {
                _leftInputTimer = -1f;
            }

            if (!rightInput.IsPressed)
            {
                _rightInputTimer = -1f;
            }

            // 新規入力開始検出
            if (leftInput.IsPressed && _leftInputTimer < 0f)
            {
                _leftInputTimer = 0f;
            }

            if (rightInput.IsPressed && _rightInputTimer < 0f)
            {
                _rightInputTimer = 0f;
            }

            // タイマー更新
            if (_leftInputTimer >= 0f)
            {
                _leftInputTimer += unscaledDeltaTime;
            }

            if (_rightInputTimer >= 0f)
            {
                _rightInputTimer += unscaledDeltaTime;
            }

            // 同時押し特殊攻撃判定
            if (_leftInputTimer >= 0f && _rightInputTimer >= 0f)
            {
                if (Mathf.Abs(_leftInputTimer - _rightInputTimer) <= INPUT_DECISION_DELAY)
                {
                    FireSpecial(target);

                    ResetInputTimers();

                    _cooldownTime = ATTACK_COOLDOWN * _reloadTimeMultiplier;

                    return;
                }
            }

            // 左入力攻撃
            if (_leftInputTimer >= 0f && _leftInputTimer > INPUT_DECISION_DELAY)
            {
                FireCurrentBullet(target);

                ResetInputTimers();

                _cooldownTime = ATTACK_COOLDOWN * _reloadTimeMultiplier;
            }

            // 右入力攻撃
            if (_rightInputTimer >= 0f && _rightInputTimer > INPUT_DECISION_DELAY)
            {
                FireCurrentBullet(target);

                ResetInputTimers();

                _cooldownTime = ATTACK_COOLDOWN * _reloadTimeMultiplier;
            }
        }

        /// <summary>
        /// 弾丸タイプ切替
        /// </summary>
        public void NextBulletType()
        {
            // ループ循環
            _currentBulletIndex = (_currentBulletIndex + 1) % _bulletCycle.Length;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 現在弾丸発射
        /// </summary>
        private void FireCurrentBullet(Transform target = null)
        {
            // 発射弾種取得
            BulletType type = _bulletCycle[_currentBulletIndex];

            // 発射通知
            OnFireBullet?.Invoke(type, target);
        }

        /// <summary>
        /// 特殊攻撃
        /// </summary>
        private void FireSpecial(Transform target = null)
        {
            // 現在は通常弾と同じ処理
            FireCurrentBullet(target);
        }

        /// <summary>
        /// 入力タイマー初期化
        /// </summary>
        private void ResetInputTimers()
        {
            // 左入力リセット
            _leftInputTimer = -1f;

            // 右入力リセット
            _rightInputTimer = -1f;
        }
    }
}