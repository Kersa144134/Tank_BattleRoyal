// ======================================================
// CameraTarget.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-04
// 更新日時 : 2025-12-04
// 概要     : カメラ追従対象データ
// ======================================================

using UnityEngine;

namespace CameraSystem.Controller
{
    /// <summary>
    /// カメラ追従対象のデータ構造
    /// Transform と位置・回転オフセットをまとめて管理
    /// </summary>
    [System.Serializable]
    public class CameraTarget
    {
        /// <summary>カメラとターゲット間の位置オフセット</summary>
        public Vector3 PositionOffset = new Vector3(0f, 5f, -10f);

        /// <summary>カメラの回転オフセット（角度）</summary>
        public Vector3 RotationOffset = Vector3.zero;

        /// <summary>回転を固定するかどうか</summary>
        public bool IsRotationFixed = false;
    }
}