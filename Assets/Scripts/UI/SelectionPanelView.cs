using _Scripts.Dices;
using _Scripts.UI.Tooltips;
using UnityEngine.Localization;
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

        private void Awake()
        {
            var template = transform.parent.GetComponentInChildren<RollButtonView>(true)
                .GetComponent<BasicTooltipTrigger>();
            ConfigureTooltip(_selectAllButton, template, "controls_select_all_tooltip");
            ConfigureTooltip(_deselectAllButton, template, "controls_deselect_all_tooltip");
        }

        private static void ConfigureTooltip(Button button, BasicTooltipTrigger template, string key)
        {
            var tooltip = button.GetComponent<BasicTooltipTrigger>();
            if (tooltip == null)
            {
                tooltip = button.gameObject.AddComponent<BasicTooltipTrigger>();
                tooltip.CopyHoverSettings(template);
            }
            tooltip.Configure(template.Prefab, new LocalizedString("String Table", key));
        }

        private void OnEnable()
        {
            _selectAllButton.onClick.AddListener(_tableController.SelectAll);
            _deselectAllButton.onClick.AddListener(_tableController.ClearSelection);
            
            _tableController.OnSelectionChanged += UpdateButtonsState;
            _diceRoller.OnRollStateChanged += UpdateButtonsState;
            
            UpdateButtonsState();
        }

        private void OnDisable()
        {
            _selectAllButton.onClick.RemoveListener(_tableController.SelectAll);
            _deselectAllButton.onClick.RemoveListener(_tableController.ClearSelection);
            
            _tableController.OnSelectionChanged -= UpdateButtonsState;
            _diceRoller.OnRollStateChanged -= UpdateButtonsState;
        }
        
        private void UpdateButtonsState()
        {
            _selectAllButton.interactable = _tableController.CanSelectAll();
            _deselectAllButton.interactable = _tableController.HasSelectedDices;
        }
    }
}
