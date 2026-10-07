using System.Collections.Generic;
using _Scripts.Attributes;
using _Scripts.Dices;
using _Scripts.Interfaces;
using UnityEngine;

namespace _Scripts.Combinations
{
    [System.Serializable]
    public struct DicePatternExample
    {
        [Range(1, 6)] public int[] Values;
    }
    
    [CreateAssetMenu(fileName = "NewCombinationConfig", menuName = "Configs/Combination Config")]
    public class CombinationConfig : ScriptableObject
    {
        [field: Header("General")]
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public int Priority { get; private set; }

        [field: Header("Display")]
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public DicePatternExample[] Patterns { get; private set; }

        [field: Header("Score")]
        [field: SerializeField] public int BasePoints { get; private set; }
        [field: SerializeField] public int Multiplier { get; private set; }
        [field: SerializeField] public int DiceRefund { get; private set; }

        [field: Header("Rule")]
        [field: SubclassSelector]
        [field: SerializeReference] public ICombinationRule Rule { get; private set; }

        public bool Matches(IReadOnlyList<Dice> dices)
        {
            if (Rule == null || dices == null)
                return false;

            return Rule.Matches(dices);
        }
    }
}