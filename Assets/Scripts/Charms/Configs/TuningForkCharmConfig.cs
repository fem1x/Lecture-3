using UnityEngine;

namespace _Scripts.Charms
{
    [CreateAssetMenu(fileName = "TuningFork", menuName = "Configs/Charms/Tuning Fork")]
    public class TuningForkCharmConfig : CharmConfigBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public float XMultiplier { get; private set; } = 1.5f;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override CharmBase CreateCharmInstance() => new TuningForkCharm(this);
    }
}
