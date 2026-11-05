using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Localization;

namespace _Scripts.Charms
{
    public abstract class CharmConfigBase : ScriptableObject
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public LocalizedString Name { get; private set; }
        [field: SerializeField] private LocalizedString Description { get; set; }
        [field: SerializeField] public Sprite Icon { get; private set; }

        private Dictionary<string, object> _cachedArgs;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public abstract CharmBase CreateCharmInstance();

        public string GetDescription()
        {
            var args = GetDescriptionArgs();
            return args.Count > 0 ? Description.GetLocalizedString(args) : Description.GetLocalizedString();
        }

        private Dictionary<string, object> GetDescriptionArgs()
        {
            if (_cachedArgs != null)
                return _cachedArgs;

            _cachedArgs = new();
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var prop in properties)
            {
                var key = char.ToLowerInvariant(prop.Name[0]) + prop.Name[1..];
                _cachedArgs[key] = prop.GetValue(this);
            }

            return _cachedArgs;
        }

        #if UNITY_EDITOR
        private void OnValidate() => _cachedArgs = null;
        #endif
    }
}