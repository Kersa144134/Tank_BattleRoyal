// ======================================================
// BulletManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-21
// 更新日時 : 2025-12-21
// 概要     : 弾丸の更新処理を管理するクラス
// ======================================================

using System.Collections.Generic;
using WeaponSystem.Data;

namespace WeaponSystem.Manager
{
    /// <summary>
    /// 弾丸の登録・更新を管理するクラス
    /// </summary>
    /// <remarks>
    /// MonoBehaviour を継承せず、
    /// SceneObjectRegistry などから明示的に Update を呼び出す
    /// </remarks>
    public class BulletManager
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// 更新対象となる弾丸リスト
        /// </summary>
        private readonly List<BulletBase> _updatableBullets = new List<BulletBase>();

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 弾丸を更新対象として登録する
        /// </summary>
        /// <param name="bullet">登録する弾丸</param>
        public void RegisterBullet(in BulletBase bullet)
        {
            if (_updatableBullets.Contains(bullet))
            {
                return;
            }

            // 更新対象リストに追加する
            _updatableBullets.Add(bullet);
        }

        /// <summary>
        /// 弾丸を更新対象から解除する
        /// </summary>
        /// <param name="bullet">解除する弾丸</param>
        public void UnregisterBullet(in BulletBase bullet)
        {
            // 更新対象リストから削除する
            _updatableBullets.Remove(bullet);
        }

        /// <summary>
        /// 登録されている弾丸を更新する
        /// </summary>
        /// <param name="deltaTime">フレーム間の経過時間</param>
        public void UpdateBullets(float deltaTime)
        {
            // 逆順ループで安全に更新する
            for (int i = _updatableBullets.Count - 1; i >= 0; i--)
            {
                BulletBase bullet = _updatableBullets[i];

                // 弾丸が無効、または破棄されている場合はスキップする
                if (bullet == null || !bullet.IsEnabled)
                {
                    continue;
                }

                // 弾丸の更新処理を実行する
                bullet.OnUpdate(deltaTime);
            }
        }
    }
}