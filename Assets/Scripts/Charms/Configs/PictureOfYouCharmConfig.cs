using UnityEngine;

namespace _Scripts.Charms
{
    [CreateAssetMenu(fileName = "PictureOfYou", menuName = "Configs/Charms/Picture of You")]
    public class PictureOfYouCharmConfig : CharmConfigBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public float XMultiplier { get; private set; } = 2.2f;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override CharmBase CreateCharmInstance() => new PictureOfYouCharm(this);
    }
}
