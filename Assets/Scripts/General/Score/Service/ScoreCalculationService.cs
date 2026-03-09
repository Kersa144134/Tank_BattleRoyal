// ======================================================
// ScoreCalculationService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-09
// 更新日時 : 2026-03-09
// 概要     : スコア計算ロジックを提供するサービスクラス
// ======================================================

using ScoreSystem.Data;

namespace ScoreSystem.Service
{
    /// <summary>
    /// スコア計算サービス
    /// </summary>
    public sealed class ScoreCalculationService
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// スコア最大値
        /// </summary>
        private readonly int _scoreMax;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// ScoreCalculationService を生成する
        /// </summary>
        /// <param name="scoreMax">スコア最大値</param>
        public ScoreCalculationService(int scoreMax)
        {
            _scoreMax = scoreMax;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 固定値をスコアに加算する
        /// </summary>
        /// <param name="scoreData">更新対象のスコアデータ</param>
        /// <param name="score">加算スコア</param>
        /// <returns>実際に増加したスコア量</returns>
        public int AddFixedScore(ref ScoreData scoreData, in int score)
        {
            if (score == 0)
            {
                return 0;
            }

            // 加算前スコアを保持
            int previousScore = scoreData.TotalScore;

            // 指定された固定スコアを累計スコアへ加算する
            scoreData.TotalScore += score;

            // スコアが上限値を超えた場合は最大値に補正する
            if (scoreData.TotalScore > _scoreMax)
            {
                scoreData.TotalScore = _scoreMax;
            }

            // 加算後スコアとの差分から実際の増加量を算出する
            int delta = scoreData.TotalScore - previousScore;

            // 呼び出し元に増加量を返す
            return delta;
        }

        /// <summary>
        /// 累積カウントに応じたスコア加算を行う
        /// </summary>
        /// <param name="scoreData">更新対象のスコアデータ</param>
        /// <param name="baseScore">基準スコア</param>
        /// <returns>実際に増加したスコア量</returns>
        public int AddCumulativeScore(ref ScoreData scoreData, in int baseScore)
        {
            if (baseScore == 0)
            {
                return 0;
            }

            // 加算前スコアを保持
            int previousScore = scoreData.TotalScore;

            // 累積スコア計算用のカウンターを増加させる
            scoreData.CumulativeCount++;

            // 累積カウントと基準スコアから今回の加算量を算出する
            int scoreToAdd = scoreData.CumulativeCount * baseScore;

            // 算出したスコアを累計スコアへ加算する
            scoreData.TotalScore += scoreToAdd;

            // スコアが上限値を超えた場合は最大値に補正する
            if (scoreData.TotalScore > _scoreMax)
            {
                scoreData.TotalScore = _scoreMax;
            }

            // 加算前後の差分から実際の増加量を算出する
            int delta = scoreData.TotalScore - previousScore;

            // 呼び出し元に増加量を返す
            return delta;
        }

        /// <summary>
        /// スコア状態を初期化する
        /// </summary>
        /// <param name="scoreData">更新対象のスコアデータ</param>
        public void ResetScore(ref ScoreData scoreData)
        {
            scoreData.TotalScore = 0;
            scoreData.CumulativeCount = 0;
        }
    }
}