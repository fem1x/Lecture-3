using System;
using _Scripts.Utility.Extensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.UI
{
    public enum SlideDirection
    {
        Top,
        Bottom,
        Left,
        Right
    }
    
    public class SlidingPanelView : MonoBehaviour
    {
        [SerializeField] private bool _startsHidden = true;
        
        [Header("Slide Animation")] 
        [SerializeField] private SlideDirection _direction;
        [SerializeField] private RectTransform _rect;
        [SerializeField] private CanvasGroup _cg;
        
        [Header("Show")]
        [SerializeField] private float _showTime = 0.5f;
        [SerializeField] private Ease  _showEase = Ease.OutCubic; 
        
        [Header("Hide")]
        [SerializeField] private float _hideTime = 0.5f;
        [SerializeField] private Ease  _hideEase = Ease.OutCubic;

        private Vector2 _hiddenPos;
        private Vector2 HiddenPos
        {
            get
            {
                if (_hiddenPos == default)
                    _hiddenPos = _rect.GetHiddenPosition(_direction);
                return _hiddenPos;
            }
        }
        
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            if (_startsHidden) HideInstant();
        }
        
        public async UniTask ShowAsync()
        {
            HideInstant();
            _cg.Show();
            await _rect.DOAnchorPos(Vector2.zero, _showTime).SetEase(_showEase);
        }

        public async UniTask HideAsync()
        {
            await _rect.DOAnchorPos(HiddenPos, _hideTime).SetEase(_hideEase)
                .OnComplete(_cg.Hide);
        }

        private void HideInstant()
        {
            _rect.anchoredPosition = HiddenPos;
        }
    }
}