// ======================================================
// SceneEventRouter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : シーン内イベントの仲介を行う
// ======================================================

using System;
using UnityEngine;
using InputSystem.Data;
using ItemSystem.Data;
using SceneSystem.Data;
using TankSystem.Data;
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
        // イベント
        // ======================================================

        /// <summary>オプションボタン押下時に発火するイベント</summary>
        public event Action OnOptionButtonPressed;

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

            // --------------------------------------------------
            // プレイヤー戦車
            // --------------------------------------------------
            if (_context.PlayerTank != null)
            {
                _context.PlayerTank.OnInputModeChangeButtonPressed += HandleInputModeChangeButtonPressed;
                _context.PlayerTank.OnFireModeChangeButtonPressed += HandleFireModeChangeButtonPressed;
                _context.PlayerTank.OnOptionButtonPressed += HandleOptionButtonPressed;
                _context.PlayerTank.OnFireBullet += HandleFireBullet;
            }

            // --------------------------------------------------
            // エネミー戦車
            // --------------------------------------------------
            // 配列が存在しない場合は終了
            if (_context.EnemyTanks != null)
            {
                for (int i = 0; i < _context.EnemyTanks.Length; i++)
                {
                    _context.EnemyTanks[i].OnFireBullet += HandleFireBullet;
                }
            }

            // --------------------------------------------------
            // 弾丸プール
            // --------------------------------------------------
            if (_context.BulletPool != null)
            {
                _context.BulletPool.OnBulletSpawned += HandleSpawnedBullet;
                _context.BulletPool.OnBulletDespawned += HandleDespawnedBullet;
            }

            // --------------------------------------------------
            // アイテムプール
            // --------------------------------------------------
            if (_context.ItemPool != null)
            {
                _context.ItemPool.OnItemActivated += HandleActivatedItem;
                _context.ItemPool.OnItemDeactivated += HandleDeactivatedItem;
            }

            // --------------------------------------------------
            // 衝突判定
            // --------------------------------------------------
            if (_context.CollisionManager != null)
            {
                _context.CollisionManager.EventRouter.OnBulletHit += HandleHitBullet;
                _context.CollisionManager.EventRouter.OnItemGet += HandleGetItem;
            }

           // 購読完了フラグを更新
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

            // --------------------------------------------------
            // プレイヤー戦車
            // --------------------------------------------------
            if (_context.PlayerTank != null)
            {
                _context.PlayerTank.OnInputModeChangeButtonPressed -= HandleInputModeChangeButtonPressed;
                _context.PlayerTank.OnFireModeChangeButtonPressed -= HandleFireModeChangeButtonPressed;
                _context.PlayerTank.OnOptionButtonPressed -= HandleOptionButtonPressed;
                _context.PlayerTank.OnFireBullet -= HandleFireBullet;
            }

            // --------------------------------------------------
            // エネミー戦車
            // --------------------------------------------------
            if (_context.EnemyTanks != null)
            {
                for (int i = 0; i < _context.EnemyTanks.Length; i++)
                {
                    _context.EnemyTanks[i].OnFireBullet -= HandleFireBullet;
                }
            }

            // --------------------------------------------------
            // 弾丸プール
            // --------------------------------------------------
            if (_context.BulletPool != null)
            {
                _context.BulletPool.OnBulletSpawned -= HandleSpawnedBullet;
                _context.BulletPool.OnBulletDespawned -= HandleDespawnedBullet;
            }

            // --------------------------------------------------
            // 衝突判定
            // --------------------------------------------------
            if (_context.CollisionManager != null)
            {
                _context.CollisionManager.EventRouter.OnBulletHit -= HandleHitBullet;
                _context.CollisionManager.EventRouter.OnItemGet -= HandleGetItem;
            }

            // 購読完了フラグを更新
            _isSubscribed = false;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // 入力
        // --------------------------------------------------
        /// <summary>
        /// 入力モード切り替えボタン押下時の処理を行うハンドラ
        /// 現在の入力モードに応じて、次の入力モードへ切り替える
        /// </summary>
        private void HandleInputModeChangeButtonPressed()
        {
            // プレイヤー戦車のキャタピラ入力モードをトグル切替
            _context.PlayerTank?.ChangeInputMode();

            // 現在の入力モードを取得
            TrackInputMode currentMode = _context.PlayerTank.InputMode;

            // 入力モードに応じてカメラターゲット用インデックスを決定
            int cameraTargetIndex =
                currentMode == TrackInputMode.Single
                ? 1
                : 0;

            // カメラの追従ターゲットを切り替え
            _context.CameraManager?.SetTargetByIndex(cameraTargetIndex);
        }

        /// <summary>
        /// 攻撃モード切り替えボタン押下時の処理を行うハンドラ
        /// </summary>
        private void HandleFireModeChangeButtonPressed()
        {
            
        }

        /// <summary>
        /// オプションボタン押下時の処理を行うハンドラ
        /// SceneManager へフェーズ切り替え通知を行う
        /// </summary>
        private void HandleOptionButtonPressed()
        {
            OnOptionButtonPressed?.Invoke();
        }

        // --------------------------------------------------
        // 弾丸
        // --------------------------------------------------
        /// <summary>
        /// 弾丸が生成された際に呼び出され、
        /// SceneObjectRegistry へ登録を行う
        /// </summary>
        /// <param name="bullet">生成された弾丸インスタンス</param>
        private void HandleSpawnedBullet(BulletBase bullet)
        {
            if (bullet == null)
            {
                return;
            }

            _context.SceneObjectRegistry?.RegisterBullet(bullet);
            _context.CollisionManager?.RegisterBullet(bullet);
        }

        /// <summary>
        /// 弾丸が破棄された際に呼び出され、
        /// SceneObjectRegistry から登録解除を行う
        /// </summary>
        /// <param name="bullet">使用終了した弾丸インスタンス</param>
        private void HandleDespawnedBullet(BulletBase bullet)
        {
            if (bullet == null)
            {
                return;
            }

            _context.SceneObjectRegistry?.UnregisterBullet(bullet);
            _context.CollisionManager?.UnregisterBullet(bullet);
        }

        /// <summary>
        /// 戦車からの発射要求を受け取り、
        /// BulletPool を通じて弾丸を生成・発射する
        /// </summary>
        /// <param name="tank">発射元となる戦車の RootManager</param>
        /// <param name="bulletType">発射する弾丸の種類</param>
        /// <param name="target">発射する弾丸の種類</param>
        /// <param name="target">弾丸の回転方向に指定するターゲット Transform</param>
        private void HandleFireBullet(
            BaseTankRootManager tank,
            BulletType bulletType,
            Transform target = null
        )
        {
            if (tank == null)
            {
                return;
            }

            Vector3 firePosition =
                tank.FirePoint.position;

            Vector3 fireDirection =
                tank.transform.forward;

            _context.BulletPool?.Spawn(
                bulletType,
                tank.TankId,
                tank.TankStatus,
                firePosition,
                fireDirection,
                target
            );
        }

        /// <summary>
        /// </summary>
        /// <param name=""></param>
        private void HandleHitBullet(BulletBase bullet)
        {
            if (bullet == null)
            {
                return;
            }
        }

        // --------------------------------------------------
        // アイテム
        // --------------------------------------------------
        /// <summary>
        /// アイテムを Scene 管理対象として登録する
        /// </summary>
        /// <param name="item">生成されたアイテムスロット</param>
        private void HandleActivatedItem(ItemSlot item)
        {
            if (item == null)
            {
                return;
            }

            _context.SceneObjectRegistry?.RegisterItem(item);
            _context.CollisionManager?.RegisterItem(item);
        }

        /// <summary>
        /// アイテムを Scene 管理対象から解除する
        /// </summary>
        /// <param name="item">使用終了または取得済みのアイテムスロット</param>
        private void HandleDeactivatedItem(ItemSlot item)
        {
            if (item == null)
            {
                return;
            }

            _context.SceneObjectRegistry?.UnregisterItem(item);
            _context.CollisionManager?.UnregisterItem(item);
        }

        /// <summary>
        /// アイテムが取得された際の処理
        /// </summary>
        /// <param name="tankRootManager">アイテムを取得した戦車の RootManager</param>
        /// <param name="itemSlot">取得されたアイテムスロット</param>
        private void HandleGetItem(
            BaseTankRootManager tankRootManager,
            ItemSlot itemSlot
        )
        {
            // アイテム除外処理
            HandleDeactivatedItem(itemSlot);

            // パラメーターアイテム
            if (itemSlot.ItemData is ParamItemData param)
            {
                // 戦車のパラメーターを増減させる
                tankRootManager.TankStatus.IncreaseParameter(
                    param.ParamType,
                    param.Value
                );

                return;
            }

            // 武装アイテム
            if (itemSlot.ItemData is WeaponItemData weapon)
            {
                return;
            }
        }
    }
}