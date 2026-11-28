using DG.Tweening;
using UnityEngine;

namespace _Scripts.UI
{
    public sealed class StaggeredRevealView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private RectTransform[] _elements;
        [SerializeField, Min(0f)] private float _delay;
        [SerializeField, Min(0f)] private float _revealDuration = 0.26f;
        [SerializeField, Min(0f)] private float _revealStagger = 0.065f;
        [SerializeField] private Ease _revealFadeEase = Ease.OutQuad;
        [SerializeField] private Ease _revealScaleEase = Ease.OutExpo;

        private CanvasGroup[] _groups;
        private Vector3[] _scales;
        private float[] _alphas;
        private Sequence _sequence;
        private bool _isInitialized;

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void Awake() => EnsureInitialized();

        private void EnsureInitialized()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            _groups = new CanvasGroup[_elements.Length];
            _scales = new Vector3[_elements.Length];
            _alphas = new float[_elements.Length];

            for (var i = 0; i < _elements.Length; i++)
            {
                var element = _elements[i];
                var group = element.GetComponent<CanvasGroup>();
                if (group == null)
                    group = element.gameObject.AddComponent<CanvasGroup>();

                _groups[i] = group;
                _scales[i] = element.localScale;
                _alphas[i] = group.alpha;
            }
        }

        private void OnDisable() => Restore();

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public void Show() => Show(_delay);

        internal Sequence Show(TweenCallback onElementReveal) => Show(_delay, onElementReveal);

        internal Sequence Show(float delay, TweenCallback onElementReveal = null)
        {
            Restore();
            _sequence = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

            for (var i = 0; i < _elements.Length; i++)
            {
                var start = delay + i * _revealStagger;
                _groups[i].alpha = 0f;
                _elements[i].localScale = _scales[i] * 0.96f;
                if (onElementReveal != null)
                    _sequence.InsertCallback(start, onElementReveal);
                _sequence.Insert(start, _groups[i].DOFade(_alphas[i], _revealDuration)
                    .SetEase(_revealFadeEase).SetUpdate(true).SetLink(gameObject));
                _sequence.Insert(start, _elements[i].DOScale(_scales[i], _revealDuration)
                    .SetEase(_revealScaleEase).SetUpdate(true).SetLink(gameObject));
            }

            return _sequence;
        }

        public void Restore()
        {
            EnsureInitialized();
            _sequence?.Kill();
            _sequence = null;

            for (var i = 0; i < _elements.Length; i++)
            {
                _groups[i].alpha = _alphas[i];
                _elements[i].localScale = _scales[i];
            }
        }
    }
}
