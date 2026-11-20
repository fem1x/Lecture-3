using System.Collections.Generic;
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
        [Header("Dice State")]
        [SerializeField] private DurabilityView _durabilityView;
        [SerializeField] private DiceDamageView _damageView;
        [SerializeField] private Image _diceImage;
        [SerializeField] private Sprite _unrolledSprite;
        [SerializeField] private List<Sprite> _diceSprites = new();

        [Header("Repair Button")]
        [SerializeField] private Button _repairButton;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Button Fade Animation")]
        [SerializeField] private float _fadeInDuration = 0.5f;
        [SerializeField] private float _fadeOutDuration = 0.15f;
        [SerializeField] private Ease _easeIn = Ease.OutQuad;
        [SerializeField] private Ease _easeOut = Ease.InQuad;

        private Tween _fadeTween;
        private bool _repairModeEnabled;
        private IReadOnlyDiceData _data;
        private DiceRepairService _repairService;

        [Inject]
        public void Construct(DiceRepairService repairService) => _repairService = repairService;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void Awake()
        {
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0f;
            _repairButton.onClick.AddListener(OnRepairClicked);
        }

        private void OnDestroy()
        {
            _fadeTween?.Kill();
            _repairButton.onClick.RemoveListener(OnRepairClicked);
            Unbind();
        }

        public void Bind(IReadOnlyDiceData data)
        {
            Unbind();
            _data = data;

            _data.OnDataChanged += RefreshState;
            _data.OnValueChanged += RefreshDiceFace;

            RefreshStateInstant();
            _diceImage.sprite = GetFaceSprite(_data.RolledValue);
        }

        public void SetRepairMode(bool enable)
        {
            _repairModeEnabled = enable;
            RefreshState();
        }

        private void Unbind()
        {
            if (_data == null) return;

            _data.OnDataChanged -= RefreshState;
            _data.OnValueChanged -= RefreshDiceFace;
            _data = null;
        }

        private void RefreshState()
        {
            if (_data == null) return;

            _damageView.SetDamage(_data.IsDamaged, _data.DamageLevel);
            _durabilityView.SetDurability(_data.CurrentDurability, _data.MaxDurability);

            var shouldShow = _repairModeEnabled && (_data.IsDamaged || _data.CurrentDurability < _data.MaxDurability);
            _canvasGroup.blocksRaycasts = shouldShow;
            _repairButton.interactable = shouldShow;

            DoFadeTween(shouldShow);
        }

        private void RefreshStateInstant()
        {
            if (_data == null) return;

            _damageView.SetDamage(_data.IsDamaged, _data.DamageLevel);
            _durabilityView.SetDurability(_data.CurrentDurability, _data.MaxDurability);

            var shouldShow = _repairModeEnabled && (_data.IsDamaged || _data.CurrentDurability < _data.MaxDurability);
            _canvasGroup.blocksRaycasts = shouldShow;
            _repairButton.interactable = shouldShow;
            _canvasGroup.alpha = shouldShow ? 1f : 0f;
        }

        private void RefreshDiceFace(int oldValue, int newValue)
        {
            if (oldValue == newValue) return;
            _diceImage.sprite = GetFaceSprite(newValue);
        }

        private void DoFadeTween(bool show)
        {
            var targetAlpha = show ? 1f : 0f;
            var duration = show ? _fadeInDuration : _fadeOutDuration;
            var ease = show ? _easeIn : _easeOut;

            _fadeTween?.Kill();
            _fadeTween = _canvasGroup.DOFade(targetAlpha, duration).SetEase(ease);
        }

        private void OnRepairClicked() => _repairService.TryRepair(_data);

        private Sprite GetFaceSprite(int value) => value == -1 ? _unrolledSprite : _diceSprites[value];
    }
}