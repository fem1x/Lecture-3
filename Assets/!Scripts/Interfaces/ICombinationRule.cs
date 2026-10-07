using System.Collections.Generic;
using _Scripts.Dices;

namespace _Scripts.Interfaces
{
    public interface ICombinationRule
    {
        bool Matches(IReadOnlyList<Dice> dices);
    }
}