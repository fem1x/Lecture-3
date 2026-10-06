using _Scripts.Managers;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Scripts.UI
{
    public class DiceRepairPanelView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private TMP_Text _tokensText;
        
        private RepairTokensManager  _repairTokensManager;

        [Inject]
        public void Construct(RepairTokensManager repairTokensManager)
        {
            _repairTokensManager = repairTokensManager;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void OnEnable() => _repairTokensManager.OnTokensCountChange += RefreshValue;
        private void OnDisable() => _repairTokensManager.OnTokensCountChange -= RefreshValue;
        
        private void RefreshValue(int newValue) => _tokensText.text = newValue.ToString();
    }
}