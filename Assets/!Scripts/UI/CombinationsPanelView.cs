using System;
using System.Collections.Generic;
using _Scripts.Configs;
using _Scripts.Dices;
using _Scripts.Interfaces;
using _Scripts.Managers;
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
        private RoundFlowController _roundFlowController;

        [Inject]
        public void Construct(CombinationScoreConfig scoreConfig, RoundFlowController roundFlowController)
        {
            _scoreConfig = scoreConfig;
            _roundFlowController = roundFlowController;
            
            if (isActiveAndEnabled)
                Subscribe();
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake() =>  Clear();
        private void OnEnable()
        {
            if (_roundFlowController != null)
                Subscribe();
        }

        private void OnDisable()
        {
            if (_roundFlowController != null)
                Unsubscribe();
        }

        private void Subscribe()
        {
            Unsubscribe();
            _roundFlowController.OnCombinationsCleared += Clear;
            _roundFlowController.OnCombinationsFound += DisplayCombinations;
        }

        private void Unsubscribe()
        {
            _roundFlowController.OnCombinationsCleared -= Clear;
            _roundFlowController.OnCombinationsFound -= DisplayCombinations;
        }
        
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