using UnityEngine;

namespace _Scripts.Charms
{
    [CreateAssetMenu(fileName = "Metronome", menuName = "Configs/Charms/Metronome")]
    public class MetronomeCharmConfig : CharmConfigBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public int EvenBonusMultiplier { get; private set; } = 5;
        [field: SerializeField] public int OddBonusPoints { get; private set; } = 30;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override CharmBase CreateCharmInstance() => new MetronomeCharm(this);
    }
}
