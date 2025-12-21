// ======================================================
// TankAttackManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2025-12-08
// 概要     : 戦車の攻撃処理を管理するクラス
//            榴弾・徹甲弾・同時押し特殊攻撃をサポート
// ======================================================

using InputSystem.Data;
using System;
using UnityEngine;
using WeaponSystem.Data;

namespace TankSystem.Manager
{
    /// <summary>
    /// 戦車の攻撃処理を管理するクラス
    /// </summary>
    public class TankAttackManager
    {
        // ======================================================
        // フィールド
        // ======================================================

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

        /// <summary>左攻撃ボタンが押されてからの経過時間（同時押し判定用、未押下時は -1）</summary>
        private float _leftInputTimer = -1f;

        /// <summary>右攻撃ボタンが押されてからの経過時間（同時押し判定用、未押下時は -1）</summary>
        private float _rightInputTimer = -1f;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>入力を受けて攻撃を決定するまでの待機時間（秒）</summary>
        private const float INPUT_DECISION_DELAY = 0.1f;

        /// <summary>攻撃クールタイム（秒）</summary>
        private const float ATTACK_COOLDOWN = 1.0f;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// 弾丸発射時に発火するイベント。引数で弾丸タイプを通知する
        /// </summary>
        public event Action<BulletType> OnFireBullet;
        
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 毎フレーム呼び出し、攻撃入力を処理する
        /// </summary>
        /// <param name="leftInput">榴弾ボタン入力</param>
        /// <param name="rightInput">徹甲弾ボタン入力</param>
        public void UpdateAttack(in ButtonState leftInput, in ButtonState rightInput)
        {
            // 引数が null の場合は処理をスキップ
            if (leftInput == null || rightInput == null)
            {
                return;
            }
            
            // クールタイム中は何もしない
            if (_cooldownTime > 0f)
            {
                _cooldownTime -= Time.deltaTime;
                return;
            }

            // 新規入力があればタイマー開始
            if (leftInput.Down && _leftInputTimer < 0f)
            {
                _leftInputTimer = 0f;
            }
            if (rightInput.Down && _rightInputTimer < 0f)
            {
                _rightInputTimer = 0f;
            }

            // タイマーを進める
            if (_leftInputTimer >= 0f)
            {
                _leftInputTimer += Time.deltaTime;
            }
            if (_rightInputTimer >= 0f)
            {
                _rightInputTimer += Time.deltaTime;
            }

            // 両方押されていれば特殊攻撃判定
            if (_leftInputTimer >= 0f && _rightInputTimer >= 0f)
            {
                if (Mathf.Abs(_leftInputTimer - _rightInputTimer) <= INPUT_DECISION_DELAY)
                {
                    FireSpecial();
                    ResetInputTimers();
                    _cooldownTime = ATTACK_COOLDOWN;
                    return;
                }
            }

            // 個別攻撃はタイマーが入力受付遅延を超えた場合に実行
            if (_leftInputTimer >= 0f && _leftInputTimer > INPUT_DECISION_DELAY)
            {
                FireCurrentBullet();
                _leftInputTimer = -1f;
                _cooldownTime = ATTACK_COOLDOWN;
            }

            if (_rightInputTimer >= 0f && _rightInputTimer > INPUT_DECISION_DELAY)
            {
                FireCurrentBullet();
                _rightInputTimer = -1f;
                _cooldownTime = ATTACK_COOLDOWN;
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
        /// 現在の弾丸タイプで発射する
        /// </summary>
        private void FireCurrentBullet()
        {
            BulletType typeToFire = _bulletCycle[_currentBulletIndex];
            OnFireBullet?.Invoke(typeToFire);
        }

        /// <summary>
        /// 特殊攻撃を実行する
        /// </summary>
        private void FireSpecial()
        {
            Debug.Log("Fire Special");
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