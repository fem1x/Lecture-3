using _Scripts.Configs;
using _Scripts.Dices;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Scripts.Managers
{
    public class DiceFactory
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        private readonly IObjectResolver _resolver;
        private readonly DiceDataConfig _diceDataConfig;
        private readonly DiceSpawnConfig _spawnConfig;

        [Inject]
        public DiceFactory(
            IObjectResolver resolver, 
            DiceDataConfig diceDataConfig, 
            DiceSpawnConfig spawnConfig)
        {
            _resolver = resolver;
            _diceDataConfig = diceDataConfig;
            _spawnConfig = spawnConfig;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 

        public Dice Create(int id, Vector3 position, Quaternion rotation)
        {
            var dice = _resolver.Instantiate(_spawnConfig.DicePrefab, position, rotation);
            var data = new DiceData(id, _diceDataConfig);
            dice.Init(data);

            return dice;
        }
    }
}