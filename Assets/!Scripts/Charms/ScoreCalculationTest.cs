using System.Collections.Generic;
using _Scripts.Charms;
using _Scripts.Dices;
using _Scripts.Interfaces;
using UnityEngine;
using VContainer;

public class ScoreCalculationTest : MonoBehaviour
{
    [SerializeField] private SixesBonusCharmConfig _sixesCharmConfig;
    [SerializeField] private List<Dice> _testDices;

    #region DI
    private IScoreCalculator _scoreCalculator;
    private CharmsService _charmsService;

    [Inject]
    public void Construct(IScoreCalculator scoreCalculator, CharmsService charmsService)
    {
        _scoreCalculator = scoreCalculator;
        _charmsService = charmsService;
    }
    #endregion

    // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

    [ContextMenu("Test Calculate")]
    public void RunTest()
    {
        // 1. Добавляем чарм в сервис
        _charmsService.AddCharm(_sixesCharmConfig);

        // 2. Считаем очки
        var result = _scoreCalculator.Calculate(_testDices);

        // 3. Смотрим лог
        Debug.Log($"Комбинация: {result.Combination.DisplayName}");
        Debug.Log($"Total Score: {result.TotalScore} (Points: {result.TotalPoints}, Mult: {result.TotalMultiplier})");
        Debug.Log($"Всего кубиков с контекстами: {result.DiceContexts.Count}");

        foreach (var ctx in result.DiceContexts)
        {
            Debug.Log($"Кубик со значением {ctx.Dice.Data.RolledValue} сгенерировал триггеров: {ctx.Triggers.Count}");
        }
    }
}