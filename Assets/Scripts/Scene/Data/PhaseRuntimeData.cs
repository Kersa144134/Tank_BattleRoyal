// ======================================================
// PhaseRuntimeData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-05
// 更新日時 : 2025-12-08
// 概要     : PhaseData を実行時形式へ変換して保持するランタイムデータ
//            フェーズごとに Updatable を HashSet で保持し、重複排除と高速参照を実現
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

        /// <summary>
        /// フェーズごとに登録されている IUpdatable を管理する辞書
        /// </summary>
        private readonly Dictionary<PhaseType, HashSet<IUpdatable>> _phaseUpdateMap =
            new Dictionary<PhaseType, HashSet<IUpdatable>>();

        /// <summary>
        /// 現在適用されているフェーズを保持するフィールド
        /// </summary>
        private PhaseType _currentPhase;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// ScriptableObject の PhaseData 配列からランタイム辞書を作成する
        /// Scene 上のコンポーネント参照を取得するため sceneRoot を追加
        /// </summary>
        /// <param name="phaseDataList">登録されているフェーズデータ一覧</param>
        /// <param name="sceneRoot">シーン上の参照取得用ルート GameObject</param>
        public PhaseRuntimeData(PhaseData phaseData, GameObject sceneRoot)
        {
            HashSet<IUpdatable> set = new HashSet<IUpdatable>();

            // SO から型情報を取得
            Type[] types = phaseData.GetUpdatableTypes();

            // Scene 上のコンポーネントを型情報から取得
            foreach (Type t in types)
            {
                IUpdatable[] foundComponents = sceneRoot.GetComponentsInChildren(t, true) as IUpdatable[];
                if (foundComponents != null)
                {
                    foreach (IUpdatable u in foundComponents)
                    {
                        if (u != null)
                            set.Add(u);
                    }
                }
            }

            _phaseUpdateMap[phaseData.Phase] = set;
        }

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// 現在のフェーズを返すプロパティ
        /// </summary>
        public PhaseType CurrentPhase => _currentPhase;

        /// <summary>
        /// 現在のフェーズで実行可能な Updatable 集合を返すプロパティ
        /// 未登録フェーズの場合は空集合を返す
        /// </summary>
        public IReadOnlyCollection<IUpdatable> CurrentPhaseUpdatables =>
            _phaseUpdateMap.ContainsKey(_currentPhase)
                ? _phaseUpdateMap[_currentPhase]
                : new HashSet<IUpdatable>();

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 現在のフェーズを外部から変更する
        /// </summary>
        /// <param name="nextPhase">変更するフェーズ</param>
        public void SetPhase(PhaseType nextPhase)
        {
            // フェーズを直接更新する
            _currentPhase = nextPhase;
        }

        /// <summary>
        /// 指定フェーズに登録されている Updatable の一覧を取得する
        /// </summary>
        /// <param name="phase">対象フェーズ</param>
        public IReadOnlyCollection<IUpdatable> GetUpdatables(PhaseType phase)
        {
            // 辞書に存在していればその集合を返す
            if (_phaseUpdateMap.ContainsKey(phase))
            {
                return _phaseUpdateMap[phase];
            }

            // 未登録フェーズの場合は空集合を返す
            return new HashSet<IUpdatable>();
        }
    }
}