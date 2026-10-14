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
        [SerializeField] private float _slideDuration = 0.35f;
        [SerializeField] private Ease _slideEase = Ease.OutBack;

        private CanvasGroup _canvasGroup;
        private float _shownY;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _shownY = _windowRect.anchoredPosition.y;
            HideInstant();
        }

        public async UniTask ShowAsync(CancellationToken ct)
        {
            transform.DOKill();
            SetInteractable(true);

            _windowRect.anchoredPosition = new Vector2(_windowRect.anchoredPosition.x, _shownY + GetHiddenOffsetY());
            Canvas.ForceUpdateCanvases();

            await DOTween.Sequence()
                .SetTarget(transform)
                .SetUpdate(true)
                .Join(_canvasGroup.DOFade(1f, _slideDuration * 0.6f).SetEase(_slideEase))
                .Join(_windowRect.DOAnchorPosY(_shownY, _slideDuration).SetEase(_slideEase))
                .WithCancellation(ct);
        }

        public async UniTask HideAsync(CancellationToken ct)
        {
            transform.DOKill();
            SetInteractable(false);

            var hideY = _shownY + GetHiddenOffsetY();

            await DOTween.Sequence()
                .SetTarget(transform)
                .SetUpdate(true)
                .Join(_canvasGroup.DOFade(0f, _slideDuration * 0.6f).SetEase(_slideEase))
                .Join(_windowRect.DOAnchorPosY(hideY, _slideDuration).SetEase(_slideEase))
                .WithCancellation(ct);
        }

        public void HideInstant()
        {
            SetInteractable(false);
            _canvasGroup.alpha = 0f;
            _windowRect.anchoredPosition = new Vector2(_windowRect.anchoredPosition.x, _shownY + GetHiddenOffsetY());
        }

        private float GetHiddenOffsetY() => -(_windowRect.rect.height > 0 ? _windowRect.rect.height * 1.5f : 1200f);

        private void SetInteractable(bool active)
        {
            _canvasGroup.blocksRaycasts = active;
            _canvasGroup.interactable = active;
        }
        
        public Vector3 GetSlotTargetPosition(RectTransform slotRect)
        {
            _windowRect.anchoredPosition = new Vector2(_windowRect.anchoredPosition.x, _shownY);
            Canvas.ForceUpdateCanvases();

            var targetPos = slotRect.position;
            _windowRect.anchoredPosition = new Vector2(_windowRect.anchoredPosition.x, _shownY + GetHiddenOffsetY());
            return targetPos;
        }
    }
}