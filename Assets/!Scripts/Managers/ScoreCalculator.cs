using System.Collections.Generic;
using System.Linq;
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

    public ScoreCalculationResult Calculate(List<Dice> selectedDices)
    {
        CombinationType type = _evaluator.Evaluate(selectedDices);
        CombinationData data = _config.GetData(type);
    
        int dicePoints = selectedDices.Sum(d => d.Value);

        return new ScoreCalculationResult(data, dicePoints, 0);
    }

    private int GetDiceValuesPoints(List<Dice> scoringDice)
    {
        int diceValuesPoints = 0;
        if (scoringDice != null)
            for (int i = 0; i < scoringDice.Count; i++)
                diceValuesPoints += scoringDice[i].Value;
        
        return diceValuesPoints;
    }
}
