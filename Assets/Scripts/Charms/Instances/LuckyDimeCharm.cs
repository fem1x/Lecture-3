using _Scripts.Data_Transfer.Contexts;

namespace _Scripts.Charms
{
    public class LuckyDimeCharm : CharmBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly LuckyDimeCharmConfig _config;
        public LuckyDimeCharm(LuckyDimeCharmConfig config) : base(config) => _config = config;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override void OnDiceScored(DiceScoreContext context)
        {
            if (context.Dice.Data.RolledValue == _config.ImmuneValue)
                context.AddTrigger(View, points: _config.BonusPoints);
        }

        public override void OnPreTakeDamage(DiceDamageContext context)
        {
            if (context.Dice.Data.RolledValue == _config.ImmuneValue)
                context.CancelDamage();
        }
    }
}