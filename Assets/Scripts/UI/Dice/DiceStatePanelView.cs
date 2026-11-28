using System.Collections.Generic;
using _Scripts.Dices;
using UnityEngine;
using VContainer;

namespace _Scripts.UI
{
    public class DiceStatePanelView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private List<DiceStateView> _diceStateViews = new();
        private DiceTableController _tableController;

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Inject]
        public void Construct(DiceTableController tableController) => _tableController = tableController;

        public void InitDiceStateViews(List<Dice> dices)
        {
            for (var i = 0; i < dices.Count; i++)
                _diceStateViews[i].Bind(dices[i], _tableController);
        }
    }
}
