using _Scripts.Data_Transfer.Contexts;

namespace _Scripts.Charms
{
    public class PorcelainCharm : CharmBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly PorcelainCharmConfig _config;

        public PorcelainCharm(PorcelainCharmConfig config) : base(config)
        {
            _config = config;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override void OnDiceScored(DiceScoreContext context)
        {
            if (context.Dice.Data.RolledValue != _config.TargetValue) return;

            context.AddTrigger(View, points: _config.BonusPoints, xMultiplier: _config.XMultiplier);
            context.Dice.Data.Degrade();
        }
    }
}