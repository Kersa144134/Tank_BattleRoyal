// ======================================================
// PhaseType.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-05
// 更新日時 : 2025-12-05
// 概要     : ゲーム内フェーズを識別する列挙体
// ======================================================

namespace SceneSystem.Data
{
    /// <summary>
    /// ゲーム全体の進行状態を示すフェーズ列挙
    /// </summary>
    public enum PhaseType
    {
        /// <summary>ゲーム開始準備や読み込み中を示すフェーズ</summary>
        Initialize,

        /// <summary>メインメニュー</summary>
        Menu,

        /// <summary>ゲーム内の通常操作フェーズ</summary>
        Play,

        /// <summary>一時停止中フェーズ</summary>
        Pause,

        /// <summary>ステージクリア後のフェーズ</summary>
        Result,

        /// <summary>ゲーム終了処理フェーズ</summary>
        Finalize
    }
}