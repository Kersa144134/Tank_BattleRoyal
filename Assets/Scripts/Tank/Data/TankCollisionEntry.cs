//// ======================================================
//// TankCollisionEntry.cs
//// 作成者   : 高橋一翔
//// 作成日時 : 2025-12-17
//// 更新日時 : 2025-12-17
//// 概要     : 戦車同士の衝突判定に必要な最小限の参照情報を保持するデータクラス
//// ======================================================

//using CollisionSystem.Interface;
//using TankSystem.Manager;
//using UnityEngine;

//namespace TankSystem.Data
//{
//    /// <summary>
//    /// 戦車同士衝突判定に必要な参照情報
//    /// </summary>
//    public sealed class TankCollisionEntry
//    {
//        /// <summary>戦車本体の Transform</summary>
//        public Transform Transform { get; private set; }

//        /// <summary>戦車の動的 OBB データ</summary>
//        public IOBBData OBB { get; private set; }

//        /// <summary>戦車本体のルートマネージャー</summary>
//        public BaseTankRootManager TankRootManager { get; private set; }

//        // ======================================================
//        // コンストラクタ
//        // ======================================================

//        /// <summary>
//        /// 戦車衝突判定用エントリを生成する
//        /// </summary>
//        /// <param name="tankRootManager">戦車本体のルートマネージャー</param>
//        /// <param name="transform">戦車本体の Transform</param>
//        /// <param name="obb">戦車の動的 OBB</param>
//        public TankCollisionEntry(
//            in BaseTankRootManager tankRootManager,
//            in Transform transform,
//            in IOBBData obb
//        )
//        {
//            TankRootManager = tankRootManager;
//            Transform = transform;
//            OBB = obb;
//        }
//    }
//}