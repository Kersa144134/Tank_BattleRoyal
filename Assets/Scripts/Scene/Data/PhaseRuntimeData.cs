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
        /// 複数のシーンルートオブジェクトからコンポーネント参照を取得
        /// </summary>
        /// <param name="phaseDataList">対象のフェーズデータ配列</param>
        /// <param name="sceneRoots">シーン上の参照取得用ルート GameObject 配列</param>
        public PhaseRuntimeData(PhaseData[] phaseDataList, GameObject[] sceneRoots)
        {
            if (phaseDataList == null || phaseDataList.Length == 0)
            {
                Debug.LogWarning("[PhaseRuntimeData] PhaseDataList が空です");
                return;
            }

            foreach (PhaseData phaseData in phaseDataList)
            {
                if (phaseData == null)
                {
                    Debug.LogWarning("[PhaseRuntimeData] PhaseData が null のためスキップ");
                    continue;
                }

                RegisterPhaseComponents(phaseData, sceneRoots);
            }
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

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 指定フェーズの IUpdatable をシーンルートから取得して登録する
        /// </summary>
        private void RegisterPhaseComponents(PhaseData phaseData, GameObject[] sceneRoots)
        {
            HashSet<IUpdatable> set = new HashSet<IUpdatable>();
            Type[] types = phaseData.GetUpdatableTypes();

            Debug.Log($"[PhaseRuntimeData] フェーズ {phaseData.Phase} のコンポーネント登録を開始 ({types.Length} 型)");

            foreach (GameObject root in sceneRoots)
            {
                if (root == null)
                {
                    Debug.LogWarning("[PhaseRuntimeData] sceneRoot が null のためスキップ");
                    continue;
                }

                GetComponentsFromRoots(root, types, set);
            }

            _phaseUpdateMap[phaseData.Phase] = set;
            Debug.Log($"[PhaseRuntimeData] フェーズ {phaseData.Phase} の登録完了 ({set.Count} 件)");
        }

        /// <summary>
        /// 指定のルートから型情報に従い IUpdatable コンポーネントを取得して登録する
        /// </summary>
        private void GetComponentsFromRoots(GameObject root, Type[] types, HashSet<IUpdatable> set)
        {
            foreach (Type t in types)
            {
                Component component = root.GetComponent(t);

                if (component != null)
                {
                    if (component is IUpdatable u)
                    {
                        if (set.Add(u))
                        {
                            Debug.Log($"[PhaseRuntimeData] 登録: {u.GetType().Name} ({component.gameObject.name})");
                        }
                        else
                        {
                            Debug.Log($"[PhaseRuntimeData] 既に登録済み: {u.GetType().Name} ({component.gameObject.name})");
                        }
                    }
                    else
                    {
                        Debug.Log($"[PhaseRuntimeData] IUpdatable ではない: {component.GetType().Name} ({component.gameObject.name})");
                    }
                }
                else
                {
                    Debug.Log($"[PhaseRuntimeData] ルート {root.name} に型 {t.Name} のコンポーネントはなし");
                }
            }
        }
    }
}