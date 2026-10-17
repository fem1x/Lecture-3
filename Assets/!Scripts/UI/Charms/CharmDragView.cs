using _Scripts.Charms;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Scripts.UI.Charms
{
    public class CharmDragView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public CharmView CharmView { get; private set; }
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _returnTweenTime = 0.5f;
        
        private Transform _originalParent;
        private int _originalSiblingIndex;
        private Vector3 _originalWorldPosition;
        
        private Canvas _rootCanvas;
        private RectTransform _dragLayer;
        
        private bool _isDraggable;
        private Tween _returnTween;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        private void Awake() => _rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        
        public void Setup(CharmConfigBase config, RectTransform dragLayer)
        {
            _dragLayer = dragLayer;
            CharmView.Setup(config);
        }
        
        public void SetDraggable(bool isDraggable) =>  _isDraggable = isDraggable;


        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_isDraggable) return;
            
            _canvasGroup.blocksRaycasts = false;
            RememberOriginalState();
            
            _rectTransform.SetParent(_dragLayer, worldPositionStays: true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDraggable) return;
            
            _rectTransform.anchoredPosition += eventData.delta / _rootCanvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDraggable) return;
            
            _canvasGroup.blocksRaycasts = true;
            
            if (_rectTransform.parent == _dragLayer)
                RestoreOriginalState();
        }

        private void RememberOriginalState()
        {
            _returnTween?.Kill();
            _originalParent = _rectTransform.parent;
            _originalSiblingIndex = _rectTransform.GetSiblingIndex();
            _originalWorldPosition = _rectTransform.position;
        }

        public void RestoreOriginalState()
        {
            _returnTween?.Kill();
            _returnTween = _rectTransform.DOMove(_originalWorldPosition, _returnTweenTime)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    _rectTransform.SetParent(_originalParent, worldPositionStays: true);
                    _rectTransform.SetSiblingIndex(_originalSiblingIndex);
                });
        }
        
        public void OnSuccessfullyEquipped()
        {
            _returnTween?.Kill();
            _canvasGroup.blocksRaycasts = true;
            Destroy(gameObject);
        }
    }
}