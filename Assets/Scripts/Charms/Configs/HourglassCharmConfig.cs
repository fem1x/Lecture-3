using UnityEngine;

namespace _Scripts.Charms
{
    [CreateAssetMenu(fileName = "Hourglass", menuName = "Configs/Charms/Hourglass")]
    public class HourglassCharmConfig : CharmConfigBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public float XMultiplier { get; private set; } = 1.6f;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override CharmBase CreateCharmInstance() => new HourglassCharm(this);
    }
}