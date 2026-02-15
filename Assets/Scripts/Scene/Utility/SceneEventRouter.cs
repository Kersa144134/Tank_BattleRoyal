// ======================================================
// SceneEventRouter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : シーン内イベントの仲介を行う
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using CollisionSystem.Data;
using InputSystem.Data;
using InputSystem.Manager;
using ItemSystem.Data;
using SceneSystem.Data;
using ScoreSystem.Manager;
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

        /// <summary>
        /// 爆発範囲内のコンテキストを一時的に格納するリスト
        /// </summary>
        private readonly List<BaseCollisionContext> _overlapResults = new List<BaseCollisionContext>();

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>フェーズ変更タイミング通知</summary>
        public event Action<PhaseType> OnPhaseChanged;
        
        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// SceneEventRouter を生成する
        /// </summary>
        /// <param name="context">初期化済みの Updatable コンテキスト</param>
        public SceneEventRouter(UpdatableContext context)
        {
            _context = context;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// イベントを購読する
        /// </summary>
        public void Subscribe()
        {
            // すでに購読済みであれば処理なし
            if (_isSubscribed)
            {
                return;
            }

            // --------------------------------------------------
            // オブジェクト群
            // --------------------------------------------------
            if (_context.SceneObjectRegistry != null)
            {
                _context.SceneObjectRegistry.ObstacleManager.OnObstacleBreaked += HandleDeactivatedObstacle;
            }

            // --------------------------------------------------
            // プレイヤー戦車
            // --------------------------------------------------
            if (_context.PlayerTank != null)
            {
                _context.PlayerTank.OnInputModeChangeButtonPressed += HandleInputModeChangeButtonPressed;
                _context.PlayerTank.OnFireModeChangeButtonPressed += HandleFireModeChangeButtonPressed;
                _context.PlayerTank.OnFireBullet += HandleFireBullet;
                _context.PlayerTank.DurabilityManager.OnDurabilityChanged += HandleDurabilityChanged;
                _context.PlayerTank.EnergyManager.OnFuelChanged += HandleFuelChanged;
                _context.PlayerTank.EnergyManager.OnAmmoChanged += HandleAmmoChanged;
                _context.PlayerTank.OnBroken += HandleBroken;
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
                    _context.EnemyTanks[i].OnBroken += HandleBroken;
                }
            }

            // --------------------------------------------------
            // 弾丸プール
            // --------------------------------------------------
            if (_context.BulletPool != null)
            {
                _context.BulletPool.OnBulletSpawned += HandleSpawnedBullet;
                _context.BulletPool.OnBulletDespawned += HandleDespawnedBullet;
                _context.BulletPool.OnBulletExploded += HandleBulletExplode;
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

            // --------------------------------------------------
            // UI
            // --------------------------------------------------
            if (_context.TitleUIManager != null)
            {
                _context.TitleUIManager.OnTitlePhaseAnimationFinished += HandleTitlePhaseAnimationFinish;
            }
            if (_context.MainUIManager != null)
            {
                _context.MainUIManager.OnReadyPhaseAnimationFinished += HandleReadyPhaseAnimationFinish;
                _context.MainUIManager.OnFinishPhaseAnimationFinished += HandleFinishPhaseAnimationFinish;
                _context.MainUIManager.OnFlashAnimationStarted += HandleFlashAnimationStart;
                _context.MainUIManager.OnFlashAnimationFinished += HandleFlashAnimationFinish;
                _context.MainUIManager.OnDieAnimationFinished += HandleDieAnimationFinish;
            }
            if (_context.ResultUIManager != null)
            {
                _context.ResultUIManager.OnResultPhaseAnimationFinished += HandleResultPhaseAnimationFinish;
            }

            // 購読完了フラグを更新
            _isSubscribed = true;
        }

        /// <summary>
        /// 購読済みのイベントをすべて解除する
        /// </summary>
        public void Dispose()
        {
            // 未購読状態であれば処理なし
            if (!_isSubscribed)
            {
                return;
            }

            // --------------------------------------------------
            // オブジェクト群
            // --------------------------------------------------
            if (_context.SceneObjectRegistry != null)
            {
                _context.SceneObjectRegistry.ObstacleManager.OnObstacleBreaked -= HandleDeactivatedObstacle;
            }
            
            // --------------------------------------------------
            // プレイヤー戦車
            // --------------------------------------------------
            if (_context.PlayerTank != null)
            {
                _context.PlayerTank.OnInputModeChangeButtonPressed -= HandleInputModeChangeButtonPressed;
                _context.PlayerTank.OnFireModeChangeButtonPressed -= HandleFireModeChangeButtonPressed;
                _context.PlayerTank.OnFireBullet -= HandleFireBullet;
                _context.PlayerTank.DurabilityManager.OnDurabilityChanged -= HandleDurabilityChanged;
                _context.PlayerTank.EnergyManager.OnFuelChanged -= HandleFuelChanged;
                _context.PlayerTank.EnergyManager.OnAmmoChanged -= HandleAmmoChanged;
                _context.PlayerTank.OnBroken -= HandleBroken;
            }

            // --------------------------------------------------
            // エネミー戦車
            // --------------------------------------------------
            if (_context.EnemyTanks != null)
            {
                for (int i = 0; i < _context.EnemyTanks.Length; i++)
                {
                    _context.EnemyTanks[i].OnFireBullet -= HandleFireBullet;
                    _context.EnemyTanks[i].OnBroken -= HandleBroken;
                }
            }

            // --------------------------------------------------
            // 弾丸プール
            // --------------------------------------------------
            if (_context.BulletPool != null)
            {
                _context.BulletPool.OnBulletSpawned -= HandleSpawnedBullet;
                _context.BulletPool.OnBulletDespawned -= HandleDespawnedBullet;
                _context.BulletPool.OnBulletExploded -= HandleBulletExplode;
            }

            // --------------------------------------------------
            // アイテムプール
            // --------------------------------------------------
            if (_context.ItemPool != null)
            {
                _context.ItemPool.OnItemActivated -= HandleActivatedItem;
                _context.ItemPool.OnItemDeactivated -= HandleDeactivatedItem;
            }

            // --------------------------------------------------
            // 衝突判定
            // --------------------------------------------------
            if (_context.CollisionManager != null)
            {
                _context.CollisionManager.EventRouter.OnBulletHit -= HandleHitBullet;
                _context.CollisionManager.EventRouter.OnItemGet -= HandleGetItem;
            }

            // --------------------------------------------------
            // UI
            // --------------------------------------------------
            if (_context.TitleUIManager != null)
            {
                _context.TitleUIManager.OnTitlePhaseAnimationFinished -= HandleTitlePhaseAnimationFinish;
            }
            if (_context.MainUIManager != null)
            {
                _context.MainUIManager.OnReadyPhaseAnimationFinished -= HandleReadyPhaseAnimationFinish;
                _context.MainUIManager.OnFinishPhaseAnimationFinished -= HandleFinishPhaseAnimationFinish;
                _context.MainUIManager.OnFlashAnimationStarted -= HandleFlashAnimationStart;
                _context.MainUIManager.OnFlashAnimationFinished -= HandleFlashAnimationFinish;
                _context.MainUIManager.OnDieAnimationFinished -= HandleDieAnimationFinish;
            }
            if (_context.ResultUIManager != null)
            {
                _context.ResultUIManager.OnResultPhaseAnimationFinished -= HandleResultPhaseAnimationFinish;
            }


            // 購読完了フラグを更新
            _isSubscribed = false;
        }

        // --------------------------------------------------
        // 入力
        // --------------------------------------------------
        /// <summary>
        /// オプションボタン押下時の処理を行うハンドラ
        /// SceneManager へフェーズ切り替え通知を行う
        /// </summary>
        public void HandleOptionButtonPressed()
        {
            // 現在適用中の入力マッピングインデックスを取得
            int current = InputManager.Instance.GetCurrentMappingIndex();

            // 次のインデックスを算出
            int next = (current == 0) ? 1 : 0;

            // 入力マッピングを切り替え
            InputManager.Instance.SetInputMapping(next);
        }

        // --------------------------------------------------
        // UI
        // --------------------------------------------------
        /// <summary>
        /// 経過時間と制限時間から残り時間を計算し、UI に表示する
        /// </summary>
        /// <param name="elapsedTime">現在までの経過時間（秒）</param>
        /// <param name="limitTime">制限時間（秒）</param>
        public void UpdateLimitTimeDisplay(in float elapsedTime, in float limitTime)
        {
            _context.MainUIManager?.UpdateLimitTimeDisplay(elapsedTime, limitTime);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // プレイヤー戦車
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
            _context.CameraManager?.SetTargetMode(cameraTargetIndex);
        }

        /// <summary>
        /// 攻撃モード切り替えボタン押下時の処理を行うハンドラ
        /// </summary>
        private void HandleFireModeChangeButtonPressed()
        {
            _context.MainUIManager?.UpdateBulletIcons();
        }

        /// <summary>
        /// 耐久値変更時の処理を行うハンドラ
        /// </summary>
        private void HandleDurabilityChanged()
        {
            _context.MainUIManager?.NotifyDurabilityChanged();
        }

        /// <summary>
        /// 燃料値変更時の処理を行うハンドラ
        /// </summary>
        private void HandleFuelChanged()
        {
            _context.MainUIManager?.NotifyFuelChanged();
        }

        /// <summary>
        /// 弾薬値変更時の処理を行うハンドラ
        /// </summary>
        private void HandleAmmoChanged()
        {
            _context.MainUIManager?.NotifyAmmoChanged();
        }

        // --------------------------------------------------
        // 戦車共通
        // --------------------------------------------------
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

            // 発射位置を設定
            Vector3 firePosition =
                tank.FirePoint.position;

            // 発射方向を設定
            Vector3 fireDirection =
                tank.FirePoint.forward;

            // 弾丸をプールから取り出し生成
            _context.BulletPool?.Spawn(
                bulletType,
                tank.TankId,
                tank.TankStatus,
                firePosition,
                fireDirection,
                target
            );

            tank.EnergyManager.ConsumeFuel();
            tank.EnergyManager.ConsumeAmmo();
        }

        /// <summary>
        /// 戦車破壊時の処理を行うハンドラ
        /// </summary>
        private void HandleBroken(int TankId)
        {
            _context.MainUIManager?.NotifyBrokenTanks(TankId);
            _context.CollisionManager?.UnregisterTank(TankId);
            _context.CameraManager?.SetTargetTransform(TankId);

            // プレイヤー戦車 ID の場合は処理なし
            if (TankId != 1)
            {
                ScoreManager.Instance.AddCumulativeScore();
            }
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
            _context.MainUIManager?.NotifyFireBullet();
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
        /// 指定した弾丸が衝突した際の処理を呼び出す
        /// </summary>
        /// <param name="bullet">衝突判定を行った弾丸インスタンス</param>
        /// <param name="context">衝突対象のコンテキスト情報</param>
        private void HandleHitBullet(BulletBase bullet, BaseCollisionContext context)
        {
            if (bullet == null)
            {
                return;
            }

            // 弾丸ヒット処理
            bullet?.OnHit(context);
        }

        /// <summary>
        /// 弾丸が爆発した際に呼び出され、
        /// 指定位置と半径に含まれる戦車コンテキストに対しダメージ処理を行う
        /// </summary>
        /// <param name="bullet">爆発した弾丸インスタンス</param>
        /// <param name="position">爆発中心位置</param>
        /// <param name="radius">爆発半径</param>
        private void HandleBulletExplode(ExplosiveBullet bullet, Vector3 position, float radius)
        {
            if (bullet == null)
            {
                return;
            }

            _overlapResults.Clear();

            // 爆発範囲内のコンテキストを取得
            List<BaseCollisionContext> overlappingContexts =
                _context.CollisionManager?.GetOverlappingCircleHorizontal(position, radius);

            if (overlappingContexts != null)
            {
                _overlapResults.AddRange(overlappingContexts);
            }

            // リストを配列に変換して渡す
            bullet?.ApplyExplodeDamage(_overlapResults.ToArray());
        }

        // --------------------------------------------------
        // 障害物
        // --------------------------------------------------
        /// <summary>
        /// 障害物を Scene 管理対象から解除する
        /// </summary>
        /// <param name="obstacle">解除対象の障害物</param>
        private void HandleDeactivatedObstacle(Transform obstacle)
        {
            if (obstacle == null)
            {
                return;
            }

            _context.SceneObjectRegistry?.UnregisterObstacle(obstacle);
            _context.CollisionManager?.UnregisterObstacle(obstacle);
        }
        
        // --------------------------------------------------
        // アイテム
        // --------------------------------------------------
        /// <summary>
        /// アイテムを Scene 管理対象として登録する
        /// </summary>
        /// <param name="item">登録対象のアイテムスロット</param>
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
        /// <param name="item">解除対象のアイテムスロット</param>
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
            // 無効化
            itemSlot.Deactivate();
            
            // パラメーターアイテム
            if (itemSlot.ItemData is ParamItemData param)
            {
                // 戦車のパラメーターを増減させる
                tankRootManager?.IncreaseParameter(
                    param.ParamType,
                    param.Value
                );
            }

            // 武装アイテム
            if (itemSlot.ItemData is WeaponItemData weapon)
            {
            }

            if (tankRootManager is PlayerTankRootManager)
            {
                _context.MainUIManager?.NotifyItemAcquired(itemSlot.ItemData.Name, itemSlot.ItemData.Type);

                ScoreManager.Instance.AddFixedScore();
            }
        }

        // --------------------------------------------------
        // UI
        // --------------------------------------------------
        /// <summary>
        /// Title フェーズアニメーション終了時に呼ばれる処理
        /// </summary>
        private void HandleTitlePhaseAnimationFinish()
        {
            OnPhaseChanged?.Invoke(PhaseType.Ready);
        }

        /// <summary>
        /// Ready フェーズアニメーション終了時に呼ばれる処理
        /// </summary>
        private void HandleReadyPhaseAnimationFinish()
        {
            OnPhaseChanged?.Invoke(PhaseType.Play);
        }

        /// <summary>
        /// Finish フェーズアニメーション終了時に呼ばれる処理
        /// </summary>
        private void HandleFinishPhaseAnimationFinish(float timeScale)
        {
            _context.SceneObjectRegistry?.ChangeTimeScale(timeScale);

            OnPhaseChanged?.Invoke(PhaseType.Result);
        }

        /// <summary>
        /// Result フェーズアニメーション終了時に呼ばれる処理
        /// </summary>
        private void HandleResultPhaseAnimationFinish()
        {
            OnPhaseChanged?.Invoke(PhaseType.Title);
        }

        /// <summary>
        /// フラッシュアニメーション開始時に呼ばれる処理
        /// </summary>
        /// <param name="timeScale">タイムスケール</param>
        private void HandleFlashAnimationStart(float timeScale)
        {
            _context.SceneObjectRegistry?.ChangeTimeScale(timeScale);
        }

        /// <summary>
        /// フラッシュアニメーション終了時に呼ばれる処理
        /// </summary>
        /// <param name="timeScale">タイムスケール</param>
        private void HandleFlashAnimationFinish(float timeScale)
        {
            _context.SceneObjectRegistry?.ChangeTimeScale(timeScale);
            _context.CameraManager?.SetTargetTransform();
        }

        /// <summary>
        /// 死亡アニメーション終了時に呼ばれる処理
        /// </summary>
        private void HandleDieAnimationFinish(float timeScale)
        {
            _context.SceneObjectRegistry?.ChangeTimeScale(timeScale);

            OnPhaseChanged?.Invoke(PhaseType.Result);
        }
    }
}