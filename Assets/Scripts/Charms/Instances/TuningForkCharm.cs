using System.Collections.Generic;
using _Scripts.Combinations;
using _Scripts.Data_Transfer.Contexts;
using _Scripts.Dices;
using UnityEngine;

namespace _Scripts.Charms
{
    public class TuningForkCharm : CharmBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly TuningForkCharmConfig _config;

        public TuningForkCharm(TuningForkCharmConfig config) : base(config) => _config = config;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override void OnCombinationScored(CombinationScoreContext context)
        {
            if (context.Combination.IsSet)
                context.AddTrigger(View, xMultiplier: _config.XMultiplier);
        }

        public override Dice SelectDurabilityDamageTarget(CombinationConfig combination, IReadOnlyList<Dice> dices)
        {
            return combination.IsSet ? dices[Random.Range(0, dices.Count)] : null;
        }
    }
}
