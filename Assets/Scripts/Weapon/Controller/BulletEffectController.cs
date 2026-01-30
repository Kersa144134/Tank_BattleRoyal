// ======================================================
// BulletEffectController.cs
// 作成者   : 高橋一翔
// 作成日   : 2026-01-29
// 更新日   : 2026-01-29
// 概要     : 弾丸の表示制御を担当するクラス
// ======================================================

using UnityEngine;

namespace WeaponSystem.Controller
{
    /// <summary>
    /// 弾丸の表示エフェクト制御クラス
    /// </summary>
    public sealed class BulletEffectController
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>制御対象となるすべての Renderer</summary>
        private readonly Renderer[] _renderers;

        /// <summary>制御対象となるすべての ParticleSystem</summary>
        private readonly ParticleSystem[] _particleSystems;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>爆発エフェクト判定用タグ名</summary>
        private const string EXPLOSION_TAG = "Explosion";

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// BulletEffectController を生成する
        /// </summary>
        /// <param name="rootTransform">弾丸のルート Transform</param>
        public BulletEffectController(in Transform rootTransform)
        {
            // 子オブジェクトを含むすべての Renderer を取得
            _renderers = rootTransform.GetComponentsInChildren<Renderer>(true);

            // 子オブジェクトを含むすべての ParticleSystem を取得
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

                // 自動再生を無効化
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
        /// 弾丸の通常表示状態を設定する
        /// </summary>
        /// <param name="isVisible">true: 表示 / false: 非表示</param>
        public void SetVisible(in bool isVisible)
        {
            if (_renderers != null)
            {
                foreach (Renderer renderer in _renderers)
                {
                    if (renderer == null)
                    {
                        continue;
                    }

                    // Explosion タグの Renderer は除外
                    if (renderer.CompareTag(EXPLOSION_TAG))
                    {
                        continue;
                    }

                    renderer.enabled = isVisible;
                }
            }

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

                // Explosion タグの Particle は除外
                if (particle.CompareTag(EXPLOSION_TAG))
                {
                    continue;
                }

                if (isVisible)
                {
                    particle.Play(true);
                }
                else
                {
                    particle.Stop(
                        true,
                        ParticleSystemStopBehavior.StopEmittingAndClear
                    );
                }
            }
        }

        /// <summary>
        /// 爆発エフェクトを再生する
        /// </summary>
        public void PlayExplosion()
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

                // タグ不一致はスキップ
                if (!particle.CompareTag(EXPLOSION_TAG))
                {
                    continue;
                }

                // 既存パーティクルを残したまま停止
                particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

                // 再生時間を0へ戻す
                particle.Simulate(0f, true, true, true);

                // 再生開始
                particle.Play(true);
            }
        }
    }
}