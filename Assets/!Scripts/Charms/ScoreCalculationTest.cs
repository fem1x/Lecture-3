using System;
using System.Collections.Generic;
using _Scripts.Charms;
using _Scripts.Dices;
using _Scripts.Interfaces;
using _Scripts.Managers;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class ScoreCalculationTest : MonoBehaviour
{
    [SerializeField] private CharmConfigBase _testCharmConfig;
    [SerializeField] private List<Dice> _testDices;

    #region DI
    private IScoreCalculator _scoreCalculator;
    private CharmsService _charmsService;
    private ScoreSequenceController _sequenceController;

    [Inject]
    public void Construct(
        IScoreCalculator scoreCalculator, 
        CharmsService charmsService, 
        ScoreSequenceController sequenceController)
    {
        _scoreCalculator = scoreCalculator;
        _charmsService = charmsService;
        _sequenceController = sequenceController;
    }
    #endregion

    // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

    private void Start()
    {
        _charmsService.AddCharm(_testCharmConfig);
    }
}