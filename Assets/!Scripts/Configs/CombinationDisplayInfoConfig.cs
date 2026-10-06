using System;
using _Scripts.Dices;
using UnityEngine;

namespace _Scripts.Configs
{
    [Serializable]
    public struct DicePatternExample
    {
        [Range(1, 6)] public int[] Values;
    }

    [Serializable]
    public struct DisplayInfo
    {
        public CombinationType CombinationType;
        public string Description;
        public DicePatternExample[] Patterns;
    }
    
    [CreateAssetMenu(fileName = "CombinationDisplayInfoConfig", menuName = "Configs/CombinationDisplayInfoConfig")]
    public class CombinationDisplayInfoConfig : ScriptableObject
    {
        [SerializeField] private DisplayInfo[] _displayInfos;

        public DisplayInfo GetDisplayInfo(CombinationType type)
        {
            foreach (var info in _displayInfos)
                if (info.CombinationType == type)
                    return info;

            return default;
        }
    }
}