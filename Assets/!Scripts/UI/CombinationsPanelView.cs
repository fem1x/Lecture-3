using System;
using System.Collections.Generic;
using _Scripts.Configs;
using _Scripts.Dices;
using _Scripts.Interfaces;
using UnityEngine;
using VContainer;

namespace _Scripts.UI
{
    public class CombinationsPanelView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private CombinationButtonView _buttonPrefab;
        [SerializeField] private Transform _container;
        
        private List<CombinationButtonView> _spawnedButtons = new();
        private CombinationScoreConfig _scoreConfig;

        [Inject]
        public void Construct(CombinationScoreConfig scoreConfig)
        {
            _scoreConfig = scoreConfig;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void DisplayCombinations(IReadOnlyList<FoundCombination> combinations)
        {
            Clear();
            
            for (int i = 0; i < combinations.Count; i++)
            {
                var combination = combinations[i];
                var button = Instantiate(_buttonPrefab, _container);
                
                button.Setup(combination, _scoreConfig);
                _spawnedButtons.Add(button);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _spawnedButtons.Count; i++)
            {
                if (_spawnedButtons[i] != null)
                    Destroy(_spawnedButtons[i].gameObject);
            }
            _spawnedButtons.Clear();
        }
    }
}