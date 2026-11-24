using System.Linq;
using _Scripts.Data_Transfer.Contexts;

namespace _Scripts.Charms
{
    public class PrismCharm : CharmBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly PrismCharmConfig _config;

        public PrismCharm(PrismCharmConfig config) : base(config) => _config = config;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override void OnCombinationScored(CombinationScoreContext context)
        {
            var dices = context.SelectedDices;
            if (dices.Select(dice => dice.Data.RolledValue).Distinct().Count() == dices.Count)
                context.AddTrigger(View, xMultiplier: _config.XMultiplier);
        }
    }
}
