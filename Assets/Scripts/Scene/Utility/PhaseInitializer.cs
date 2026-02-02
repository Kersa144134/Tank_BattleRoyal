// ======================================================
// PhaseInitializer.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-02
// 更新日時 : 2026-02-02
// 概要     : フェーズデータと IUpdatable から
//            フェーズごとの登録対象を抽出し PhaseController に登録する
// ======================================================

using System;
using System.Linq;
using UnityEngine;
using SceneSystem.Controller;
using SceneSystem.Data;
using SceneSystem.Interface;

namespace SceneSystem.Utility
{
    public sealed class PhaseInitializer
    {
        /// <summary>
        /// フェーズごとにシーン上の Updatable を抽出して PhaseController に登録
        /// </summary>
        /// <param name="phaseController">対象の PhaseController</param>
        /// <param name="allUpdatables">シーン上の IUpdatable 配列</param>
        /// <param name="phaseDataList">全フェーズデータ</param>
        public void Initialize(
            in PhaseController phaseController,
            in IUpdatable[] allUpdatables,
            in PhaseData[] phaseDataList
        )
        {
            foreach (PhaseData phaseData in phaseDataList)
            {
                string[] typeNames = phaseData.GetUpdatableTypeNames();

                // 型名から Type 配列に変換
                Type[] targetTypes = typeNames
                    .Select(t =>
                    {
                        Type type = Type.GetType(t);
                        if (type != null) return type;

                        // 全アセンブリから検索
                        return AppDomain.CurrentDomain.GetAssemblies()
                            .Select(a => a.GetType(t))
                            .FirstOrDefault(tt => tt != null);
                    })
                    .Where(tt => tt != null)
                    .ToArray();

                // フェーズに紐づくコンポーネントのみ抽出
                IUpdatable[] phaseUpdatables = Array.FindAll(allUpdatables, u =>
                    u != null && targetTypes.Any(tt => tt.IsAssignableFrom(u.GetType()))
                );

                // PhaseController に登録
                phaseController.AssignPhaseUpdatables(phaseData.Phase, phaseUpdatables);

                // デバッグログ
                string[] names = phaseUpdatables.Select(u => u.GetType().FullName).ToArray();
                Debug.Log($"[PhaseInitializer] フェーズ {phaseData.Phase} に {phaseUpdatables.Length} 件登録: {string.Join(", ", names)}");
            }
        }
    }
}