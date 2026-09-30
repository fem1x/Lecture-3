using System;
using System.Collections.Generic;
using _Scripts.Dices;
using _Scripts.Interfaces;
using _Scripts.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace _Scripts.Managers
{
    public class RoundFlowController : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        public event Action<List<FoundCombination>> OnCombinationsFound;
        public event Action OnCombinationsCleared;
        
        private bool _isFirstRoll = true;
        
        private DiceTableController _tableController;
        private CombinationEvaluator _evaluator;
        private IScoreCalculator _calculator;
        private ScoreManager _scoreManager;
        private ResourceManager _resourceManager;
        
        [Inject]
        public void Construct(
            DiceTableController tableController,
            CombinationEvaluator evaluator,
            IScoreCalculator calculator,
            ScoreManager scoreManager,
            ResourceManager resourceManager)
        {
            _tableController = tableController;
            _evaluator = evaluator;
            _calculator = calculator;
            _scoreManager = scoreManager;
            _resourceManager = resourceManager;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        
        public void OnRoll(InputValue inputValue)
        {
            if (_tableController.IsRolling) return;
            ExecuteRollAsync().Forget();
        }
        
        private async UniTaskVoid ExecuteRollAsync()
        {
            if (_isFirstRoll)
            {
                OnCombinationsCleared?.Invoke();
                await _tableController.FirstThrowAsync();
                _isFirstRoll = false;
            }
            else
            {
                var selectedCount = _tableController.GetSelectedDices().Count;
                var totalCount = _tableController.GetAllDices().Count;

                if (selectedCount == totalCount)
                {
                    Debug.LogWarning("All dices are locked!");
                    return;
                }

                if (!_resourceManager.TryUseReroll()) return;
                
                OnCombinationsCleared?.Invoke();
                await _tableController.RerollAsync();
            }

            List<FoundCombination> foundCombs = _evaluator.FindAllCombinations(_tableController.GetAllDices());
            OnCombinationsFound?.Invoke(foundCombs);
        }
        
        public void OnScoreButtonClicked()
        {
            if (_tableController.IsRolling) return;

            var selectedDices = _tableController.GetSelectedDices();
            if (selectedDices.Count == 0) return;

            var result = _calculator.Calculate(selectedDices);
            _scoreManager.AddScore(result.TotalScore);

            _resourceManager.UpdateDiceAfterRound(selectedDices.Count, result.DiceRefund);
            ResetRound();
        }

        private void ResetRound()
        {
            _resourceManager.ResetRerolls();

            _tableController.ClearSelection();
            _isFirstRoll = true;

            OnCombinationsCleared?.Invoke();
        }
    }
}