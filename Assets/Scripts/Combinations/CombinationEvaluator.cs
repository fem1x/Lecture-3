using System.Collections.Generic;
using _Scripts.Charms;
using _Scripts.Combinations;
using _Scripts.Dices;

namespace _Scripts.Managers
{
    public class CombinationEvaluator
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly ActiveCombinationsService _activeCombinationsService;
        private readonly CharmsService _charmsService;

        public CombinationEvaluator(ActiveCombinationsService activeCombinationsService, CharmsService charmsService)
        {
            _activeCombinationsService = activeCombinationsService;
            _charmsService = charmsService;
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

                foreach (var charm in _charmsService.ActiveCharms)
                {
                    if (charm.MatchesCombination(config, selectedDices))
                        return config;
                }
            }

            return null;
        }
    }
}
