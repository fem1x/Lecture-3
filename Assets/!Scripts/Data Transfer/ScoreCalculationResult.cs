using System.Collections.Generic;
using _Scripts.Combinations;
using _Scripts.Structs___Enums.Contexts;

namespace _Scripts.Dices
{
    public struct ScoreCalculationResult
    {
        public CombinationConfig Combination { get; }
        public IReadOnlyList<DiceScoreContext> DiceContexts { get; }
        
        public int BonusPoints { get; }
        public int BonusMultiplier { get; }
        
        public int TotalPoints => Combination.BasePoints + BonusPoints;
        public int TotalMultiplier => Combination.Multiplier + BonusMultiplier;
        public int TotalScore => TotalPoints * TotalMultiplier;

        public ScoreCalculationResult(
            CombinationConfig combination, 
            int bonusPoints, 
            int bonusMultiplier, 
            IReadOnlyList<DiceScoreContext> diceContexts)
        {
            Combination = combination;
            BonusPoints = bonusPoints;
            BonusMultiplier = bonusMultiplier;
            DiceContexts = diceContexts;
        }
    }
}