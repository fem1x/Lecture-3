using System.Collections.Generic;
using System.Linq;
using _Scripts.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace _Scripts.Dices
{
    public class DiceTableController : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private List<Dice> _dices;
        private DiceRoller _diceRoller;
    
        [Inject]
        public void Construct(DiceRoller diceRoller)
        {
            _diceRoller = diceRoller;
        }
        
        public bool IsRolling => _diceRoller.IsRolling;
        public IReadOnlyList<Dice> GetAllDices() => _dices;
        public IReadOnlyList<Dice> GetSelectedDices() => _dices.Where(d => d.IsLocked).ToList();
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 

        private void OnEnable() => CombinationButtonView.OnAnyButtonClicked += SelectCombination;
        private void OnDisable() => CombinationButtonView.OnAnyButtonClicked -= SelectCombination;
        
        public void InitDiceList(List<Dice> dices) => _dices = dices;
        public void OnRmbClick(InputValue value) => ClearSelection();

        public async UniTask FirstThrowAsync()
        {
            await _diceRoller.FirstRollAsync(_dices);
        }

        public async UniTask RerollAsync()
        {
            var activeDices = _dices.Where(d => !d.IsLocked).ToList();
            if (activeDices.Count > 0)
                await _diceRoller.RerollAsync(activeDices);
        }
        
        public void ClearSelection()
        {
            foreach (var dice in _dices)
                if (dice != null) dice.SetLock(false);
        }
        
        public void SelectCombination(FoundCombination selected)
        {
            ClearSelection();
            foreach (var dice in selected.Dices)
                if (dice != null) dice.SetLock(true);
        }
    }
}
