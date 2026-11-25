using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    [RequireComponent(typeof(Image))]
    public sealed class SettingsTabAccentView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField, Min(0f)] private float _showTime = 0.2f;
        [SerializeField, Min(0f)] private float _hideTime = 0.2f;
        [SerializeField] private Ease _showEase = Ease.OutCubic;
        [SerializeField] private Ease _hideEase = Ease.InCubic;

        private Image _image;
        private Vector3 _shownScale;
        private float _shownAlpha;
        private Tween _tween;

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            _image = GetComponent<Image>();
            _shownScale = transform.localScale;
            _shownAlpha = _image.color.a;
        }

        private void OnDisable() => _tween?.Kill();

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void SetSelected(bool selected, bool animate)
        {
            _tween?.Kill();
            if (!animate)
            {
                _image.enabled = selected;
                var color = _image.color;
                color.a = selected ? _shownAlpha : 0f;
                _image.color = color;
                transform.localScale = new Vector3(selected ? _shownScale.x : 0f, _shownScale.y, _shownScale.z);
                return;
            }

            _image.enabled = true;
            var duration = selected ? _showTime : _hideTime;
            var ease = selected ? _showEase : _hideEase;
            _tween = DOTween.Sequence().SetUpdate(true)
                .Join(transform.DOScaleX(selected ? _shownScale.x : 0f, duration).SetEase(ease))
                .Join(_image.DOFade(selected ? _shownAlpha : 0f, duration).SetEase(ease));
        }
    }
}
