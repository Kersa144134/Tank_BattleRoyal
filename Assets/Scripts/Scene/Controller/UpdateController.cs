// ======================================================
// UpdateController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-05
// 更新日時 : 2025-12-15
// 概要     : 指定された IUpdatable オブジェクトを保持し OnUpdate を実行するコントローラ
//            GC 発生を抑制するため、実行時は配列キャッシュを使用する
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
        /// 登録管理用のセット
        /// 重複登録を防ぐ目的で HashSet を使用する
        /// </summary>
        private readonly HashSet<IUpdatable> _updateSet = new HashSet<IUpdatable>();

        /// <summary>
        /// 毎フレーム実行用の Updatable 配列キャッシュ
        /// foreach を排除し GC を発生させないために使用する
        /// </summary>
        private IUpdatable[] _updateArray = new IUpdatable[0];

        /// <summary>
        /// 登録内容に変更があったかどうかを示すフラグ
        /// true の場合のみ配列キャッシュを再構築する
        /// </summary>
        private bool _isDirty = true;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// 現在登録されている Updatable を読み取り専用コレクションとして返す
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
        /// <param name="updatable">登録する IUpdatable</param>
        public void Add(in IUpdatable updatable)
        {
            // null の場合は登録しない
            if (updatable == null)
            {
                return;
            }

            // 新規追加に成功した場合のみ Dirty を立てる
            if (_updateSet.Add(updatable))
            {
                _isDirty = true;
            }
        }

        /// <summary>
        /// 登録されている全 Updatable を削除する
        /// </summary>
        public void Clear()
        {
            // 登録情報を全削除
            _updateSet.Clear();

            // 実行配列を空にする
            _updateArray = new IUpdatable[0];

            // キャッシュ再構築が不要になるよう Dirty を解除
            _isDirty = false;
        }

        // --------------------------------------------------
        // IUpdatable イベント
        // --------------------------------------------------
        /// <summary>
        /// シーン開始時に呼ばれる初期化処理
        /// </summary>
        public void OnEnter()
        {
            // 実行前にキャッシュを最新化する
            RebuildCache();

            // for ループで GC を発生させずに実行する
            for (int i = 0; i < _updateArray.Length; i++)
            {
                _updateArray[i].OnEnter();
            }
        }

        /// <summary>
        /// シーン終了時に呼ばれる終了処理
        /// </summary>
        public void OnExit()
        {
            // 実行前にキャッシュを最新化する
            RebuildCache();

            // for ループで GC を発生させずに実行する
            for (int i = 0; i < _updateArray.Length; i++)
            {
                _updateArray[i].OnExit();
            }
        }

        /// <summary>
        /// フェーズ開始時に呼ばれる初期化処理
        /// </summary>
        public void OnPhaseEnter()
        {
            // 実行前にキャッシュを最新化する
            RebuildCache();

            // for ループで GC を発生させずに実行する
            for (int i = 0; i < _updateArray.Length; i++)
            {
                _updateArray[i].OnPhaseEnter();
            }
        }

        /// <summary>
        /// フェーズ終了時に呼ばれる終了処理
        /// </summary>
        public void OnPhaseExit()
        {
            // 実行前にキャッシュを最新化する
            RebuildCache();

            // for ループで GC を発生させずに実行する
            for (int i = 0; i < _updateArray.Length; i++)
            {
                _updateArray[i].OnPhaseExit();
            }
        }

        /// <summary>
        /// OnUpdate を毎フレーム実行
        /// </summary>
        public void OnUpdate()
        {
            // 実行前にキャッシュを最新化する
            RebuildCache();

            // for ループで GC を発生させずに実行する
            for (int i = 0; i < _updateArray.Length; i++)
            {
                _updateArray[i].OnUpdate();
            }
        }

        /// <summary>
        /// LateUpdate 相当の処理を毎フレーム実行
        /// </summary>
        public void OnLateUpdate()
        {
            // 実行前にキャッシュを最新化する
            RebuildCache();

            // for ループで GC を発生させずに実行する
            for (int i = 0; i < _updateArray.Length; i++)
            {
                _updateArray[i].OnLateUpdate();
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 登録内容に変更があった場合のみ、実行用配列キャッシュを再構築する
        /// </summary>
        private void RebuildCache()
        {
            // 変更がない場合は再構築しない
            if (!_isDirty)
            {
                return;
            }

            // 登録数に応じた配列を新規生成する
            _updateArray = new IUpdatable[_updateSet.Count];

            // 配列へのコピー用インデックス
            int index = 0;

            // HashSet から配列へ要素を転写する
            foreach (IUpdatable updatable in _updateSet)
            {
                _updateArray[index] = updatable;
                index++;
            }

            // 再構築完了のため Dirty を解除する
            _isDirty = false;
        }
    }
}