// ======================================================
// CameraManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-04
// 更新日時 : 2025-12-04
// 概要     : カメラ制御の統括クラス
//            追従対象の配列を管理し、追従クラスに渡す
// ======================================================

using UnityEngine;
using CameraSystem.Controller;

namespace CameraSystem.Manager
{
    /// <summary>
    /// 複数ターゲットを管理し、CameraFollow に渡す役割を持つ
    /// </summary>
    public class CameraManager : MonoBehaviour
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
        // Unityイベント
        // ======================================================

        private void Awake()
        {
            // CameraFollowController クラスを生成
            _followController = new CameraFollowController();

            // 配列を渡す
            _followController.Initialize(cameraTransform, cameraTargets);
        }

        private void LateUpdate()
        {
            _followController.UpdateFollow();
        }
    }
}