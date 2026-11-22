using System.Collections.Generic;
using _Scripts.Dices;
using _Scripts.Managers;
using UnityEngine;
using VContainer;

namespace _Scripts.UI
{
    public class DiceRepairPanelView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private List<DiceRepairView> _repairViews = new();

        private RepairTokensManager _repairTokensManager;
        private DiceTableController _diceTableController;
        private bool _isBound;

        [Inject]
        public void Construct(
            RepairTokensManager repairTokensManager,
            DiceTableController diceTableController)
        {
            _repairTokensManager = repairTokensManager;
            _diceTableController = diceTableController;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public void SetRepairMode(bool enable)
        {
            if (enable && !_isBound)
                BindDices();

            var count = Mathf.Min(_repairViews.Count, _diceTableController.AllDices.Count);
            for (var i = 0; i < count; i++)
                _repairViews[i].SetRepairMode(enable);
        }

        private void BindDices()
        {
            var dices = _diceTableController.AllDices;
            var count = Mathf.Min(_repairViews.Count, dices.Count);

            for (var i = 0; i < count; i++)
                _repairViews[i].Bind(dices[i].Data);

            _isBound = true;
        }
    }
}
