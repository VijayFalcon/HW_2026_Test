// Tracks the current run's score and the all-time best score, persisting
// the best across sessions via PlayerPrefs.

using System;
using UnityEngine;

namespace DoofusDiaries.Core
{
    public class ScoreManager
    {
        private const string BestScoreKey = "DoofusDiaries.BestScore";

        public int CurrentScore { get; private set; }
        public int BestScore { get; private set; }

        public event Action<int> OnScoreChanged;

        public ScoreManager()
        {
            BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        }

        public void Reset()
        {
            CurrentScore = 0;
            OnScoreChanged?.Invoke(CurrentScore);
        }

        public void RegisterTileEntered(Vector2Int tileGridPosition)
        {
            CurrentScore++;
            OnScoreChanged?.Invoke(CurrentScore);

            if (CurrentScore > BestScore)
            {
                BestScore = CurrentScore;
                PlayerPrefs.SetInt(BestScoreKey, BestScore);
            }
        }
    }
}
