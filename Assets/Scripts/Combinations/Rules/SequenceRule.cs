namespace _Scripts.Combinations.Rules
{
    [System.Serializable]
    public class SequenceRule : CombinationRuleBase
    {
        protected override bool CheckRule(DiceListAnalyzer analyzer)
        {
            var maxLength = 0;
            for (int i = 1; i <= 6; i++)
            {
                if (analyzer.Frequencies[i] == 1)
                {
                    maxLength++;
                    if (maxLength == RequiredDiceCount) return true;
                }
                else
                {
                    maxLength = 0;
                }
            }
            return false;
        }
    }
}