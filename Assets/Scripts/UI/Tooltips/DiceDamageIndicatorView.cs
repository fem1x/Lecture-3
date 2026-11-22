using DG.Tweening;
using _Scripts.Utility.Extensions;
using TMPro;
using UnityEngine;

namespace _Scripts.UI.Tooltips
{
    public sealed class DiceDamageIndicatorView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private CanvasGroup _canvasGroup;
        [Header("Animation")]
        [SerializeField, Min(0f)] private float _showDuration = 0.22f;
        [SerializeField, Range(0.05f, 1f)] private float _showStartScale = 0.76f;
        [SerializeField] private Ease _showScaleEase = Ease.OutBack;
        [SerializeField] private Ease _showFadeEase = Ease.OutQuad;
        [SerializeField, Min(0f)] private float _hideDuration = 0.15f;
        [SerializeField, Range(0.05f, 1f)] private float _hideEndScale = 0.84f;
        [SerializeField] private Ease _hideScaleEase = Ease.InQuad;
        [SerializeField] private Ease _hideFadeEase = Ease.InQuad;

        [Header("Damage Change")]
        [SerializeField, Min(1f)] private float _damagePopScale = 1.3f;
        [SerializeField, Min(0f)] private float _damagePopInTime = 0.07f;
        [SerializeField, Min(0f)] private float _damagePopOutTime = 0.5f;

        private Tween _animation;
        private Tween _popTween;
        private bool _popAfterShow;
        private Vector3 _restScale;
        private bool _showing;
        private int _lastDamage = int.MinValue;

        public bool IsShowing => _showing;
        public Vector2 MaximumSize => Vector2.Scale(((RectTransform)transform).rect.size,
            (Vector2)_restScale) * Mathf.Max(1f, _damagePopScale);

        public void InitializeHidden()
        {
            _restScale = transform.localScale;
            HideImmediately();
        }

        public void SetDamage(int damage)
        {
            if (_lastDamage == damage) return;
            _lastDamage = damage;
            _text.text = $"-{damage}";
        }

        public void ShowDamageChange(int damage)
        {
            SetDamage(damage);
            SetVisible(true);
            if (_animation != null && _animation.IsActive())
                _popAfterShow = true;
            else
                PlayDamagePop();
        }

        private void PlayDamagePop()
        {
            _popAfterShow = false;
            _popTween?.Kill();
            transform.localScale = _restScale;
            _popTween = transform.DoScalePop(_damagePopScale, _damagePopInTime, _damagePopOutTime)
                .SetUpdate(true);
        }

        public void SetVisible(bool visible)
        {
            if (_showing == visible) return;
            _showing = visible;
            if (!visible)
            {
                _popAfterShow = false;
                _popTween?.Kill();
                _popTween = null;
                transform.localScale = _restScale;
            }
            _animation?.Kill();
            _animation = null;

            if (visible && !gameObject.activeSelf)
            {
                transform.localScale = _restScale * _showStartScale;
                gameObject.SetActive(true);
            }

            var duration = visible ? _showDuration : _hideDuration;
            var targetScale = _restScale * (visible ? 1f : _hideEndScale);
            var targetAlpha = visible ? 1f : 0f;
            if (duration <= 0f)
            {
                transform.localScale = targetScale;
                _canvasGroup.alpha = targetAlpha;
                if (!visible) gameObject.SetActive(false);
                return;
            }

            _animation = DOTween.Sequence().SetUpdate(true)
                .Join(transform.DOScale(targetScale, duration).SetEase(visible ? _showScaleEase : _hideScaleEase))
                .Join(_canvasGroup.DOFade(targetAlpha, duration).SetEase(visible ? _showFadeEase : _hideFadeEase))
                .OnComplete(() =>
                {
                    _animation = null;
                    if (!visible) gameObject.SetActive(false);
                    else if (_popAfterShow) PlayDamagePop();
                });
        }

        public void HideImmediately()
        {
            _showing = false;
            _popAfterShow = false;
            _popTween?.Kill();
            _popTween = null;
            _animation?.Kill();
            _animation = null;
            _canvasGroup.alpha = 0f;
            transform.localScale = _restScale;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _popTween?.Kill();
            _animation?.Kill();
        }
    }
}
