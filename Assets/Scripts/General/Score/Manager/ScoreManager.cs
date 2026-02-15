// ======================================================
// ScoreManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-15
// 更新日時 : 2026-02-15
// 概要     : スコア管理クラス
// ======================================================

using UnityEngine;

namespace ScoreSystem.Manager
{
    /// <summary>
    /// ゲーム内スコアを管理するクラス
    /// </summary>
    public sealed class ScoreManager : MonoBehaviour
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>シングルトンインスタンス</summary>
        public static ScoreManager Instance { get; private set; }

        /// <summary>累計スコア</summary>
        public int TotalScore { get; private set; }

        /// <summary>累積加算用カウンター</summary>
        private int _cumulativeCount;

        // ======================================================
        // Unity イベント
        // ======================================================

        private void Awake()
        {
            // シングルトン制御
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 初期化
            TotalScore = 0;
            _cumulativeCount = 0;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 固定値をスコアに加算する
        /// アイテム取得など、常に同じ値を加算する場合に使用
        /// </summary>
        /// <param name="score">加算する固定スコア</param>
        public void AddFixedScore(int score = 1)
        {
            TotalScore += score;
        }

        /// <summary>
        /// 累積加算を行う
        /// 呼び出すたびにカウントが増え、加算量が count × baseScore になる
        /// 敵撃破など、回数に応じて加算量が増加する場合に使用
        /// </summary>
        /// <param name="baseScore">加算する基準値</param>
        public void AddCumulativeScore(int baseScore = 1)
        {
            _cumulativeCount++;
            int scoreToAdd = _cumulativeCount * baseScore;
            TotalScore += scoreToAdd;
        }

        /// <summary>
        /// スコアをリセットする
        /// シーン開始時などに呼び出す
        /// </summary>
        public void ResetScore()
        {
            TotalScore = 0;
            _cumulativeCount = 0;
        }
    }
}