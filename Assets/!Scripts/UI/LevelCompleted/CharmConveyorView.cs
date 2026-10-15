using _Scripts.Charms;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI
{
    public class CharmConveyorView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _reverseButton;

        #region DI
        private CharmsConveyorService  _conveyorService;

        [Inject]
        public void Construct(CharmsConveyorService conveyorService)
        {
            _conveyorService = conveyorService;
            _conveyorService.OnStateChanged += RefreshButtonsState;
            RefreshButtonsState();
        }
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void OnEnable()
        {
            _nextButton.onClick.AddListener(HandleNextButtonClick);
            _reverseButton.onClick.AddListener(HandleReverseButtonClick);
        }

        private void OnDisable()
        {
            _nextButton.onClick.RemoveListener(HandleNextButtonClick);
            _reverseButton.onClick.RemoveListener(HandleReverseButtonClick);
        }

        private void OnDestroy()
        {
            _conveyorService.OnStateChanged -= RefreshButtonsState;
        }
        
        private void HandleNextButtonClick() => _conveyorService.MoveNext();
        private void HandleReverseButtonClick() => _conveyorService.MoveReverse();

        private void RefreshButtonsState()
        {
            _nextButton.interactable = _conveyorService.CanMoveNext;
            _reverseButton.interactable = _conveyorService.CanMoveReverse;
        }
    }
}