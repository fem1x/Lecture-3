using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Configs
{
    [Serializable]
    public class LevelSection
    {
        [field: SerializeField]
        public List<LevelConfig> Levels { get; private set; } = new();
    }
    
    [CreateAssetMenu(fileName = "LevelsListConfig", menuName = "Configs/Level/LevelsListConfig")]
    public class LevelsListConfig : ScriptableObject
    {
        [field: SerializeField] public List<LevelSection> Sections { get; private set; } = new();
    }
}