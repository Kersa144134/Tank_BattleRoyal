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

        [Header("コンポーネント参照")]
        /// <summary>シーン上のオブジェクト Transform を保持するレジストリー</summary>
        [SerializeField] private SceneObjectRegistry _sceneRegistry;

        [Header("戦車オブジェクト")]
        /// <summary>プレイヤー戦車や敵戦車の GameObject 配列。TankRootManager 派生がアタッチされている場合、戦車ごとにプールを生成</summary>
        [SerializeField] private GameObject[] _tankObjects;

        [Serializable]
        public class BulletPoolEntry
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
        // 辞書
        // ======================================================

        /// <summary>使用中の弾丸を種類ごとに保持</summary>
        private readonly Dictionary<BulletType, List<BulletBase>> _activePool = new Dictionary<BulletType, List<BulletBase>>();

        /// <summary>未使用の弾丸を種類ごとに保持</summary>
        private readonly Dictionary<BulletType, List<BulletBase>> _inactivePool = new Dictionary<BulletType, List<BulletBase>>();

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void OnEnter()
        {
            // 戦車オブジェクトごとにプール初期化を行う
            foreach (GameObject tankObj in _tankObjects)
            {
                // 戦車ルートであることを確認（弾丸プール生成対象かの判定）
                if (!tankObj.TryGetComponent<BaseTankRootManager>(out BaseTankRootManager _))
                {
                    // 戦車でないオブジェクトはスキップ
                    continue;
                }

                // 戦車専用の弾丸ルートオブジェクトを生成
                GameObject bulletRootObject = new GameObject($"{tankObj.name}_BulletRoot");

                // BulletPool 配下にまとめて配置（Hierarchy 整理用）
                bulletRootObject.transform.SetParent(transform);

                // 弾丸生成時に使用する Transform を取得
                Transform bulletRootTransform = bulletRootObject.transform;

                // 弾丸タイプごとにプールを初期化
                foreach (BulletPoolEntry entry in bulletEntries)
                {
                    // 未使用プールが未生成の場合は作成
                    if (!_inactivePool.ContainsKey(entry.Type))
                    {
                        _inactivePool[entry.Type] = new List<BulletBase>();
                    }

                    // 使用中プールが未生成の場合は作成
                    if (!_activePool.ContainsKey(entry.Type))
                    {
                        _activePool[entry.Type] = new List<BulletBase>();
                    }

                    // 初期生成数ぶんだけ弾丸を生成し、戦車専用ルート配下に配置
                    for (int i = 0; i < entry.InitialCount; i++)
                    {
                        CreateNewBullet(entry, bulletRootTransform);
                    }
                }
            }
        }

        public void OnExit()
        {
            foreach (List<BulletBase> list in _activePool.Values)
            {
                foreach (BulletBase bullet in list)
                {
                    bullet.OnDespawnRequested -= Despawn;
                }
            }

            foreach (List<BulletBase> list in _inactivePool.Values)
            {
                foreach (BulletBase bullet in list)
                {
                    bullet.OnDespawnRequested -= Despawn;
                }
            }
        }

        // ======================================================
        // プールイベント
        // ======================================================

        /// <summary>
        /// 指定した弾丸タイプの弾丸をプールから取得し、
        /// 発射位置と進行方向を設定したうえで発射処理を行う
        /// </summary>
        /// <param name="type">発射する弾丸の種類</param>
        /// <param name="position">弾丸を生成・発射するワールド座標</param>
        /// <param name="direction">弾丸の進行方向を表す正規化済みベクトル</param>
        /// <returns>発射に成功した場合は使用中状態となった弾丸インスタンス</returns>
        public BulletBase Spawn(BulletType type, in Vector3 position, in Vector3 direction)
        {
            BulletPoolEntry entry = bulletEntries.Find(e => e.Type == type);
            if (entry == null)
            {
                Debug.LogError($"[BulletPool] BulletType {type} の BulletPoolEntry が未設定です。");
                return null;
            }

            // 未使用の弾丸を IsEnabled で検索
            BulletBase bullet = _inactivePool[type].Find(b => !b.IsEnabled);

            // 未使用の弾丸が存在しなければ発射中止
            if (bullet == null)
            {
                Debug.LogWarning($"[BulletPool] 弾丸発射中止: {type} (未使用弾丸なし)");
                return null;
            }

            // 発射位置を設定
            bullet.SetSpawnPosition(position);

            // 弾丸タイプごとの追加パラメータをセット
            if (bullet is ExplosiveBullet explosive)
            {
                // direction は Spawn 引数をそのまま使用
                explosive.SetParams(explosive.ExplosiveRadius);
            }

            // 発射
            bullet.OnEnter(position, direction);

            // プール管理
            _inactivePool[type].Remove(bullet);
            _activePool[type].Add(bullet);

            // SceneObjectRegistry に登録して更新委譲
            _sceneRegistry.RegisterBullet(bullet);

            return bullet;
        }

        /// <summary>
        /// 使用が終了した弾丸を非アクティブ状態へ遷移させ、管理対象のプールへ戻す処理を行う
        /// </summary>
        /// <param name="bullet">非アクティブ化してプールへ戻す対象の弾丸インスタンス</param>
        public void Despawn(BulletBase bullet)
        {
            // 弾丸のタイプを取得
            BulletType type = bullet switch
            {
                ExplosiveBullet _ => BulletType.Explosive,
                PenetrationBullet _ => BulletType.Penetration,
                HomingBullet _ => BulletType.Homing,
                _ => throw new Exception("[BulletPool] 無効な弾丸タイプです。")
            };

            // アクティブプールから削除
            if (_activePool.TryGetValue(type, out List<BulletBase> activeList))
            {
                activeList.Remove(bullet);
            }

            // 非アクティブプールへ戻す
            if (_inactivePool.TryGetValue(type, out List<BulletBase> inactiveList))
            {
                inactiveList.Add(bullet);
            }

            // SceneObjectRegistry から登録解除
            _sceneRegistry.UnregisterBullet(bullet);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 指定された弾丸定義エントリと親 Transform を基に、
        /// 新しい弾丸インスタンスを生成し、未使用状態としてプールへ登録する
        /// 生成された弾丸は指定した親オブジェクト配下に配置される
        /// </summary>
        /// <param name="entry">生成対象となる弾丸のプール定義情報</param>
        /// <param name="parent">生成される弾丸オブジェクトの親となる Transform</param>
        /// <returns>プール登録が完了した弾丸インスタンス</returns>
        private BulletBase CreateNewBullet(in BulletPoolEntry entry, in Transform parent)
        {
            // 親を戦車ルートに指定
            Transform bulletTransform = Instantiate(entry.Prefab, parent);

            // ScriptableObject からロジックインスタンスを生成
            BulletBase bulletBase = entry.Data.CreateInstance();

            // Transform を渡して初期化
            bulletBase.Initialize(bulletTransform);

            // デスポーン要求イベントを購読
            bulletBase.OnDespawnRequested += Despawn;

            // 未使用リストへ追加
            _inactivePool[entry.Type].Add(bulletBase);

            return bulletBase;
        }
    }
}