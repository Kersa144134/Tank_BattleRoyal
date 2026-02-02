// ======================================================
// PhaseController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-05
// 更新日時 : 2026-01-23
// 概要     : フェーズ遷移を制御し、UpdateController に更新対象を指示するコントローラ
// ======================================================

using System;
using System.Collections.Generic;
using SceneSystem.Data;
using SceneSystem.Interface;

namespace SceneSystem.Controller
{
    /// <summary>
    /// フェーズ切替と UpdateController の操作を担当するコントローラ
    /// </summary>
    public class PhaseController
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>実行時フェーズ情報を保持するランタイムデータ</summary>
        private readonly PhaseRuntimeData _runtimeData;

        /// <summary>OnUpdate 実行を担う UpdateController</summary>
        private readonly UpdateController _updateController;

        // ======================================================
        // コンストラクタ
        // ======================================================

        public PhaseController(in PhaseRuntimeData runtimeData, in UpdateController updateController)
        {
            _runtimeData = runtimeData;
            _updateController = updateController;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズを変更し、対応する IUpdatable を UpdateController にセットする
        /// </summary>
        public void ChangePhase(in PhaseType nextPhase)
        {
            // 同じフェーズなら何もしない
            if (_runtimeData.CurrentPhase == nextPhase)
            {
                return;
            }

            // フェーズ更新
            _runtimeData.SetPhase(nextPhase);

            // UpdateController をリセット
            ClearUpdateControllerTargets();

            // 新フェーズの Updatable を追加
            AssignPhaseTargets(nextPhase);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// UpdateController に登録されている Updatable をすべて削除する
        /// </summary>
        private void ClearUpdateControllerTargets()
        {
            _updateController.Clear();
        }

        /// <summary>
        /// 指定フェーズの Updatable を UpdateController に追加する
        /// </summary>
        private void AssignPhaseTargets(in PhaseType phase)
        {
            // フェーズに紐づく Updatable 集合を取得する
            IReadOnlyCollection<IUpdatable> updatables = _runtimeData.GetUpdatables(phase);

            foreach (IUpdatable updatable in updatables)
            {
                _updateController.Add(updatable);
                UnityEngine.Debug.Log($"[PhaseController] UpdateController に追加: {updatable.GetType().FullName}");
            }

            UnityEngine.Debug.Log($"[PhaseController] 合計 {updatables.Count} 件の Updatable を登録しました");
        }
    }
}