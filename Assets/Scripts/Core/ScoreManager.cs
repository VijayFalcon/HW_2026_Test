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
        /// Call this whenever the player successfully lands on a pulpit.
        /// Only counts if it's a *different* pulpit than the one they left
        /// (guarded here even though the ring layout makes fromSlot == toSlot
        /// impossible today, in case movement rules change later).
        /// </summary>
        public void RegisterSuccessfulMove(int fromSlot, int toSlot)
        {
            if (fromSlot == toSlot) return;

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
