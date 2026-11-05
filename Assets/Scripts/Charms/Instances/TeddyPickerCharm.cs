using _Scripts.Data_Transfer.Contexts;
using VContainer;

namespace _Scripts.Charms
{
    public class TeddyPickerCharm : CharmBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly TeddyPickerCharmConfig _config;
        public TeddyPickerCharm(TeddyPickerCharmConfig config) : base(config) => _config = config;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public override void OnCombinationScored(CombinationScoreContext context)
        {
            if (context.SelectedDices.Count <= _config.MaxDicesThreshold)
                context.AddTrigger(View, xMultiplier: _config.XMultiplier);
        }
    }
}