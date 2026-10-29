using System.Collections.Generic;
using _Scripts.Combinations;
using _Scripts.Managers;
using _Scripts.UI.CodeLock;
using _Scripts.Utility.Extensions;
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
                HideLock();
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
        }

        private void HideLock() => _group.Hide();

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
    }
}