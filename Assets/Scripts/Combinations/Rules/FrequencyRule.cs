using _Scripts.Combinations;
using _Scripts.Combinations.Rules;
using UnityEngine;

namespace _Scripts.Interfaces
{
    [System.Serializable]
    public class FrequencyRule : CombinationRuleBase
    {
        [field: SerializeField] public int RequiredFrequency { get; private set; }

        protected override bool CheckRule(DiceListAnalyzer analyzer)
        {
            return analyzer.HasFrequency(RequiredFrequency);
        }
    }
}