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

using SceneSystem.Interface;
using System;
using System.Collections.Generic;
using TankSystem.Manager;
using UnityEngine;
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
            // 戦車ごとのプール生成
            foreach (GameObject tankObj in _tankObjects)
            {
                if (tankObj.TryGetComponent<BaseTankRootManager>(out BaseTankRootManager _))
                {
                    foreach (BulletPoolEntry entry in bulletEntries)
                    {
                        // 種類ごとの未使用リストを作成
                        if (!_inactivePool.ContainsKey(entry.Type))
                        {
                            _inactivePool[entry.Type] = new List<BulletBase>();
                        }

                        // 種類ごとの使用中リストを作成
                        if (!_activePool.ContainsKey(entry.Type))
                        {
                            _activePool[entry.Type] = new List<BulletBase>();
                        }

                        // 初期数だけ弾丸を生成
                        for (int i = 0; i < entry.InitialCount; i++)
                        {
                            CreateNewBullet(entry);
                        }
                    }
                }
            }
        }

        // ======================================================
        // プールイベント
        // ======================================================

        /// <summary>
        /// 指定した弾丸タイプを発射し、位置と方向を設定する
        /// 未使用の弾丸が存在しない場合は発射を中止する
        /// </summary>
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
        /// 使用後の弾丸を非アクティブ化し、プールへ戻す
        /// </summary>
        public void Despawn(in BulletBase bullet)
        {
            // 弾丸を無効化
            bullet.OnExit();

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
        /// プレハブを元に新しい弾丸を生成し、プールへ追加する
        /// </summary>
        private BulletBase CreateNewBullet(in BulletPoolEntry entry)
        {
            // 弾丸の Transform を生成
            Transform bulletTransform = Instantiate(entry.Prefab, transform);

            // ScriptableObject からロジックインスタンスを生成
            BulletBase bulletBase = entry.Data.CreateInstance();

            // Transform を渡して初期化
            bulletBase.Initialize(bulletTransform);

            // 未使用リストへ追加
            _inactivePool[entry.Type].Add(bulletBase);

            return bulletBase;
        }
    }
}