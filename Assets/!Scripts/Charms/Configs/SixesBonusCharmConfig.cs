using UnityEngine;

namespace _Scripts.Charms
{
    [CreateAssetMenu(fileName = "SixesBonusCharmConfig", menuName = "Configs/Charms/Sixes Bonus")]
    public class SixesBonusCharmConfig : CharmConfigBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public int TargetValue { get; private set; } = 6;
        [field: SerializeField] public int BonusPoints { get; private set; } = 25;
        [field: SerializeField] public int BonusMultiplier { get; private set; } = 2;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override Charm CreateCharmInstance() => new SixesBonusCharm(this);
    }
}