// ======================================================
// FaceTarget.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-14
// 更新日時 : 2025-12-14
// 概要     : 任意の Transform を指定ターゲット方向に向ける処理を担当
// ======================================================

using UnityEngine;

namespace TransformSystem.Utility
{
    /// <summary>
    /// 指定された Transform をターゲット方向に向ける汎用クラス
    /// </summary>
    public class FaceTarget
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>向きを制御する対象 Transform</summary>
        private Transform _sourceTransform;

        /// <summary>向く対象の Transform</summary>
        private Transform _targetTransform;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="source">向きを制御する Transform</param>
        /// <param name="target">向く対象の Transform</param>
        public FaceTarget(Transform source, Transform target)
        {
            _sourceTransform = source;
            _targetTransform = target;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// Transform をターゲット方向に向ける
        /// </summary>
        public void UpdateRotation()
        {
            // 対象 Transform またはターゲットが存在しない場合は処理をスキップ
            if (_sourceTransform == null || _targetTransform == null)
            {
                return;
            }
            
            // LookAt により瞬時にターゲット方向を向く
            _sourceTransform.LookAt(_targetTransform.position);

            // 自分の Y 軸を180度回転して背面を向かせる
            _sourceTransform.Rotate(0f, 180f, 0f, Space.Self);
        }

        /// <summary>
        /// ターゲット Transform を変更する
        /// </summary>
        /// <param name="target">新しいターゲット Transform</param>
        public void SetTarget(Transform target)
        {
            _targetTransform = target;
        }

        /// <summary>
        /// 現在のターゲット Transform を取得する
        /// </summary>
        /// <returns>ターゲット Transform</returns>
        public Transform GetTargetTransform()
        {
            return _targetTransform;
        }
    }
}