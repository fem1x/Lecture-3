using _Scripts.Data_Transfer.Contexts;

namespace _Scripts.Charms
{
    public class HeartShapedBoxCharm : CharmBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly HeartShapedBoxCharmConfig _config;

        public HeartShapedBoxCharm(HeartShapedBoxCharmConfig config) : base(config)
        {
            _config = config;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override void OnDiceScored(DiceScoreContext context)
        {
            var data = context.Dice.Data;
            if (data.CurrentDurability >= data.MaxDurability) return;

            context.AddTrigger(View, multiplier: _config.BonusMultiplier);
        }
    }
}