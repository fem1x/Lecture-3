namespace _Scripts.Dices
{
    public struct ScoreCalculationResult
    {
        public readonly CombinationData Data;
        private readonly int _dicePoints;
        private readonly int _diceMultiplier;
        
        public int TotalScore => (Data.BasePoints + _dicePoints) * (Data.Multiplier + _diceMultiplier);

        public ScoreCalculationResult(CombinationData data, int dicePoints, int diceMultiplier)
        {
            Data = data;
            _dicePoints = dicePoints;
            _diceMultiplier = diceMultiplier;
        }
    }
}