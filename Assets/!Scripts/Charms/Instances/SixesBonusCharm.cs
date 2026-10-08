using _Scripts.Structs___Enums.Contexts;

namespace _Scripts.Charms
{
    public class SixesBonusCharm : Charm
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