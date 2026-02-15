// ======================================================
// CameraFollowController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-04
// 更新日時 : 2025-12-13
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

        /// <summary>カメラ Transform</summary>
        private readonly Transform _cameraTransform;

        /// <summary>追従対象配列</summary>
        private readonly CameraTarget[] _targets;

        /// <summary>ターゲット Transform</summary>
        private Transform _targetTransform;

        /// <summary>現在の追従モードインデックス</summary>
        private int _currentTargetModeIndex = 0;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>追従速度</summary>
        private const float FOLLOW_SPEED = 20f;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// CameraTarget 配列を登録し、カメラ Transform を取得
        /// </summary>
        /// <param name="cameraTransform">カメラ Transform</param>
        /// <param name="targetArray">追従対象の CameraTarget 配列</param>
        /// <param name="targetTransform">ターゲット Transform</param>
        public CameraFollowController(
            in Transform cameraTransform,
            in CameraTarget[] targetArray,
            in Transform targetTransform)
        {
            _cameraTransform = cameraTransform;
            _targets = targetArray;
            _targetTransform = targetTransform;
        }

        // ======================================================
        // セッター
        // ======================================================

        /// <summary>
        /// 追従モードを指定インデックスに変更
        /// </summary>
        /// <param name="index">配列インデックス</param>
        public void SetTargetMode(in int index)
        {
            if (_targets == null || index < 0 || _targets.Length <= index)
            {
                return;
            }

            _currentTargetModeIndex = index;
        }

        /// <summary>
        /// 追従対象の Transform を変更
        /// </summary>
        /// <param name="targetTransform">新しいターゲット Transform</param>
        public void SetTargetTransform(in Transform targetTransform)
        {
            if (targetTransform == null)
            {
                return;
            }

            _targetTransform = targetTransform;
        }

        // ======================================================
        // ゲッター
        // ======================================================

        /// <summary>
        /// 現在の追従モードインデックスを取得
        /// </summary>
        /// <returns>有効な追従モードインデックス</returns>
        public int GetCurrentTargetModeIndex()
        {
            return _currentTargetModeIndex;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 配列のターゲットを追従
        /// </summary>
        /// <param name="deltaTime">経過時間</param>
        public void UpdateFollow(in float deltaTime)
        {
            // カメラやターゲットが設定されていない場合は処理をスキップ
            if (_cameraTransform == null ||
                _targets == null ||
                _targets.Length == 0 ||
                _targets[_currentTargetModeIndex] == null ||
                _targetTransform == null
            )
            {
                return;
            }

            CameraTarget target = _targets[_currentTargetModeIndex];

            // 位置追従
            _cameraTransform.position = CalculateFollowPosition(deltaTime, target);

            // 回転追従
            _cameraTransform.rotation = CalculateFollowRotation(deltaTime, target);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// ターゲットとオフセットに基づき、補間済みの追従位置を計算して返す
        /// </summary>
        /// <param name="deltaTime">経過時間</param>
        /// <param name="target">追従対象の CameraTarget データ</param>
        /// <returns>補間済みのカメラ追従位置</returns>
        private Vector3 CalculateFollowPosition(in float deltaTime, in CameraTarget target)
        {
            Vector3 targetPos = _targetTransform.position;

            if (target.IsRotationFixed)
            {
                // 回転固定の場合はワールド座標オフセットを直接加算
                targetPos += target.PositionOffset;
            }
            else
            {
                // ターゲット回転に応じたローカルオフセットをワールド座標に変換して加算
                targetPos += _targetTransform.TransformVector(target.PositionOffset);
            }

            // 補間してカメラ位置に適用
            return Vector3.Lerp(_cameraTransform.position, targetPos, FOLLOW_SPEED * deltaTime);
        }

        /// <summary>
        /// ターゲットとオフセットに基づき、補間済みの追従回転を計算して返す
        /// </summary>
        /// <param name="target">追従対象の CameraTarget データ</param>
        /// <returns>補間済みのカメラ追従回転</returns>
        private Quaternion CalculateFollowRotation(in float deltaTime, in CameraTarget target)
        {
            Quaternion targetRotation;

            if (target.IsRotationFixed)
            {
                // 回転固定の場合は前回の回転からオフセット角度まで補間
                targetRotation = Quaternion.Slerp(
                    _cameraTransform.rotation,
                    Quaternion.Euler(target.RotationOffset),
                    FOLLOW_SPEED * deltaTime
                );
            }
            else
            {
                // ターゲット回転にオフセットを加え、前回の回転から補間
                targetRotation = Quaternion.Slerp(
                    _cameraTransform.rotation,
                    _targetTransform.rotation * Quaternion.Euler(target.RotationOffset),
                    FOLLOW_SPEED * deltaTime
                );
            }

            return targetRotation;
        }
    }
}