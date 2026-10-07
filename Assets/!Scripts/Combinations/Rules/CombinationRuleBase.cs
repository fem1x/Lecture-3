using System.Collections.Generic;
using _Scripts.Dices;
using _Scripts.Interfaces;
using UnityEngine;

namespace _Scripts.Combinations.Rules
{
    [System.Serializable]
    public abstract class CombinationRuleBase : ICombinationRule
    {
        [field: SerializeField] public int RequiredDiceCount { get; private set; }

        public bool Matches(IReadOnlyList<Dice> dices)
        {
            if (dices == null || dices.Count != RequiredDiceCount)
                return false;

            var analyzer = new DiceListAnalyzer(dices);
            if (analyzer.ValidDiceCount != RequiredDiceCount)
                return false;

            return CheckRule(analyzer);
        }

        protected abstract bool CheckRule(DiceListAnalyzer analyzer);
    }
}