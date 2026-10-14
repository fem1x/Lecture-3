using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class SlidingWindowView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private RectTransform _windowRect;
        [SerializeField] private float _duration = 0.35f;
        [SerializeField] private Ease _ease = Ease.OutBack;

        private CanvasGroup _canvasGroup;
        private float _shownY;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _shownY = _windowRect.anchoredPosition.y;
        }

        public void HideInstant()
        {
            SetInteractable(false);
            _canvasGroup.alpha = 0f;
            _windowRect.anchoredPosition = new Vector2(_windowRect.anchoredPosition.x, _shownY + GetHiddenOffsetY());
        }

        public async UniTask ShowAsync(CancellationToken ct)
        {
            transform.DOKill();
            SetInteractable(true);
            _windowRect.anchoredPosition = new Vector2(_windowRect.anchoredPosition.x, _shownY + GetHiddenOffsetY());

            await DOTween.Sequence()
                .SetTarget(transform)
                .SetUpdate(true)
                .Join(_canvasGroup.DOFade(1f, _duration * 0.6f).SetEase(_ease))
                .Join(_windowRect.DOAnchorPosY(_shownY, _duration).SetEase(_ease))
                .WithCancellation(ct);
        }

        public Tween HideTween()
        {
            SetInteractable(false);
            var hideY = _shownY + GetHiddenOffsetY();
    
            return DOTween.Sequence()
                .Join(_canvasGroup.DOFade(0f, _duration * 0.6f).SetEase(_ease))
                .Join(_windowRect.DOAnchorPosY(hideY, _duration).SetEase(_ease));
        }

        private float GetHiddenOffsetY() => -(_windowRect.rect.height > 0 ? _windowRect.rect.height * 1.5f : 1200f);

        private void SetInteractable(bool active)
        {
            _canvasGroup.blocksRaycasts = active;
            _canvasGroup.interactable = active;
        }
    }
}