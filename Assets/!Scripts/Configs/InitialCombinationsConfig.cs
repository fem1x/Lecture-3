using System.Collections.Generic;
using _Scripts.Combinations;
using UnityEngine;

namespace _Scripts.Configs
{
    [CreateAssetMenu(fileName = "InitialCombinationsConfig", menuName = "Configs/InitialCombinationsConfig")]
    public class InitialCombinationsConfig : ScriptableObject
    {
        [field: SerializeField] public List<CombinationConfig> DefaultCombinations { get; private set; }
    }
}