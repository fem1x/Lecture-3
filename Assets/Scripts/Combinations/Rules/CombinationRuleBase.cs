using System.Collections.Generic;
using _Scripts.Dices;
using _Scripts.Interfaces;
using UnityEngine;

namespace _Scripts.Combinations.Rules
{
    [System.Serializable]
    public abstract class CombinationRuleBase : ICombinationRule
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public int RequiredDiceCount { get; private set; }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public bool Matches(IReadOnlyList<Dice> dices) => MatchesWithDiceCount(dices, RequiredDiceCount);

        protected bool MatchesWithDiceCount(IReadOnlyList<Dice> dices, int diceCount)
        {
            if (dices == null || dices.Count != diceCount)
                return false;

            var analyzer = new DiceListAnalyzer(dices);
            if (analyzer.ValidDiceCount != diceCount)
                return false;

            return CheckRule(analyzer);
        }

        protected abstract bool CheckRule(DiceListAnalyzer analyzer);
    }
}
