using _Scripts.Data_Transfer.Contexts;

namespace _Scripts.Charms
{
    public class VelvetGloveCharm : CharmBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly VelvetGloveCharmConfig _config;

        public VelvetGloveCharm(VelvetGloveCharmConfig config) : base(config)
        {
            _config = config;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override void OnCombinationScored(CombinationScoreContext context)
        {
            foreach (var dice in context.SelectedDices)
            {
                var data = dice.Data;
                if (data.CurrentDurability < data.MaxDurability || data.DamageLevel > 0)
                    return;
            }

            context.AddTrigger(View, xMultiplier: _config.XMultiplier);
        }
    }
}