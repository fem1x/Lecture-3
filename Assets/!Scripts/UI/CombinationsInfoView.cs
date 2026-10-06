using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI
{
    public class CombinationsInfoView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private List<Button> _buttons;
        [SerializeField] private GameObject _infoPanel;
        
        private AudioFilterController _audioFilterController;

        [Inject]
        public void Construct(AudioFilterController audioFilterController)
        {
            _audioFilterController = audioFilterController;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        private void OnEnable()
        {
            foreach (var button in _buttons)
                button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            foreach (var button in _buttons)
                button.onClick.RemoveListener(HandleClick);
        }  
        
        private void HandleClick()
        {
            var isActive = _infoPanel.activeSelf;
            _infoPanel.SetActive(!isActive);
            
            _audioFilterController.ToggleFilter(!isActive);
        }
    }
}