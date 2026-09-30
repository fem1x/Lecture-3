using _Scripts.Dices;
using UnityEngine;

namespace _Scripts.Configs
{
    [CreateAssetMenu(fileName = "CombinationScoreConfig", menuName = "Configs/CombinationScoreConfig")]
    public class CombinationScoreConfig : ScriptableObject
    {
        [field: SerializeField] public CombinationData[] Combinations { get; private set; }
        
        public CombinationData GetData(CombinationType type)
        {
            for (int i = 0; i < Combinations.Length; i++)
            {
                if (Combinations[i].CombinationType == type)
                    return Combinations[i];
            }

            return default;
        }
    }
}