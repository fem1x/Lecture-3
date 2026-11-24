using System.Collections.Generic;
using _Scripts.Dices;

namespace _Scripts.Combinations.Rules
{
    [System.Serializable]
    public class SequenceRule : CombinationRuleBase
    {
        public bool Matches(IReadOnlyList<Dice> dices, int diceCount)
            => MatchesWithDiceCount(dices, diceCount);

        protected override bool CheckRule(DiceListAnalyzer analyzer)
        {
            var maxLength = 0;
            for (int i = 1; i <= 6; i++)
            {
                if (analyzer.Frequencies[i] == 1)
                {
                    maxLength++;
                    if (maxLength == analyzer.ValidDiceCount) return true;
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
