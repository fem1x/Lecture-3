using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace _Scripts.Charms
{
    public class ActiveCharmsPanelView : MonoBehaviour
    {
        [SerializeField] private CharmView _charmViewPrefab;
        [SerializeField] private Transform _container;

        private readonly Dictionary<Charm, CharmView> _views = new();

        private CharmsService _charmsService;

        [Inject]
        public void Construct(CharmsService charmsService)
        {
            _charmsService = charmsService;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void OnEnable()
        {
            _charmsService.OnCharmAdded += HandleCharmAdded;
            _charmsService.OnCharmRemoved += HandleCharmRemoved;
        }

        private void OnDisable()
        {
            _charmsService.OnCharmAdded -= HandleCharmAdded;
            _charmsService.OnCharmRemoved -= HandleCharmRemoved;
        }

        private void HandleCharmAdded(Charm charm)
        {
            var view = Instantiate(_charmViewPrefab, _container);
            view.Init(charm.Config.Icon);
            charm.AttachView(view);
            _views.Add(charm, view);
        }

        private void HandleCharmRemoved(Charm charm)
        {
            if (_views.Remove(charm, out var view))
            {
                charm.DetachView();
                Destroy(view.gameObject);
            }
        }
    }
}