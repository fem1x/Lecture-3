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
        [SerializeField] private RectTransform _slotRect;
        [SerializeField] private CharmEquippedView _charmPrefab;
        [SerializeField] private Transform _charmHolder;
        [SerializeField] private GameObject _emptyPlaceholder;
        public bool IsEmpty => _currentEquipped == null;

        private CharmEquippedView _currentEquipped;
        private CharmBase _activeCharm;
        
        #region DI
        private CharmReplaceView _replaceView;
        private CharmsConveyorService _conveyorService;
        private IObjectResolver _resolver;
        
        [Inject]
        public void Construct(
            CharmsConveyorService conveyorService, 
            IObjectResolver resolver, 
            CharmReplaceView replaceView)
        {
            _conveyorService = conveyorService;
            _resolver = resolver;
            _replaceView = replaceView;
        }
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null) return;
            if (!eventData.pointerDrag.TryGetComponent<CharmDragView>(out var dragView)) return;
            
            if (IsEmpty)
            {
                TryEquip(dragView);
                return;
            }

            dragView.StartPending();
            _replaceView.Show(
                _slotRect, 
                _currentEquipped.CharmGeneralView, 
                dragView.GeneralView,
                onConfirm: () => Replace(dragView),
                onCancel: () => dragView.StopPending()
            );
        }

        private void TryEquip(CharmDragView dragView)
        {
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

        private void Replace(CharmDragView dragView)
        {
            var oldCharm = _activeCharm;
            Clear();

            var newCharm = _conveyorService.Replace(oldCharm, dragView.GeneralView.Config);
            Equip(newCharm);
    
            dragView.OnSuccessfullyEquipped();
        }
    }
}