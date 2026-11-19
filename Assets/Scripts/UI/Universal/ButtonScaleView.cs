using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class ButtonScaleView : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private Button _button;

        [Header("Scale")]
        [SerializeField] private float _hoverScale = 1.1f;
        [SerializeField] private float _pressedScale = 0.9f;

        [Header("Animation")]
        [SerializeField] private float _duration = 0.15f;
        [SerializeField] private Ease _ease = Ease.OutBack;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private Vector3 _defaultScale;
        private Tween _scaleTween;

        private bool _isHovered;
        private bool _isPressed;

        private void Awake()
        {
            _defaultScale = transform.localScale;
        }

        private void OnDisable()
        {
            _scaleTween?.Kill();
            transform.localScale = _defaultScale;

            _isHovered = false;
            _isPressed = false;
        }

        private void OnDestroy() => _scaleTween?.Kill();

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            UpdateScale();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            UpdateScale();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_button.interactable) return;

            _isPressed = true;
            UpdateScale();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            UpdateScale();
        }

        private void UpdateScale()
        {
            var targetScale = _isPressed
                ? _defaultScale * _pressedScale
                : _isHovered
                    ? _defaultScale * _hoverScale
                    : _defaultScale;

            _scaleTween?.Kill();
            _scaleTween = transform.DOScale(targetScale, _duration)
                .SetEase(_ease)
                .SetLink(gameObject);
        }
    }
}