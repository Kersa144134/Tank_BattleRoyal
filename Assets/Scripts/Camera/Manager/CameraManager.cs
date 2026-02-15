// ======================================================
// CameraManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-04
// 更新日時 : 2025-12-05
// 概要     : カメラ制御の統括クラス
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using CameraSystem.Controller;
using SceneSystem.Interface;
using SceneSystem.Manager;
using TankSystem.Manager;

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
        // フィールド
        // ======================================================

        /// <summary>プレイヤー追従前のモードをキャッシュ</summary>
        private int _cachedPlayerTargetModeIndex = 0;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>戦車 ID に応じた Transform 辞書</summary>
        private Dictionary<int, Transform> _tankTransformMap = new Dictionary<int, Transform>();

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>プレイヤー以外のターゲット追従時に設定するインデックス</summary>
        private const int NON_PLAYER_TARGET_MODE_INDEX = 2;

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

            _tankTransformMap.Clear();

            foreach (Transform tank in _sceneRegistry.Tanks)
            {
                if (!tank.TryGetComponent<BaseTankRootManager>(out BaseTankRootManager rootManager))
                {
                    continue;
                }

                // 戦車IDを取得
                int tankId = rootManager.TankId;

                // 辞書に登録
                if (!_tankTransformMap.ContainsKey(tankId))
                {
                    _tankTransformMap.Add(tankId, tank);
                }
            }
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
        public void SetTargetMode(in int targetModeIndex)
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
        /// tankId が未入力または無効な場合はプレイヤー Transform を設定
        /// プレイヤー以外の場合は TargetMode を 2 に変更し、プレイヤー復帰時に元のモードに戻す
        /// </summary>
        /// <param name="tankId">追従したい戦車 ID（未指定なら 0）</param>
        public void SetTargetTransform(in int tankId = 0)
        {
            Transform targetTransform;

            // 無効な ID の場合
            if (!_tankTransformMap.TryGetValue(tankId, out targetTransform) || targetTransform == null)
            {
                targetTransform = _sceneRegistry.Tanks[0];

                // キャッシュしたモードに戻す
                _followController.SetTargetMode(_cachedPlayerTargetModeIndex);
            }
            else
            {
                // 追従モードインデックスをキャッシュ
                _cachedPlayerTargetModeIndex = _followController.GetCurrentTargetModeIndex();

                // 非プレイヤーモードに切り替え
                _followController.SetTargetMode(NON_PLAYER_TARGET_MODE_INDEX);
            }

            // 追従コントローラーに Transform を設定
            _followController.SetTargetTransform(targetTransform);
        }
    }
}