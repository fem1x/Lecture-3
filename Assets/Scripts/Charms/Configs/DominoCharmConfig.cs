using UnityEngine;

namespace _Scripts.Charms
{
    [CreateAssetMenu(fileName = "Domino", menuName = "Configs/Charms/Domino")]
    public class DominoCharmConfig : CharmConfigBase
    {
        public override CharmBase CreateCharmInstance() => new DominoCharm(this);
    }
}
