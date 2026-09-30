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
    public class DiceManager : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private int _initialDiceCount = 15;
        [SerializeField] private int _rerollsPerRound = 2;

        private int _currentDiceCount;
        private int _rerollsLeft;
        private bool _isFirstRoll = true;
        private List<Dice> _dices;
    
        private DiceRoller _diceRoller;
        private CombinationsPanelView _combinationsPanel;
        private CombinationEvaluator _evaluator;
        private IScoreCalculator _calculator;
        private ScoreManager _scoreManager;
    
        [Inject]
        public void Construct(
            DiceRoller diceRoller, 
            CombinationsPanelView combinationsPanel, 
            CombinationEvaluator evaluator,  
            IScoreCalculator calculator,
            ScoreManager scoreManager)
        {
            _diceRoller = diceRoller;
            _combinationsPanel = combinationsPanel;
            _evaluator = evaluator;
            _calculator = calculator;
            _scoreManager = scoreManager;
        }
    
        public void Init(List<Dice> dices)
        {
            _dices = dices;
            _currentDiceCount = _initialDiceCount;
            _rerollsLeft = _rerollsPerRound;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
    
        private void OnEnable() => CombinationButtonView.OnAnyButtonClicked += SelectCombination;
        private void OnDisable() => CombinationButtonView.OnAnyButtonClicked -= SelectCombination;
        
        public void OnRoll(InputValue value)
        {
            if (_diceRoller.IsRolling) return;
            ExecuteRollAsync().Forget();
        }

        private async UniTaskVoid ExecuteRollAsync()
        {
            if (_isFirstRoll)
            {
                _combinationsPanel.Clear();
                
                await _diceRoller.FirstRollAsync(_dices);
                _isFirstRoll = false;
            }
            else if (_rerollsLeft > 0)
            {
                _combinationsPanel.Clear();
                
                var activeDices = _dices.Where(d => !d.IsLocked).ToList();
                if (activeDices.Count == 0) return;
                await _diceRoller.RerollAsync(activeDices);
                _rerollsLeft--;
            }
            else
            {
                return;
            }
            
            List<FoundCombination> foundCombs = _evaluator.FindAllCombinations(_dices);
            _combinationsPanel.DisplayCombinations(foundCombs);
        }
        
        public void OnScoreButtonClicked()
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
        }
        
        private void SelectCombination(FoundCombination selected)
        {
            foreach (var dice in _dices) //Deselect all
                dice.SetLock(false);

            foreach (var dice in selected.Dices) //Select dices in combination
                dice.SetLock(true);
        }
    }
}
