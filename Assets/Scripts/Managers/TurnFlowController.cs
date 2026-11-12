using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Data_Transfer.Contexts;
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
        [Header("Input")]
        [SerializeField] private InputActionReference _rollAction;

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
        private LevelFlowController _levelFlowController;
        private ISfxPlayer _sfxPlayer;
        private ScoreSequenceController _scoreSequenceController;
        private CodeLockService _codeLockService;
        private CombinationEvaluator _evaluator;
        private LockSequenceController _lockSequenceController;
        private ReturnSequenceController _returnSequenceController;
        private EndTurnSequenceController _endTurnSequenceController;
        
        [Inject]
        public void Construct(
            DiceTableController tableController,
            IScoreCalculator calculator,
            ScoreManager scoreManager,
            ResourceManager resourceManager,
            DiceSpawner diceSpawner,
            DiceRoller diceRoller,
            LevelFlowController levelFlowController,
            ISfxPlayer sfxPlayer,
            ScoreSequenceController scoreSequenceController,
            CodeLockService codeLockService,
            CombinationEvaluator evaluator,
            LockSequenceController lockSequenceController,
            ReturnSequenceController returnSequenceController,
            EndTurnSequenceController endTurnSequenceController)
        {
            _tableController = tableController;
            _calculator = calculator;
            _scoreManager = scoreManager;
            _resourceManager = resourceManager;
            _diceSpawner = diceSpawner;
            _diceRoller = diceRoller;
            _levelFlowController = levelFlowController;
            _sfxPlayer = sfxPlayer;
            _scoreSequenceController = scoreSequenceController;
            _codeLockService = codeLockService;
            _evaluator = evaluator;
            _lockSequenceController = lockSequenceController;
            _returnSequenceController = returnSequenceController;
            _endTurnSequenceController = endTurnSequenceController;
        }
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 

        private void OnEnable()
        {
            if (_rollAction == null) return;
            
            _rollAction.action.Enable();
            _rollAction.action.performed += HandleRollPerformed;
        }

        private void OnDisable()
        {
            if (_rollAction == null) return;
            
            _rollAction.action.performed -= HandleRollPerformed;
            _rollAction.action.Disable();
        }

        private void HandleRollPerformed(InputAction.CallbackContext ctx)
        {
            if (!_diceRoller.CanRoll) return;
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
            
            _sfxPlayer.Play(_submitSfx);
            
            ExecuteScoreFlowAsync(selectedDices).Forget();
        }

        private async UniTaskVoid ExecuteScoreFlowAsync(List<Dice> selectedDices)
        {
            _isScoring = true;
            _tableController.SetInteractionLock(true);
    
            if (_codeLockService.IsActive)
                await CodeLockFlow(selectedDices);
            else 
                await StandardFlow(selectedDices);

            if (_scoreManager.HasEnoughScore)
            {
                _isScoring = false;
                _tableController.SetInteractionLock(false);
                _tableController.DeactivateAllDices();
                return;
            }
    
            PrepareNextTurn(selectedDices);

            _isScoring = false;
            _tableController.SetInteractionLock(false);
            _levelFlowController.CheckDefeatCondition();
        }

        private async UniTask StandardFlow(List<Dice> selectedDices)
        {
            var scoreSequence = _calculator.Calculate(selectedDices);
            OnScoreCalculated?.Invoke(scoreSequence, selectedDices);
    
            await _scoreSequenceController.PlaySequenceAsync(scoreSequence);

            if (_scoreManager.HasEnoughScore) return;

            await _endTurnSequenceController.ProcessAllDicesAsync(selectedDices, forceDegrade: false, destroyCancellationToken);
        }

        private async UniTask CodeLockFlow(List<Dice> selectedDices)
        {
            var combination = _evaluator.Evaluate(selectedDices);
            var result = _codeLockService.EvaluateAttempt(selectedDices, combination);
            if (result == LockBreakResult.None)
            {
                Debug.LogWarning("Invalid unlock attempt!");
                return;
            }
            
            await _lockSequenceController.PlaySequenceAsync(selectedDices, result);
            _codeLockService.BreakLock(result, selectedDices);

            if (result == LockBreakResult.Forced)
                await _endTurnSequenceController.ProcessAllDicesAsync(selectedDices, forceDegrade: true, destroyCancellationToken);
            else
                await _returnSequenceController.ReturnAllDiceAsync(selectedDices);
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