using System.Collections.Generic;
using _Scripts.Dices;
using _Scripts.Interfaces;
using UnityEngine;

namespace _Scripts.Combinations.Rules
{
    [System.Serializable]
    public class DumpRule : ICombinationRule
    {
        [field: SerializeField] public int MinDiceCount { get; private set; } = 1;
        [field: SerializeField] public int MaxDiceCount { get; private set; } = 5;

        public bool Matches(IReadOnlyList<Dice> dices)
        {
            var count = dices.Count;
            if (count < MinDiceCount || count > MaxDiceCount)
                return false;

            var analyzer = new DiceListAnalyzer(dices);
            return analyzer.ValidDiceCount == count;
        }
    }
}