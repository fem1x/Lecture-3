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
        [Header("Button")]
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
        private DiceRepairService _repairService;

        [Inject]
        public void Construct(DiceRepairService repairService)
        {
            _repairService = repairService;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        private void Awake()
        {
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0f;
        }

        public void Bind(IReadOnlyDiceData data)
        {
            _data = data;
            
            _data.OnDataChanged += RefreshState;
            _repairButton.onClick.AddListener(OnRepairClicked);

            RefreshStateInstant();
        }

        private void OnDestroy()
        {
            _fadeTween?.Kill();
            _repairButton.onClick.RemoveListener(OnRepairClicked);
            
            if (_data == null) return;
            _data.OnDataChanged -= RefreshState;
        }
        
        public void SetRepairMode(bool enable)
        {
            _repairModeEnabled = enable;
            RefreshState();
        }
        
        private void RefreshState()
        {
            if (_data == null) return;

            var shouldShow = _repairModeEnabled && (_data.IsDamaged || _data.CurrentDurability < _data.MaxDurability);

            _canvasGroup.blocksRaycasts = shouldShow;
            _repairButton.interactable = shouldShow;

            DoFadeTween(shouldShow);
        }
        
        private void RefreshStateInstant()
        {
            if (_data == null) return;

            var shouldShow = _repairModeEnabled && _data.IsDamaged;
            _canvasGroup.blocksRaycasts = shouldShow;
            _repairButton.interactable = shouldShow;
            _canvasGroup.alpha = shouldShow ? 1f : 0f;
        }

        private void DoFadeTween(bool show)
        {
            var targetAlpha = show ? 1f : 0f;
            var duration = show ? _fadeInDuration : _fadeOutDuration;
            var ease = show ? _easeIn : _easeOut;

            _fadeTween?.Kill();
            _fadeTween = _canvasGroup.DOFade(targetAlpha, duration).SetEase(ease);
        }
        
        private void OnRepairClicked()
        {
            _repairService.TryRepair(_data);
        }
    }
}