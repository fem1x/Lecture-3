using _Scripts.Combinations;
using _Scripts.Managers;
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
        
        private void ShowLock(CombinationConfig combinationCode)
        {
            if (combinationCode == null)
            {
                HideLock();
                return;
            }
            
            _codeNameEvent.StringReference = combinationCode.DisplayName;
            _group.Show();
        }

        private void HideLock() => _group.Hide();
    }
}