using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Scripts.UI.Tooltips
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class BasicTooltipView : MonoBehaviour, IPointerClickHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private const float ShowFadeDurationRatio = 0.36f;
        private const float ShowSlideDistance = 18f;
        private const float HideSlideDistance = 12f;

        [SerializeField] private TMP_Text _content;
        [SerializeField] private TextRevealView _textReveal;
        [SerializeField] private RectTransform _arrow;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Show Animation")]
        [SerializeField, Min(0f)] private float _showDuration = 0.22f;
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
        private Tween _textTransition;
        private bool _isShowing;

        internal event Action DismissRequested;

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            _restScale = transform.localScale;
            InitializeSpriteAssets(_content.spriteAsset, new HashSet<TMP_SpriteAsset>());
        }

        private static void InitializeSpriteAssets(TMP_SpriteAsset asset, HashSet<TMP_SpriteAsset> initialized)
        {
            if (asset == null || !initialized.Add(asset)) return;

            asset.UpdateLookupTables();
            if (asset.fallbackSpriteAssets == null) return;

            foreach (var fallback in asset.fallbackSpriteAssets)
                InitializeSpriteAssets(fallback, initialized);
        }

        private void OnDisable()
        {
            _isShowing = false;
            SetDismissible(false);
            _transition?.Kill();
            _transition = null;
            _textTransition?.Kill();
            _textTransition = null;
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        internal void SetDismissible(bool dismissible) => _canvasGroup.blocksRaycasts = dismissible;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_canvasGroup.blocksRaycasts && eventData.button == PointerEventData.InputButton.Left)
                DismissRequested?.Invoke();
        }

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
            _textTransition?.Kill();
            _textTransition = _textReveal.Show().OnComplete(() => _textTransition = null);

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
                transform.localScale = _restScale;
                SetSlideOffset(ShowSlideDistance);
            }

            _transition?.Kill();
            var animation = DOTween.Sequence().SetUpdate(true)
                .Join(DOTween.To(() => _slideOffset, SetSlideOffset, 0f, _showDuration).SetEase(_showSlideEase))
                .Join(_canvasGroup.DOFade(1f, _showDuration * ShowFadeDurationRatio).SetEase(_showFadeEase));

            if (transform.localScale != _restScale)
                animation.Join(transform.DOScale(_restScale, _showDuration).SetEase(Ease.OutCubic));

            _transition = animation.OnComplete(() => _transition = null);
        }

        public void Hide()
        {
            if (!_isShowing) return;

            _isShowing = false;
            SetDismissible(false);
            _textTransition?.Kill();
            _textTransition = null;
            _textReveal.Restore();
            if (_hideDuration <= 0f)
            {
                HideImmediately();
                return;
            }

            _transition?.Kill();
            _transition = DOTween.Sequence().SetUpdate(true)
                .Join(transform.DOScale(_restScale * 0.93f, _hideDuration).SetEase(_hideScaleEase))
                .Join(DOTween.To(() => _slideOffset, SetSlideOffset, HideSlideDistance, _hideDuration).SetEase(_hideSlideEase))
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
