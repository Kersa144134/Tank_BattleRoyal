// ==============================================================================
// TrackInputModeHolder.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-16
// 更新日時 : 2025-12-16
// 概要     : 現在のキャタピラ入力モードを管理し、変更を通知する
// ==============================================================================

using System;

namespace InputSystem.Data
{
    /// <summary>
    /// キャタピラ入力モードの状態を一元管理する
    /// </summary>
    internal sealed class TrackInputModeHolder
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>現在適用されている入力モード</summary>
        private TrackInputMode _currentMode;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>入力モードが変更された際に通知されるイベント</summary>
        public event Action<TrackInputMode> OnModeChanged;

        // ======================================================
        // 公開メソッド
        // ======================================================

        /// <summary>
        /// 現在の入力モードを設定する
        /// </summary>
        public void SetMode(TrackInputMode mode)
        {
            // 同一モードの場合は何もしない
            if (_currentMode == mode)
            {
                return;
            }

            // モードを更新
            _currentMode = mode;

            // 変更通知を発行
            OnModeChanged?.Invoke(_currentMode);
        }

        /// <summary>
        /// 現在の入力モードを取得する
        /// </summary>
        public TrackInputMode GetMode()
        {
            return _currentMode;
        }
    }
}