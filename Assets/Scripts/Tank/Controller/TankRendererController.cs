// ======================================================
// TankRendererController.cs
// 作成者   : 高橋一翔
// 作成日   : 2026-01-29
// 更新日   : 2026-01-29
// 概要     : 戦車の Renderer 表示制御を担当するクラス
// ======================================================

using UnityEngine;

namespace TankSystem.Controller
{
    /// <summary>
    /// 戦車 Renderer 表示制御クラス
    /// </summary>
    public sealed class TankRendererController
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>Renderer 配列</summary>
        private readonly Renderer[] _renderers;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>Explosion タグ</summary>
        private const string EXPLOSION_TAG = "Explosion";

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// TankRendererController 生成
        /// </summary>
        public TankRendererController(in Transform rootTransform)
        {
            _renderers = rootTransform.GetComponentsInChildren<Renderer>(true);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 爆発時の Renderer 状態設定
        /// </summary>
        public void SetExplosionRendererState()
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

                renderer.enabled = renderer.CompareTag(EXPLOSION_TAG);
            }
        }
    }
}