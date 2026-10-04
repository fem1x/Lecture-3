using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class CombinationsInfoView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _infoPanel;
        
        private void OnEnable() => _button.onClick.AddListener(HandleClick);
        private void OnDisable() => _button.onClick.RemoveListener(HandleClick);
        
        private void HandleClick()
        {
            bool isActive = _infoPanel.activeSelf;
            _infoPanel.SetActive(!isActive);
        }
    }
}