using System.Linq;
using _Scripts.Data_Transfer.Contexts;

namespace _Scripts.Charms
{
    public class MetronomeCharm : CharmBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly MetronomeCharmConfig _config;

        public MetronomeCharm(MetronomeCharmConfig config) : base(config) => _config = config;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override void OnDiceScored(DiceScoreContext context)
        {
            var parity = context.BaseDicePoints % 2;
            if (context.SelectedDices.Any(dice => dice.Data.RolledValue % 2 != parity))
                return;

            if (parity == 0)
                context.AddTrigger(View, multiplier: _config.EvenBonusMultiplier);
            else
                context.AddTrigger(View, points: _config.OddBonusPoints);
        }
    }
}
