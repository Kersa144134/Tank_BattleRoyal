// ======================================================
// DynamicOBBData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-16
// 更新日時 : 2025-12-16
// 概要     : 動的オブジェクト用 OBB データ
// ======================================================

using CollisionSystem.Interface;
using UnityEngine;

namespace CollisionSystem.Data
{
    /// <summary>
    /// 動的 OBB データ
    /// </summary>
    public class DynamicOBBData : IOBBData
    {
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>OBB の中心座標（ワールド基準）</summary>
        public Vector3 Center { get; private set; }

        /// <summary>OBB の半サイズ（ローカル基準）</summary>
        public Vector3 HalfSize { get; private set; }

        /// <summary>OBB の回転（ワールド基準）</summary>
        public Quaternion Rotation { get; private set; }

        /// <summary>OBB の Transform</summary>
        public Transform Transform => _transform;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>OBB を追従する Transform</summary>
        private readonly Transform _transform;

        /// <summary>Transform 基準のローカル中心オフセット</summary>
        private readonly Vector3 _localCenter;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 動的 OBB を初期化する
        /// </summary>
        /// <param name="transform">追従する Transform</param>
        /// <param name="localCenter">Transform 基準のローカル中心オフセット</param>
        /// <param name="halfSize">半サイズ</param>
        public DynamicOBBData(
            in Transform transform,
            in Vector3 localCenter,
            in Vector3 halfSize
        )
        {
            _transform = transform;
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
        /// 毎フレーム呼び出して Transform に基づき Center と Rotation を更新する
        /// </summary>
        public void Update()
        {
            if (_transform == null)
            {
                return;
            }

            // 中心座標の更新
            Center = _transform.position + _localCenter;

            // 回転の更新
            Rotation = _transform.rotation;
        }
    }
}