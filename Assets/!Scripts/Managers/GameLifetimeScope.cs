using _Scripts;
using _Scripts.Configs;
using _Scripts.Dices;
using _Scripts.Interfaces;
using _Scripts.Managers;
using _Scripts.Scriptable_Objects.Configs;
using _Scripts.UI;
using _Scripts.Utility;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
    [Header("Configs")]
    [SerializeField] private DiceRollConfig _diceRollConfig;
    [SerializeField] private DiceSpawnConfig _diceSpawnConfig;
    [SerializeField] private CombinationScoreConfig  _combinationScoreConfig;
    [SerializeField] private ResourceConfig  _resourceConfig;
    [SerializeField] private LevelsListConfig  _levelsListConfig;
    [SerializeField] private SfxConfig _sfxConfig;
    [SerializeField] private AudioFilterConfig _audioFilterConfig;
    [SerializeField] private DiceDataConfig _diceDataConfig;
    
    [Header("Scene")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private TurnFlowController _turnFlowController;
    [SerializeField] private DiceTableController _diceTableController;
    [SerializeField] private DiceSpawnPoint _spawnPoint;
    [SerializeField] private CombinationsPanelView _combinationsPanel;
    [SerializeField] private ScoreSequenceController _scoreSequenceController;
    [SerializeField] private DiceNetPanelView _diceNetPanelView;
    [SerializeField] private CurrentCombinationView _currentCombinationView;
    [SerializeField] private DiceStatePanelView _diceStatePanelView;
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
    
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
        builder.RegisterInstance(_sfxConfig);
        builder.RegisterInstance(_audioFilterConfig);
        builder.RegisterInstance(_diceDataConfig);
    }
    
    private void RegisterServices(IContainerBuilder builder)
    {
        //Scene objects
        builder.RegisterComponent(_mainCamera);
        builder.RegisterComponent(_spawnPoint);
        builder.RegisterComponent(_diceTableController);
        builder.RegisterComponent(_turnFlowController);
        builder.RegisterComponent(_combinationsPanel);
        builder.RegisterComponent(_scoreSequenceController);
        builder.RegisterComponent(_diceNetPanelView);
        builder.RegisterComponent(_currentCombinationView);
        builder.RegisterComponent(_diceStatePanelView);
        
        //C# classes
        builder.Register<SfxPlayer>(Lifetime.Singleton).As<ISfxPlayer>();
        builder.Register<AudioFilterController>(Lifetime.Singleton);
        builder.Register<CameraShaker>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        builder.Register<ResourceManager>(Lifetime.Singleton);
        builder.Register<DiceSpawner>(Lifetime.Singleton);
        builder.Register<DiceRoller>(Lifetime.Singleton);
        builder.Register<CombinationEvaluator>(Lifetime.Singleton);
        builder.Register<ScoreCalculator>(Lifetime.Singleton).As<IScoreCalculator>();
        builder.Register<ScoreManager>(Lifetime.Singleton);
        builder.Register<LevelFlowController>(Lifetime.Singleton);
        builder.Register<RepairTokensManager>(Lifetime.Singleton);
        builder.Register<DiceFactory>(Lifetime.Singleton);
        
        //Entry Point
        builder.RegisterEntryPoint<Bootstrap>();
    }
}
