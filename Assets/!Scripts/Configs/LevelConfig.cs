using System.Collections.Generic;
using _Scripts.Combinations;
using UnityEngine;

namespace _Scripts.Configs
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Configs/Level/LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        [Header("General stats")]
        [field: SerializeField] public int ScoreQuota { get; private set; } = 1000;
        [field: SerializeField] public int TokensReward { get; private set; } = 1;
        
        [Header("Code Lock")]
        [field: SerializeField] public List<CombinationConfig> LockCombinationsPool { get; private set; }
        
        public CombinationConfig GetRandomLock() => LockCombinationsPool[Random.Range(0, LockCombinationsPool.Count)];
        public bool HasLock => LockCombinationsPool.Count > 0;
    }
}