using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace _Scripts.Charms
{
    public abstract class CharmConfigBase : ScriptableObject
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("General")]
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public LocalizedString Name { get; private set; }
        [field: SerializeField] public LocalizedString Description { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public abstract CharmBase CreateCharmInstance();
        
        protected virtual Dictionary<string, object> GetDescriptionArgs() => null;
        public string GetDescription()
        {
            var args = GetDescriptionArgs();
            if (args == null || args.Count == 0)
                return Description.GetLocalizedString();

            return Description.GetLocalizedString(args);
        }
    }
}