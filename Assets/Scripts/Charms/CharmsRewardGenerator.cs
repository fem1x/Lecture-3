using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace _Scripts.Charms
{
    public class CharmsRewardGenerator
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly CharmsPoolService _charmsPoolService;

        [Inject]
        public CharmsRewardGenerator(CharmsPoolService charmsPoolService)
        {
            _charmsPoolService = charmsPoolService;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public List<CharmConfigBase> GenerateCharmsRewards(int count = 3)
        {
            var currentPool = _charmsPoolService.GetAvailableCharms();
            var selectedCharms = new List<CharmConfigBase>();
            
            for (int i = 0; i < count; i++)
            {
                if (currentPool.Count == 0)
                {
                    Debug.LogError("Not enough charms in the pool!");
                    return selectedCharms;
                }
                
                var randomIndex = Random.Range(0, currentPool.Count);
                selectedCharms.Add(currentPool[randomIndex]);
                currentPool.RemoveAt(randomIndex);
            }
            
            return selectedCharms;
        }
    }
}