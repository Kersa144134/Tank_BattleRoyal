// ======================================================
// TankEffectController.cs
// 作成者   : 高橋一翔
// 作成日   : 2026-01-29
// 更新日   : 2026-01-29
// 概要     : 戦車の表示エフェクト制御を担当するクラス
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

        /// <summary>制御対象となるすべての Renderer</summary>
        private readonly Renderer[] _renderers;

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
            _renderers = rootTransform.GetComponentsInChildren<Renderer>(true);
            _particleSystems = rootTransform.GetComponentsInChildren<ParticleSystem>(true);

            if (_particleSystems == null)
            {
                return;
            }

            // 起動時にパーティクルを停止する
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
            SetExplosionRendererState();

            PlayByTag(EXPLOSION_TAG);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 爆発時に爆発以外の Renderer を非表示にする
        /// </summary>
        private void SetExplosionRendererState()
        {
            if (_renderers == null)
            {
                return;
            }

            foreach (Renderer renderer in _renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                // Explosion タグのみ表示
                renderer.enabled = renderer.CompareTag(EXPLOSION_TAG);
            }
        }

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