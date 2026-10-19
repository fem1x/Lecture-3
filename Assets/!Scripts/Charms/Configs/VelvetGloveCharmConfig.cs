using UnityEngine;

namespace _Scripts.Charms
{
    [CreateAssetMenu(fileName = "VelvetGlove", menuName = "Configs/Charms/Velvet Glove")]
    public class VelvetGloveCharmConfig : CharmConfigBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public float XMultiplier { get; private set; } = 1.8f;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override CharmBase CreateCharmInstance() => new VelvetGloveCharm(this);
    }
}