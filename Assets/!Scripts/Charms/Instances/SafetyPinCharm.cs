using _Scripts.Data_Transfer.Contexts;
using UnityEngine;

namespace _Scripts.Charms
{
    public class SafetyPinCharm : CharmBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly SafetyPinCharmConfig _config;

        public SafetyPinCharm(SafetyPinCharmConfig config) : base(config)
        {
            _config = config;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override void OnDiceDegrading(DiceDegradeContext context)
        {
            if (Random.value <= _config.SaveChance)
            {
                context.Cancel();
                context.Dice.Data.Repair();
            }
        }
    }
}