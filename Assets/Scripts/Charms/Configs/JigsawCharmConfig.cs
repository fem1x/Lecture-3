using UnityEngine;

namespace _Scripts.Charms
{
    [CreateAssetMenu(fileName = "Jigsaw", menuName = "Configs/Charms/Jigsaw")]
    public class JigsawCharmConfig : CharmConfigBase
    {
        public override CharmBase CreateCharmInstance() => new JigsawCharm(this);
    }
}
