using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Scripts.UI.Tooltips
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class BasicTooltipView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private const float ShowFadeDurationRatio = 0.12f / 0.22f;
        private const float SlideDistance = 12f;

        [SerializeField] private TMP_Text _content;
        [SerializeField] private RectTransform _arrow;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Show Animation")]
        [SerializeField, Min(0f)] private float _showDuration = 0.22f;
        [SerializeField] private Ease _showScaleEase = Ease.OutBack;
        [SerializeField] private Ease _showSlideEase = Ease.OutCubic;
        [SerializeField] private Ease _showFadeEase = Ease.OutQuad;

        [Header("Hide Animation")]
        [SerializeField, Min(0f)] private float _hideDuration = 0.14f;
        [SerializeField] private Ease _hideScaleEase = Ease.InQuad;
        [SerializeField] private Ease _hideSlideEase = Ease.InQuad;
        [SerializeField] private Ease _hideFadeEase = Ease.InQuad;

        private Vector3 _restScale;
        private Vector2 _anchorPosition;
        private Vector2 _direction;
        private float _slideOffset;
        private Tween _transition;
        private bool _isShowing;

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake() => _restScale = transform.localScale;

        private void OnDisable()
        {
            _isShowing = false;
            _transition?.Kill();
            _transition = null;
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void SetContent(string text, Vector2 direction)
        {
            _content.text = text;
            _direction = direction;

            var nearEdge = (Vector2.one - direction) * 0.5f;
            ((RectTransform)transform).pivot = nearEdge;
            _arrow.anchorMin = nearEdge;
            _arrow.anchorMax = nearEdge;
            _arrow.anchoredPosition = Vector2.zero;
            _arrow.localRotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.up, -direction));
        }

        public void SetPosition(Vector2 position)
        {
            _anchorPosition = position;
            ApplyPosition();
        }

        public void Show()
        {
            if (_isShowing) return;

            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            _isShowing = true;

            if (_showDuration <= 0f)
            {
                _transition?.Kill();
                _transition = null;
                _canvasGroup.alpha = 1f;
                transform.localScale = _restScale;
                SetSlideOffset(0f);
                return;
            }

            if (_transition == null)
            {
                _canvasGroup.alpha = 0f;
                transform.localScale = _restScale * 0.88f;
                SetSlideOffset(SlideDistance);
            }

            _transition?.Kill();
            _transition = DOTween.Sequence().SetUpdate(true)
                .Join(transform.DOScale(_restScale, _showDuration).SetEase(_showScaleEase, 2f))
                .Join(DOTween.To(() => _slideOffset, SetSlideOffset, 0f, _showDuration).SetEase(_showSlideEase))
                .Join(_canvasGroup.DOFade(1f, _showDuration * ShowFadeDurationRatio).SetEase(_showFadeEase))
                .OnComplete(() => _transition = null);
        }

        public void Hide()
        {
            if (!_isShowing) return;

            _isShowing = false;
            if (_hideDuration <= 0f)
            {
                HideImmediately();
                return;
            }

            _transition?.Kill();
            _transition = DOTween.Sequence().SetUpdate(true)
                .Join(transform.DOScale(_restScale * 0.93f, _hideDuration).SetEase(_hideScaleEase))
                .Join(DOTween.To(() => _slideOffset, SetSlideOffset, SlideDistance, _hideDuration).SetEase(_hideSlideEase))
                .Join(_canvasGroup.DOFade(0f, _hideDuration).SetEase(_hideFadeEase))
                .OnComplete(() =>
                {
                    _transition = null;
                    HideImmediately();
                });
        }

        public void HideImmediately() => gameObject.SetActive(false);

        private void SetSlideOffset(float offset)
        {
            _slideOffset = offset;
            ApplyPosition();
        }

        private void ApplyPosition() => transform.localPosition = _anchorPosition - _direction * _slideOffset;
    }
}
