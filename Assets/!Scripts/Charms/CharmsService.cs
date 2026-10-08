using System;
using System.Collections.Generic;

namespace _Scripts.Charms
{
    public class CharmsService
    {
        private readonly List<Charm> _activeCharms = new();
        public IReadOnlyList<Charm> ActiveCharms => _activeCharms;
        
        public event Action<Charm> OnCharmAdded;
        public event Action<Charm> OnCharmRemoved;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void AddCharm(CharmConfigBase charmConfig)
        {
            var charm = charmConfig.CreateCharmInstance();
            _activeCharms.Add(charm);
            OnCharmAdded?.Invoke(charm);
        }

        public void RemoveCharm(Charm charm)
        {
            _activeCharms.Remove(charm);
            OnCharmRemoved?.Invoke(charm);
        }
        
    }
}