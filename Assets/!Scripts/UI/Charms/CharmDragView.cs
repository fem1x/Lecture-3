using _Scripts.Charms;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Scripts.UI.Charms
{
    public class CharmDragView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public CharmGeneralView GeneralView { get; private set; }
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        [Header("Return Tween")]
        [SerializeField] private float _returnTweenTime = 0.35f;
        [SerializeField] private Ease _returnEase = Ease.OutCubic;
        
        [Header("Revealing")]
        [SerializeField] Sprite _unrevealedSprite;

        private bool _isRevealed;
        private Sprite _revealedSprite;
        
        private RectTransform _cellHolder;
        private RectTransform _dragLayer;
        private bool _isDraggable;
        private Tween _returnTween;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void Setup(CharmConfigBase config, RectTransform dragLayer, RectTransform cellHolder)
        {
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

        public void SetDraggable(bool isDraggable) => _isDraggable = isDraggable;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_isDraggable) return;

            _returnTween?.Kill();
            _canvasGroup.blocksRaycasts = false;
            
            _rectTransform.SetParent(_dragLayer, worldPositionStays: true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDraggable) return;

            _rectTransform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDraggable) return;

            _canvasGroup.blocksRaycasts = true;

            if (_rectTransform.parent == _dragLayer)
                ReturnToConveyor();
        }

        public void ReturnToConveyor()
        {
            _returnTween?.Kill();
            _returnTween = _rectTransform.DOMove(_cellHolder.position, _returnTweenTime)
                .SetEase(_returnEase)
                .OnComplete(() =>
                {
                    _rectTransform.SetParent(_cellHolder, worldPositionStays: false);
                    _rectTransform.anchoredPosition = Vector2.zero;
                });
        }

        public void InstantReturnToConveyor()
        {
            _returnTween?.Kill();
            _canvasGroup.blocksRaycasts = true;

            if (_rectTransform.parent != _cellHolder)
            {
                _rectTransform.SetParent(_cellHolder, worldPositionStays: false);
                _rectTransform.anchoredPosition = Vector2.zero;
            }
        }

        public void OnSuccessfullyEquipped()
        {
            _returnTween?.Kill();
            Destroy(gameObject);
        }
    }
}