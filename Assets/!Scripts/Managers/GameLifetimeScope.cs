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
    [SerializeField] ResourceConfig  _resourceConfig;
    [SerializeField] LevelsListConfig  _levelsListConfig;
    
    [Header("Scene")]
    [SerializeField] private TurnFlowController _turnFlowController;
    [SerializeField] private DiceTableController _diceTableController;
    [SerializeField] private DiceSpawnPoint _spawnPoint;
    [SerializeField] private CombinationsPanelView _combinationsPanel;
    [SerializeField] private ScoreView _scoreText;
    
    protected override void Configure(IContainerBuilder builder)
    {
        RegisterConfigs(builder);
        RegisterServices(builder);
    }
    
    private void RegisterConfigs(IContainerBuilder builder)
    {
        builder.RegisterInstance(_diceRollConfig);
        builder.RegisterInstance(_diceSpawnConfig);
        builder.RegisterInstance(_combinationScoreConfig);
        builder.RegisterInstance(_resourceConfig);
        builder.RegisterInstance(_levelsListConfig);
    }
    
    private void RegisterServices(IContainerBuilder builder)
    {
        //Scene objects
        builder.RegisterComponent(_spawnPoint);
        builder.RegisterComponent(_diceTableController);
        builder.RegisterComponent(_turnFlowController);
        builder.RegisterComponent(_combinationsPanel);
        builder.RegisterComponent(_scoreText);
        
        //C# classes
        builder.Register<ResourceManager>(Lifetime.Singleton);
        builder.Register<DiceSpawner>(Lifetime.Singleton);
        builder.Register<DiceRoller>(Lifetime.Singleton);
        builder.Register<CombinationEvaluator>(Lifetime.Singleton);
        builder.Register<ScoreCalculator>(Lifetime.Singleton).As<IScoreCalculator>();
        builder.Register<ScoreManager>(Lifetime.Singleton);
        builder.Register<LevelFlowController>(Lifetime.Singleton);
        
        //Entry Point
        builder.RegisterEntryPoint<Bootstrap>();
    }
}
