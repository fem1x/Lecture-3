using _Scripts;
using _Scripts.Configs;
using _Scripts.Dices;
using _Scripts.Interfaces;
using _Scripts.Managers;
using _Scripts.UI;
using _Scripts.Utility;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [Header("Configs")]
    [SerializeField] DiceRollConfig _diceRollConfig;
    [SerializeField] DiceSpawnConfig _diceSpawnConfig;
    [SerializeField] CombinationScoreConfig  _combinationScoreConfig;
    
    [Header("Scene")]
    [SerializeField] private DiceManager _diceManager;
    [SerializeField] private DiceSpawnPoint _spawnPoint;
    [SerializeField] private CombinationsPanelView _combinationsPanel;
    [SerializeField] private ScoreView _scoreText;
    
    protected override void Configure(IContainerBuilder builder)
    {
        RegisterConfigs(builder);
        RegisterServices(builder);
        //RegisterClients(builder);
    }
    
    private void RegisterConfigs(IContainerBuilder builder)
    {
        builder.RegisterInstance(_diceRollConfig);
        builder.RegisterInstance(_diceSpawnConfig);
        builder.RegisterInstance(_combinationScoreConfig);
    }
    
    private void RegisterServices(IContainerBuilder builder)
    {
        builder.RegisterComponent(_spawnPoint);
        builder.RegisterComponent(_diceManager);
        builder.RegisterComponent(_combinationsPanel);
        builder.RegisterComponent(_scoreText);
        builder.Register<DiceSpawner>(Lifetime.Singleton);
        builder.Register<DiceRoller>(Lifetime.Singleton);
        builder.Register<CombinationEvaluator>(Lifetime.Singleton);
        builder.Register<ScoreCalculator>(Lifetime.Singleton).As<IScoreCalculator>();
        builder.Register<ScoreManager>(Lifetime.Singleton);
        
        builder.RegisterEntryPoint<Bootstrap>();
    }

    private void RegisterClients(IContainerBuilder builder)
    {
    }
}
