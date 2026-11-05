namespace _Scripts.Combinations.Rules
{
    [System.Serializable]
    public class TwoDupletsRule : CombinationRuleBase
    {
        protected override bool CheckRule(DiceListAnalyzer analyzer)
        {
            var pairCount = 0;
            for (int i = 1; i <= 6; i++)
            {
                if (analyzer.Frequencies[i] >= 2)
                    pairCount++;
            }

            return pairCount == 2;
        }
    }
}