// ======================================================
// PhaseController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-05
// 更新日時 : 2026-02-02
// 概要     : フェーズ遷移を制御し、UpdateController に更新対象を指示するコントローラ
// ======================================================

using System;
using System.Collections.Generic;
using SceneSystem.Data;
using SceneSystem.Interface;
using UnityEngine;

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

        /// <summary>UpdateController への参照</summary>
        private readonly UpdateController _updateController;

        /// <summary>現在のフェーズ</summary>
        private PhaseType _currentPhase = PhaseType.None;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>フェーズごとの Updatable 配列を保持する辞書</summary>
        private readonly Dictionary<PhaseType, IUpdatable[]> _phaseUpdatablesMap = new Dictionary<PhaseType, IUpdatable[]>();

        // ======================================================
        // コンストラクタ
        // ======================================================

        public PhaseController(in UpdateController updateController)
        {
            _updateController = updateController;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズに紐づく IUpdatable を登録する
        /// </summary>
        /// <param name="phase">登録対象のフェーズ</param>
        /// <param name="updatables">フェーズに属する IUpdatable 配列</param>
        public void AssignPhaseUpdatables(in PhaseType phase, in IUpdatable[] updatables)
        {
            // フェーズごとに内部辞書に格納
            _phaseUpdatablesMap[phase] = updatables;

            // UpdateController にも現在フェーズなら登録
            if (_currentPhase == phase)
            {
                foreach (IUpdatable updatable in updatables)
                {
                    _updateController.Add(updatable);
                }
            }
        }

        /// <summary>
        /// フェーズを変更し、対応する IUpdatable を UpdateController に登録する
        /// </summary>
        /// <param name="nextPhase">変更先フェーズ</param>
        public void ChangePhase(in PhaseType nextPhase)
        {
            // 同じフェーズなら何もしない
            if (_currentPhase == nextPhase)
            {
                return;
            }

            // 現在フェーズ更新
            _currentPhase = nextPhase;

            // UpdateController をリセット
            _updateController.Clear();

            // 新フェーズの Updatable を登録
            if (_phaseUpdatablesMap.TryGetValue(nextPhase, out IUpdatable[] phaseUpdatables))
            {
                foreach (IUpdatable updatable in phaseUpdatables)
                {
                    _updateController.Add(updatable);
                }
            }
        }
    }
}