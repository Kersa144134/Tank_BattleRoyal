// ======================================================
// UpdatableCollector.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2025-12-08
// 概要     : 指定ルートから IUpdatable コンポーネントを取得するクラス
// ======================================================

using SceneSystem.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SceneSystem.Data
{
    /// <summary>
    /// IUpdatable を実装しているコンポーネントをシーンルートから収集する
    /// </summary>
    public class UpdatableCollector
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定されたルート GameObject 配列から IUpdatable コンポーネントを収集する
        /// </summary>
        /// <param name="roots">探索対象となる GameObject 配列</param>
        /// <param name="typeNames">
        /// 取得対象の型名配列
        /// null または空の場合はすべての IUpdatable を取得
        /// </param>
        /// <returns>収集した IUpdatable 配列</returns>
        public IUpdatable[] Collect(in GameObject[] roots, in string[] typeNames = null, string phaseName = "Unknown")
        {
            HashSet<IUpdatable> updatables = new HashSet<IUpdatable>();

            foreach (GameObject root in roots)
            {
                if (root == null)
                {
                    continue;
                }

                // root および子オブジェクトに存在する全 IUpdatable を取得
                IUpdatable[] allUpdatables = root.GetComponentsInChildren<IUpdatable>(true);

                // 型指定なしの場合はすべて登録
                if (typeNames == null || typeNames.Length == 0)
                {
                    foreach (IUpdatable u in allUpdatables)
                    {
                        updatables.Add(u);
                    }

                    continue;
                }

                // ======================================================
                // 型名が指定されている場合
                // ======================================================
                foreach (string typeName in typeNames)
                {
                    // 空文字は無視
                    if (string.IsNullOrEmpty(typeName))
                    {
                        continue;
                    }

                    // --------------------------------------------------
                    // 型解決（アセンブリ探索込み）
                    // ScriptableObject に記載された型名から Type を取得
                    // --------------------------------------------------
                    Type targetType = Type.GetType(typeName)
                        ?? AppDomain.CurrentDomain.GetAssemblies()
                            .Select(a => a.GetType(typeName))
                            .FirstOrDefault(t => t != null);

                    // 型解決できなかった場合は次へ
                    if (targetType == null)
                    {
                        continue;
                    }

                    // --------------------------------------------------
                    // 取得した IUpdatable 配列から指定型に合致するものを追加
                    // targetType が抽象クラスやインターフェースの場合も対応
                    // --------------------------------------------------
                    foreach (IUpdatable u in allUpdatables)
                    {
                        if (targetType.IsAssignableFrom(u.GetType()))
                        {
                            updatables.Add(u);
                        }
                    }
                }
            }

            // ======================================================
            // フェーズごとのログ表示
            // ======================================================
            string[] updatableTypeNames = updatables.Select(u => u.GetType().FullName).ToArray();
            UnityEngine.Debug.Log(
                $"[UpdatableCollector] フェーズ {phaseName} に {updatables.Count} 件の IUpdatable を収集: {string.Join(", ", updatableTypeNames)}"
            );

            // HashSet から配列に変換して返却
            return updatables.ToArray();
        }
    }
}