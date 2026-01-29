// ======================================================
// TankEffectController.cs
// 作成者   : 高橋一翔
// 作成日   : 2026-01-29
// 更新日   : 2026-01-29
// 概要     : 戦車の表示制御を担当するクラス
// ======================================================

using UnityEngine;

namespace TankSystem.Controller
{
    /// <summary>
    /// 戦車の表示エフェクト制御クラス
    /// </summary>
    public sealed class TankEffectController
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>制御対象となるすべての ParticleSystem</summary>
        private readonly ParticleSystem[] _particleSystems;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>ダメージエフェクト判定用タグ名</summary>
        private const string DAMAGE_TAG = "Damage";

        /// <summary>爆発エフェクト判定用タグ名</summary>
        private const string EXPLOSION_TAG = "Explosion";

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// TankEffectController を生成する
        /// </summary>
        /// <param name="rootTransform">戦車のルート Transform</param>
        public TankEffectController(in Transform rootTransform)
        {
            // 子オブジェクトを含むすべての ParticleSystem を取得
            _particleSystems = rootTransform.GetComponentsInChildren<ParticleSystem>(true);

            if (_particleSystems == null)
            {
                return;
            }

            // すべての Particle を停止
            foreach (ParticleSystem particle in _particleSystems)
            {
                if (particle == null)
                {
                    continue;
                }

                particle.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear
                );
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ダメージエフェクトを再生する
        /// </summary>
        public void PlayDamage()
        {
            PlayByTag(DAMAGE_TAG);
        }

        /// <summary>
        /// 爆発エフェクトを再生する
        /// </summary>
        public void PlayExplosion()
        {
            PlayByTag(EXPLOSION_TAG);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 指定タグの ParticleSystem を再生する
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

                // 指定タグのみ対象
                if (!particle.CompareTag(tagName))
                {
                    continue;
                }

                // 既存パーティクルを即停止 + 削除
                particle.Clear(true);

                // 再生
                particle.Play(true);
            }
        }
    }
}