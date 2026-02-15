// ======================================================
// BulletPool.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-12
// 更新日時 : 2025-12-14
// 概要     : 弾丸の見た目とロジックを種類ごとに管理するオブジェクトプール
//            弾丸生成・有効化・更新・無効化を一括で扱う
//            更新処理は SceneObjectRegistry に委譲
//            戦車ごとのプールも生成可能
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using SceneSystem.Interface;
using SceneSystem.Manager;
using TankSystem.Data;
using TankSystem.Manager;
using WeaponSystem.Data;

namespace WeaponSystem.Manager
{
    /// <summary>
    /// 弾丸オブジェクトとロジックの管理を行うプールクラス
    /// </summary>
    public class BulletPool : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        /// <summary>弾丸プール設定を表すエントリークラス</summary>
        [Serializable] public class BulletPoolEntry
        {
            /// <summary>弾丸タイプ（識別用）</summary>
            public BulletType Type;

            /// <summary>弾丸を見た目として持つ Transform プレハブ</summary>
            public Transform Prefab;

            /// <summary>ロジック定義を持つ ScriptableObject</summary>
            public BulletData Data;

            /// <summary>初期生成する弾丸数</summary>
            public int InitialCount = 10;
        }

        /// <summary>各弾丸タイプのプレハブと設定リスト</summary>
        [SerializeField] private List<BulletPoolEntry> bulletEntries = new List<BulletPoolEntry>();

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>シーン上オブジェクトの Transform を一元管理するレジストリー</summary>
        private SceneObjectRegistry _sceneRegistry;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>戦車IDごとの使用中タイプ別弾丸リスト</summary>
        private readonly Dictionary<int, Dictionary<BulletType, List<BulletBase>>> _activePools
            = new Dictionary<int, Dictionary<BulletType, List<BulletBase>>>();

        /// <summary>戦車IDごとの未使用タイプ別弾丸キュー</summary>
        private readonly Dictionary<int, Dictionary<BulletType, Queue<BulletBase>>> _inactivePools
            = new Dictionary<int, Dictionary<BulletType, Queue<BulletBase>>>();

        // ======================================================
        // 定数
        // ======================================================

        // 戦車専用の弾丸ルートオブジェクトを生成
        private const string BULLET_ROOT_NAME = "_BulletRoot";

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// 弾丸を更新対象として登録通知するイベント
        /// </summary>
        public event Action<BulletBase> OnBulletSpawned;

        /// <summary>
        /// 弾丸を更新対象から解除通知するイベント
        /// </summary>
        public event Action<BulletBase> OnBulletDespawned;

        /// <summary>
        /// 弾丸が爆発したことを通知するイベント
        /// </summary>
        public event Action<ExplosiveBullet, Vector3, float> OnBulletExploded;

        // ======================================================
        // セッター
        // ======================================================

        /// <summary>
        /// シーン内オブジェクト管理用のレジストリー参照を設定する
        /// </summary>
        /// <param name="sceneRegistry">シーンに存在する各種オブジェクト情報を一元管理するレジストリー</param>
        public void SetSceneRegistry(SceneObjectRegistry sceneRegistry)
        {
            _sceneRegistry = sceneRegistry;
        }

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void OnEnter()
        {
            foreach (Transform tank in _sceneRegistry.Tanks)
            {
                if (!tank.TryGetComponent<BaseTankRootManager>(out BaseTankRootManager tankRoot))
                {
                    continue;
                }

                int tankId = tankRoot.TankId;

                // 戦車専用の弾丸ルートオブジェクトを生成
                GameObject bulletRootObject = new GameObject($"{tank.name}" + BULLET_ROOT_NAME);
                bulletRootObject.transform.SetParent(transform);
                Transform bulletRootTransform = bulletRootObject.transform;

                // 戦車ごとのプール辞書を初期化
                if (!_inactivePools.ContainsKey(tankId))
                {
                    _inactivePools[tankId] = new Dictionary<BulletType, Queue<BulletBase>>();
                    _activePools[tankId] = new Dictionary<BulletType, List<BulletBase>>();
                }

                foreach (BulletPoolEntry entry in bulletEntries)
                {
                    _inactivePools[tankId][entry.Type] = new Queue<BulletBase>();
                    _activePools[tankId][entry.Type] = new List<BulletBase>();

                    for (int i = 0; i < entry.InitialCount; i++)
                    {
                        CreateNewBullet(tankId, entry, bulletRootTransform);
                    }
                }
            }
        }

        public void OnExit()
        {
            // 戦車IDごとの使用プールを走査してイベント解除
            foreach (KeyValuePair<int, Dictionary<BulletType, List<BulletBase>>> tankActivePair in _activePools)
            {
                int tankId = tankActivePair.Key;
                Dictionary<BulletType, List<BulletBase>> typeDict = tankActivePair.Value;

                foreach (List<BulletBase> bulletList in typeDict.Values)
                {
                    foreach (BulletBase bullet in bulletList)
                    {
                        bullet.OnDespawnRequested -= Despawn;

                        if (bullet is ExplosiveBullet explosive)
                        {
                            explosive.OnExploded -= HandleExplodedBullet;
                        }
                    }
                }
            }

            // 戦車IDごとの未使用プールを走査してイベント解除
            foreach (KeyValuePair<int, Dictionary<BulletType, Queue<BulletBase>>> tankInactivePair in _inactivePools)
            {
                int tankId = tankInactivePair.Key;
                Dictionary<BulletType, Queue<BulletBase>> typeDict = tankInactivePair.Value;

                foreach (Queue<BulletBase> bulletQueue in typeDict.Values)
                {
                    foreach (BulletBase bullet in bulletQueue)
                    {
                        bullet.OnDespawnRequested -= Despawn;

                        if (bullet is ExplosiveBullet explosive)
                        {
                            explosive.OnExploded -= HandleExplodedBullet;
                        }
                    }
                }
            }

            // 戦車IDごとの辞書をクリア
            _activePools.Clear();
            _inactivePools.Clear();
        }

        // ======================================================
        // プールイベント
        // ======================================================

        /// <summary>
        /// 指定した弾丸タイプの弾丸をプールから取得し、
        /// 発射位置と進行方向を設定したうえで発射処理を行う
        /// </summary>
        /// <param name="type">発射する弾丸の種類</param>
        /// <param name="tankStatus">発射元の戦車の ID</param>
        /// <param name="tankStatus">発射元の戦車のパラメーター</param>
        /// <param name="position">弾丸を生成・発射するワールド座標</param>
        /// <param name="direction">弾丸の進行方向を表す正規化済みベクトル</param>
        /// <param name="target">弾丸の回転方向に指定するターゲット Transform</param>
        /// <returns>発射に成功した場合は使用中状態となった弾丸インスタンス</returns>
        public BulletBase Spawn(
            BulletType type,
            in int tankId,
            in TankStatus tankStatus,
            in Vector3 position,
            in Vector3 direction,
            in Transform target = null)
        {
            BulletPoolEntry entry = bulletEntries.Find(e => e.Type == type);
            if (entry == null) return null;

            // tankId に応じた未使用プールを取得
            if (!_inactivePools.TryGetValue(tankId, out Dictionary<BulletType, Queue<BulletBase>> tankPool) ||
                !tankPool.TryGetValue(type, out Queue<BulletBase> targetPool) ||
                targetPool.Count == 0)
            {
                return null;
            }

            // 弾丸取得
            BulletBase bullet = targetPool.Dequeue();

            // 未使用の弾丸が存在しなければ発射中止
            if (bullet == null)
            {
                return null;
            }

            // 発射位置を設定
            bullet.SetSpawnPosition(position);

            // 弾丸タイプごとの追加パラメータをセット
            if (bullet is ExplosiveBullet explosive)
            {
                explosive.SetParams(explosive.ExplosiveRadius);
            }
            if (bullet is PenetrationBullet penetration)
            {
                penetration.SetParams(penetration.PenetrationSpeed);
            }
            if (bullet is HomingBullet homing)
            {
                homing.SetParams(target, homing.RotateSpeed);
            }

            // 戦車ステータスを反映
            bullet.ApplyTankStatus(tankStatus);

            // 発射
            bullet.OnEnter(position, direction);

            // プール管理
            _activePools[tankId][type].Add(bullet);
            _inactivePools[tankId][type].Enqueue(bullet);

            // 弾丸生成イベントを通知する
            OnBulletSpawned?.Invoke(bullet);

            return bullet;
        }

        /// <summary>
        /// 使用が終了した弾丸を非アクティブ状態へ遷移させ、管理対象のプールへ戻す処理を行う
        /// </summary>
        /// <param name="bullet">非アクティブ化してプールへ戻す対象の弾丸インスタンス</param>
        public void Despawn(BulletBase bullet)
        {
            BulletType type = bullet switch
            {
                ExplosiveBullet _ => BulletType.Explosive,
                PenetrationBullet _ => BulletType.Penetration,
                HomingBullet _ => BulletType.Homing,
                _ => BulletType.Explosive
            };

            int tankId = bullet.BulletId;

            // アクティブプールから削除
            if (_activePools.TryGetValue(tankId, out Dictionary<BulletType, List<BulletBase>> activeDict) &&
                activeDict.TryGetValue(type, out List<BulletBase> activeList))
            {
                activeList.Remove(bullet);
            }

            // 非アクティブプールへ戻す
            if (_inactivePools.TryGetValue(tankId, out Dictionary<BulletType, Queue<BulletBase>> inactiveDict) &&
                inactiveDict.TryGetValue(type, out Queue<BulletBase> inactiveQueue))
            {
                inactiveQueue.Enqueue(bullet);
            }

            // 弾丸破棄イベントを通知する
            OnBulletDespawned?.Invoke(bullet);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 新しい弾丸インスタンスを生成し、未使用状態としてプールへ登録する
        /// 生成された弾丸は指定した親オブジェクト配下に配置する
        /// </summary>
        /// <param name="entry">生成対象となる弾丸のプール定義情報</param>
        /// <param name="parent">生成される弾丸オブジェクトの親となる Transform</param>
        /// <returns>プール登録が完了した弾丸インスタンス</returns>
        private BulletBase CreateNewBullet(in int tankId, in BulletPoolEntry entry, in Transform parent)
        {
            // 親を戦車ルートに指定
            Transform bulletTransform = Instantiate(entry.Prefab, parent);

            // ScriptableObject からロジックインスタンスを生成
            BulletBase bullet = entry.Data.CreateInstance();

            // Transform を渡して初期化
            bullet.Initialize(tankId, bulletTransform);

            // イベント購読
            bullet.OnDespawnRequested += Despawn;
            if (bullet is ExplosiveBullet explosive)
            {
                explosive.OnExploded += HandleExplodedBullet;
            }

            // 未使用リストへ追加
            _inactivePools[bullet.BulletId][entry.Type].Enqueue(bullet);

            return bullet;
        }

        /// <summary>
        /// 弾丸爆発時の処理を行うハンドラ
        /// </summary>
        /// <param name="bullet">ExplosiveBullet インスタンス</param>
        /// <param name="position">爆発の中心位置</param>
        /// <param name="radius">爆発の半径</param>
        private void HandleExplodedBullet(
            ExplosiveBullet bullet,
            Vector3 position,
            float radius
        )
        {
            OnBulletExploded?.Invoke(bullet, position, radius);
        }
    }
}