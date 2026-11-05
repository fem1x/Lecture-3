using UnityEngine;

namespace _Scripts.Combinations.Rules
{
    public enum ComparisonType
    {
        GreaterOrEqual,
        LessOrEqual,
        Equal,
    }
    
    [System.Serializable]
    public class SumRule : CombinationRuleBase
    {
        [field: SerializeField] public ComparisonType Comparison { get; private set; }
        [field: SerializeField] public int RequiredSum { get; private set; }

        protected override bool CheckRule(DiceListAnalyzer analyzer)
        {
            return Comparison switch
            {
                ComparisonType.Equal => analyzer.TotalSum == RequiredSum,
                ComparisonType.LessOrEqual => analyzer.TotalSum <= RequiredSum,
                ComparisonType.GreaterOrEqual => analyzer.TotalSum >= RequiredSum,
                _ => false
            };
        }
    }
}