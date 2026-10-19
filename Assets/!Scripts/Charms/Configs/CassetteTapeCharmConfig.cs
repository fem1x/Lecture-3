using UnityEngine;

namespace _Scripts.Charms
{
    [CreateAssetMenu(fileName = "CassetteTape", menuName = "Configs/Charms/Cassette Tape")]
    public class CassetteTapeCharmConfig : CharmConfigBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public float XMultiplierPerStep { get; private set; } = 1.25f;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override CharmBase CreateCharmInstance() => new CassetteTapeCharm(this);
    }
}