using System.Collections.Generic;
using _Scripts.Combinations;
using _Scripts.Managers;
using _Scripts.UI.CodeLock;
using _Scripts.Utility.Extensions;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using VContainer;

namespace _Scripts.UI
{
    public class CodeLockView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public List<CodeLockSlotView> Slots { get; private set; }
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private LocalizeStringEvent _codeNameEvent;

        [Header("Show / Hide")] 
        [SerializeField] private RectTransform _rect;
        [SerializeField] private float _yOffset = 200f;
        
        [Header("Show")]
        [SerializeField] private float _showTime = 0.5f;
        [SerializeField] private Ease  _showEase = Ease.OutCubic; 
        
        [Header("Hide")]
        [SerializeField] private float _hideTime = 0.5f;
        [SerializeField] private Ease  _hideEase = Ease.OutCubic;

        [Header("Quota Overlay")]
        [SerializeField] private CodeLockQuotaView _quotaView;

        private Tween _activeTween;
        private CodeLockService _codeLockService;

        [Inject]
        public void Construct(CodeLockService codeLockService)
        {
            _codeLockService = codeLockService;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Start()
        {
            var lockCombo = _codeLockService.CurrentLock;
            if (lockCombo != null)
                ShowLock(lockCombo);
            else
                HideLockInstant();
        }

        private void OnEnable()
        {
            _codeLockService.OnLockSet += ShowLock;
            _codeLockService.OnLockBroken += HideLock;
        }
        
        private void OnDisable()
        {
            _codeLockService.OnLockSet -= ShowLock;
            _codeLockService.OnLockBroken -= HideLock;
        }
        
        private void ShowLock(CombinationConfig combo)
        {
            if (combo == null)
            {
                HideLock();
                return;
            }
            
            SetupSlots(combo);
            _codeNameEvent.StringReference = combo.DisplayName;
            _group.Show();
            
            DoShowTween();
        }

        private void HideLock()
        {
            DoHideTween();
            _quotaView.Hide();
        }

        private void HideLockInstant()
        {
            _quotaView.Hide();
            _group.Hide();
        }

        private void SetupSlots(CombinationConfig combo)
        {
            int requiredCount = combo.Patterns.Length > 0 
                ? combo.Patterns[0].Values.Length 
                : 5;
            
            for (int i = 0; i < Slots.Count; i++)
            {
                Slots[i].SetActiveState(i < requiredCount);
            }
        }

        public void ClearSlots()
        {
            foreach (var slot in Slots)
                slot.RemoveValue();
        }

        private void DoShowTween()
        {
            _activeTween.Kill();
            _rect.anchoredPosition = new Vector2(_rect.anchoredPosition.x, _yOffset);
            _activeTween = _rect.DOAnchorPosY(0, _showTime).SetEase(_showEase)
                .OnComplete(() => _quotaView.Show());
        }

        private void DoHideTween()
        {
            _activeTween.Kill();
            _activeTween = _rect.DOAnchorPosY(_yOffset, _hideTime).SetEase(_hideEase)
                .OnComplete(() => _group.Hide());
        }
    }
}