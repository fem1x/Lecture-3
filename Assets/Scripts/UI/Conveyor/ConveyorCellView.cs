using _Scripts.UI.Charms;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.UI
{
    public class ConveyorCellView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public RectTransform CharmHolder { get; private set; }
        [SerializeField] private CanvasGroup _inspectHint;
        [SerializeField] private CanvasGroup _dragHint;

        [Header("Hint Animation")]
        [SerializeField, Min(0f)] private float _hintShowDelay = 0.34f;
        [FormerlySerializedAs("_hintDuration")]
        [SerializeField, Min(0f)] private float _hintShowDuration = 0.28f;
        [SerializeField] private Ease _hintShowEase = Ease.OutCubic;
        [SerializeField, Min(0f)] private float _hintHideDuration = 0.18f;
        [SerializeField] private Ease _hintHideEase = Ease.InCubic;

        public CharmDragView CurrentCharm { get; private set; }

        private RectTransform _inspectHintRect;
        private RectTransform _dragHintRect;
        private float _inspectHintShownX;
        private float _dragHintShownX;
        private Sequence _hintTween;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        private void Awake()
        {
            (_inspectHintRect, _inspectHintShownX) = InitializeHint(_inspectHint);
            (_dragHintRect, _dragHintShownX) = InitializeHint(_dragHint);
        }

        private static (RectTransform rect, float shownX) InitializeHint(CanvasGroup hint)
        {
            var rect = (RectTransform)hint.transform;
            var position = rect.anchoredPosition;
            rect.anchoredPosition = new Vector2(0f, position.y);
            hint.alpha = 0f;
            return (rect, position.x);
        }

        private void OnDestroy() => _hintTween?.Kill();

        public void AttachCharm(CharmDragView card) => CurrentCharm = card;
        public void DetachCharm() => CurrentCharm = null;
        
        public void SetDraggable(bool isDraggable)
        {
            if (CurrentCharm != null)
                CurrentCharm.SetDraggable(isDraggable);
        }

        public void SetHintsVisible(bool visible)
        {
            _hintTween?.Kill();

            var alpha = visible ? 1f : 0f;
            var delay = visible ? _hintShowDelay : 0f;
            var duration = visible ? _hintShowDuration : _hintHideDuration;
            var ease = visible ? _hintShowEase : _hintHideEase;
            _hintTween = DOTween.Sequence()
                .Insert(delay, _inspectHintRect.DOAnchorPosX(visible ? _inspectHintShownX : 0f, duration).SetEase(ease))
                .Insert(delay, _dragHintRect.DOAnchorPosX(visible ? _dragHintShownX : 0f, duration).SetEase(ease))
                .Insert(delay, _inspectHint.DOFade(alpha, duration))
                .Insert(delay, _dragHint.DOFade(alpha, duration));
        }
    }
}
