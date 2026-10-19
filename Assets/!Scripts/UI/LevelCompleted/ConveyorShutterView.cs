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
            _activeTween?.Kill();
            _activeTween = _rect.DOAnchorPosY(_openY, _openTime).SetEase(_openEase);
        }

        public void Close()
        {
            _activeTween?.Kill();
            _activeTween = _rect.DOAnchorPosY(0, _closeTime).SetEase(_closeEase);
        }
        
        public void ReOpen()
        {
            InstantClose();
            Open();
        }
        
        private void InstantClose()
        {
            _activeTween?.Kill();
            _rect.anchoredPosition = new Vector2(_rect.anchoredPosition.x, 0);
        }
    }
}