// ======================================================
// TankAttackManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2025-12-08
// 概要     : 戦車の攻撃処理を管理する
//            MonoBehaviour 非継承
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
        // フィールド
        // ======================================================

        /// <summary>攻撃ボタンの入力状態</summary>
        private bool _fireInput;

        /// <summary>攻撃クールタイム管理</summary>
        private float _cooldownTime;

        /// <summary>攻撃の最大クールタイム（秒）</summary>
        private readonly float _maxCooldown = 0.5f;

        /// <summary>発射位置のトランスフォーム</summary>
        private readonly Transform _firePoint;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 攻撃マネージャーを初期化する
        /// </summary>
        /// <param name="firePoint">発射位置</param>
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
        /// <param name="fireInput">攻撃ボタン入力</param>
        public void UpdateAttack(bool fireInput)
        {
            _fireInput = fireInput;

            // クールタイム経過
            if (_cooldownTime > 0f)
            {
                _cooldownTime -= Time.deltaTime;
                return;
            }

            // 攻撃入力があれば発射
            if (_fireInput)
            {
                Fire();
                _cooldownTime = _maxCooldown;
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 弾丸を発射する
        /// </summary>
        private void Fire()
        {
            Debug.Log("Fire");
        }
    }
}