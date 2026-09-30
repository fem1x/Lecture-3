using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Configs
{
    [CreateAssetMenu(fileName = "LevelsListConfig", menuName = "Configs/Level/LevelsListConfig")]
    public class LevelsListConfig : ScriptableObject
    {
        [field: SerializeField] public List<LevelConfig> LevelsList {get; private set;}
    }
}