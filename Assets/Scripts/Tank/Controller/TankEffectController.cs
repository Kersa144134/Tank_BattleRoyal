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
        // コンポーネント参照
        // ======================================================

        /// <summary>ForceField 制御対象</summary>
        private readonly ForceFieldController _forceFieldController;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>制御対象となるすべての ParticleSystem</summary>
        private readonly ParticleSystem[] _particleSystems;

        /// <summary>制御対象となるすべての Renderer</summary>
        private readonly Renderer[] _renderers;

        /// <summary>現在のターゲット ForceField 目標値</summary>
        private float _targetForceFieldTargetValue;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>ダメージエフェクト判定用タグ名</summary>
        private const string DAMAGE_TAG = "Damage";

        /// <summary>爆発エフェクト判定用タグ名</summary>
        private const string EXPLOSION_TAG = "Explosion";

        /// <summary>ターゲット OFF 時の値</summary>
        private const float TARGET_OFF_VALUE = -1f;

        /// <summary>ターゲット ON 時の値</summary>
        private const float TARGET_ON_VALUE = 1f;

        /// <summary>ターゲット補間速度</summary>
        private const float TARGET_LERP_SPEED = 10f;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// TankEffectController を生成する
        /// </summary>
        /// <param name="rootTransform">戦車のルート Transform</param>
        public TankEffectController(in Transform rootTransform)
        {
            // ---------------------------------------------
            // Renderer
            // ---------------------------------------------
            _renderers = rootTransform.GetComponentsInChildren<Renderer>(true);

            // ---------------------------------------------
            // パーティクル
            // ---------------------------------------------
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

            // ---------------------------------------------
            // ターゲット ForceField
            // ---------------------------------------------
            _forceFieldController = rootTransform.GetComponentInChildren<ForceFieldController>(true);
            _targetForceFieldTargetValue = TARGET_OFF_VALUE;
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

        /// <summary>
        /// ForceField アニメーションの目標値を設定する
        /// </summary>
        /// <param name="isOn">true = ON / false = OFF</param>
        public void SetForceField(in bool isOn)
        {
            if (isOn)
            {
                _targetForceFieldTargetValue = TARGET_ON_VALUE;

                return;
            }

            _targetForceFieldTargetValue = TARGET_OFF_VALUE;
        }

        /// <summary>
        /// ForceField アニメーション更新
        /// </summary>
        public void UpdateForceField(in float deltaTime)
        {
            if (_forceFieldController == null)
            {
                return;
            }

            float currentValue = _forceFieldController.openCloseProgress;

            float nextValue = Mathf.MoveTowards(
                currentValue,
                _targetForceFieldTargetValue,
                TARGET_LERP_SPEED * deltaTime
            );

            _forceFieldController.openCloseProgress = nextValue;
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