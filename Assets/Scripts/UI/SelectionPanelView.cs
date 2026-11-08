using System.Collections.Generic;
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
        private DiceRoller _diceRoller;
        
        [Inject]
        public void Construct(DiceTableController tableController, DiceRoller diceRoller)
        {
            _tableController = tableController;
            _diceRoller = diceRoller;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void OnEnable()
        {
            _selectAllButton.onClick.AddListener(_tableController.SelectAll);
            _deselectAllButton.onClick.AddListener(_tableController.ClearSelection);
            
            _tableController.OnSelectionChanged += UpdateButtonsState;
            _diceRoller.OnRollCompleted += HandleRollCompleted;
            
            UpdateButtonsState();
        }

        private void OnDisable()
        {
            _selectAllButton.onClick.RemoveListener(_tableController.SelectAll);
            _deselectAllButton.onClick.RemoveListener(_tableController.ClearSelection);
            
            _tableController.OnSelectionChanged -= UpdateButtonsState;
            _diceRoller.OnRollCompleted -= HandleRollCompleted;
        }
        
        private void UpdateButtonsState()
        {
            _selectAllButton.interactable = _tableController.CanSelectAll();
            _deselectAllButton.interactable = _tableController.GetSelectedDices().Count > 0;
        }
        
        private void HandleRollCompleted(bool isFirstThrow, List<Dice> dices) => UpdateButtonsState();
    }
}