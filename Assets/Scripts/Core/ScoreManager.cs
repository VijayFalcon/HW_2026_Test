using System;
using UnityEngine;

namespace DoofusDiaries.Core
{
    /// <summary>
    /// Tracks the current run's score and the all-time best (persisted via
    /// PlayerPrefs so it survives between sessions). Plain C# class rather
    /// than a MonoBehaviour since it has no need for the GameObject
    /// lifecycle -- GameManager owns and drives it directly.
    /// </summary>
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

        /// <summary>
        /// Call this whenever the player lands on a tile it hasn't just come
        /// from (PlayerController already de-duplicates repeated landings on
        /// the same tile before raising the event that leads here).
        /// </summary>
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
