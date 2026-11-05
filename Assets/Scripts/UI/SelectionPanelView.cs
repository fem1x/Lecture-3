using _Scripts.Dices;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI
{
    public class SelectionPanelView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private Button _selectAllButton;
        [SerializeField] private Button _deselectAllButton;
        
        private DiceTableController _tableController;
        
        [Inject]
        public void Construct(DiceTableController tableController) => _tableController = tableController;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void OnEnable()
        {
            _selectAllButton.onClick.AddListener(_tableController.SelectAll);
            _deselectAllButton.onClick.AddListener(_tableController.ClearSelection);
        }

        private void OnDisable()
        {
            _selectAllButton.onClick.RemoveListener(_tableController.SelectAll);
            _deselectAllButton.onClick.RemoveListener(_tableController.ClearSelection);
        }
    }
}