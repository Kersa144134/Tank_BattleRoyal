// ======================================================
// UpdatableContext.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : 初期化済み参照をまとめて保持するコンテキスト
// ======================================================

using SceneSystem.Interface;
using CameraSystem.Manager;
using InputSystem.Manager;
using TankSystem.Manager;
using WeaponSystem.Manager;

namespace SceneSystem.Data
{
    /// <summary>
    /// UpdatableBootstrapper により生成される
    /// 初期化済み参照を保持するデータコンテナ
    /// </summary>
    public sealed class UpdatableContext
    {
        /// <summary>シーン内の全 IUpdatable</summary>
        public IUpdatable[] Updatables;

        /// <summary>弾丸プール</summary>
        public BulletPool BulletPool;

        /// <summary>カメラ管理</summary>
        public CameraManager CameraManager;

        /// <summary>入力管理</summary>
        public InputManager InputManager;

        /// <summary>カメラ管理</summary>
        public TankCollisionManager TankCollisionManager;

        /// <summary>プレイヤー戦車</summary>
        public PlayerTankRootManager PlayerTank;

        /// <summary>エネミー戦車配列</summary>
        public EnemyTankRootManager[] EnemyTanks;
    }
}