using _Scripts.Interfaces;
using _Scripts.Managers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI
{
    public class DiceRepairView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private Button _repairButton;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        [Header("Button Fade animation")]
        [SerializeField] private float _fadeInDuration = 0.5f;
        [SerializeField] private float _fadeOutDuration = 0.15f;
        [SerializeField] private Ease _easeIn = Ease.OutQuad;
        [SerializeField] private Ease _easeOut = Ease.InQuad;

        private Tween _fadeTween;
        private bool _repairModeEnabled;
        private IReadOnlyDiceData _data;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        private void Awake()
        {
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0f;
        }

        public void Bind(IReadOnlyDiceData data)
        {
            _data = data;
            
            _data.OnDataChanged += RefreshButtonState;
            _repairButton.onClick.AddListener(OnRepairClicked);
            
            RefreshButtonState();
        }

        private void OnDestroy()
        {
            _fadeTween?.Kill();
            _repairButton.onClick.RemoveListener(OnRepairClicked);
            
            if (_data == null) return;
            _data.OnDataChanged -= RefreshButtonState;
        }
        
        public void SetRepairMode(bool enable)
        {
            _repairModeEnabled = enable;
            _canvasGroup.blocksRaycasts = enable;
            DoFadeTween(enable);
            RefreshButtonState();
        }
        
        private void RefreshButtonState()
        {
            if (_data == null) return;
            
            var canRepair = _repairModeEnabled && _data.IsDamaged;
            _repairButton.interactable = canRepair;
        }

        private void DoFadeTween(bool enable)
        {
            var duration =  enable ? _fadeInDuration : _fadeOutDuration;
            var ease = enable ? _easeIn : _easeOut;
            var targetAlpha = enable ? 1f : 0f;
            _fadeTween?.Kill();
            _fadeTween = _canvasGroup.DOFade(targetAlpha, duration).SetEase(ease);
        }
        
        private void OnRepairClicked()
        {
            Debug.Log("OnRepairClicked");
        }
    }
}