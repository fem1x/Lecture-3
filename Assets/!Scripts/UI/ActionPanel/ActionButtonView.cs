using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class ActionButtonView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button _button;
        [SerializeField] private LocalizeStringEvent _stringEvent;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Image _icon;
        
        [Header("Default State")]
        [SerializeField] private LocalizedString _defaultString;
        [SerializeField] private Color _defaultMainColor;
        [SerializeField] private Color _defaultTextColor;
        [SerializeField] private Sprite _defaultIcon;

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public void SetInteractable(bool state) => _button.interactable = state;

        public void SetAppearance(LocalizedString text, Color mainColor, Color textColor, Sprite icon)
        {
            _stringEvent.StringReference = text;
            _button.image.color = mainColor;
            _text.color = textColor;
            
            if (_icon != null)
            {
                _icon.sprite = icon;
                _icon.color = textColor;
            }
        }

        public void SetDefaultAppearance()
        {
            SetAppearance(_defaultString, _defaultMainColor, _defaultTextColor, _defaultIcon);
        }
    }
}