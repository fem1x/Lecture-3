using System.Linq;
using _Scripts.Data_Transfer.Contexts;
using UnityEngine.UI;

namespace _Scripts.Charms
{
    public class CassetteTapeCharm : CharmBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly CassetteTapeCharmConfig _config;
        public CassetteTapeCharm(CassetteTapeCharmConfig config) : base(config) => _config = config;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override void OnCombinationScored(CombinationScoreContext context)
        {
            if (context.Combination.Id != "underload") return;
            
            var totalDamageLevel = context.SelectedDices.Sum(dice => dice.Data.DamageLevel);
            for (var i = 0; i < totalDamageLevel; i++)
            {
                context.AddTrigger(View, xMultiplier: _config.XMultiplierPerStep);
            }
        }
    }
}