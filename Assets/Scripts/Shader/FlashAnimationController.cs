// ======================================================
// FlashAnimationController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-01-21
// 更新日時 : 2026-01-21
// 概要     : 画面フラッシュ演出を制御する
// ======================================================

using UnityEngine;

namespace ShaderSystem.Controller
{
    /// <summary>
    /// 画面全体のフラッシュ演出を制御するクラス
    /// </summary>
    public sealed class FlashAnimationController
    {
        // ======================================================
        // 列挙体
        // ======================================================

        /// <summary>
        /// フラッシュイベント種別
        /// </summary>
        private enum FlashEventType
        {
            BinarizationEnable,
            BinarizationDisable,
            GreyScaleEnable,
            GreyScaleDisable
        }

        // ======================================================
        // 構造体
        // ======================================================

        /// <summary>
        /// タイムラインイベント構造体
        /// </summary>
        private readonly struct FlashEvent
        {
            /// <summary>発火時刻</summary>
            public readonly float Time;

            /// <summary>実行内容</summary>
            public readonly FlashEventType EventType;

            public FlashEvent(in float time, in FlashEventType eventType)
            {
                Time = time;
                EventType = eventType;
            }
        }

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>2 値化ポストプロセスコントローラー</summary>
        private readonly BinarizationPostProcessController _binarizationPostProcessController;

        /// <summary>グレースケールポストプロセスコントローラー</summary>
        private readonly GreyScalePostProcessController _greyScalePostProcessController;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>現在の経過時間</summary>
        private float _elapsedTime;

        /// <summary>フラッシュ演出が再生中かどうか</summary>
        private bool _isPlaying;

        /// <summary>現在処理中のイベントインデックス</summary>
        private int _eventIndex;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>フラッシュ演出全体の長さ</summary>
        private const float FLASH_TOTAL_DURATION = 2.0f;

        /// <summary>Binarization 1 回目 有効化時刻</summary>
        private const float BINARIZATION_ENABLE_TIME_FIRST = 0.0f;

        /// <summary>Binarization 1 回目 無効化時刻</summary>
        private const float BINARIZATION_DISABLE_TIME_FIRST = 0.2f;

        /// <summary>Binarization 2 回目 有効化時刻</summary>
        private const float BINARIZATION_ENABLE_TIME_SECOND = 1.9f;

        /// <summary>Binarization 2 回目 無効化時刻</summary>
        private const float BINARIZATION_DISABLE_TIME_SECOND = 2.0f;

        /// <summary>GreyScale 有効化時刻</summary>
        private const float GREYSCALE_ENABLE_TIME = 0.2f;

        /// <summary>GreyScale 無効化時刻</summary>
        private const float GREYSCALE_DISABLE_TIME = 1.9f;

        // ======================================================
        // タイムライン
        // ======================================================

        /// <summary>フラッシュ演出イベント配列</summary>
        private readonly FlashEvent[] _flashEvents =
        {
            // --------------------------------------------------
            // 1 回目 Binarization
            // --------------------------------------------------
            new FlashEvent(
                BINARIZATION_ENABLE_TIME_FIRST,
                FlashEventType.BinarizationEnable
            ),

            new FlashEvent(
                BINARIZATION_DISABLE_TIME_FIRST,
                FlashEventType.BinarizationDisable
            ),

            // --------------------------------------------------
            // GreyScale
            // --------------------------------------------------
            new FlashEvent(
                GREYSCALE_ENABLE_TIME,
                FlashEventType.GreyScaleEnable
            ),

            new FlashEvent(
                GREYSCALE_DISABLE_TIME,
                FlashEventType.GreyScaleDisable
            ),

            // --------------------------------------------------
            // 2 回目 Binarization
            // --------------------------------------------------
            new FlashEvent(
                BINARIZATION_ENABLE_TIME_SECOND,
                FlashEventType.BinarizationEnable
            ),

            new FlashEvent(
                BINARIZATION_DISABLE_TIME_SECOND,
                FlashEventType.BinarizationDisable
            ),
        };

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// フラッシュ演出コントローラーを生成する
        /// </summary>
        public FlashAnimationController(
            in BinarizationPostProcessController binarizationController,
            in GreyScalePostProcessController greyScaleController)
        {
            _binarizationPostProcessController = binarizationController;
            _greyScalePostProcessController = greyScaleController;

            _elapsedTime = 0.0f;
            _eventIndex = 0;
            _isPlaying = false;

            _binarizationPostProcessController.DisableFullScreenPass();
            _greyScalePostProcessController.DisableFullScreenPass();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フラッシュ演出を更新する
        /// </summary>
        public void Update(in float deltaTime)
        {
            // 再生中でなければ処理しない
            if (_isPlaying == false)
            {
                return;
            }

            // 経過時間を進める
            _elapsedTime += deltaTime;

            // イベントが残っていない場合は終了判定のみ行う
            if (_eventIndex < _flashEvents.Length)
            {
                // 次のイベント時刻に到達しているか判定
                if (_elapsedTime >= _flashEvents[_eventIndex].Time)
                {
                    ExecuteEvent(_flashEvents[_eventIndex].EventType);

                    // 次のイベントへ進める
                    _eventIndex++;
                }
            }

            // 全体時間を超えたら停止
            if (_elapsedTime >= FLASH_TOTAL_DURATION)
            {
                Stop();
            }
        }

        /// <summary>
        /// フラッシュ演出を開始する
        /// </summary>
        public void Play()
        {
            if (_isPlaying)
            {
                return;
            }

            _elapsedTime = 0.0f;
            _eventIndex = 0;

            _isPlaying = true;
        }

        /// <summary>
        /// フラッシュ演出を停止する
        /// </summary>
        public void Stop()
        {
            if (!_isPlaying)
            {
                return;
            }

            _binarizationPostProcessController.DisableFullScreenPass();
            _greyScalePostProcessController.DisableFullScreenPass();

            _isPlaying = false;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// タイムラインイベントを実行する
        /// </summary>
        private void ExecuteEvent(in FlashEventType eventType)
        {
            switch (eventType)
            {
                case FlashEventType.BinarizationEnable:
                    _binarizationPostProcessController.EnableFullScreenPass();
                    break;

                case FlashEventType.BinarizationDisable:
                    _binarizationPostProcessController.DisableFullScreenPass();
                    break;

                case FlashEventType.GreyScaleEnable:
                    _greyScalePostProcessController.EnableFullScreenPass();
                    break;

                case FlashEventType.GreyScaleDisable:
                    _greyScalePostProcessController.DisableFullScreenPass();
                    break;
            }
        }
    }
}