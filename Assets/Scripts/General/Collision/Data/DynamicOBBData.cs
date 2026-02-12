// ======================================================
// DynamicOBBData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-16
// 更新日時 : 2025-12-18
// 概要     : 動的オブジェクト用 OBB データ
// ======================================================

using UnityEngine;
using CollisionSystem.Interface;

namespace CollisionSystem.Data
{
    /// <summary>
    /// BaseCollisionContext の移動予定座標・回転を基準に OBB を更新する汎用動的 OBB
    /// </summary>
    public class DynamicOBBData : IOBBData
    {
        // ======================================================
        // プロパティ
        // ======================================================

        public Vector3 Center { get; private set; }
        public Vector3 HalfSize { get; private set; }
        public Quaternion Rotation { get; private set; }

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>ローカル中心オフセット</summary>
        private readonly Vector3 _localCenter;

        // ======================================================
        // コンストラクタ
        // ======================================================

        public DynamicOBBData(
            in Vector3 localCenter,
            in Vector3 halfSize
        )
        {
            _localCenter = localCenter;
            HalfSize = halfSize;

            Center = Vector3.zero;
            Rotation = Quaternion.identity;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// BaseCollisionContext の予定座標・回転を基準に毎フレーム OBB を更新
        /// </summary>
        /// <param name="plannedPosition">基準となるワールド座標</param>
        /// <param name="plannedRotation">基準となる回転</param>
        public void Update(in Vector3 plannedPosition, in Quaternion plannedRotation)
        {
            Center = plannedPosition + _localCenter;
            Rotation = plannedRotation;
        }
    }
}