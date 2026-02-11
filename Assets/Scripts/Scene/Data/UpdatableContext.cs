// ======================================================
// UpdatableContext.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : 初期化済み参照をまとめて保持するコンテキスト
// ======================================================

using CameraSystem.Manager;
using CollisionSystem.Manager;
using ItemSystem.Manager;
using SceneSystem.Interface;
using SceneSystem.Manager;
using TankSystem.Manager;
using UISystem.Manager;
using WeaponSystem.Manager;

namespace SceneSystem.Data
{
    /// <summary>
    /// 初期化済み参照を保持するデータコンテキスト
    /// </summary>
    public sealed class UpdatableContext
    {
        /// <summary>シーン内の全 IUpdatable</summary>
        public IUpdatable[] Updatables;

        /// <summary>シーン内のオブジェクトを一元管理するレジストリー</summary>
        public SceneObjectRegistry SceneObjectRegistry;

        /// <summary>弾丸プール</summary>
        public BulletPool BulletPool;

        /// <summary>カメラ管理</summary>
        public CameraManager CameraManager;

        /// <summary>カメラ管理</summary>
        public CollisionManager CollisionManager;

        /// <summary>アイテムプール</summary>
        public ItemPool ItemPool;

        /// <summary>タイトル UI 管理</summary>
        public TitleUIManager TitleUIManager;

        /// <summary>メイン UI 管理</summary>
        public MainUIManager MainUIManager;

        /// <summary>リザルト UI 管理</summary>
        public ResultUIManager ResultUIManager;

        /// <summary>プレイヤー戦車</summary>
        public PlayerTankRootManager PlayerTank;

        /// <summary>エネミー戦車配列</summary>
        public EnemyTankRootManager[] EnemyTanks;
    }
}