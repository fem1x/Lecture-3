using UnityEngine;

namespace _Scripts.Charms
{
    public abstract class CharmConfigBase : ScriptableObject
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string Title { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public abstract Charm CreateCharmInstance();
    }
}