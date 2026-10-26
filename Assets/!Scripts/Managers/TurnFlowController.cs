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
        [Header("SFX")]
        [SerializeField] private string _submitSfx;
        
        private bool _isScoring;
        
        public event Action<ScoreSequencePlan, List<Dice>> OnScoreCalculated;
        
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
        private DiceDamageService _damageService;
        private CodeLockService _codeLockService;
        private CombinationEvaluator _evaluator;
        
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
            ScoreSequenceController scoreSequenceController,
            DiceDamageService damageService,
            CodeLockService  codeLockService,
            CombinationEvaluator evaluator)
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
            _damageService = damageService;
            _codeLockService = codeLockService;
            _evaluator = evaluator;
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

                await _diceRoller.RerollAsync(_tableController.GetUnlockedDices());
            }

            var activeRolledDices = _tableController.AllDices
                .Where(d => d != null && d.Data.RolledValue > 0)
                .ToList();
            
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
            
            if (_codeLockService.IsActive)
                await CodeLockFlow(selectedDices);
            else 
                await StandardFlow(selectedDices);
            
            _isScoring = false;

            if (_scoreManager.HasEnoughScore)
            {
                _tableController.DeactivateAllDices();
                return;
            }
            
            PrepareNextTurn(selectedDices);
            _levelFlowController.CheckDefeatCondition();
        }

        private async UniTask StandardFlow(List<Dice> selectedDices)
        {
            var scoreSequence = _calculator.Calculate(selectedDices);
            OnScoreCalculated?.Invoke(scoreSequence, selectedDices);
            
            await _scoreSequenceController.PlaySequenceAsync(scoreSequence);
            _damageService.DamageDices(selectedDices);
        }

        private async UniTask CodeLockFlow(List<Dice> selectedDices)
        {
            var combination = _evaluator.Evaluate(selectedDices);
            var result = _codeLockService.EvaluateAttempt(selectedDices, combination);
            if (result == LockBreakResult.None)
            {
                Debug.Log("Invalid unlock attempt!");
                return;
            }
            
            //TODO: SequenceFLow
            
            _codeLockService.BreakLock(result, selectedDices);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        }

        private void PrepareNextTurn(List<Dice> playedDices)
        {
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

            _tableController.ClearSelection();
        }
    }
}