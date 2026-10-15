using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Charms
{
    public class CharmsService
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly List<Charm> _activeCharms = new();
        
        public int MaxSlots { get; private set; } = 3;
        public IReadOnlyList<Charm> ActiveCharms => _activeCharms;
        
        public event Action<Charm> OnCharmAdded;
        public event Action<Charm> OnCharmRemoved;
        public event Action<int> OnMaxSlotsChanged;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private bool CanAddCharm => _activeCharms.Count < MaxSlots;
        private bool CanUpgradeSlots => MaxSlots < 6;

        public bool TryUpgradeSlots()
        {
            if (!CanUpgradeSlots)
            {
                Debug.LogWarning("MaxSlots is already maxed!");
                return false;
            }
            
            MaxSlots++;
            OnMaxSlotsChanged?.Invoke(MaxSlots);
            return true;
        }
        
        public bool TryAddCharm(CharmConfigBase charmConfig)
        {
            if (!CanAddCharm)
            {
                Debug.LogWarning("No more charm slots available!");
                return false;
            }
            
            var charm = charmConfig.CreateCharmInstance();
            _activeCharms.Add(charm);
            OnCharmAdded?.Invoke(charm);
            return true;
        }

        public void RemoveCharm(Charm charm)
        {
            _activeCharms.Remove(charm);
            OnCharmRemoved?.Invoke(charm);
        }

        public void ReplaceCharm(Charm oldCharm, CharmConfigBase newCharmConfig)
        {
            RemoveCharm(oldCharm);
            TryAddCharm(newCharmConfig);
        }
    }
}