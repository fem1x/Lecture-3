using System;
using _Scripts.Charms;
using _Scripts.Utility.Extensions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace _Scripts.UI.Charms
{
    public class CharmDragView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public CharmGeneralView GeneralView { get; private set; }
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        [Header("Return Tween")]
        [SerializeField] private float _returnTweenTime = 0.35f;
        [SerializeField] private Ease _returnEase = Ease.OutCubic;
        
        [Header("Revealing")]
        [SerializeField] private Sprite _unrevealedSprite;

        public event Action<CharmConfigBase> OnClicked;

        private Vector3 _defaultScale;
        private bool _isPending;
        private bool _isRevealed;
        private Sprite _revealedSprite;
        private CharmConfigBase _config;
        
        private RectTransform _cellHolder;
        private RectTransform _dragLayer;
        private bool _isDraggable;
        private bool _isConsumed;
        private int? _dragPointerId;
        private Tween _returnTween;
        private CharmDragState _dragState;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        [Inject]
        public void Construct(CharmDragState dragState) => _dragState = dragState;

        private void OnDisable()
        {
            _dragState?.End(this);
            _dragPointerId = null;
            _canvasGroup.blocksRaycasts = true;
        }

        private void Start()
        {
            _defaultScale = transform.localScale;
        }

        public void Setup(CharmConfigBase config, RectTransform dragLayer, RectTransform cellHolder)
        {
            _config = config;
            _dragLayer = dragLayer;
            _cellHolder = cellHolder;
            
            GeneralView.Setup(config);
            _revealedSprite = config.Icon;
            
            SetRevealed(false);
        }

        public void SetRevealed(bool isRevealed)
        {
            _isRevealed = isRevealed;
            var sprite = _isRevealed ? _revealedSprite : _unrevealedSprite;
            GeneralView.SetIconSprite(sprite);
        }

        public void SetDraggable(bool isDraggable)
        {
            _isDraggable = isDraggable;
            if (!isDraggable) _dragState.End(this);
        }

        public bool IsDraggedBy(int pointerId) => _dragPointerId == pointerId;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.dragging) return;
            if (!_isRevealed) return;

            OnClicked?.Invoke(_config);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_isDraggable || _isPending || _dragPointerId.HasValue) return;
            if (!_dragState.TryBegin(this)) return;

            _returnTween?.Kill();
            _dragPointerId = eventData.pointerId;
            _canvasGroup.blocksRaycasts = false;
            
            _rectTransform.SetParent(_dragLayer, worldPositionStays: true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsDraggedBy(eventData.pointerId)) return;

            _rectTransform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!IsDraggedBy(eventData.pointerId)) return;

            _dragState.End(this);
            _dragPointerId = null;
            _canvasGroup.blocksRaycasts = true;

            if (!_isPending && _rectTransform.parent == _dragLayer) 
                ReturnToConveyor();
        }

        public void ReturnToConveyor()
        {
            _returnTween?.Kill();
            var startPosition = _rectTransform.position;
            var enteredHolder = false;
            var holderEntryProgress = 0f;
            var holderEntryPosition = Vector3.zero;
            _returnTween = DOVirtual.Float(0f, 1f, _returnTweenTime, progress =>
                {
                    if (!enteredHolder)
                    {
                        _rectTransform.position = Vector3.LerpUnclamped(startPosition, _cellHolder.position, progress);
                        var localPosition = _cellHolder.InverseTransformPoint(_rectTransform.position);
                        if (!_cellHolder.rect.Contains(localPosition)) return;

                        _rectTransform.SetParent(_cellHolder, worldPositionStays: true);
                        enteredHolder = true;
                        holderEntryProgress = progress;
                        holderEntryPosition = _rectTransform.localPosition;
                    }

                    var remainingProgress = Mathf.InverseLerp(holderEntryProgress, 1f, progress);
                    _rectTransform.localPosition = Vector3.LerpUnclamped(holderEntryPosition, Vector3.zero, remainingProgress);
                })
                .SetEase(_returnEase)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    _rectTransform.SetParent(_cellHolder, worldPositionStays: false);
                    _rectTransform.anchoredPosition = Vector2.zero;
                });
        }

        public void InstantReturnToConveyor()
        {
            if (_isConsumed)
            {
                Destroy(gameObject);
                return;
            }

            _dragState.End(this);
            _dragPointerId = null;
            _isPending = false;
            _returnTween?.Kill();
            _canvasGroup.blocksRaycasts = true;
            transform.localScale = Vector3.one;

            if (_rectTransform.parent != _cellHolder)
                _rectTransform.SetParent(_cellHolder, worldPositionStays: false);

            _rectTransform.anchoredPosition = Vector2.zero;
        }

        public void Consume()
        {
            FinishDragForConsumption();
            Destroy(gameObject);
        }

        public void AnimateScrap(
            Vector3 targetPosition,
            float duration,
            Ease pullEase,
            (float Strength, float Frequency, AnimationCurve Growth) shake,
            Ease shrinkEase,
            float fadeStartTime,
            float rotationAngle,
            float rotationDuration,
            Ease rotationEase)
        {
            FinishDragForConsumption();

            var startRotation = _rectTransform.localRotation;
            DOTween.Sequence()
                .Join(CreateScrapPullTween(targetPosition, duration, pullEase, shake))
                .Join(_rectTransform.DOScale(0f, duration).SetEase(shrinkEase))
                .Join(DOVirtual.Float(0f, rotationAngle, rotationDuration,
                        angle => _rectTransform.localRotation = startRotation * Quaternion.AngleAxis(angle, Vector3.forward))
                    .SetEase(rotationEase))
                .Insert(fadeStartTime, _canvasGroup.DOFade(0f, duration - fadeStartTime))
                .SetUpdate(true)
                .SetLink(gameObject)
                .OnComplete(() => Destroy(gameObject));
        }

        private Tween CreateScrapPullTween(
            Vector3 targetPosition,
            float duration,
            Ease ease,
            (float Strength, float Frequency, AnimationCurve Growth) shake)
        {
            if (shake.Strength <= 0f || shake.Frequency <= 0f)
                return _rectTransform.DOMove(targetPosition, duration).SetEase(ease);

            var startPosition = _rectTransform.position;
            return DOVirtual.Float(0f, 1f, duration, progress =>
            {
                var travel = DOVirtual.EasedValue(0f, 1f, progress, ease);
                var position = Vector3.LerpUnclamped(startPosition, targetPosition, travel);
                var phase = progress * duration * shake.Frequency * (2f * Mathf.PI);
                var offset = new Vector3(Mathf.Sin(phase), Mathf.Sin(phase * 0.79f + 1.2f), 0f);
                var amplitude = shake.Strength * Mathf.Clamp01(shake.Growth.Evaluate(progress));
                _rectTransform.position = position + _dragLayer.TransformVector(offset * (amplitude * 0.707f));
            }).SetEase(Ease.Linear);
        }

        private void FinishDragForConsumption()
        {
            _isConsumed = true;
            _isDraggable = false;
            _dragState.End(this);
            _dragPointerId = null;
            _returnTween?.Kill();
            _canvasGroup.blocksRaycasts = false;
        }

        public void StartPending()
        {
            _dragState.End(this);
            _isPending = true;
            _returnTween?.Kill();
            transform.DoScaleOut(time: 0.2f);
        }

        public void StopPending()
        {
            _isPending = false;
            transform.localScale = _defaultScale;
            transform.DoScaleIn(0.2f);
            ReturnToConveyor();
        }
    }
}
