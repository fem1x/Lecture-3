using _Scripts.UI.Charms;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.UI
{
    public class ConveyorCellView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public RectTransform CharmHolder { get; private set; }
        [SerializeField] private CanvasGroup _inspectHint;
        [SerializeField] private CanvasGroup _dragHint;
        [SerializeField, Min(0f)] private float _hintTravel = 112f;
        [SerializeField, Min(0f)] private float _hintDuration = 0.24f;

        public CharmDragView CurrentCharm { get; private set; }

        private Sequence _hintTween;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
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

            var travel = visible ? _hintTravel : 0f;
            var alpha = visible ? 1f : 0f;
            var ease = visible ? Ease.OutCubic : Ease.InCubic;
            _hintTween = DOTween.Sequence()
                .Join(((RectTransform)_inspectHint.transform).DOAnchorPosX(-travel, _hintDuration).SetEase(ease))
                .Join(((RectTransform)_dragHint.transform).DOAnchorPosX(travel, _hintDuration).SetEase(ease))
                .Join(_inspectHint.DOFade(alpha, _hintDuration))
                .Join(_dragHint.DOFade(alpha, _hintDuration));
        }
    }
}
