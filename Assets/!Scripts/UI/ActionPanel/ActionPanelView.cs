using System.Collections.Generic;
using _Scripts.Combinations;
using _Scripts.Dices;
using _Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using VContainer;

namespace _Scripts.UI
{
    public class ActionPanelView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("UI Elements")]
        [SerializeField] private TMP_Text _combinationNameText;
        [SerializeField] private LocalizedString _placeholderText;
        
        [Header("Widgets")]
        [SerializeField] private GameObject _pointsWidget;
        [SerializeField] private GameObject _lockWidget;
        
        private ActiveSelectionService _selectionService;
        private CodeLockService _codeLockService;

        [Inject]
        public void Construct(ActiveSelectionService selectionService, CodeLockService codeLockService)
        {
            _selectionService = selectionService;
            _codeLockService = codeLockService;
            
            _selectionService.OnSelectionEvaluated += HandleSelection;
            _codeLockService.OnLockBroken += HandleLockBroken;
            _codeLockService.OnLockSet += HandleLockSet;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        private void Start()
        {
            UpdateWidgets(_codeLockService.IsActive);
            SetPlaceholder();
        }
        
        private void OnDestroy()
        {
            _selectionService.OnSelectionEvaluated -= HandleSelection;
            _codeLockService.OnLockBroken -= HandleLockBroken;
            _codeLockService.OnLockSet -= HandleLockSet;
        }
        
        private void HandleSelection(List<Dice> dices, CombinationConfig combo)
        {
            if (combo == null)
                SetPlaceholder();
            else
                _combinationNameText.text = combo.DisplayName.GetLocalizedString();
        }
        
        private void SetPlaceholder() => _combinationNameText.text = _placeholderText.GetLocalizedString();

        private void HandleLockSet(CombinationConfig lockCode) => UpdateWidgets(lockCode != null);
        private void HandleLockBroken() => UpdateWidgets(_codeLockService.IsActive);
        
        private void UpdateWidgets(bool hasLock)
        {
            _lockWidget.SetActive(hasLock);
            _pointsWidget.SetActive(!hasLock);
        }
    }
}
