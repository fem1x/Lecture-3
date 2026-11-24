using System;
using System.Collections.Generic;
using _Scripts.Charms;
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
        private readonly CharmsService _charmsService;

        [Inject]
        public ActiveSelectionService(
            DiceTableController diceTableController,
            CombinationEvaluator combinationEvaluator,
            CharmsService charmsService)
        {
            _diceTableController = diceTableController;
            _combinationEvaluator = combinationEvaluator;
            _charmsService = charmsService;

            _diceTableController.OnSelectionChanged += HandleSelectionChanged;
            _charmsService.OnCharmAdded += HandleCharmChanged;
            _charmsService.OnCharmRemoved += HandleCharmChanged;
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

        private void HandleCharmChanged(CharmBase charm) => HandleSelectionChanged();

        public void Dispose()
        {
            if (_diceTableController != null)
                _diceTableController.OnSelectionChanged -= HandleSelectionChanged;

            _charmsService.OnCharmAdded -= HandleCharmChanged;
            _charmsService.OnCharmRemoved -= HandleCharmChanged;
        }
    }
}
