using System.Collections.Generic;
using System.Linq;

public class ScoreCalculator
{
    public (string Combination, int Score) EvaluateHand(List<int> diceValues)
    {
        return ("Сумма", diceValues.Sum());    
    }
}
