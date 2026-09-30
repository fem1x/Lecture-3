using System.Collections.Generic;
using _Scripts.Configs;
using _Scripts.Utility;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Scripts
{
    public class DiceSpawner
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        private readonly DiceSpawnConfig _config;
        private readonly IObjectResolver _resolver;
        private readonly Vector3 _spawnPoint;
        
        [Inject]
        public DiceSpawner(DiceSpawnConfig config, DiceSpawnPoint spawnPoint, IObjectResolver resolver)
        {
            _config = config;
            _spawnPoint = spawnPoint.transform.position;
            _resolver = resolver;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        
        public List<Dice> SpawnDices(int count = 5)
        {
            var dices = new List<Dice>();
            for (int i = 0; i < count; i++)
            {
                var startingRotation = _config.RandomizeStartingRotation ? Random.rotation : Quaternion.identity;
                var startingPosition = GetDiceStartingPosition(i, count);
                
                var dice = _resolver.Instantiate(_config.DicePrefab, startingPosition, startingRotation);
                dices.Add(dice);
            }

            return dices;
        }
        
        private Vector3 GetDiceStartingPosition(int i, int totalCount)
        {
            float centerOffset = (totalCount - 1) * 0.5f;
            var offset = Vector3.right * _config.Spacing * (i - centerOffset);
            return _spawnPoint + offset;
        }
    }
}