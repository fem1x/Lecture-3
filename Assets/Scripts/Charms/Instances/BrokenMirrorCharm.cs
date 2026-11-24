using _Scripts.Data_Transfer.Contexts;

namespace _Scripts.Charms
{
    public class BrokenMirrorCharm : CharmBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly BrokenMirrorCharmConfig _config;

        public BrokenMirrorCharm(BrokenMirrorCharmConfig config) : base(config) => _config = config;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override void OnDiceScored(DiceScoreContext context)
        {
            if (context.Dice.Data.CurrentDurability == 1)
                context.AddTrigger(View, points: _config.BonusPoints, xMultiplier: _config.XMultiplier);
        }
    }
}
