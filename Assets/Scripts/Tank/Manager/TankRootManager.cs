// ======================================================
// TankRootManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-04
// 更新日時 : 2025-12-05
// 概要     : 戦車の各種制御を統合管理する
// ======================================================

using InputSystem.Data;
using SceneSystem.Interface;
using System;
using TankSystem.Controller;
using TankSystem.Data;
using TankSystem.Service;
using TankSystem.Utility;
using UnityEngine;

namespace TankSystem.Manager
{
    /// <summary>
    /// 戦車の各種制御を統括するクラス
    /// </summary>
    public class TankRootManager : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        /// <summary>戦車本体の当たり判定中心位置</summary>
        [SerializeField] private Vector3 _hitboxCenter;

        /// <summary>戦車本体の当たり判定スケール</summary>
        [SerializeField] private Vector3 _hitboxSize;

        /// <summary>障害物オブジェクトの Transform 配列</summary>
        [SerializeField] private Transform[] _obstacles;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        // --------------------------------------------------
        // パラメーター
        // --------------------------------------------------

        // --------------------------------------------------
        // 攻撃力
        // --------------------------------------------------
        /// <summary>戦車の攻撃管理クラス</summary>
        private TankAttackManager _attackManager;

        // --------------------------------------------------
        // 防御力
        // --------------------------------------------------

        // --------------------------------------------------
        // 機動力
        // --------------------------------------------------
        /// <summary>戦車の機動管理クラス</summary>
        private TankMobilityManager _mobilityManager;

        /// <summary>左右キャタピラ入力から前進量・旋回量を算出するコントローラ</summary>
        private TankTrackController _trackController = new TankTrackController();

        /// <summary>AABB の距離判定による衝突チェックを行うコントローラ</summary>
        private AABBCollisionController _aabbCollisionController = new AABBCollisionController();

        /// <summary>OBB から AABBを計算して生成するためのファクトリー</summary>
        private AABBFactory _aabbFactory = new AABBFactory();

        // --------------------------------------------------
        // 入力
        // --------------------------------------------------
        /// <summary>戦車の入力管理クラス</summary>
        private TankInputManager _inputManager = new TankInputManager();

        // --------------------------------------------------
        // サービス
        // --------------------------------------------------
        /// <summary>戦車当たり判定サービス</summary>
        private TankCollisionService _collisionService;

        // ======================================================
        // フィールド
        // ======================================================

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// オプションボタン押下時に発火するイベント
        /// 外部から登録してUIやサウンド処理などを接続可能
        /// </summary>
        public event Action OnOptionButtonPressed;
        
        // ======================================================
        // IUpdatableイベント
        // ======================================================

        public void OnEnter()
        {
            _attackManager = new TankAttackManager(transform);

            _collisionService = new TankCollisionService(
                transform,
                _hitboxCenter,
                _hitboxSize,
                _obstacles,
                _aabbFactory,
                _aabbCollisionController
            );

            _mobilityManager = new TankMobilityManager(
                _trackController,
                _collisionService,
                transform,
                _hitboxCenter,
                _hitboxSize,
                _obstacles
            );
        }

        public void OnUpdate()
        {
            // --------------------------------------------------
            // 入力
            // --------------------------------------------------
            // 入力取得
            _inputManager.UpdateInput();

            // 入力変換
            float leftMobility = _inputManager.LeftStick.y;
            float rightMobility = _inputManager.RightStick.y;

            // --------------------------------------------------
            // オプション
            // --------------------------------------------------
            if (_inputManager.GetButton(TankInputKeys.INPUT_OPTION)?.Down == true)
            {
                // オプションイベントを発火
                OnOptionButtonPressed?.Invoke();
            }

            // --------------------------------------------------
            // 攻撃
            // --------------------------------------------------
            // 辞書から攻撃ボタンを取得して更新
            ButtonState heButton = _inputManager.GetButton(TankInputKeys.INPUT_HE_FIRE);
            ButtonState apButton = _inputManager.GetButton(TankInputKeys.INPUT_AP_FIRE);

            // 攻撃処理
            _attackManager.UpdateAttack(heButton, apButton);

            // --------------------------------------------------
            // 機動
            // --------------------------------------------------
            // 前進・旋回処理
            _mobilityManager.ApplyMobility(leftMobility, rightMobility);

            
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
    }
}