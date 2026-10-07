using System.Collections.Generic;
using _Scripts.Combinations;
using _Scripts.Dices;

namespace _Scripts.Managers
{
    public class CombinationEvaluator
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly ActiveCombinationsService _activeCombinationsService;

        public CombinationEvaluator(ActiveCombinationsService activeCombinationsService)
        {
            _activeCombinationsService = activeCombinationsService;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public CombinationConfig Evaluate(IReadOnlyList<Dice> selectedDices)
        {
            if (selectedDices == null || selectedDices.Count == 0)
                return null;

            var activeConfigs = _activeCombinationsService.ActiveCombinations;
            foreach (var config in activeConfigs)
            {
                if (config.Matches(selectedDices))
                    return config;
            }

            return null;
        }
    }
}