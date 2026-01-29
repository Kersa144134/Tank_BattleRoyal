// ======================================================
// TankParticleController.cs
// 作成者   : 高橋一翔
// 作成日   : 2026-01-29
// 更新日   : 2026-01-29
// 概要     : 戦車の Particle 制御を担当するクラス
// ======================================================

using UnityEngine;

namespace TankSystem.Controller
{
    /// <summary>
    /// 戦車 Particle 制御クラス
    /// </summary>
    public sealed class TankParticleController
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>ParticleSystem 配列</summary>
        private readonly ParticleSystem[] _particleSystems;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>Damageタグ</summary>
        private const string DAMAGE_TAG = "Damage";

        /// <summary>Explosionタグ</summary>
        private const string EXPLOSION_TAG = "Explosion";

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// TankParticleController 生成
        /// </summary>
        public TankParticleController(in Transform rootTransform)
        {
            _particleSystems = rootTransform.GetComponentsInChildren<ParticleSystem>(true);

            if (_particleSystems == null)
            {
                return;
            }

            foreach (ParticleSystem particle in _particleSystems)
            {
                if (particle == null)
                {
                    continue;
                }

                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ダメージエフェクト再生
        /// </summary>
        public void PlayDamage()
        {
            PlayByTag(DAMAGE_TAG);
        }

        /// <summary>
        /// 爆発エフェクト再生
        /// </summary>
        public void PlayExplosion()
        {
            PlayByTag(EXPLOSION_TAG);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// パーティクルをタグ指定で再生する
        /// </summary>
        private void PlayByTag(in string tagName)
        {
            if (_particleSystems == null)
            {
                return;
            }

            foreach (ParticleSystem particle in _particleSystems)
            {
                if (particle == null)
                {
                    continue;
                }

                if (!particle.CompareTag(tagName))
                {
                    continue;
                }

                particle.Clear(true);
                particle.Play(true);
            }
        }
    }
}