using _Scripts.Combinations;
using _Scripts.Dices;
using _Scripts.Structs___Enums.Contexts;

namespace _Scripts.Charms
{
    public abstract class Charm
    {
        public CharmConfigBase Config { get; }
        public CharmView View { get; private set; }
        
        protected Charm(CharmConfigBase config)
        {
            Config = config;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public void AttachView(CharmView view) => View = view;
        public void DetachView() => View = null;
        
        public virtual void OnRoll() { }

        public virtual void OnDiceScored(DiceScoreContext context) { }

        public virtual void OnCombinationScored(CombinationScoreContext context) { }

        public virtual void OnFinalizeScore(FinalizeScoreContext context) { }

        public virtual void OnRoundEnd() { }
    }
}