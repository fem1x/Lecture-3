using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Scripts.UI
{
    public class HoverScaleView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private bool _insideLayoutGroup;

        [Header("Scale")]
        [SerializeField] private float _hoverScale = 1.06f;

        [Header("Animation")]
        [SerializeField] private float _duration = 0.14f;
        [SerializeField] private Ease _ease = Ease.OutCubic;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private RectTransform _rectTransform;
        private Vector3 _defaultScale;
        private Tween _scaleTween;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;

            if (!_insideLayoutGroup)
                CenterPivot();

            _defaultScale = transform.localScale;
        }

        private void OnDisable()
        {
            _scaleTween?.Kill();
            transform.localScale = _defaultScale;
        }

        private void OnDestroy() => _scaleTween?.Kill();

        public void OnPointerEnter(PointerEventData eventData) => AnimateScale(_defaultScale * _hoverScale);

        public void OnPointerExit(PointerEventData eventData) => AnimateScale(_defaultScale);

        private void CenterPivot()
        {
            var center = new Vector2(0.5f, 0.5f);
            var deltaPivot = _rectTransform.pivot - center;
            var deltaPosition = new Vector2(
                deltaPivot.x * _rectTransform.rect.width * _rectTransform.localScale.x,
                deltaPivot.y * _rectTransform.rect.height * _rectTransform.localScale.y);

            _rectTransform.pivot = center;
            _rectTransform.anchoredPosition -= deltaPosition;
        }

        private void AnimateScale(Vector3 targetScale)
        {
            _scaleTween?.Kill();
            _scaleTween = transform.DOScale(targetScale, _duration)
                .SetEase(_ease)
                .SetUpdate(true)
                .SetLink(gameObject);
        }
    }
}