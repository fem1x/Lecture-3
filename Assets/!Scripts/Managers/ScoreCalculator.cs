using System.Collections.Generic;
using _Scripts.Combinations;
using _Scripts.Dices;
using _Scripts.Interfaces;
using _Scripts.Managers;

public class ScoreCalculator : IScoreCalculator
{
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
    private readonly CombinationEvaluator _evaluator;
    public ScoreCalculator(CombinationEvaluator evaluator)
    {
        _evaluator = evaluator;
    }
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 

    public ScoreCalculationResult Calculate(List<Dice> selectedDices)
    {
        CombinationConfig config = _evaluator.Evaluate(selectedDices);
        if (config == null)
            return default;

        int dicePoints = GetDiceValuesPoints(selectedDices);

        return new ScoreCalculationResult(config, dicePoints, 0);
    }

    private int GetDiceValuesPoints(List<Dice> scoringDice)
    {
        int sum = 0;
        if (scoringDice != null)
            for (int i = 0; i < scoringDice.Count; i++)
                sum += scoringDice[i].Data.RolledValue;
        
        return sum;
    }
}
