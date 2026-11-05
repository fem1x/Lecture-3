using System;
using System.Collections.Generic;
using _Scripts.Combinations;
using _Scripts.Dices;
using VContainer;

namespace _Scripts.Managers
{
    public class ActiveSelectionService : IDisposable
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public event Action<List<Dice>, CombinationConfig> OnSelectionEvaluated;
        
        private readonly DiceTableController _diceTableController;
        private readonly CombinationEvaluator _combinationEvaluator;

        [Inject]
        public ActiveSelectionService(DiceTableController diceTableController, CombinationEvaluator combinationEvaluator)
        {
            _diceTableController = diceTableController;
            _combinationEvaluator = combinationEvaluator;

            _diceTableController.OnSelectionChanged += HandleSelectionChanged;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        private void HandleSelectionChanged()
        {
            var selectedDices = _diceTableController.GetSelectedDices();

            if (selectedDices.Count == 0)
            {
                OnSelectionEvaluated?.Invoke(selectedDices, null);
                return;
            }

            var combination = _combinationEvaluator.Evaluate(selectedDices);
            OnSelectionEvaluated?.Invoke(selectedDices, combination);
        }

        public void Dispose()
        {
            if (_diceTableController != null)
                _diceTableController.OnSelectionChanged -= HandleSelectionChanged;
        }
    }
}