using System.Collections.Generic;

namespace _Scripts.Dices
{
    public struct FoundCombination
    {
        public readonly CombinationType Type;
        public readonly IReadOnlyList<Dice> Dices;

        public FoundCombination(CombinationType type, IReadOnlyList<Dice> dices)
        {
            Type = type;
            Dices = dices;
        }
    }
}