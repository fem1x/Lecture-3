using UnityEngine;

namespace _Scripts.Charms
{
    [CreateAssetMenu(fileName = "Matryoshka", menuName = "Configs/Charms/Matryoshka")]
    public class MatryoshkaCharmConfig : CharmConfigBase
    {
        public override CharmBase CreateCharmInstance() => new MatryoshkaCharm(this);
    }
}
