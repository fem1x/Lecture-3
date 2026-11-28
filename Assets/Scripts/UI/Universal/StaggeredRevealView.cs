using System;
using DG.Tweening;
using UnityEngine;
using VContainer;

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
        [SerializeField] private float _initialScale = 0.96f;
        [SerializeField] private bool _blockInteraction;
        [SerializeField] private string _revealSfx = "ElementAppear";

        private ISfxPlayer _sfxPlayer;
        private CanvasGroup[] _groups;
        private Vector3[] _scales;
        private float[] _alphas;
        private Sequence _sequence;
        private bool _isInitialized;

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Inject]
        public void Construct(ISfxPlayer sfxPlayer) => _sfxPlayer = sfxPlayer;

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

        internal Sequence Show(Action<RectTransform> onElementReveal) => Show(_delay, onElementReveal);

        internal void Prepare()
        {
            Restore();
            for (var i = 0; i < _elements.Length; i++)
            {
                _groups[i].alpha = 0f;
                _elements[i].localScale = _scales[i] * _initialScale;
                if (_blockInteraction)
                {
                    _groups[i].blocksRaycasts = false;
                    _groups[i].interactable = false;
                }
            }
        }

        internal Sequence Show(float delay, Action<RectTransform> onElementReveal = null,
            Action<Sequence, int, float> insertChildReveal = null)
        {
            Prepare();
            _sequence = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

            for (var i = 0; i < _elements.Length; i++)
            {
                var start = delay + i * _revealStagger;
                _groups[i].alpha = 0f;
                _elements[i].localScale = _scales[i] * _initialScale;
                if (!string.IsNullOrEmpty(_revealSfx))
                    _sequence.InsertCallback(start, () => _sfxPlayer?.Play(_revealSfx));
                if (onElementReveal != null)
                {
                    var element = _elements[i];
                    _sequence.InsertCallback(start, () => onElementReveal(element));
                }
                _sequence.Insert(start, _groups[i].DOFade(_alphas[i], _revealDuration)
                    .SetEase(_revealFadeEase).SetUpdate(true).SetLink(gameObject));
                _sequence.Insert(start, _elements[i].DOScale(_scales[i], _revealDuration)
                    .SetEase(_revealScaleEase).SetUpdate(true).SetLink(gameObject));
                if (_blockInteraction)
                {
                    var group = _groups[i];
                    _sequence.InsertCallback(start + _revealDuration, () =>
                    {
                        group.blocksRaycasts = true;
                        group.interactable = true;
                    });
                }
                insertChildReveal?.Invoke(_sequence, i, start);
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
                if (_blockInteraction)
                {
                    _groups[i].blocksRaycasts = true;
                    _groups[i].interactable = true;
                }
                _elements[i].localScale = _scales[i];
            }
        }
    }
}
