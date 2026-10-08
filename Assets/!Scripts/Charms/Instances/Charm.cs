using _Scripts.Combinations;
using _Scripts.Dices;

namespace _Scripts.Charms
{
    public abstract class Charm
    {
        public CharmConfigBase Config { get; }
        
        protected Charm(CharmConfigBase config)
        {
            Config = config;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public virtual void OnRoll() { }

        public virtual void OnDiceScored(Dice dice, CombinationConfig combination) { }

        public virtual void OnCombinationScored(CombinationConfig combination) { }

        public virtual void OnFinalizeScore() { }

        public virtual void OnRoundEnd() { }
    }
}