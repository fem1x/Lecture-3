using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Charms
{
    [CreateAssetMenu(fileName = "Porcelain", menuName = "Configs/Charms/Porcelain")]
    public class PorcelainCharmConfig : CharmConfigBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Stats")]
        [field: SerializeField] public int TargetValue { get; private set; } = 6;
        [field: SerializeField] public int BonusPoints { get; private set; } = 50;
        [field: SerializeField] public float XMultiplier { get; private set; } = 1.4f;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override CharmBase CreateCharmInstance() => new PorcelainCharm(this);
        
        protected override Dictionary<string, object> GetDescriptionArgs() => new()
        {
            { "targetValue", TargetValue },
            { "bonusPoints", BonusPoints },
            { "xMultiplier", XMultiplier.ToString("0.0") },
        };
    }
}