using _Scripts.Data_Transfer.Contexts;
using VContainer;

namespace _Scripts.Charms
{
    public class AirbagCharm : CharmBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly AirbagCharmConfig _config;
        public AirbagCharm(AirbagCharmConfig config) : base(config) => _config = config;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override void OnPreTakeDamage(DiceDamageContext context)
        {
            if (context.PlayedDices.Count <= _config.MaxDicesThreshold)
                context.CancelDamage();
        }
    }
}