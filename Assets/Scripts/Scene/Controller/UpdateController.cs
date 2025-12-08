// ======================================================
// UpdateController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-05
// 更新日時 : 2025-12-08
// 概要     : 指定された IUpdatable オブジェクトを保持し OnUpdate を実行するコントローラ
// ======================================================

using System.Collections.Generic;
using SceneSystem.Interface;

namespace SceneSystem.Controller
{
    /// <summary>
    /// OnUpdate を実行する対象を保持し、毎フレーム処理を実行するコントローラ
    /// </summary>
    public class UpdateController
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// 実行対象を保持するセット
        /// 実行順が不要なため HashSet を採用し、高速な追加・削除を実現する
        /// </summary>
        private readonly HashSet<IUpdatable> _updateSet = new HashSet<IUpdatable>();

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// 現在登録されている Updatable を読み取り専用リストとして返す
        /// </summary>
        public IReadOnlyCollection<IUpdatable> GetRegisteredUpdatables()
        {
            return _updateSet;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // 登録処理
        // --------------------------------------------------
        /// <summary>
        /// 更新対象を追加する
        /// </summary>
        public void Add(in IUpdatable updatable)
        {
            // null の場合は無視
            if (updatable == null)
            {
                return;
            }

            _updateSet.Add(updatable);
        }

        /// <summary>
        /// 登録されている全 Updatable を削除する
        /// </summary>
        public void Clear()
        {
            _updateSet.Clear();
        }

        // --------------------------------------------------
        // 実行処理
        // --------------------------------------------------
        /// <summary>
        /// シーン開始時に呼ばれる初期化処理
        /// </summary>
        public void OnEnter()
        {
            foreach (IUpdatable updatable in _updateSet)
            {
                updatable.OnEnter();
            }
        }

        /// <summary>
        /// シーン終了時に呼ばれる終了処理
        /// </summary>
        public void OnExit()
        {
            foreach (IUpdatable updatable in _updateSet)
            {
                updatable.OnExit();
            }
        }

        /// <summary>
        /// フェーズ開始時に呼ばれる初期化処理
        /// </summary>
        public void OnPhaseEnter()
        {
            foreach (IUpdatable updatable in _updateSet)
            {
                updatable.OnPhaseEnter();
            }
        }

        /// <summary>
        /// フェーズ終了時に呼ばれる終了処理
        /// </summary>
        public void OnPhaseExit()
        {
            foreach (IUpdatable updatable in _updateSet)
            {
                updatable.OnPhaseExit();
            }
        }
        
        /// <summary>
        /// OnUpdate を毎フレーム実行
        /// </summary>
        public void OnUpdate()
        {
            foreach (IUpdatable updatable in _updateSet)
            {
                updatable.OnUpdate();
            }
        }

        /// <summary>
        /// LateUpdate 相当の処理を毎フレーム実行
        /// </summary>
        public void OnLateUpdate()
        {
            foreach (IUpdatable updatable in _updateSet)
            {
                updatable.OnLateUpdate();
            }
        }
    }
}