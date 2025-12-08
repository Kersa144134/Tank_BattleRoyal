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

        /// <summary>入力を受けて攻撃を決定するまでの待機時間（秒）</summary>
        private const float INPUT_DECISION_DELAY = 0.1f;

        /// <summary>攻撃クールタイム（秒）</summary>
        private const float ATTACK_COOLDOWN = 1.0f;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>弾丸を発射する発射位置の Transform</summary>
        private readonly Transform _firePoint;

        /// <summary>攻撃クールタイムの残り時間（秒）</summary>
        private float _cooldownTime = 0f;

        /// <summary>榴弾ボタンが押されてからの経過時間（同時押し判定用、未押下時は -1）</summary>
        private float _heInputTimer = -1f;

        /// <summary>徹甲弾ボタンが押されてからの経過時間（同時押し判定用、未押下時は -1）</summary>
        private float _apInputTimer = -1f;

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
            // クールタイム中は何もしない
            if (_cooldownTime > 0f)
            {
                _cooldownTime -= Time.deltaTime;
                return;
            }

            // 新規入力があればタイマー開始
            if (heInput && _heInputTimer < 0f) _heInputTimer = 0f;
            if (apInput && _apInputTimer < 0f) _apInputTimer = 0f;

            // タイマーを進める
            if (_heInputTimer >= 0f) _heInputTimer += Time.deltaTime;
            if (_apInputTimer >= 0f) _apInputTimer += Time.deltaTime;

            // 両方押されていれば特殊攻撃判定
            if (_heInputTimer >= 0f && _apInputTimer >= 0f)
            {
                if (Mathf.Abs(_heInputTimer - _apInputTimer) <= INPUT_DECISION_DELAY)
                {
                    FireSpecial();
                    ResetInputTimers();
                    _cooldownTime = ATTACK_COOLDOWN;
                    return;
                }
            }

            // 個別攻撃はタイマーが入力受付遅延を超えた場合に実行
            if (_heInputTimer >= 0f && _heInputTimer > INPUT_DECISION_DELAY)
            {
                FireHE();
                _heInputTimer = -1f;
                _cooldownTime = ATTACK_COOLDOWN;
            }

            if (_apInputTimer >= 0f && _apInputTimer > INPUT_DECISION_DELAY)
            {
                FireAP();
                _apInputTimer = -1f;
                _cooldownTime = ATTACK_COOLDOWN;
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 榴弾攻撃を実行する
        /// </summary>
        private void FireHE()
        {
            Debug.Log("Fire High-Explosive");
        }

        /// <summary>
        /// 徹甲弾攻撃を実行する
        /// </summary>
        private void FireAP()
        {
            Debug.Log("Fire Armor-Piercing");
        }

        /// <summary>
        /// 同時押しによる特殊攻撃を実行する
        /// </summary>
        private void FireSpecial()
        {
            Debug.Log("Fire Special Attack");
        }

        /// <summary>
        /// 入力タイマーをリセットし、次の入力受付を初期化する
        /// </summary>
        private void ResetInputTimers()
        {
            _heInputTimer = -1f;
            _apInputTimer = -1f;
        }
    }
}