using System.Threading;
using _Scripts.Utility.Extensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.UI
{
    public enum SlideDirection { Top, Bottom, Left, Right }

    public class SlidingPanelView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private bool _startsHidden = true;

        [Header("Slide Animation")]
        [SerializeField] private SlideDirection _direction;
        [SerializeField] private RectTransform _rect;
        [SerializeField] private CanvasGroup _cg;
        [SerializeField] private RectTransform _boundsRect;
        [SerializeField] private PanelRevealView _panelReveal;

        [Header("Show")]
        [SerializeField] private float _showTime = 0.5f;
        [SerializeField] private Ease _showEase = Ease.OutCubic;

        [Header("Hide")]
        [SerializeField] private float _hideTime = 0.5f;
        [SerializeField] private Ease _hideEase = Ease.OutCubic;

        private Vector2 _shownPos;
        private bool _isInitialized;

        internal RectTransform Content => _rect;
        internal CanvasGroup Group => _cg;
        internal float ShowDuration => _showTime;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void Awake()
        {
            EnsureInitialized();

            if (_startsHidden)
                HideInstant();
        }

        public async UniTask ShowAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            EnsureInitialized();
            _rect.DOKill();

            if (_cg.alpha <= 0.01f)
                _rect.anchoredPosition = _rect.GetHiddenPosition(_direction, _shownPos, _boundsRect);

            _cg.Show();

            Tween animation = _rect.DOAnchorPos(_shownPos, _showTime).SetEase(_showEase);
            if (_panelReveal != null && !_panelReveal.IsRevealed)
            {
                animation = DOTween.Sequence()
                    .SetTarget(_rect)
                    .SetLink(gameObject)
                    .Join(animation)
                    .Join(_panelReveal.Show());
            }

            await animation
                .SetUpdate(true)
                .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, ct);
        }

        public async UniTask HideAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            EnsureInitialized();
            _rect.DOKill();

            var hiddenPos = _rect.GetHiddenPosition(_direction, _shownPos, _boundsRect);

            await _rect.DOAnchorPos(hiddenPos, _hideTime)
                .SetEase(_hideEase)
                .SetUpdate(true)
                .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, ct);

            if (Vector2.Distance(_rect.anchoredPosition, hiddenPos) <= 0.01f)
            {
                _cg.Hide();
                _panelReveal?.HideInstant();
            }
        }

        internal void StopSlide() => _rect.DOKill();

        public void ShowInstant()
        {
            EnsureInitialized();
            _rect.DOKill();
            _rect.anchoredPosition = _shownPos;
            _panelReveal?.ShowInstant();
            _cg.Show();
        }

        public void HideInstant()
        {
            EnsureInitialized();
            _rect.DOKill();

            _rect.anchoredPosition = _rect.GetHiddenPosition(_direction, _shownPos, _boundsRect);
            _cg.Hide();
            _panelReveal?.HideInstant();
        }

        private void EnsureInitialized()
        {
            if (_isInitialized) return;

            _shownPos = _rect.anchoredPosition;
            _isInitialized = true;
        }
    }
}
