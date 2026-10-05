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

        [Header("SFX")]
        [SerializeField] private string _submitSfx;
        
        private bool _isScoring;
        
        #region DI
        private DiceTableController _tableController;
        private IScoreCalculator _calculator;
        private ScoreManager _scoreManager;
        private ResourceManager _resourceManager;
        private DiceSpawner _diceSpawner;
        private DiceRoller _diceRoller;
        private LevelFlowController  _levelFlowController;
        private ISfxPlayer _sfxPlayer;
        private ScoreSequenceController _scoreSequenceController;
        
        [Inject]
        public void Construct(
            DiceTableController tableController,
            IScoreCalculator calculator,
            ScoreManager scoreManager,
            ResourceManager resourceManager,
            DiceSpawner diceSpawner,
            DiceRoller  diceRoller,
            LevelFlowController levelFlowController,
            ISfxPlayer sfxPlayer,
            ScoreSequenceController scoreSequenceController)
        {
            _tableController = tableController;
            _calculator = calculator;
            _scoreManager = scoreManager;
            _resourceManager = resourceManager;
            _diceSpawner =  diceSpawner;
            _diceRoller = diceRoller;
            _levelFlowController = levelFlowController;
            _sfxPlayer = sfxPlayer;
            _scoreSequenceController = scoreSequenceController;
        }
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
       
        public void OnRoll(InputValue inputValue)
        {
            if (_diceRoller.IsRolling || _isScoring) return;
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

            var activeRolledDices = _tableController.AllDices
                .Where(d => d != null && d.Value > 0)
                .ToList();
            
            OnCombinationsCleared?.Invoke();
        }
        
        public void OnScoreButtonClicked()
        {
            if (_diceRoller.IsRolling || _isScoring) return;
            
            var selectedDices = _tableController.GetSelectedDices();
            if (selectedDices.Count == 0) return;
            
            _sfxPlayer.Play(_submitSfx); //SFX
            
            ExecuteScoreFlowAsync(selectedDices).Forget();
        }

        private async UniTaskVoid ExecuteScoreFlowAsync(List<Dice> selectedDices)
        {
            _isScoring = true;
            
            var result = _calculator.Calculate(selectedDices);
            await _scoreSequenceController.PlaySequenceAsync(selectedDices, result);
            
            _isScoring = false;

            if (_scoreManager.HasEnoughScore) 
                return;
            
            _tableController.DegradePlayedDices(selectedDices, 1);
            _resourceManager.AddRefundDice(result.Data.DiceRefund);
            PrepareNextTurn(selectedDices);
            _levelFlowController.CheckDefeatCondition();
        }

        private void PrepareNextTurn(List<Dice> playedDices)
        {
            _tableController.ClearSelection();
            
            int countToReturn = Mathf.Min(playedDices.Count, _resourceManager.DiceCount);
            if (countToReturn > 0)
            {
                var dicesToReturn = playedDices.Take(countToReturn).ToList();
                _tableController.AddPendingDices(dicesToReturn);
                _diceSpawner.ResetDicesToSpawn(_tableController.PendingDices);
                
                var eliminatedDices = playedDices.Skip(countToReturn).ToList();
                foreach (var extraDice in eliminatedDices)
                    extraDice.gameObject.SetActive(false);
            }
            else
            {
                foreach (var dice in playedDices)
                    dice.gameObject.SetActive(false);
            }
        }
    }
}