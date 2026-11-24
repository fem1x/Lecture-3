using UnityEngine;

namespace _Scripts.Charms
{
    [CreateAssetMenu(fileName = "Megaphone", menuName = "Configs/Charms/Megaphone")]
    public class MegaphoneCharmConfig : CharmConfigBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public int SumThreshold { get; private set; } = 25;
        [field: SerializeField] public float XMultiplierPerStep { get; private set; } = 1.2f;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override CharmBase CreateCharmInstance() => new MegaphoneCharm(this);
    }
}
