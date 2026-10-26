using System;
using System.Collections.Generic;
using _Scripts.Combinations;
using _Scripts.Dices;
using _Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI.CodeLock
{
    [Serializable]
    public struct LockStateVisuals
    {
        [field: SerializeField] public Color CombinationTextColor { get; private set; }
        [field: SerializeField] public Color MainColor { get; private set; }
        [field: SerializeField] public Color ButtonTextColor { get; private set; }
        [field: SerializeField] public LocalizedString WidgetString { get; private set; }
        [field: SerializeField] public Sprite WidgetIcon { get; private set; }
        [field: SerializeField] public LocalizedString ButtonString { get; private set; }
        [field: SerializeField] public Sprite ButtonIcon { get; private set; }
    }
    
    public class LockWidgetView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("UI Elements")] 
        [SerializeField] private TMP_Text _combinationText;
        [SerializeField] private LocalizeStringEvent _widgetStringEvent;
        [SerializeField] private Image _widgetIcon;
        [SerializeField] private ActionButtonView _buttonView;
        
        [Header("States")]
        [SerializeField] private LockStateVisuals _successState;
        [SerializeField] private LockStateVisuals _forcedState;
        [SerializeField] private LockStateVisuals _noneState;
        
        #region DI
        private ActiveSelectionService _selectionService;
        private CodeLockService _codeLockService;

        [Inject]
        public void Construct(ActiveSelectionService selectionService, CodeLockService codeLockService)
        {
            _selectionService = selectionService;
            _codeLockService = codeLockService;
        }
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Start() => ApplyState(_noneState);
        private void OnEnable() => _selectionService.OnSelectionEvaluated += HandleSelection;
        private void OnDisable()
        {
            _selectionService.OnSelectionEvaluated -= HandleSelection;
            _combinationText.color = Color.white;
        }

        private void HandleSelection(List<Dice> dices, CombinationConfig combo)
        {
            if (dices.Count == 0 || !_codeLockService.IsActive)
            {
                ApplyState(_noneState);
                return;
            }
            
            var result = _codeLockService.EvaluateAttempt(dices, combo);
            
            if (result == LockBreakResult.Success)
                ApplyState(_successState);
            else if (result == LockBreakResult.Forced)
                ApplyState(_forcedState);
            else
                ApplyState(_noneState);
        }
        
        private void ApplyState(LockStateVisuals state)
        {
            _combinationText.color = state.CombinationTextColor;
            
            _widgetStringEvent.StringReference = state.WidgetString;
            _widgetIcon.sprite = state.WidgetIcon;
            _widgetIcon.color = state.MainColor;
            
            _buttonView.SetAppearance(state.ButtonString, state.MainColor, state.ButtonTextColor, state.ButtonIcon);
        }
    }
}