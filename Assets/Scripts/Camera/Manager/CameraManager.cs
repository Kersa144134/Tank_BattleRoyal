// ======================================================
// CameraManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-04
// 更新日時 : 2025-12-05
// 概要     : カメラ制御の統括クラス
//            追従対象の配列を管理し、追従クラスに渡す
//            入力で追従ターゲットを切り替え可能
// ======================================================

using CameraSystem.Controller;
using InputSystem.Manager;
using SceneSystem.Interface;
using UnityEngine;

namespace CameraSystem.Manager
{
    /// <summary>
    /// 複数ターゲットを管理し、CameraFollow に渡す役割を持つ
    /// </summary>
    public class CameraManager : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("追従設定")]
        [Tooltip("カメラTransform")]
        [SerializeField] private Transform cameraTransform;

        [Tooltip("カメラが追従するターゲット情報配列")]
        [SerializeField] private CameraTarget[] cameraTargets;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>カメラ追従ロジック</summary>
        private CameraFollowController _followController;

        // ======================================================
        // IUpdatableイベント
        // ======================================================

        public void OnEnter()
        {
            // CameraFollowController クラスを生成
            _followController = new CameraFollowController();

            // カメラ座標とターゲット配列を渡す
            _followController.Initialize(cameraTransform, cameraTargets);
        }

        public void OnUpdate()
        {
            // ターゲット切替
            CheckTargetSwitchInput();
        }

        public void OnLateUpdate()
        {
            // ターゲット追従
            _followController.UpdateFollow();
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
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 入力によるターゲット切替処理
        /// <summary>
        private void CheckTargetSwitchInput()
        {
            // 左スティックボタンでターゲット 1 切替
            if (InputManager.Instance.LeftStickButton.Down)
            {
                int current = _followController.GetCurrentTargetIndex();

                // トグル処理
                int nextIndex = (current == 1) ? 0 : 1;
                _followController.SetTarget(nextIndex);
            }

            // 右スティックボタンでターゲット 2 切替
            if (InputManager.Instance.RightStickButton.Down)
            {
                int current = _followController.GetCurrentTargetIndex();

                // トグル処理
                int nextIndex = (current == 2) ? 0 : 2;
                _followController.SetTarget(nextIndex);
            }
        }
    }
}