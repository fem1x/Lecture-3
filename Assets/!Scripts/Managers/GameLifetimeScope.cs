using _Scripts;
using _Scripts.Configs;
using _Scripts.Managers;
using _Scripts.Utility;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [Header("Configs")]
    [SerializeField] DiceRollConfig _diceRollConfig;
    [SerializeField] DiceSpawnConfig _diceSpawnConfig;
    
    [Header("Scene Objects")]
    [SerializeField] private DiceSpawnPoint _spawnPoint;
    
    protected override void Configure(IContainerBuilder builder)
    {
        RegisterConfigs(builder);
        RegisterServices(builder);
        RegisterClients(builder);
    }
    
    private void RegisterConfigs(IContainerBuilder builder)
    {
        builder.RegisterInstance(_diceRollConfig);
        builder.RegisterInstance(_diceSpawnConfig);
    }
    
    private void RegisterServices(IContainerBuilder builder)
    {
        builder.RegisterComponent(_spawnPoint);
        builder.Register<DiceSpawner>(Lifetime.Singleton);
        builder.Register<DiceRoller>(Lifetime.Singleton);
        
        builder.Register<Bootstrap>(Lifetime.Singleton).As<IInitializable>();
    }

    private void RegisterClients(IContainerBuilder builder)
    {
    }
}
