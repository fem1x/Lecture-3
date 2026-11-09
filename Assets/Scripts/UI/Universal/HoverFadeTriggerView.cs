using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.UI
{
    public class HoverFadeThroughView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private CanvasGroup _contentCg;
        [SerializeField] private RectTransform _rect;
        
        [SerializeField] private float _hoverAlpha = 0.15f;
        [SerializeField] private float _fadeDuration = 0.3f;
        [SerializeField] private Ease _ease = Ease.OutQuart;

        private bool _isHovered;
        private Tween _fadeTween;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void OnDisable()
        {
            _fadeTween?.Kill();
            _isHovered = false;
        }

        private void Update()
        {
            if (_contentCg.alpha <= 0.01f && !_isHovered) return;

            var mousePosition = Mouse.current.position.ReadValue();
            var isInside = RectTransformUtility.RectangleContainsScreenPoint(_rect, mousePosition);

            if (isInside && !_isHovered)
                SetHover(true);
            else if (!isInside && _isHovered)
                SetHover(false);
        }

        private void SetHover(bool hover)
        {
            _isHovered = hover;
            _fadeTween?.Kill();

            _contentCg.blocksRaycasts = !hover;

            var targetAlpha = hover ? _hoverAlpha : 1f;
            _fadeTween = _contentCg.DOFade(targetAlpha, _fadeDuration)
                .SetEase(_ease)
                .SetLink(gameObject);
        }
    }
}