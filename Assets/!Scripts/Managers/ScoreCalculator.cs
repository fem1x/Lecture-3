using System.Collections.Generic;
using _Scripts.Charms;
using _Scripts.Combinations;
using _Scripts.Dices;
using _Scripts.Interfaces;
using _Scripts.Managers;
using _Scripts.Structs___Enums.Contexts;
using VContainer;

public class ScoreCalculator : IScoreCalculator
{
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
    private readonly CombinationEvaluator _evaluator;
    private readonly CharmsService _charmsService;
    
    [Inject]
    public ScoreCalculator(CombinationEvaluator evaluator, CharmsService charmsService)
    {
        _evaluator = evaluator;
        _charmsService = charmsService;
    }
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 

    public ScoreCalculationResult Calculate(List<Dice> selectedDices)
    {
        var combinationConfig = _evaluator.Evaluate(selectedDices);
        if (combinationConfig == null)
            return default;

        var totalBonusPoints = 0;
        var totalBonusMultiplier = 0;
        var diceContexts = new List<DiceScoreContext>(selectedDices.Count);

        foreach (var dice in selectedDices)
        {
            var context = new DiceScoreContext(dice, combinationConfig);

            totalBonusPoints += context.BaseDicePoints;

            //Charms
            foreach (var charm in _charmsService.ActiveCharms)
                charm.OnDiceScored(context);

            foreach (var trigger in context.Triggers)
            {
                totalBonusPoints += trigger.BonusPoints;
                totalBonusMultiplier += trigger.BonusMultiplier;
            }
            diceContexts.Add(context);
        }
        
        return new ScoreCalculationResult(combinationConfig, totalBonusPoints, totalBonusMultiplier, diceContexts);
    }
}
