// ======================================================
// CameraManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-04
// 更新日時 : 2025-12-05
// 概要     : カメラ制御の統括クラス
//            追従対象の配列を管理し、追従クラスに渡す
//            入力で追従ターゲットを切り替え可能
// ======================================================

using UnityEngine;
using CameraSystem.Controller;
using InputSystem.Manager;
using SceneSystem.Interface;

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
            _followController = new CameraFollowController(cameraTransform, cameraTargets);
        }

        public void OnUpdate()
        {
            
        }

        public void OnLateUpdate()
        {
            // ターゲット追従
            _followController.UpdateFollow();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定されたインデックスのターゲットへ切り替える
        /// </summary>
        /// <param name="targetIndex">
        /// 設定するターゲットのインデックス
        /// </param>
        public void SetTargetByIndex(int targetIndex)
        {
            // 現在設定されているターゲットインデックスを取得
            int currentIndex = _followController.GetCurrentTargetIndex();

            // 同一ターゲットが指定された場合は解除（未選択状態へ戻す）
            if (currentIndex == targetIndex)
            {
                _followController.SetTarget(0);
                return;
            }

            // 指定されたインデックスのターゲットを設定
            _followController.SetTarget(targetIndex);
        }
    }
}