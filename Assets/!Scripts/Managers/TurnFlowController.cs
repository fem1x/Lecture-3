using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Dices;
using _Scripts.Interfaces;
using _Scripts.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace _Scripts.Managers
{
    public class TurnFlowController : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        public event Action<IReadOnlyList<FoundCombination>> OnCombinationsFound;
        public event Action OnCombinationsCleared;

        [SerializeField] private string _submitSfx;
        
        private DiceTableController _tableController;
        private CombinationEvaluator _evaluator;
        private IScoreCalculator _calculator;
        private ScoreManager _scoreManager;
        private ResourceManager _resourceManager;
        private DiceSpawner _diceSpawner;
        private DiceRoller _diceRoller;
        private LevelFlowController  _levelFlowController;
        private ISfxPlayer _sfxPlayer;
        
        [Inject]
        public void Construct(
            DiceTableController tableController,
            CombinationEvaluator evaluator,
            IScoreCalculator calculator,
            ScoreManager scoreManager,
            ResourceManager resourceManager,
            DiceSpawner diceSpawner,
            DiceRoller  diceRoller,
            LevelFlowController levelFlowController,
            ISfxPlayer sfxPlayer)
        {
            _tableController = tableController;
            _evaluator = evaluator;
            _calculator = calculator;
            _scoreManager = scoreManager;
            _resourceManager = resourceManager;
            _diceSpawner =  diceSpawner;
            _diceRoller = diceRoller;
            _levelFlowController = levelFlowController;
            _sfxPlayer = sfxPlayer;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
       
        public void OnRoll(InputValue inputValue)
        {
            if (_diceRoller.IsRolling) return;
            ExecuteRollAsync().Forget();
        }
        
        private async UniTaskVoid ExecuteRollAsync()
        {
            if (_tableController.HasPendingDices)
            {
                OnCombinationsCleared?.Invoke();

                var dicesToThrow = _tableController.ExtractPendingDices();
                await _diceRoller.ThrowAsync(dicesToThrow);
            }
            else
            {
                if (!_tableController.HasUnlockedDices)
                {
                    Debug.LogWarning("All dices are locked!");
                    return;
                }

                if (!_resourceManager.TryUseReroll()) return;

                OnCombinationsCleared?.Invoke();
                await _diceRoller.RerollAsync(_tableController.GetUnlockedDices());
            }

            var activeRolledDices = _tableController.GetAllDices()
                .Where(d => d != null && d.Value > 0)
                .ToList();
            
            List<FoundCombination> foundCombs = _evaluator.FindAllCombinations(activeRolledDices);
            OnCombinationsFound?.Invoke(foundCombs);
        }
        
        public void OnScoreButtonClicked()
        {
            if (_diceRoller.IsRolling) return;
            
            var selectedDices = _tableController.GetSelectedDices();
            if (selectedDices.Count == 0) return;
            
            _sfxPlayer.Play(_submitSfx); //SFX
            FinishTurn(selectedDices);
            
            if (_scoreManager.HasEnoughScore)
                return;
            
            PrepareNextTurn(selectedDices);
            
            _levelFlowController.CheckDefeatCondition();
        }

        private void FinishTurn(List<Dice> playedDices)
        {
            var result = _calculator.Calculate(playedDices);
            _scoreManager.AddScore(result.TotalScore);

            _tableController.DegradePlayedDices(playedDices, 1);
            
            _resourceManager.AddRefundDice(result.DiceRefund);

            OnCombinationsCleared?.Invoke();
        }

        private void PrepareNextTurn(List<Dice> playedDices)
        {
            _tableController.ClearSelection();
            
            int countToReturn = Mathf.Min(playedDices.Count, _resourceManager.DiceCount);
            if (countToReturn > 0)
            {
                var dicesToReturn = playedDices.Take(countToReturn).ToList();
        
                _tableController.SetPendingDices(dicesToReturn);
            }
            else
            {
                _tableController.SetPendingDices(new List<Dice>()); //0 dices left
            }
            
            _diceSpawner.ResetDicesToSpawn(playedDices);
        }
    }
}