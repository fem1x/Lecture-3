using System.Collections.Generic;
using _Scripts.Dices;
using UnityEngine;
using VContainer;

namespace _Scripts.UI
{
    public class DiceNetPanelView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private List<DiceNetView> _diceNetViews;
        
        private DiceTableController  _diceTableController;

        [Inject]
        public void Construct(DiceTableController diceTableController)
        {
            _diceTableController = diceTableController;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void OnEnable() => _diceTableController.OnDicesStateChanged += RefreshAll;
        private void OnDisable() => _diceTableController.OnDicesStateChanged -= RefreshAll;
        
        private void RefreshAll(List<Dice> tableDices)
        {
            for (int i = 0; i < _diceNetViews.Count && i < tableDices.Count; i++)
            {
                var dice = tableDices[i];
                _diceNetViews[i].UpdateView(dice.Faces, dice.CurrentDurability, dice.MaxDurability);
            }
        }
    }
}