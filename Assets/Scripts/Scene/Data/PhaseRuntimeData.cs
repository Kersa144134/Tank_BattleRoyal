// ======================================================
// PhaseRuntimeData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-05
// 更新日時 : 2025-12-08
// 概要     : PhaseData を実行時形式へ変換して保持するランタイムデータ
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using SceneSystem.Interface;

namespace SceneSystem.Data
{
    /// <summary>
    /// フェーズデータの実行時バージョンを管理するクラス
    /// </summary>
    public class PhaseRuntimeData
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>フェーズごとに登録されている IUpdatable を管理する辞書</summary>
        private readonly Dictionary<PhaseType, HashSet<IUpdatable>> _phaseUpdateMap =
            new Dictionary<PhaseType, HashSet<IUpdatable>>();

        /// <summary>現在適用されているフェーズ</summary>
        private PhaseType _currentPhase;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>現在のフェーズを取得する</summary>
        public PhaseType CurrentPhase => _currentPhase;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>フェーズに対して IUpdatable 配列を登録する</summary>
        public void RegisterPhase(in PhaseType phase, in IUpdatable[] updatables)
        {
            // 配列を HashSet に変換して登録する
            _phaseUpdateMap[phase] = new HashSet<IUpdatable>(updatables);
        }
        
        /// <summary>現在のフェーズを外部から設定する</summary>
        /// <param name="nextPhase">変更するフェーズ</param>
        public void SetPhase(in PhaseType nextPhase)
        {
            _currentPhase = nextPhase;
        }

        /// <summary>指定フェーズに登録されている Updatable の一覧を取得する</summary>
        public IReadOnlyCollection<IUpdatable> GetUpdatables(in PhaseType phase)
        {
            return _phaseUpdateMap.ContainsKey(phase) ? _phaseUpdateMap[phase] : Array.Empty<IUpdatable>();
        }
    }
}