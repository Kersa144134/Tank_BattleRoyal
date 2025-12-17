// ======================================================
// SceneEventRouter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : シーン内イベントの仲介を行う
// ======================================================

using UnityEngine;
using SceneSystem.Data;
using TankSystem.Manager;
using WeaponSystem.Data;

namespace SceneSystem.Utility
{
    /// <summary>
    /// プレイヤーおよびエネミーから発生する
    /// シーン内イベントを仲介するクラス
    /// </summary>
    public sealed class SceneEventRouter
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>シーン内で共有される各コンポーネント参照</summary>
        private readonly UpdatableContext _context;

        /// <summary>イベント購読が完了しているかどうかを示すフラグ</summary>
        private bool _isSubscribed;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// SceneEventRouter を生成し
        /// 必要な TankRootManager のイベント購読を開始する
        /// </summary>
        /// <param name="context">
        /// 初期化済みの UpdatableContext
        /// </param>
        public SceneEventRouter(UpdatableContext context)
        {
            _context = context;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// PlayerTank および EnemyTank の
        /// 発射イベントを購読する
        /// </summary>
        public void Subscribe()
        {
            // すでに購読済みであれば何もしない
            if (_isSubscribed)
            {
                return;
            }

            // プレイヤー戦車が存在する場合のみ購読
            if (_context.PlayerTank != null)
            {
                // プレイヤーの弾発射イベントを登録
                _context.PlayerTank.OnFireBullet +=
                    HandleFireBullet;
            }

            // EnemyTank 配列が存在しない場合はここで終了
            if (_context.EnemyTanks == null)
            {
                // 購読完了フラグを立てる
                _isSubscribed = true;
                return;
            }

            // すべての EnemyTank の発射イベントを購読
            for (int i = 0; i < _context.EnemyTanks.Length; i++)
            {
                _context.EnemyTanks[i].OnFireBullet +=
                    HandleFireBullet;
            }

            // すべての購読が完了したためフラグを更新
            _isSubscribed = true;
        }

        /// <summary>
        /// 購読済みのイベントをすべて解除する
        /// Scene 破棄時や再生成時に呼び出される想定
        /// </summary>
        public void Dispose()
        {
            // 未購読状態であれば何もしない
            if (!_isSubscribed)
            {
                return;
            }

            // プレイヤー戦車が存在する場合のみ解除
            if (_context.PlayerTank != null)
            {
                _context.PlayerTank.OnFireBullet -=
                    HandleFireBullet;
            }

            // EnemyTank 配列が存在する場合のみ解除
            if (_context.EnemyTanks != null)
            {
                for (int i = 0; i < _context.EnemyTanks.Length; i++)
                {
                    _context.EnemyTanks[i].OnFireBullet -=
                        HandleFireBullet;
                }
            }

            // 購読解除が完了したためフラグを更新
            _isSubscribed = false;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 戦車から弾発射イベントを受け取り
        /// BulletPool を通じて弾丸を生成・発射する
        /// </summary>
        /// <param name="tank">
        /// 発射元となる戦車の RootManager
        /// </param>
        /// <param name="bulletType">
        /// 発射する弾丸の種類
        /// </param>
        private void HandleFireBullet(
            BaseTankRootManager tank,
            BulletType bulletType
        )
        {
            // BulletPool または Tank が未設定なら処理しない
            if (_context.BulletPool == null || tank == null)
            {
                return;
            }

            // 発射位置を戦車の FirePoint から取得
            Vector3 firePosition =
                tank.FirePoint.position;

            // 発射方向を戦車の前方ベクトルから取得
            Vector3 fireDirection =
                tank.transform.forward;

            // BulletPool を使用して弾丸を生成・発射
            _context.BulletPool.Spawn(
                bulletType,
                tank.TankStatus,
                firePosition,
                fireDirection
            );
        }
    }
}