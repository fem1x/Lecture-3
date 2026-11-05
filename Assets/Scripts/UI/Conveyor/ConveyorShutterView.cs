using DG.Tweening;
using UnityEngine;

namespace _Scripts.UI
{
    public class ConveyorShutterView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Open")]
        [SerializeField] private float _openTime = 0.5f;
        [SerializeField] private Ease _openEase = Ease.OutExpo;
        
        [Header("Close")]
        [SerializeField] private float _closeTime = 0.5f;
        [SerializeField] private Ease _closeEase = Ease.OutExpo;

        private bool _isClosed = false;
        private RectTransform _rect;
        private float _openY;
        private Tween _activeTween;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            _rect = (RectTransform)transform;
            _openY = _rect.anchoredPosition.y;
        }
        
        private void OnDestroy() => _activeTween?.Kill();

        public void Open()
        {
            if (!_isClosed) return;
            _isClosed = false;
            
            _activeTween?.Kill();
            _activeTween = _rect.DOAnchorPosY(_openY, _openTime).SetEase(_openEase);
        }

        public void Close()
        {
            if (_isClosed) return;
            _isClosed = true;
            
            _activeTween?.Kill();
            _activeTween = _rect.DOAnchorPosY(0, _closeTime).SetEase(_closeEase);
        }
        
        public void InstantClose()
        {
            _isClosed = true;
            _activeTween?.Kill();
            _rect.anchoredPosition = new Vector2(_rect.anchoredPosition.x, 0);
        }
    }
}