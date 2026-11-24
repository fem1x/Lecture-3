using _Scripts.Data_Transfer.Contexts;

namespace _Scripts.Charms
{
    public class PictureOfYouCharm : CharmBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly PictureOfYouCharmConfig _config;

        public PictureOfYouCharm(PictureOfYouCharmConfig config) : base(config) => _config = config;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override void OnFinalizeScore(FinalizeScoreContext context)
        {
            if (context.RemainingRerolls == 0)
                context.AddTrigger(View, xMultiplier: _config.XMultiplier);
        }
    }
}
