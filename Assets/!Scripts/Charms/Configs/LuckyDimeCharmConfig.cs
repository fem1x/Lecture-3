using UnityEngine;

namespace _Scripts.Charms
{
    [CreateAssetMenu(fileName = "LuckyDime", menuName = "Configs/Charms/Lucky Dime")]

    public class LuckyDimeCharmConfig : CharmConfigBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public int ImmuneValue { get; private set; } = 1;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public override CharmBase CreateCharmInstance() => new LuckyDimeCharm(this);
    }
}