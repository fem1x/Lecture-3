using UnityEngine;

namespace _Scripts.Combinations.Rules
{
    [System.Serializable]
    public class FullHouseRule : CombinationRuleBase
    {
        protected override bool CheckRule(DiceListAnalyzer analyzer)
        {
            bool hasThree = false;
            bool hasTwo = false;

            for (int i = 1; i <= 6; i++)
            {
                if (analyzer.Frequencies[i] == 3) 
                    hasThree = true;
                else if (analyzer.Frequencies[i] == 2) 
                    hasTwo = true;
            }

            return hasThree && hasTwo;
        }
    }
}