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

        [Header("Rolling Dots")]
        [SerializeField] private GameObject _rollingDots;
        [SerializeField] private RectTransform[] _dotRects;
        [SerializeField, Min(1f)] private float _dotFontSize = 48f;
        [SerializeField, Min(0f)] private float _dotJumpHeight = 10f;
        [SerializeField, Min(0.01f)] private float _dotJumpDuration = 0.4f;
        [SerializeField, Min(0f)] private float _dotStagger = 0.14f;
        [SerializeField, Min(0f)] private float _dotCyclePause = 0.15f;
        [SerializeField] private Ease _dotRiseEase = Ease.OutQuad;
        [SerializeField] private Ease _dotFallEase = Ease.InQuad;

        private enum ActionState { Throw, Reroll, Rolling }

        private readonly LocalizedString _rollingLabel = new("String Table", "roll_button_rolling");
        private readonly LocalizedString _throwLabel = new("String Table", "roll_button_throw");
        private readonly LocalizedString _rerollLabel = new("String Table", "roll_button_reroll");
        private readonly LocalizedString _throwTooltip = new("String Table", "controls_throw_tooltip");
        private readonly LocalizedString _rerollTooltip = new("String Table", "controls_reroll_tooltip");
        private BasicTooltipTrigger _tooltip;
        private ActionState? _state;
        private Vector2[] _dotPositions;
        private Tween _dotsTween;
        private Tween _textTween;
        private Tween _iconTween;
        private Vector3 _iconScale;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            _iconScale = _icon.transform.localScale;
            _dotPositions = new Vector2[_dotRects.Length];
            for (var i = 0; i < _dotRects.Length; i++)
            {
                _dotPositions[i] = _dotRects[i].anchoredPosition;
                _dotRects[i].GetComponent<TMP_Text>().fontSize = _dotFontSize;
            }
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
            _flow.OnRollStateChanged += RefreshAction;
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            _state = null;
            RefreshAction();
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(_tooltip.DismissButtonHover);
            _button.onClick.RemoveListener(PlayIconAccent);
            _button.onClick.RemoveListener(_flow.OnRollButtonClicked);
            _flow.OnRollActionChanged -= RefreshAction;
            _flow.OnRollStateChanged -= RefreshAction;
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            _textTween?.Kill();
            _iconTween?.Kill();
            _dotsTween?.Kill();
            _textReveal.Restore();
            _icon.transform.localScale = _iconScale;
        }

        private void Update()
        {
            _button.interactable = _flow.CanPerformRoll;
        }

        private void RefreshAction()
        {
            var state = _flow.IsRolling ? ActionState.Rolling :
                _flow.IsThrowAction ? ActionState.Throw : ActionState.Reroll;
            if (_state == state) return;
            _state = state;
            var isRolling = state == ActionState.Rolling;
            _icon.enabled = !isRolling;
            _rollingDots.SetActive(isRolling);
            _dotsTween?.Kill();
            if (isRolling)
            {
                _iconTween?.Kill();
                _icon.transform.localScale = _iconScale;
                PlayRollingDots();
            }
            else
            {
                _icon.sprite = state == ActionState.Throw ? _throwIcon : _rerollIcon;
                PlayIconAccent();
            }
            RefreshLabel();
            _tooltip.RefreshContent();
        }

        private void PlayRollingDots()
        {
            var duration = Mathf.Max(0.01f, _dotJumpDuration);
            var sequence = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
            for (var i = 0; i < _dotRects.Length; i++)
            {
                var dot = _dotRects[i];
                dot.anchoredPosition = _dotPositions[i];
                var baseY = _dotPositions[i].y;
                var jump = DOTween.Sequence()
                    .Append(dot.DOAnchorPosY(baseY + _dotJumpHeight, duration * 0.5f).SetEase(_dotRiseEase))
                    .Append(dot.DOAnchorPosY(baseY, duration * 0.5f).SetEase(_dotFallEase));
                sequence.Insert(i * _dotStagger, jump);
            }
            sequence.AppendInterval(_dotCyclePause);
            _dotsTween = sequence.SetLoops(-1);
        }

        private (string title, string text) GetTooltipContent()
        {
            if (_flow.IsRolling) return (null, null);
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
                _label.text = (_state == ActionState.Rolling ? _rollingLabel :
                    _state == ActionState.Throw ? _throwLabel : _rerollLabel).GetLocalizedString();
                _textReveal.Restore();
                _textTween = _textReveal.Show();
            });
        }
    }
}
