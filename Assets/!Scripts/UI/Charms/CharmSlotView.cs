using _Scripts.Charms;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;
using VContainer.Unity;

namespace _Scripts.UI.Charms
{
    public class CharmSlotView : MonoBehaviour, IDropHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private CharmEquippedView _charmPrefab;
        [SerializeField] private Transform _charmHolder;
        [SerializeField] private GameObject _emptyPlaceholder;
        public bool IsEmpty => _currentEquipped == null;

        private CharmEquippedView _currentEquipped;
        private CharmBase _activeCharm;
        
        private CharmsConveyorService _conveyorService;
        private IObjectResolver _resolver;
        
        [Inject]
        public void Construct(CharmsConveyorService conveyorService, IObjectResolver resolver)
        {
            _conveyorService = conveyorService;
            _resolver = resolver;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null) return;
            if (!eventData.pointerDrag.TryGetComponent<CharmDragView>(out var dragView)) return;
            if (!IsEmpty) return;

            if (_conveyorService.TryClaim(dragView.GeneralView.Config, out var createdCharm))
            {
                Equip(createdCharm);
                dragView.OnSuccessfullyEquipped();
            }
            else
            {
                dragView.ReturnToConveyor();
            }
        }

        private void Equip(CharmBase charm)
        {
            _emptyPlaceholder.SetActive(false);
            _activeCharm = charm;

            _currentEquipped = _resolver.Instantiate(_charmPrefab, _charmHolder);
            _currentEquipped.CharmGeneralView.Setup(charm.Config);
            _currentEquipped.PlayTriggerAnimation();

            charm.AttachView(_currentEquipped);
        }
        
        private void Clear()
        {
            if (_currentEquipped != null)
            {
                _activeCharm?.DetachView();
                Destroy(_currentEquipped.gameObject);
                _currentEquipped = null;
                _activeCharm = null;
            }

            _emptyPlaceholder.SetActive(true);
        }
    }
}