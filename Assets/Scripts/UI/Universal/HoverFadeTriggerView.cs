using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Scripts.UI
{
    public class HoverFadeThroughView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private CanvasGroup _contentCg;
        
        [SerializeField] private float _hoverAlpha = 0.3f;
        [SerializeField] private float _fadeDuration = 0.2f;
        [SerializeField] private Ease _ease = Ease.OutQuad;

        private bool _isHovered;
        private Tween _fadeTween;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void OnDisable()
        {
            _fadeTween?.Kill();
            _isHovered = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_contentCg.alpha <= 0.01f && !_isHovered) return;

            _isHovered = true;
            _fadeTween?.Kill();
            _contentCg.blocksRaycasts = false;
            
            _fadeTween = _contentCg.DOFade(_hoverAlpha, _fadeDuration)
                .SetEase(_ease)
                .SetLink(gameObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isHovered) return;

            _isHovered = false;
            _fadeTween?.Kill();
            _contentCg.blocksRaycasts = true;
            
            _fadeTween = _contentCg.DOFade(1f, _fadeDuration)
                .SetEase(_ease)
                .SetLink(gameObject);
        }
    }
}