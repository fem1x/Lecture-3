using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public sealed class CheckboxFeedbackView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private Toggle _toggle;
        [SerializeField] private Image _checkmark;
        [SerializeField] private Color _checkedColor = new Color32(85, 57, 106, 255);
        [SerializeField, Min(0f)] private float _backgroundTime = 0.2f;

        [Header("Check")]
        [SerializeField, Min(0f)] private float _restScale = 0.72f;
        [SerializeField, Min(1f)] private float _peakScale = 1.28f;
        [SerializeField, Min(0f)] private float _peakTime = 0.065f;
        [SerializeField, Min(0f)] private float _settleTime = 0.24f;
        [SerializeField, Min(0f)] private float _appearTime = 0.045f;
        [SerializeField] private Ease _peakEase = Ease.OutCubic;
        [SerializeField] private Ease _settleEase = Ease.OutQuart;

        [Header("Uncheck")]
        [SerializeField, Min(0f)] private float _disappearTime = 0.065f;
        [SerializeField] private Ease _disappearEase = Ease.InQuad;

        private Color _uncheckedColor;
        private Vector3 _baseScale;
        private Tween _feedback;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            _uncheckedColor = _toggle.image.color;
            _baseScale = _checkmark.rectTransform.localScale;
        }

        private void OnEnable()
        {
            ApplyInstant(_toggle.isOn);
            _toggle.onValueChanged.AddListener(Animate);
        }

        private void OnDisable()
        {
            _toggle.onValueChanged.RemoveListener(Animate);
            _feedback?.Kill();
        }

        public void SetValueWithoutNotify(bool enabled)
        {
            _toggle.SetIsOnWithoutNotify(enabled);
            if (isActiveAndEnabled)
                ApplyInstant(enabled);
        }

        private void ApplyInstant(bool enabled)
        {
            _feedback?.Kill();
            _toggle.image.color = enabled ? _checkedColor : _uncheckedColor;
            var color = _checkmark.color;
            color.a = enabled ? 1f : 0f;
            _checkmark.color = color;
            _checkmark.rectTransform.localScale = _baseScale * (enabled ? 1f : _restScale);
        }

        private void Animate(bool enabled)
        {
            _feedback?.Kill();
            var mark = _checkmark.rectTransform;
            var feedback = DOTween.Sequence().SetUpdate(true)
                .Join(_toggle.image.DOColor(enabled ? _checkedColor : _uncheckedColor, _backgroundTime));
            if (enabled)
            {
                feedback.Insert(0f, _checkmark.DOFade(1f, _appearTime))
                    .Insert(0f, mark.DOScale(_baseScale * _peakScale, _peakTime).SetEase(_peakEase))
                    .Insert(_peakTime, mark.DOScale(_baseScale, _settleTime).SetEase(_settleEase));
            }
            else
            {
                feedback.Insert(0f, _checkmark.DOFade(0f, _disappearTime).SetEase(_disappearEase))
                    .Insert(0f, mark.DOScale(_baseScale * _restScale, _disappearTime).SetEase(_disappearEase));
            }
            _feedback = feedback;
        }
    }
}
