using System.Collections.Generic;
using _Scripts.Configs;
using VContainer;

namespace _Scripts.Combinations
{
    public class ActiveCombinationsService
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly List<CombinationConfig> _activeCombinations = new();
        public IReadOnlyList<CombinationConfig> ActiveCombinations => _activeCombinations;
        
        [Inject]
        public ActiveCombinationsService(InitialCombinationsConfig config)
        {
            if (config.DefaultCombinations != null)
            {
                SetCombinations(config.DefaultCombinations);
            }
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public void AddCombination(CombinationConfig config)
        {
            if (_activeCombinations.Contains(config)) return;
            
            _activeCombinations.Add(config);
            SortList();
        }

        public void RemoveCombination(CombinationConfig config)
        {
            _activeCombinations.Remove(config);
        }

        public void SetCombinations(IReadOnlyList<CombinationConfig> configs)
        {
            _activeCombinations.Clear();
            if (configs == null) return;
            
            _activeCombinations.AddRange(configs);
            SortList();
        }
        
        private void SortList()
        {
            _activeCombinations.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }
    }
}