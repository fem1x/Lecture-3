using UnityEngine;

namespace _Scripts.Charms
{
    [CreateAssetMenu(fileName = "Prism", menuName = "Configs/Charms/Prism")]
    public class PrismCharmConfig : CharmConfigBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public float XMultiplier { get; private set; } = 1.8f;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override CharmBase CreateCharmInstance() => new PrismCharm(this);
    }
}
