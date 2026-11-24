using System.Collections.Generic;
using _Scripts.Combinations;
using _Scripts.Combinations.Rules;
using _Scripts.Dices;

namespace _Scripts.Charms
{
    public class DominoCharm : CharmBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public DominoCharm(DominoCharmConfig config) : base(config) { }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override bool MatchesCombination(CombinationConfig combination, IReadOnlyList<Dice> dices)
        {
            return combination.Rule is SequenceRule sequence && sequence.Matches(dices, 4);
        }
    }
}
