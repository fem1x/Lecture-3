using System.Linq;
using _Scripts.Data_Transfer.Contexts;

namespace _Scripts.Charms
{
    public class MegaphoneCharm : CharmBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly MegaphoneCharmConfig _config;

        public MegaphoneCharm(MegaphoneCharmConfig config) : base(config) => _config = config;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override void OnCombinationScored(CombinationScoreContext context)
        {
            if (context.Combination.Id != "overload")
                return;

            var excess = context.SelectedDices.Sum(dice => dice.Data.RolledValue) - _config.SumThreshold;
            for (var i = 0; i < excess; i++)
                context.AddTrigger(View, xMultiplier: _config.XMultiplierPerStep);
        }
    }
}
