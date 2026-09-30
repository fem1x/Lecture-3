using System.Collections.Generic;
using _Scripts.Configs;
using _Scripts.Dices;
using _Scripts.Interfaces;
using _Scripts.Managers;

public class ScoreCalculator : IScoreCalculator
{
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
    private readonly CombinationScoreConfig _config;
    private readonly CombinationEvaluator _evaluator;
    public ScoreCalculator(CombinationScoreConfig config,  CombinationEvaluator evaluator)
    {
        _config = config;
        _evaluator = evaluator;
    }
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 

    public ScoreCalculationResult Calculate(IReadOnlyList<Dice> scoringDice)
    {
        CombinationType type = _evaluator.Evaluate(scoringDice);
        CombinationData data = _config.GetData(type);
        int dicePoints = GetDiceValuesPoints(scoringDice);
        
        return new ScoreCalculationResult(
            type, 
            data.BasePoints, 
            dicePoints, 
            data.Multiplier, 
            data.DiceRefund
        );
    }

    private int GetDiceValuesPoints(IReadOnlyList<Dice> scoringDice)
    {
        int diceValuesPoints = 0;
        if (scoringDice != null)
            for (int i = 0; i < scoringDice.Count; i++)
                diceValuesPoints += scoringDice[i].Value;
        
        return diceValuesPoints;
    }
}
