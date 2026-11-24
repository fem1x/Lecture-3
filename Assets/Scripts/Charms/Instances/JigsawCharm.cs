using System.Collections.Generic;
using _Scripts.Combinations;
using _Scripts.Combinations.Rules;
using _Scripts.Dices;

namespace _Scripts.Charms
{
    public class JigsawCharm : CharmBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public JigsawCharm(JigsawCharmConfig config) : base(config) { }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override bool MatchesCombination(CombinationConfig combination, IReadOnlyList<Dice> dices)
        {
            return combination.Rule is MonoRule mono && mono.Matches(dices, 4);
        }
    }
}
