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
        private readonly DiceManager _diceManager;
        private readonly ScoreManager _scoreManager;

        [Inject]
        public Bootstrap(DiceSpawner diceSpawner, DiceManager diceManager, ScoreManager scoreManager)
        {
            _diceSpawner = diceSpawner;
            _diceManager = diceManager;
            _scoreManager = scoreManager;
        }
        
        public void Initialize()
        {
            var dices = _diceSpawner.SpawnDices();
            _diceManager.Init(dices);
            
            _scoreManager.SetQuota(1000);
            _scoreManager.ResetScore();
        }
    }
}