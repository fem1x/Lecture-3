using _Scripts.Combinations;
using _Scripts.Data_Transfer.Contexts;
using _Scripts.Dices;
using _Scripts.UI.Charms;

namespace _Scripts.Charms
{
    public abstract class CharmBase
    {
        public CharmConfigBase Config { get; }
        public CharmEquippedView View { get; private set; }
        
        protected CharmBase(CharmConfigBase config)
        {
            Config = config;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public void AttachView(CharmEquippedView view) => View = view;
        public void DetachView() => View = null;
        
        public virtual void OnRoll() { }
        public virtual void OnDiceScored(DiceScoreContext context) { }
        public virtual void OnCombinationScored(CombinationScoreContext context) { }
        public virtual void OnFinalizeScore(FinalizeScoreContext context) { }
        public virtual void OnRoundEnd() { }
        public virtual void OnPreTakeDamage(DiceDamageContext context) { }
        public virtual void OnDiceDegrading(DiceDegradeContext context) { }
    }
}