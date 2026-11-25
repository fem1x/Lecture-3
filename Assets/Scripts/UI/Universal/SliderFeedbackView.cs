using _Scripts.Managers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI
{
    public sealed class SliderFeedbackView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private Slider _slider;
        [SerializeField] private string _hoverSfx = "ButtonHover";
        [SerializeField] private string _clickSfx = "ButtonClick";
        [SerializeField] private string _disabledClickSfx = "ButtonDisabledClick";
        [SerializeField] private string _moveSfx = "SliderMoved";
        [SerializeField, Min(0f)] private float _soundInterval = 0.05f;
        [SerializeField] private float _hoverScale = 1.1f;
        [SerializeField] private float _pressedScale = 0.88f;
        [SerializeField, Min(0f)] private float _pressTime = 0.065f;
        [SerializeField, Min(0f)] private float _releaseTime = 0.18f;

        private ISfxPlayer _sfxPlayer;
        private RectTransform _handle;
        private Vector3 _baseScale;
        private Tween _feedback;
        private bool _hovered;
        private bool _pressed;
        private float _nextSoundTime;

        [Inject]
        public void Construct(ISfxPlayer sfxPlayer) => _sfxPlayer = sfxPlayer;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            _handle = _slider.handleRect;
            _baseScale = _handle.localScale;
        }

        private void OnEnable()
        {
            _nextSoundTime = 0f;
            _slider.onValueChanged.AddListener(HandleValueChanged);
        }

        private void OnDisable()
        {
            _slider.onValueChanged.RemoveListener(HandleValueChanged);
            _feedback?.Kill();
            _handle.localScale = _baseScale;
            _hovered = false;
            _pressed = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            if (_slider.IsInteractable())
                _sfxPlayer.Play(_hoverSfx);
            PlayFeedback();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            PlayFeedback();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (!_slider.IsInteractable())
            {
                _sfxPlayer.Play(_disabledClickSfx);
                return;
            }
            _sfxPlayer.Play(_clickSfx);
            _pressed = true;
            PlayFeedback();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_pressed || eventData.button != PointerEventData.InputButton.Left) return;
            _pressed = false;
            PlayFeedback();
        }

        private void HandleValueChanged(float _)
        {
            if (!_slider.IsInteractable() || Time.unscaledTime < _nextSoundTime) return;
            _nextSoundTime = Time.unscaledTime + _soundInterval;
            _sfxPlayer.Play(_moveSfx);
        }

        private void PlayFeedback()
        {
            _feedback?.Kill();
            var scale = _pressed ? _pressedScale : _hovered && _slider.IsInteractable() ? _hoverScale : 1f;
            _feedback = _handle.DOScale(_baseScale * scale, _pressed ? _pressTime : _releaseTime)
                .SetEase(_pressed ? Ease.OutQuad : Ease.OutBack).SetUpdate(true);
        }
    }
}
