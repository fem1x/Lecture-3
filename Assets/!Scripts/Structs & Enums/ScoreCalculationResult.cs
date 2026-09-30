namespace _Scripts.Dices
{
    public struct ScoreCalculationResult
    {
        public readonly CombinationType Type;
        public readonly int BasePoints;
        public readonly int DicePoints;
        public readonly int Multiplier;
        public readonly int DiceRefund;
        
        public int TotalScore => (BasePoints + DicePoints) * Multiplier;

        public ScoreCalculationResult(CombinationType type, int basePoints, int dicePoints, int multiplier, int diceRefund)
        {
            Type = type;
            BasePoints = basePoints;
            DicePoints = dicePoints;
            Multiplier = multiplier;
            DiceRefund = diceRefund;
        }
    }
}