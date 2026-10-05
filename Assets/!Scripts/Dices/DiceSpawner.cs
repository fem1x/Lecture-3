using System.Collections.Generic;
using _Scripts.Configs;
using _Scripts.Managers;
using _Scripts.Utility;
using UnityEngine;
using VContainer;

namespace _Scripts.Dices
{
    public class DiceSpawner
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        private readonly DiceSpawnConfig _config;
        private readonly Vector3 _spawnPoint;
        private readonly DiceFactory _diceFactory;
        
        [Inject]
        public DiceSpawner(DiceSpawnConfig config, DiceSpawnPoint spawnPoint, DiceFactory diceFactory)
        {
            _config = config;
            _spawnPoint = spawnPoint.transform.position;
            _diceFactory = diceFactory;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        
        public List<Dice> SpawnDices(int count = 5)
        {
            var dices = new List<Dice>();
            for (int i = 0; i < count; i++)
            {
                var dice = _diceFactory.Create(i);
                dices.Add(dice);
            }
            return dices;
        }
        
        public void ResetDicesToSpawn(List<Dice> dices)
        {
            int count = dices.Count;
            for (int i = 0; i < count; i++)
            {
                var dice = dices[i];
                if (dice == null) continue;

                var startingPosition = GetDiceStartingPosition(i, count);
                var startingRotation = _config.RandomizeStartingRotation ? Random.rotation : Quaternion.identity;

                dice.PrepareForRoll();
                dice.TeleportTo(startingPosition, startingRotation);
            }
        }
        
        private Vector3 GetDiceStartingPosition(int i, int totalCount)
        {
            float centerOffset = (totalCount - 1) * 0.5f;
            var offset = Vector3.right * _config.Spacing * (i - centerOffset);
            return _spawnPoint + offset;
        }
    }
}