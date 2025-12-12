// ======================================================
// BulletPool.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-12
// 更新日時 : 2025-12-12
// 概要     : 弾丸の見た目とロジックを種類ごとに管理するオブジェクトプール
//            弾丸生成・有効化・更新・無効化を一括で扱う
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using SceneSystem.Interface;
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

        [System.Serializable]
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
        // プール管理コンテナ
        // ======================================================

        /// <summary>未使用の弾丸を種類ごとに保持</summary>
        private readonly Dictionary<BulletType, List<BulletBase>> _inactivePool = new Dictionary<BulletType, List<BulletBase>>();

        /// <summary>使用中の弾丸を種類ごとに保持</summary>
        private readonly Dictionary<BulletType, List<BulletBase>> _activePool = new Dictionary<BulletType, List<BulletBase>>();

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void OnEnter()
        {
            // すべての BulletPoolEntry を初期化
            foreach (BulletPoolEntry entry in bulletEntries)
            {
                // 種類ごとの未使用リストを作成
                _inactivePool[entry.Type] = new List<BulletBase>();

                // 種類ごとの使用中リストを作成
                _activePool[entry.Type] = new List<BulletBase>();

                // 初期数だけ弾丸を生成
                for (int i = 0; i < entry.InitialCount; i++)
                {
                    CreateNewBullet(entry);
                }
            }
        }

        public void OnUpdate()
        {
            // 経過時間を取得
            float deltaTime = Time.deltaTime;

            // すべての弾丸タイプを処理
            foreach (KeyValuePair<BulletType, List<BulletBase>> pair in _activePool)
            {
                // 種類識別
                BulletType type = pair.Key;

                // 種類ごとのアクティブリストを取得
                List<BulletBase> list = pair.Value;

                // 逆順で更新（Remove による崩れ防止）
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    BulletBase bullet = list[i];

                    // 無効弾や null はスキップ
                    if (bullet == null || bullet.IsEnabled == false)
                    {
                        continue;
                    }

                    // ロジックの更新処理
                    bullet.OnUpdate(deltaTime);

                    // 更新後に終了状態になった場合はプールへ戻す
                    if (bullet.IsEnabled == false)
                    {
                        Despawn(bullet);
                    }
                }
            }
        }

        public void OnLateUpdate()
        {

        }

        public void OnExit()
        {
            
        }

        public void OnPhaseEnter()
        {

        }

        public void OnPhaseExit()
        {

        }

        // ======================================================
        // プールイベント
        // ======================================================

        /// <summary>
        /// 指定した弾丸タイプを発射し、位置と方向を設定する
        /// </summary>
        public BulletBase Spawn(BulletType type, Vector3 position, Vector3 direction)
        {
            BulletPoolEntry entry = bulletEntries.Find(e => e.Type == type);
            if (entry == null)
            {
                Debug.LogError($"BulletType {type} の BulletPoolEntry が未設定です。");
                return null;
            }

            // 未使用の弾丸を IsEnabled で検索
            BulletBase bullet = _inactivePool[type].Find(b => !b.IsEnabled);

            // 無ければ新規生成
            if (bullet == null)
            {
                bullet = CreateNewBullet(entry);
            }

            // 発射位置を設定
            bullet.SetSpawnPosition(position);

            // 発射
            bullet.OnEnter(direction);

            return bullet;
        }

        /// <summary>
        /// 使用後の弾丸を非アクティブ化し、プールへ戻す
        /// </summary>
        private void Despawn(BulletBase bullet)
        {
            // 弾丸を無効化
            bullet.OnExit();
        }
        
        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// プレハブを元に新しい弾丸を生成し、プールへ追加する
        /// </summary>
        private BulletBase CreateNewBullet(BulletPoolEntry entry)
        {
            // 弾丸の Transform を生成
            Transform bulletTransform = Instantiate(entry.Prefab, this.transform);

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