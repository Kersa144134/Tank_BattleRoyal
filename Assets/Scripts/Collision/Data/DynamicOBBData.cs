// ======================================================
// DynamicOBBData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-16
// 更新日時 : 2025-12-18
// 概要     : 動的 OBB データ（BaseCollisionContext連動）
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

        /// <summary>参照する汎用コンテキスト</summary>
        private readonly BaseCollisionContext _context;

        // ======================================================
        // コンストラクタ
        // ======================================================

        public DynamicOBBData(
            in BaseCollisionContext context,
            in Vector3 localCenter,
            in Vector3 halfSize
        )
        {
            _context = context;
            _localCenter = localCenter;
            HalfSize = halfSize;

            Center = Vector3.zero;
            Rotation = Quaternion.identity;
            Update();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// BaseCollisionContext の予定座標・回転を基準に毎フレーム OBB を更新
        /// </summary>
        public void Update()
        {
            if (_context == null)
            {
                return;
            }

            // PlannedNextPosition / PlannedNextRotation を基準に OBB 更新
            Center = _context.PlannedNextPosition + _localCenter;
            Rotation = _context.PlannedNextRotation;
        }
    }
}