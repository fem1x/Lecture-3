using System.Collections.Generic;
using _Scripts.Configs;
using VContainer;
using VContainer.Unity;

namespace _Scripts.Managers
{
    public class Bootstrap : IInitializable
    {
        private readonly DiceSpawner _diceSpawner;

        [Inject]
        public Bootstrap(DiceSpawner diceSpawner)
        {
            _diceSpawner = diceSpawner;
        }
        
        public void Initialize()
        {
            var dices = _diceSpawner.SpawnDices();
        }
    }
}