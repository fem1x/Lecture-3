using System.Linq;
using _Scripts.Data_Transfer.Contexts;

namespace _Scripts.Charms
{
    public class HourglassCharm : CharmBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly HourglassCharmConfig _config;
        public HourglassCharm(HourglassCharmConfig config) : base(config) => _config = config;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override void OnCombinationScored(CombinationScoreContext context)
        {
            var diceList = context.SelectedDices;
            var hasOne = diceList.Any(dice => dice.Data.RolledValue == 1);
            var hasSix = diceList.Any(dice => dice.Data.RolledValue == 6);
            
            if (hasOne && hasSix)
                context.AddTrigger(View, xMultiplier:  _config.XMultiplier);
        }
    }
}