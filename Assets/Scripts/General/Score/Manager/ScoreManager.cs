// ======================================================
// ScoreManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-15
// 更新日時 : 2026-03-08
// 概要     : スコア管理クラス
// ======================================================

using System;
using UnityEngine;

namespace ScoreSystem.Manager
{
    /// <summary>
    /// ゲーム内スコアを管理するクラス
    /// </summary>
    public sealed class ScoreManager : MonoBehaviour
    {
        // ======================================================
        // シングルトン
        // ======================================================

        /// <summary>シングルトンインスタンス</summary>
        public static ScoreManager Instance { get; private set; }

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>累計スコア</summary>
        private int _totalScore;

        /// <summary>累積加算用カウンター</summary>
        private int _cumulativeCount;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>累計スコア</summary>
        public int TotalScore => _totalScore;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>スコア最大値</summary>
        public const int SCORE_MAX = 99999999;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// スコア変動通知イベント
        /// 引数は加算されたスコア量
        /// </summary>
        public event Action<int> OnScoreChanged;

        // ======================================================
        // Unity イベント
        // ======================================================

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 初期化
            _totalScore = 0;
            _cumulativeCount = 0;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 固定値をスコアに加算する
        /// </summary>
        /// <param name="score">加算する固定スコア</param>
        public void AddFixedScore(int score = 1)
        {
            if (score == 0)
            {
                return;
            }

            // 加算前スコア保存
            int previousScore = _totalScore;

            // スコア加算
            _totalScore += score;

            // カンスト処理
            if (_totalScore > SCORE_MAX)
            {
                _totalScore = SCORE_MAX;
            }

            // スコア変動量
            int delta = _totalScore - previousScore;

            // 変動量が 0 の場合は通知なし
            if (delta == 0)
            {
                return;
            }

            // スコア変動通知
            OnScoreChanged?.Invoke(delta);
        }

        /// <summary>
        /// 累積加算を行う
        /// </summary>
        /// <param name="baseScore">加算する基準値</param>
        public void AddCumulativeScore(int baseScore = 1)
        {
            if (baseScore == 0)
            {
                return;
            }

            // 加算前スコア保存
            int previousScore = _totalScore;

            // 累積カウント増加
            _cumulativeCount++;

            // 加算量計算
            int scoreToAdd = _cumulativeCount * baseScore;

            // スコア加算
            _totalScore += scoreToAdd;

            // カンスト処理
            if (_totalScore > SCORE_MAX)
            {
                _totalScore = SCORE_MAX;
            }

            // スコア変動量
            int delta = _totalScore - previousScore;

            // 変動量が 0 の場合は通知なし
            if (delta == 0)
            {
                return;
            }

            // スコア変動通知
            OnScoreChanged?.Invoke(delta);
        }

        /// <summary>
        /// スコアをリセットする
        /// シーン開始時などに呼び出す
        /// </summary>
        public void ResetScore()
        {
            _totalScore = 0;
            _cumulativeCount = 0;

            // スコア変動通知
            OnScoreChanged?.Invoke(0);
        }
    }
}