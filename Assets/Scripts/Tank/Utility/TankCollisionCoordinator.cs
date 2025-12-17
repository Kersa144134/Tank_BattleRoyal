// ======================================================
// TankCollisionCoordinator.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : 戦車同士の衝突判定と解決を統括する
// ======================================================

using System.Collections.Generic;
using TankSystem.Data;
using TankSystem.Service;

namespace TankSystem.Utility
{
    /// <summary>
    /// 戦車同士の衝突判定登録・更新・解決を一元管理するクラス
    /// </summary>
    public sealed class TankCollisionCoordinator
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>戦車衝突判定サービス</summary>
        private readonly TankVersusTankCollisionService _service;

        /// <summary>戦車IDと衝突エントリの対応表</summary>
        private readonly Dictionary<int, TankCollisionEntry> _entries;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// TankCollisionCoordinator を生成する
        /// </summary>
        public TankCollisionCoordinator()
        {
            // サービス生成
            _service =
                new TankVersusTankCollisionService();

            // 辞書生成
            _entries =
                new Dictionary<int, TankCollisionEntry>();

            // 衝突イベント購読
            _service.OnTankVersusTankHit +=
                HandleTankHit;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 戦車の衝突判定エントリを登録する
        /// </summary>
        /// <param name="entry">登録する衝突判定エントリ</param>
        public void Register(TankCollisionEntry entry)
        {
            // 戦車IDを採番
            int tankId =
                _entries.Count;

            // 辞書へ登録
            _entries.Add(tankId, entry);

            // サービスへ反映
            _service.SetTankEntries(_entries);
        }

        /// <summary>
        /// 毎フレームの衝突判定更新を行う
        /// </summary>
        public void Update()
        {
            // 衝突判定を更新
            _service.UpdateCollisionChecks();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 戦車同士が接触した際の解決処理
        /// </summary>
        private void HandleTankHit(int tankIdA, int tankIdB)
        {
            // 各戦車の前進量を取得
            float deltaA =
                GetDeltaForward(tankIdA);

            float deltaB =
                GetDeltaForward(tankIdB);

            // 衝突解決処理を実行
            _service.ResolveTankVersusTank(
                tankIdA,
                tankIdB,
                deltaA,
                deltaB
            );
        }

        /// <summary>
        /// 戦車IDから直前フレームの前進量を取得する
        /// </summary>
        private float GetDeltaForward(int tankId)
        {
            // 対応エントリがなければ 0
            if (!_entries.TryGetValue(
                tankId,
                out TankCollisionEntry entry))
            {
                return 0f;
            }

            // TankRootManager 未設定なら 0
            if (entry.TankRootManager == null)
            {
                return 0f;
            }

            // 戦車側で管理している前進量を返す
            return entry.TankRootManager.DeltaForward;
        }
    }
}