// ======================================================
// TankEffectManager.cs
// 作成者   : 高橋一翔
// 作成日   : 2026-01-29
// 更新日   : 2026-01-29
// 概要     : 戦車エフェクトの統合管理を行うクラス
// ======================================================

using UnityEngine;

namespace TankSystem.Controller
{
    /// <summary>
    /// 戦車エフェクト統合管理クラス
    /// </summary>
    public sealed class TankEffectManager
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>Renderer 制御クラス</summary>
        private readonly TankRendererController _rendererController;

        /// <summary>Particle 制御クラス</summary>
        private readonly TankParticleController _particleController;

        /// <summary>ForceField 制御クラス</summary>
        private readonly TankForceFieldController _forceFieldController;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// TankEffectManager 生成
        /// </summary>
        /// <param name="rootTransform">戦車のルート Transform</param>
        public TankEffectManager(in Transform rootTransform)
        {
            _rendererController = new TankRendererController(rootTransform);
            _particleController = new TankParticleController(rootTransform);
            _forceFieldController = new TankForceFieldController(rootTransform);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ダメージエフェクト再生
        /// </summary>
        public void PlayDamage()
        {
            _particleController.PlayDamage();
        }

        /// <summary>
        /// 爆発エフェクト再生
        /// </summary>
        public void PlayExplosion()
        {
            _rendererController.SetExplosionRendererState();
            _particleController.PlayExplosion();
        }

        /// <summary>
        /// ForceField エフェクト切り替え
        /// </summary>
        public void SetForceField(in bool isOn)
        {
            _forceFieldController.SetForceField(isOn);
        }

        /// <summary>
        /// ForceField エフェクト更新
        /// </summary>
        public void UpdateForceField(in float deltaTime)
        {
            _forceFieldController.UpdateForceField(deltaTime);
        }
    }
}