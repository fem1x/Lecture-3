using UnityEngine;

namespace _Scripts.Charms
{
    [CreateAssetMenu(fileName = "TeddyPicker", menuName = "Configs/Charms/Teddy Picker")]
    public class TeddyPickerCharmConfig : CharmConfigBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public int MaxDicesThreshold { get; private set; } = 2;
        [field: SerializeField] public float XMultiplier { get; private set; } = 2.0f;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public override CharmBase CreateCharmInstance() => new TeddyPickerCharm(this);
    }
}