using System.Collections.Generic;
using _Scripts.UI.Charms;
using UnityEngine;
using VContainer;

namespace _Scripts.Charms
{
    public class ActiveCharmsPanelView : MonoBehaviour
    {
        [SerializeField] private CharmEquippedView _charmViewPrefab;
        [SerializeField] private Transform _container;

        private readonly Dictionary<CharmBase, CharmGeneralView> _views = new();

        private CharmsService _charmsService;

        [Inject]
        public void Construct(CharmsService charmsService)
        {
            _charmsService = charmsService;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
    }
}