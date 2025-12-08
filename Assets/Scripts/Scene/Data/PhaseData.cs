// ======================================================
// PhaseData.cs
// 作成者 : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2025-12-08
// 概要 : フェーズごとの実行対象を GameObject 経由で登録する ScriptableObject
// Scene 上の IUpdatable を参照し、UpdateController に渡す
// ======================================================

using System;
using UnityEngine;
using UnityEditor;

namespace SceneSystem.Data
{
    /// <summary>
    /// フェーズごとの設定内容を保持する ScriptableObject
    /// GameObject 経由で IUpdatable を取得する
    /// </summary>
    [CreateAssetMenu(fileName = "PhaseData", menuName = "SceneSystem/PhaseData")]
    public class PhaseData : ScriptableObject
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        /// <summary>フェーズの種別</summary>
        [SerializeField] private PhaseType _phaseType;

        /// <summary>実行対象として使用するコンポーネントの型情報</summary>
        [SerializeField] private MonoScript[] _updatableScripts;

        public PhaseType Phase => _phaseType;

        // 実行時に Type を返す
        public Type[] GetUpdatableTypes()
        {
            Type[] types = new Type[_updatableScripts.Length];
            for (int i = 0; i < _updatableScripts.Length; i++)
            {
                types[i] = _updatableScripts[i].GetClass();
            }
            return types;
        }
    }
}