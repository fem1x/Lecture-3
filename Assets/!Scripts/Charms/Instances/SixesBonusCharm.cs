using _Scripts.Data_Transfer.Contexts;

namespace _Scripts.Charms
{
    public class SixesBonusCharm : CharmBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly SixesBonusCharmConfig _config;
        public SixesBonusCharm(SixesBonusCharmConfig config) : base(config)
        {
            _config = config;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override void OnDiceScored(DiceScoreContext context)
        {
            if (context.Dice.Data.RolledValue != _config.TargetValue)
                return;
            
            context.AddTrigger(View, _config.BonusPoints, _config.BonusMultiplier);
        }
    }
}