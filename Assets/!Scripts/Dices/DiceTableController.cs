using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Interfaces;
using _Scripts.Managers;
using _Scripts.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace _Scripts.Dices
{
    public class DiceTableController : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private List<Dice> _dices;
        private DiceRoller _diceRoller;
    
        [Inject]
        public void Construct(DiceRoller diceRoller)
        {
            _diceRoller = diceRoller;
        }
        
        public IReadOnlyList<Dice> GetAllDices() => _dices;
        public List<Dice> GetSelectedDices() => _dices.Where(d => d.IsLocked).ToList();
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 

        private void OnEnable() => CombinationButtonView.OnAnyButtonClicked += SelectCombination;
        private void OnDisable() => CombinationButtonView.OnAnyButtonClicked -= SelectCombination;
        
        public void InitDiceList(List<Dice> dices)
        {
            _dices = dices;
        }
        
        public async UniTask FirstThrowAsync()
        {
            await _diceRoller.FirstRollAsync(_dices);
        }

        public async UniTask RerollAsync()
        {
            var activeDices = _dices.Where(d => !d.IsLocked).ToList();
            if (activeDices.Count > 0)
                await _diceRoller.RerollAsync(activeDices);
        }
        
        
        /*public void OnScoreButtonClicked()
        {
            if (_diceRoller.IsRolling) return;
            
            List<Dice> selectedDices = _dices.Where(d => d.IsLocked).ToList();
            if (selectedDices.Count == 0)
            {
                Debug.LogWarning("Dices aren't selected!");
                return;
            }
           
            ScoreCalculationResult result = _calculator.Calculate(selectedDices);
            Debug.Log($"Combination: {result.Type} | Score: ({result.BasePoints} + {result.DicePoints}) * {result.Multiplier} = {result.TotalScore}");
            _scoreManager.AddScore(result.TotalScore);
            _resourceManager.UpdateDiceAfterRound(selectedDices.Count, result.DiceRefund);
        }*/
        
        public void ClearSelection()
        {
            foreach (var dice in _dices)
                if (dice != null) dice.SetLock(false);
        }
        
        public void SelectCombination(FoundCombination selected)
        {
            ClearSelection();
            foreach (var dice in selected.Dices)
                if (dice != null) dice.SetLock(true);
        }
    }
}
