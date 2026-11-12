using _Scripts.Utility.Extensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.UI
{
    public enum SlideDirection { Top, Bottom, Left, Right }

    public class SlidingPanelView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private bool _startsHidden = true;
        
        [Header("Slide Animation")]
        [SerializeField] private SlideDirection _direction;
        [SerializeField] private RectTransform _rect;
        [SerializeField] private CanvasGroup _cg;
        
        [Header("Show")]
        [SerializeField] private float _showTime = 0.5f;
        [SerializeField] private Ease _showEase = Ease.OutCubic;
        
        [Header("Hide")]
        [SerializeField] private float _hideTime = 0.5f;
        [SerializeField] private Ease _hideEase = Ease.OutCubic;

        private Vector2 _shownPos;
        private bool _isInitialized;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            EnsureInitialized();

            if (_startsHidden)
                HideInstant();
        }

        public async UniTask ShowAsync()
        {
            EnsureInitialized();
            _rect.DOKill();

            _rect.anchoredPosition = _rect.GetHiddenPosition(_direction, _shownPos);
            _cg.Show();

            await _rect.DOAnchorPos(_shownPos, _showTime)
                .SetEase(_showEase)
                .SetUpdate(true);
        }

        public async UniTask HideAsync()
        {
            EnsureInitialized();
            _rect.DOKill();

            var hiddenPos = _rect.GetHiddenPosition(_direction, _shownPos);

            await _rect.DOAnchorPos(hiddenPos, _hideTime)
                .SetEase(_hideEase)
                .SetUpdate(true);

            _cg.Hide();
        }

        public void HideInstant()
        {
            EnsureInitialized();
            _rect.DOKill();

            _rect.anchoredPosition = _rect.GetHiddenPosition(_direction, _shownPos);
            _cg.Hide();
        }

        private void EnsureInitialized()
        {
            if (_isInitialized) return;

            _shownPos = _rect.anchoredPosition;
            _isInitialized = true;
        }
    }
}