using _Scripts.Combinations;
using _Scripts.Dices;

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

        public override void OnDiceScored(Dice dice, CombinationConfig combination)
        {
            if (dice.Data.RolledValue != _config.TargetValue)
                return;

            
        }
    }
}