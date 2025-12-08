// ======================================================
// TankAttackManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2025-12-08
// 概要     : 戦車の攻撃処理を管理する
//            MonoBehaviour 非継承
//            榴弾・徹甲弾・同時押し特殊攻撃をサポート
// ======================================================

using UnityEngine;

namespace TankSystem.Manager
{
    /// <summary>
    /// 戦車の攻撃処理を管理する純粋クラス
    /// </summary>
    public class TankAttackManager
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>同時押し猶予時間（秒）</summary>
        private const float SIMULTANEOUS_INPUT_WINDOW = 0.2f;

        /// <summary>攻撃クールタイム（秒）</summary>
        private const float ATTACK_COOLDOWN = 0.5f;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>弾丸を発射する発射位置の Transform</summary>
        private readonly Transform _firePoint;

        /// <summary>攻撃クールタイムの残り時間（秒）</summary>
        private float _cooldownTime = 0f;

        /// <summary>榴弾ボタンが押されてからの経過時間（同時押し判定用、未押下時は -1）</summary>
        private float _heInputTime = -1f;

        /// <summary>徹甲弾ボタンが押されてからの経過時間（同時押し判定用、未押下時は -1）</summary>
        private float _apInputTime = -1f;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 攻撃処理に必要な発射位置を設定し、クールタイムを初期化する
        /// </summary>
        /// <param name="firePoint">弾丸発射位置の Transform</param>
        public TankAttackManager(Transform firePoint)
        {
            _firePoint = firePoint;
            _cooldownTime = 0f;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 毎フレーム呼び出し、攻撃入力を処理する
        /// </summary>
        /// <param name="heInput">榴弾ボタン入力</param>
        /// <param name="apInput">徹甲弾ボタン入力</param>
        public void UpdateAttack(bool heInput, bool apInput)
        {
            float time = Time.time;

            // クールタイム経過
            if (_cooldownTime > 0f)
            {
                _cooldownTime -= Time.deltaTime;
                return;
            }

            // 榴弾入力を記録
            if (heInput)
            {
                _heInputTime = time;
            }

            // 徹甲弾入力を記録
            if (apInput)
            {
                _apInputTime = time;
            }

            // 同時押し判定処理
            if (_heInputTime > 0f && _apInputTime > 0f)
            {
                // 両方押されていて猶予時間内なら特殊攻撃
                if (Mathf.Abs(_heInputTime - _apInputTime) <= SIMULTANEOUS_INPUT_WINDOW)
                {
                    FireSpecial();
                    ResetInputTimes();
                    _cooldownTime = ATTACK_COOLDOWN;
                    return;
                }
                else
                {
                    // 猶予時間を過ぎた場合、それぞれの個別攻撃を実行
                    if (_heInputTime > SIMULTANEOUS_INPUT_WINDOW)
                    {
                        FireHE();
                        _heInputTime = -1f;
                        _cooldownTime = ATTACK_COOLDOWN;
                    }

                    if (_apInputTime > SIMULTANEOUS_INPUT_WINDOW)
                    {
                        FireAP();
                        _apInputTime = -1f;
                        _cooldownTime = ATTACK_COOLDOWN;
                    }
                }
            }

            // 個別攻撃判定
            if (heInput)
            {
                // 押下時刻をリセット
                _heInputTime = 0f;
            }

            if (apInput)
            {
                // 押下時刻をリセット
                _apInputTime = 0f;
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        private void FireHE()
        {
            Debug.Log("Fire High-Explosive");
        }

        private void FireAP()
        {
            Debug.Log("Fire Armor-Piercing");
        }

        private void FireSpecial()
        {
            Debug.Log("Fire Special Attack");
        }

        private void ResetInputTimes()
        {
            _heInputTime = -1f;
            _apInputTime = -1f;
        }
    }
}