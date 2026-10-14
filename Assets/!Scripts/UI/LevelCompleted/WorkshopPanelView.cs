using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class WorkshopPanelView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Controls")]
        [SerializeField] private Button _nextLevelButton;

        [Header("Panels")]
        [SerializeField] private DiceRepairPanelView _diceRepairPanel;
        [SerializeField] private CharmConveyorView _charmConveyor;

        public event Action OnNextLevelRequested;
        
        private CanvasGroup _canvasGroup;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake() => _canvasGroup = GetComponent<CanvasGroup>();

        private void OnEnable() => _nextLevelButton.onClick.AddListener(HandleNextLevelButtonClick);
        private void OnDisable() => _nextLevelButton.onClick.RemoveListener(HandleNextLevelButtonClick);

        public void SetVisible(bool isVisible)
        {
            _canvasGroup.alpha = isVisible ? 1f : 0f;
            _canvasGroup.blocksRaycasts = isVisible;
            _canvasGroup.interactable = isVisible;
        }

        public void SetNextLevelInteractable(bool interactable) => _nextLevelButton.interactable = interactable;

        private void HandleNextLevelButtonClick() => OnNextLevelRequested?.Invoke();
    }
}