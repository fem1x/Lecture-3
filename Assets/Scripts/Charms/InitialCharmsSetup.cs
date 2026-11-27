using System;
using _Scripts.Managers;
using _Scripts.UI.Charms;
using UnityEngine;
using VContainer;

namespace _Scripts.Charms
{
    public class InitialCharmsSetup : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private CharmConfigBase[] _initialCharms = Array.Empty<CharmConfigBase>();
        [SerializeField] private Transform _slotsContainer;

        private Bootstrap _bootstrap;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        [Inject]
        public void Construct(Bootstrap bootstrap)
        {
            _bootstrap = bootstrap;
            _bootstrap.OnGameStarted += EquipInitialCharms;
        }

        private void OnDestroy()
        {
            if (_bootstrap != null)
                _bootstrap.OnGameStarted -= EquipInitialCharms;
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void EquipInitialCharms()
        {
            if (_initialCharms.Length == 0) return;

            var slots = _slotsContainer.GetComponentsInChildren<CharmSlotView>(true);
            foreach (var slot in slots)
                slot.ClearForNewGame();

            Array.Sort(slots, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
            var slotIndex = 0;
            foreach (var config in _initialCharms)
            {
                if (config == null) continue;
                if (slotIndex >= 5 || slotIndex >= slots.Length)
                {
                    Debug.LogWarning("Not enough slots for all initial charms. Up to five can be equipped.", this);
                    break;
                }

                slots[slotIndex++].EquipInitialCharm(config);
            }
        }
    }
}
