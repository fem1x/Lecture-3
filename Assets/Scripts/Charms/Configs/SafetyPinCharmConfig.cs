using UnityEngine;

namespace _Scripts.Charms
{
    [CreateAssetMenu(fileName = "SafetyPin", menuName = "Configs/Charms/Safety Pin")]
    public class SafetyPinCharmConfig : CharmConfigBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField, Range(0f, 1f)] public float SaveChance { get; private set; } = 0.33f;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override CharmBase CreateCharmInstance() => new SafetyPinCharm(this);
    }
}