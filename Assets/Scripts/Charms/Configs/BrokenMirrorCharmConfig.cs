using UnityEngine;

namespace _Scripts.Charms
{
    [CreateAssetMenu(fileName = "BrokenMirror", menuName = "Configs/Charms/Broken Mirror")]
    public class BrokenMirrorCharmConfig : CharmConfigBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public int BonusPoints { get; private set; } = 40;
        [field: SerializeField] public float XMultiplier { get; private set; } = 1.8f;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override CharmBase CreateCharmInstance() => new BrokenMirrorCharm(this);
    }
}
