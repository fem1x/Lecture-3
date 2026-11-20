using System.Collections.Generic;
using _Scripts.Dices;
using UnityEngine;
using VContainer;

namespace _Scripts.UI.Workshop
{
    public class UpgradePanelView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private List<UpgradeSectionView> _sections = new();

        private DiceTableController _diceTableController;

        [Inject]
        public void Construct(DiceTableController diceTableController) 
            => _diceTableController = diceTableController;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public void BindSections()
        {
            var dices = _diceTableController.AllDices;
            var count = Mathf.Min(_sections.Count, dices.Count);

            for (var i = 0; i < count; i++)
                _sections[i].Bind(dices[i].Data);
        }
    }
}