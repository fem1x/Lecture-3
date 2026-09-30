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

        [Inject]
        public Bootstrap(DiceSpawner diceSpawner, DiceManager diceManager)
        {
            _diceSpawner = diceSpawner;
            _diceManager = diceManager;
        }
        
        public void Initialize()
        {
            var dices = _diceSpawner.SpawnDices();
            _diceManager.Init(dices);
        }
    }
}