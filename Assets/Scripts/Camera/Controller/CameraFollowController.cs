// ======================================================
// CameraFollowController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-04
// 更新日時 : 2025-12-04
// 概要     : ターゲット追従ロジッククラス
//            CameraTarget 配列を参照し、オフセットの適用、ターゲット切替に対応
// ======================================================

using UnityEngine;

namespace CameraSystem.Controller
{
    /// <summary>
    /// CameraManager から呼ばれ、指定ターゲットを追従するロジック
    /// </summary>
    public class CameraFollowController
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>追従対象配列（CameraTarget）</summary>
        private CameraTarget[] _targets;

        /// <summary>カメラ Transform</summary>
        private Transform _cameraTransform;

        /// <summary>現在追従中のターゲットインデックス</summary>
        private int _currentTargetIndex = 0;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>追従速度</summary>
        private const float FOLLOW_SPEED = 5f;

        // ======================================================
        // 初期化
        // ======================================================

        /// <summary>
        /// CameraTarget 配列を登録し、カメラ Transform を取得
        /// </summary>
        /// <param name="cameraTransform">カメラ Transform</param>
        /// <param name="targetArray">追従対象の CameraTarget 配列</param>
        public void Initialize(Transform cameraTransform, CameraTarget[] targetArray)
        {
            _cameraTransform = cameraTransform;
            _targets = targetArray;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 配列のターゲットを追従
        /// </summary>
        public void UpdateFollow()
        {
            if (_cameraTransform == null || _targets == null || _targets.Length == 0 || _targets[0] == null || _targets[0].TargetTransform == null)
            {
                return;
            }

            CameraTarget target = _targets[_currentTargetIndex];

            // ターゲットの座標を取得
            Vector3 targetPos = target.TargetTransform.position;

            // オフセットを追加
            targetPos += target.TargetTransform.TransformVector(target.PositionOffset);

            // 補間してカメラに適用
            _cameraTransform.position = Vector3.Lerp(_cameraTransform.position, targetPos, FOLLOW_SPEED * Time.deltaTime);

            // ターゲットの回転を取得
            Quaternion targetRotation = target.TargetTransform.rotation;

            // オフセットを追加
            targetRotation *= Quaternion.Euler(target.RotationOffset);

            // 補間してカメラに適用
            _cameraTransform.rotation = Quaternion.Slerp(_cameraTransform.rotation, targetRotation, FOLLOW_SPEED * Time.deltaTime);
        }

        /// <summary>
        /// 現在追従中のターゲットを指定インデックスに変更
        /// </summary>
        /// <param name="index">配列インデックス</param>
        public void SetTarget(int index)
        {
            if (_targets == null || index < 0 || index >= _targets.Length) return;
            _currentTargetIndex = index;
        }

        /// <summary>
        /// 現在追従中のターゲットインデックスを取得
        /// </summary>
        /// <returns>有効なターゲットインデックス、無効な場合は -1</returns>
        public int GetCurrentTargetIndex()
        {
            if (_targets == null || _targets.Length == 0) return -1;
            if (_targets[_currentTargetIndex] == null || _targets[_currentTargetIndex].TargetTransform == null) return -1;
            return _currentTargetIndex;
        }
    }
}