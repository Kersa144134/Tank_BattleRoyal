// ======================================================
// CameraManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-04
// 更新日時 : 2025-12-05
// 概要     : カメラ制御の統括クラス
// ======================================================

using UnityEngine;
using CameraSystem.Controller;
using SceneSystem.Interface;
using SceneSystem.Manager;

namespace CameraSystem.Manager
{
    /// <summary>
    /// カメラ制御の統括クラス
    /// </summary>
    public class CameraManager : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("追従設定")]
        /// <summary>カメラが追従するターゲット情報配列</summary>
        [SerializeField] private CameraTarget[] _cameraTargets;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>シーン上オブジェクトの Transform を一元管理するレジストリー</summary>
        private SceneObjectRegistry _sceneRegistry;

        /// <summary>カメラ追従ロジック</summary>
        private CameraFollowController _followController;

        // ======================================================
        // セッター
        // ======================================================

        /// <summary>
        /// シーン内オブジェクト管理用のレジストリー参照を設定する
        /// </summary>
        /// <param name="sceneRegistry">シーンに存在する各種オブジェクト情報を一元管理するレジストリー</param>
        public void SetSceneRegistry(SceneObjectRegistry sceneRegistry)
        {
            _sceneRegistry = sceneRegistry;
        }

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void OnEnter()
        {
            _followController = new CameraFollowController(_sceneRegistry.Camera, _cameraTargets, _sceneRegistry.Tanks[0]);
        }

        public void OnLateUpdate(in float unscaledDeltaTime)
        {
            float deltaTime = Time.deltaTime;

            // ターゲット追従
            _followController.UpdateFollow(deltaTime);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定されたインデックスの追従モードへ切り替える
        /// </summary>
        /// <param name="targetModeIndex">
        /// 設定する追従モードのインデックス
        /// </param>
        public void SetTargetByIndex(in int targetModeIndex)
        {
            int currentIndex = _followController.GetCurrentTargetModeIndex();

            if (currentIndex == targetModeIndex)
            {
                return;
            }

            _followController.SetTargetMode(targetModeIndex);
        }

        /// <summary>
        /// 追従対象の Transform を変更
        /// null の場合はプレイヤー Transform を設定する
        /// </summary>
        /// <param name="newTargetTransform">ターゲット Transform</param>
        public void SetTargetTransform(in Transform target = null)
        {
            // null の場合はプレイヤー Transform を対象とする
            Transform targetTransform = target ?? _sceneRegistry.Tanks[0];

            _followController.SetTargetTransform(targetTransform);
        }
    }
}