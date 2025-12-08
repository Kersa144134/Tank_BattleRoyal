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
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// ScriptableObject の PhaseData 配列からランタイム辞書を作成する
        /// 複数のシーン上オブジェクトからコンポーネント参照を取得
        /// </summary>
        /// <param name="phaseDataList">対象のフェーズデータ配列</param>
        /// <param name="sceneRoots">シーン上の参照取得用ルート GameObject 配列</param>
        public PhaseRuntimeData(PhaseData[] phaseDataList, GameObject[] sceneRoots)
        {
            // PhaseDataList が null または空の場合は警告を出して終了
            if (phaseDataList == null || phaseDataList.Length == 0)
            {
                Debug.LogWarning("[PhaseRuntimeData] PhaseDataList が空です");
                return;
            }

            // 配列内の各 PhaseData を処理
            foreach (PhaseData phaseData in phaseDataList)
            {
                // null の PhaseData はスキップ
                if (phaseData == null)
                {
                    Debug.LogWarning("[PhaseRuntimeData] PhaseData が null のためスキップ");
                    continue;
                }

                // フェーズに紐づく IUpdatable を登録
                RegisterPhaseComponents(phaseData, sceneRoots);
            }
        }

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>現在のフェーズを取得する</summary>
        public PhaseType CurrentPhase => _currentPhase;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>現在のフェーズを外部から設定する</summary>
        /// <param name="nextPhase">変更するフェーズ</param>
        public void SetPhase(PhaseType nextPhase)
        {
            _currentPhase = nextPhase;
        }

        /// <summary>指定フェーズに登録されている IUpdatable の一覧を取得する</summary>
        /// <param name="phase">対象フェーズ</param>
        public IReadOnlyCollection<IUpdatable> GetUpdatables(PhaseType phase)
        {
            // 辞書に存在する場合はその集合を返す
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
        /// <param name="phaseData">対象フェーズデータ</param>
        /// <param name="sceneRoots">シーン上の参照取得用ルート配列</param>
        private void RegisterPhaseComponents(PhaseData phaseData, GameObject[] sceneRoots)
        {
            // フェーズに紐づく IUpdatable の集合を初期化
            HashSet<IUpdatable> set = new HashSet<IUpdatable>();

            // PhaseData から型情報を取得
            Type[] types = phaseData.GetUpdatableTypes();

            // 各ルートオブジェクトからコンポーネントを取得して登録
            foreach (GameObject root in sceneRoots)
            {
                if (root == null) continue;
                GetComponentsFromRoots(root, types, set);
            }

            // 辞書にフェーズと IUpdatable 集合を登録
            _phaseUpdateMap[phaseData.Phase] = set;
        }

        /// <summary>
        /// 指定のルートから型情報に従い IUpdatable コンポーネントを取得して登録する
        /// </summary>
        /// <param name="root">コンポーネント検索対象のルート GameObject</param>
        /// <param name="types">取得するコンポーネント型の配列</param>
        /// <param name="set">登録対象の HashSet</param>
        private void GetComponentsFromRoots(GameObject root, Type[] types, HashSet<IUpdatable> set)
        {
            foreach (Type t in types)
            {
                // ルート直下のコンポーネントを取得
                Component component = root.GetComponent(t);

                // IUpdatable の場合のみ登録
                if (component != null && component is IUpdatable u)
                {
                    set.Add(u);
                }
            }
        }
    }
}