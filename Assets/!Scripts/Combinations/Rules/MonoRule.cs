namespace _Scripts.Combinations.Rules
{
    [System.Serializable]
    public class MonoRule : CombinationRuleBase
    {
        protected override bool CheckRule(DiceListAnalyzer analyzer)
        {
            var freqs = analyzer.Frequencies;
            
            var evenCount = freqs[2] + freqs[4] + freqs[6];
            var oddCount = freqs[1] + freqs[3] + freqs[5];
            
            return evenCount == 0 ||  oddCount == 0;
        }
    }
}