using UnityEngine;

namespace _Scripts.Charms
{
    [CreateAssetMenu(fileName = "Airbag", menuName = "Configs/Charms/Airbag")]
    public class AirbagCharmConfig : CharmConfigBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public int MaxDicesThreshold { get; private set; } = 2;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public override CharmBase CreateCharmInstance() => new AirbagCharm(this);
    }
}