using UnityEngine;

namespace _Scripts.Charms
{
    [CreateAssetMenu(fileName = "HeartShapedBox", menuName = "Configs/Charms/Heart-Shaped Box")]
    public class HeartShapedBoxCharmConfig : CharmConfigBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public int BonusMultiplier { get; private set; } = 4;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override CharmBase CreateCharmInstance() => new HeartShapedBoxCharm(this);
    }
}