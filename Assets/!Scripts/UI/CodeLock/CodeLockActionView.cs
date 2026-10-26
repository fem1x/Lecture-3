using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace _Scripts.UI.CodeLock
{
    [Serializable]
    public struct LockStateVisuals
    {
        [field: SerializeField] public Color CombinationTextColor { get; private set; }
        [field: SerializeField] public Color MainColor { get; private set; }
        [field: SerializeField] public Color ButtonColor { get; private set; }
        [field: SerializeField] public LocalizedString WidgetString { get; private set; }
        [field: SerializeField] public Sprite WidgetIcon { get; private set; }
        [field: SerializeField] public LocalizedString ButtonString { get; private set; }
        [field: SerializeField] public Sprite ButtonIcon { get; private set; }
    }
    
    public class CodeLockActionView : MonoBehaviour
    {
        [Header("UI Elements")] 
        [SerializeField] private TMP_Text _combinationText;
        [SerializeField] private LocalizeStringEvent _widgetStringEvent;
        [SerializeField] private Image _widgetIcon;
        
        [Header("Button")]
        [SerializeField] private Button _button;
        [SerializeField] private LocalizeStringEvent _buttonStringEvent;
        [SerializeField] private TMP_Text _buttonText;
        [SerializeField] private Image _buttonIcon;
        
        [Header("States")]
        [SerializeField] private LockStateVisuals _successState;
        [SerializeField] private LockStateVisuals _forcedState;
        [SerializeField] private LockStateVisuals _noneState;

        private void ApplyState(LockStateVisuals state)
        {
            _combinationText.color = state.CombinationTextColor;
            
            _widgetStringEvent.StringReference = state.WidgetString;
            _widgetIcon.sprite = state.WidgetIcon;
            _widgetIcon.color = state.MainColor;
            
            _buttonStringEvent.StringReference =  state.ButtonString;
            _button.image.color = state.MainColor;
            _buttonText.color = state.ButtonColor;
            _buttonIcon.sprite = state.ButtonIcon;
            _buttonIcon.color = state.ButtonColor;
        }
    }
}