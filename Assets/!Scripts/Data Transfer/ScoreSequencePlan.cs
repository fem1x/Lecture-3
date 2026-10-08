using System.Collections.Generic;
using _Scripts.Combinations;
using _Scripts.Structs___Enums.Contexts;

namespace _Scripts.Dices
{
    public struct ScoreSequencePlan
    {
        public CombinationConfig Combination { get; }
        public int TotalPoints { get; }
        public int TotalMultiplier { get; }
        public int TotalScore { get; }
        public IReadOnlyList<ScoreStep> Steps { get; }

        public ScoreSequencePlan(
            CombinationConfig combination,
            int totalPoints,
            int totalMultiplier,
            IReadOnlyList<ScoreStep> steps)
        {
            Combination = combination;
            TotalPoints = totalPoints;
            TotalMultiplier = totalMultiplier;
            TotalScore = totalPoints * totalMultiplier;
            Steps = steps;
        }
    }
}