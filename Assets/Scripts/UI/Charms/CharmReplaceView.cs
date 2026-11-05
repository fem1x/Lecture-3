using System;
using _Scripts.Utility.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace _Scripts.UI.Charms
{
    public class CharmReplaceView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("References")]
        [SerializeField] private CanvasGroup _cg;
        [SerializeField] private LocalizeStringEvent _charmStringEvent;
        
        [Header("Icons")]
        [SerializeField] private Image _oldCharmIcon;
        [SerializeField] private Image _newCharmIcon;
        
        [Header("Buttons")]
        [SerializeField] private Button _confirmBtn;
        [SerializeField] private Button _cancelBtn;
        
        private Action _onConfirm;
        private Action _onCancel;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Start() => Hide();
        
        private void OnEnable()
        {
            _confirmBtn.onClick.AddListener(HandleConfirm);
            _cancelBtn.onClick.AddListener(HandleCancel);
        }

        private void OnDisable()
        {
            _confirmBtn.onClick.RemoveListener(HandleConfirm);
            _cancelBtn.onClick.RemoveListener(HandleCancel);
        }
        
        public void Show(
            RectTransform target, 
            CharmGeneralView oldView, 
            CharmGeneralView newView, 
            Action onConfirm, 
            Action onCancel)
        {
            _onConfirm = onConfirm;
            _onCancel = onCancel;

            transform.position = target.position;
            _charmStringEvent.StringReference = oldView.Config.Name;
            _oldCharmIcon.sprite = oldView.Config.Icon;
            _newCharmIcon.sprite = newView.Config.Icon;
            
            _cg.Show();
        }

        private void Hide()
        {
            _onConfirm = null;
            _onCancel = null;
            _cg.Hide();
        }

        private void HandleConfirm()
        {
            var action = _onConfirm;
            Hide();
            action?.Invoke();
        }

        private void HandleCancel()
        {
            var action = _onCancel;
            Hide();
            action?.Invoke();
        }
    }
}