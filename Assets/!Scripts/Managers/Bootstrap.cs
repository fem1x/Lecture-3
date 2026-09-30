using System.Collections.Generic;
using _Scripts.Configs;
using _Scripts.Dices;
using VContainer;
using VContainer.Unity;

namespace _Scripts.Managers
{
    public class Bootstrap : IInitializable
    {
        private readonly DiceSpawner _diceSpawner;
        private readonly DiceTableController _diceTableController;
        private readonly ScoreManager _scoreManager;
        private readonly ResourceManager _resourceManager;

        [Inject]
        public Bootstrap(DiceSpawner diceSpawner, DiceTableController diceTableController, ScoreManager scoreManager, ResourceManager resourceManager)
        {
            _diceSpawner = diceSpawner;
            _diceTableController = diceTableController;
            _scoreManager = scoreManager;
            _resourceManager = resourceManager;
        }
        
        public void Initialize()
        {
            var dices = _diceSpawner.SpawnDices();
            _diceTableController.InitDiceList(dices);
            _resourceManager.SetInitialCounts();
            
            
            _scoreManager.SetQuota(1000);
            _scoreManager.ResetScore();
        }
    }
}