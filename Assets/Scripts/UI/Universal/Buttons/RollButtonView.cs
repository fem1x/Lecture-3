using _Scripts.Managers;
using _Scripts.UI.Tooltips;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public sealed class RollButtonView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private TurnFlowController _flow;
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private TextRevealView _textReveal;
        [SerializeField] private Sprite _throwIcon;
        [SerializeField] private Sprite _rerollIcon;
        [SerializeField, Min(0f)] private float _iconAccentDuration = 0.3f;

        private readonly LocalizedString _throwLabel = new("String Table", "roll_button_throw");
        private readonly LocalizedString _rerollLabel = new("String Table", "roll_button_reroll");
        private readonly LocalizedString _throwTooltip = new("String Table", "controls_throw_tooltip");
        private readonly LocalizedString _rerollTooltip = new("String Table", "controls_reroll_tooltip");
        private BasicTooltipTrigger _tooltip;
        private bool? _showingThrow;
        private Tween _textTween;
        private Tween _iconTween;
        private Vector3 _iconScale;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            _iconScale = _icon.transform.localScale;
            _tooltip = GetComponent<BasicTooltipTrigger>();
            _tooltip.ContentProvider = GetTooltipContent;
            _tooltip.ConfigureButtonHover();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(_tooltip.DismissButtonHover);
            _button.onClick.AddListener(PlayIconAccent);
            _button.onClick.AddListener(_flow.OnRollButtonClicked);
            _flow.OnRollActionChanged += RefreshAction;
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            _showingThrow = null;
            RefreshAction();
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(_tooltip.DismissButtonHover);
            _button.onClick.RemoveListener(PlayIconAccent);
            _button.onClick.RemoveListener(_flow.OnRollButtonClicked);
            _flow.OnRollActionChanged -= RefreshAction;
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            _textTween?.Kill();
            _iconTween?.Kill();
            _textReveal.Restore();
            _icon.transform.localScale = _iconScale;
        }

        private void Update()
        {
            _button.interactable = _flow.CanPerformRoll;
        }

        private void RefreshAction()
        {
            var isThrow = _flow.IsThrowAction;
            if (_showingThrow == isThrow) return;
            _showingThrow = isThrow;
            _icon.sprite = isThrow ? _throwIcon : _rerollIcon;
            PlayIconAccent();
            RefreshLabel();
            _tooltip.RefreshContent();
        }

        private (string title, string text) GetTooltipContent()
        {
            var isThrow = _flow.IsThrowAction;
            var text = (isThrow ? _throwTooltip : _rerollTooltip).GetLocalizedString();
            return (null, text);
        }

        private void PlayIconAccent()
        {
            _iconTween?.Kill();
            _icon.transform.localScale = _iconScale * 0.7f;
            _iconTween = _icon.transform.DOScale(_iconScale, _iconAccentDuration)
                .SetEase(Ease.OutBack).SetUpdate(true).SetLink(gameObject);
        }

        private void OnLocaleChanged(UnityEngine.Localization.Locale _)
        {
            RefreshLabel();
            _tooltip.RefreshContent();
        }
        private void RefreshLabel()
        {
            _textTween?.Kill();
            _textReveal.Restore();
            _textTween = _textReveal.Hide().OnComplete(() =>
            {
                _label.text = (_showingThrow == true ? _throwLabel : _rerollLabel).GetLocalizedString();
                _textReveal.Restore();
                _textTween = _textReveal.Show();
            });
        }
    }
}
