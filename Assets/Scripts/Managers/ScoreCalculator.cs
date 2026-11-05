using System.Collections.Generic;
using _Scripts.Charms;
using _Scripts.Combinations;
using _Scripts.Data_Transfer.Contexts;
using _Scripts.Dices;
using _Scripts.Interfaces;
using _Scripts.Managers;
using UnityEngine;
using VContainer;

public class ScoreCalculator : IScoreCalculator
{
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
    private readonly CombinationEvaluator _evaluator;
    private readonly CharmsService _charmsService;
    private readonly DiceTableController _tableController;
    private readonly ResourceManager _resourceManager;
    
    [Inject]
    public ScoreCalculator(
        CombinationEvaluator evaluator, 
        CharmsService charmsService,  
        DiceTableController tableController, 
        ResourceManager resourceManager)
    {
        _evaluator = evaluator;
        _charmsService = charmsService;
        _tableController = tableController;
        _resourceManager = resourceManager;
    }
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 

    public ScoreSequencePlan Calculate(List<Dice> selectedDices)
    {
        var combination = _evaluator.Evaluate(selectedDices);
        if (combination == null)
            return default;

        var steps = new List<ScoreStep>();
        var totalPoints = combination.BasePoints;
        var totalMultiplier = combination.Multiplier;

        // 1. OnDiceScored
        foreach (var dice in selectedDices)
        {
            var diceValue = dice.Data.RolledValue;
            totalPoints += diceValue;
            steps.Add(new(ScoreStepType.DiceScored, diceValue, sourceTransform: dice.transform));
            
            //Charms
            var context = new DiceScoreContext(dice, combination);
            foreach (var charm in _charmsService.ActiveCharms)
                charm.OnDiceScored(context); 
            
            AddTriggers(context.Triggers, steps, ref totalPoints, ref totalMultiplier);
        }
        
        // 2. OnCombinationScored
        steps.Add(new(ScoreStepType.CombinationBase, 0, 0));    
        
        //Charms
        var combContext = new CombinationScoreContext(combination, selectedDices);
        foreach (var charm in _charmsService.ActiveCharms)
            charm.OnCombinationScored(combContext);
        
        AddTriggers(combContext.Triggers, steps, ref totalPoints, ref totalMultiplier);
        
        // 3. OnFinalizeScore
        //Charms
        var finalContext = new FinalizeScoreContext(
            totalPoints, 
            totalMultiplier, 
            _resourceManager.RerollsCount, 
            _resourceManager.DiceCount, 
            _tableController.AllDices, 
            selectedDices);
        
        foreach (var charm in _charmsService.ActiveCharms)
            charm.OnFinalizeScore(finalContext);
        
        AddTriggers(finalContext.Triggers, steps, ref totalPoints, ref totalMultiplier);
        
        // 4. Multiply to final score
        steps.Add(new(ScoreStepType.Multiply));
        
        return new ScoreSequencePlan(combination, totalPoints, totalMultiplier, steps);
    }
    
    private void AddTriggers(List<CharmTriggerResult> triggers, List<ScoreStep> steps, ref int points, ref int mult)
    {
        for (var i = 0; i < triggers.Count; i++)
        {
            var t = triggers[i];
            points += t.BonusPoints;
            mult += t.BonusMultiplier;

            if (t.XMultiplier > 1f)
                mult = Mathf.RoundToInt(mult * t.XMultiplier);

            steps.Add(new(ScoreStepType.CharmTrigger, t.BonusPoints, t.BonusMultiplier, t.XMultiplier, t.View?.transform, t.View));
        }
    }
}