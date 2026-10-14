using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class CharmConveyorView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private Button _nextCharmButton;
        [SerializeField] private Button _previousCharmButton;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void OnEnable()
        {
            _nextCharmButton.onClick.AddListener(HandleNextCharmButtonClick);
            _previousCharmButton.onClick.AddListener(HandlePreviousCharmButtonClick);
        }

        private void OnDisable()
        {
            _nextCharmButton.onClick.RemoveListener(HandleNextCharmButtonClick);
            _previousCharmButton.onClick.RemoveListener(HandlePreviousCharmButtonClick);
        }
        
        private void HandleNextCharmButtonClick()
        {
            
        }

        private void HandlePreviousCharmButtonClick()
        {
            
        }
    }
}