// ======================================================
// UpdatableCollector.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2025-12-08
// 概要     : 指定ルートから IUpdatable コンポーネントを取得するクラス
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using SceneSystem.Interface;

namespace SceneSystem.Data
{
    /// <summary>
    /// IUpdatable を実装しているコンポーネントをシーンルートから取得する
    /// </summary>
    public static class UpdatableCollector
    {
        /// <summary>
        /// 指定ルート配列から IUpdatable を収集する
        /// </summary>
        /// <param name="roots">コンポーネント探索対象の GameObject 配列</param>
        /// <param name="types">取得対象の型情報。null または空の場合はすべての IUpdatable を取得</param>
        public static IUpdatable[] Collect(in GameObject[] roots, in Type[] types = null)
        {
            HashSet<IUpdatable> set = new HashSet<IUpdatable>();

            foreach (GameObject root in roots)
            {
                if (root == null) continue;

                // 型情報が null または空の場合は root の全 IUpdatable を取得
                if (types == null || types.Length == 0)
                {
                    foreach (IUpdatable u in root.GetComponents<IUpdatable>())
                    {
                        set.Add(u);
                    }
                }
                else
                {
                    // 指定型ごとに取得
                    foreach (Type t in types)
                    {
                        if (t == null) continue;

                        Component component = root.GetComponent(t);
                        if (component != null && component is IUpdatable u)
                        {
                            set.Add(u);
                        }
                    }
                }
            }

            return new List<IUpdatable>(set).ToArray();
        }
    }
}