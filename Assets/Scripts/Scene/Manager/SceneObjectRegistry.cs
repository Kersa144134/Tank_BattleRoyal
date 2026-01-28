// ======================================================
// SceneObjectRegistry.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-12
// 更新日時 : 2025-12-12
// 概要     : シーン上の戦車・障害物・アイテムを一元管理するレジストリクラス
// ======================================================

using System;
using UnityEngine;
using ItemSystem.Data;
using SceneSystem.Data;
using SceneSystem.Interface;
using TankSystem.Manager;
using WeaponSystem.Data;
using WeaponSystem.Manager;

namespace SceneSystem.Manager
{
    /// <summary>
    /// シーン内の当たり判定対象オブジェクトを一元管理するレジストリ
    /// </summary>
    public class SceneObjectRegistry : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("メインカメラ")]
        /// <summary>メインカメラ</summary>
        [SerializeField] private Transform _mainCamera;

        [Header("戦車")]
        /// <summary>戦車の Transform 配列</summary>
        [SerializeField] private Transform[] _tanks;

        [Header("障害物")]
        /// <summary>障害物の親 Transform</summary>
        [SerializeField] private Transform _obstacleRoot;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>弾丸管理マネージャー</summary>
        private BulletManager _bulletManager = new BulletManager();

        /// <summary>アイテム管理担当マネージャー</summary>
        private ItemManager _itemManager;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>障害物オブジェクトの Transform 配列</summary>
        private Transform[] _obstacles;

        /// <summary>ゲームの経過時間</summary>
        private float _elapsedTime;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// プレイヤー戦車 Transform を返す
        /// </summary>
        public Transform[] Tanks => _tanks;

        /// <summary>
        /// 障害物 Transform 配列を返す
        /// </summary>
        public Transform[] Obstacles => _obstacles;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>通常時のタイムスケール</summary>
        private const float DEFAULT_TIME_SCALE = 1.0f;

        /// <summary>Finish フェーズ中のタイムスケール</summary>
        private const float FINISH_PHASE_TIME_SCALE = 0.25f;

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void OnEnter()
        {
            _itemManager = new ItemManager(_mainCamera);

            InitializeTanks();
            InitializeObstacles();
        }

        public void OnUpdate(in float unscaledDeltaTime, in float elapsedTime)
        {
            float deltaTime = Time.deltaTime;
            _elapsedTime = elapsedTime;

            _bulletManager.UpdateBullets(deltaTime);
            _itemManager.UpdateItems(elapsedTime);
        }

        public void OnLateUpdate(in float unscaledDeltaTime)
        {
            _itemManager.DeactivateItems();
        }

        public void OnPhaseEnter(in PhaseType phase)
        {
            if (phase == PhaseType.Finish)
            {
                ChangeTimeScale(FINISH_PHASE_TIME_SCALE);
            }
        }

        public void OnPhaseExit(in PhaseType phase)
        {
            if (phase == PhaseType.Finish)
            {
                ChangeTimeScale(DEFAULT_TIME_SCALE);
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 弾丸を更新対象として登録する
        /// </summary>
        /// <param name="bullet">登録する弾丸</param>
        public void RegisterBullet(in BulletBase bullet)
        {
            _bulletManager.RegisterBullet(bullet);
        }

        /// <summary>
        /// 弾丸を更新対象から解除する
        /// </summary>
        /// <param name="bullet">解除する弾丸</param>
        public void UnregisterBullet(in BulletBase bullet)
        {
            _bulletManager.UnregisterBullet(bullet);
        }

        /// <summary>
        /// アイテムスロットを更新対象として登録する
        /// </summary>
        /// <param name="slot">追加するスロット</param>
        public void RegisterItem(in ItemSlot slot)
        {
            _itemManager.RegisterItem(slot, _elapsedTime);
        }

        /// <summary>
        /// アイテムスロットを更新対象から解除する
        /// </summary>
        /// <param name="slot">削除するスロット</param>
        public void UnregisterItem(in ItemSlot slot)
        {
            _itemManager.UnregisterItem(slot);
        }

        /// <summary>
        /// シーン上のオブジェクトに影響するタイムスケールを変更する
        /// </summary>
        /// <param name="timeScale">変更後のタイムスケール</param>
        public void ChangeTimeScale(in float timeScale)
        {
            Time.timeScale = timeScale;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 戦車 Transform 配列を初期化し、TankId を自動割り当てする
        /// </summary>
        private void InitializeTanks()
        {
            if (_tanks == null || _tanks.Length == 0)
            {
                Debug.LogWarning("[SceneObjectRegistry] 戦車 Transform 配列が未設定です。");
                return;
            }

            // 戦車ごとに TankId を 1 から順に設定
            for (int i = 0; i < _tanks.Length; i++)
            {
                Transform tankTransform = _tanks[i];
                if (tankTransform == null)
                {
                    continue;
                }

                // TankRootManager 取得
                BaseTankRootManager tankManager = tankTransform.GetComponent<BaseTankRootManager>();
                if (tankManager == null)
                {
                    Debug.LogWarning($"[SceneObjectRegistry] {_tanks[i].name} に BaseTankRootManager がアタッチされていません。");
                    continue;
                }

                // TankId を設定（1スタート）
                tankManager.TankId = i + 1;
            }
        }
        
        /// <summary>
        /// シーン内の障害物を親オブジェクトから取得して配列に格納
        /// </summary>
        private void InitializeObstacles()
        {
            if (_obstacleRoot == null)
            {
                _obstacles = Array.Empty<Transform>();
                Debug.LogWarning("[SceneObjectRegistry] 障害物親オブジェクトが未設定です。");
                return;
            }

            // 親オブジェクトの子すべてを取得
            int count = _obstacleRoot.childCount;
            _obstacles = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                _obstacles[i] = _obstacleRoot.GetChild(i);
            }
        }
    }
}