using _Scripts.Combinations;

namespace _Scripts.Dices
{
    public struct ScoreCalculationResult
    {
        public CombinationConfig Combination { get; }
        private readonly int _dicePoints;
        private readonly int _diceMultiplier;
        
        public int TotalScore => (Combination.BasePoints + _dicePoints) * (Combination.Multiplier + _diceMultiplier);

        public ScoreCalculationResult(CombinationConfig combination, int dicePoints, int diceMultiplier)
        {
            Combination = combination;
            _dicePoints = dicePoints;
            _diceMultiplier = diceMultiplier;
        }
    }
}