// Defines the three selectable difficulty tiers and the tile-survival
// target each one requires to win a run.

namespace DoofusDiaries.Core
{
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    public static class DifficultyTargets
    {
        public const int Easy = 50;
        public const int Medium = 100;
        public const int Hard = 200;

        public static int GetTarget(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Easy: return Easy;
                case Difficulty.Medium: return Medium;
                case Difficulty.Hard: return Hard;
                default: return Easy;
            }
        }
    }
}
