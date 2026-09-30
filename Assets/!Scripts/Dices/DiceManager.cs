using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace _Scripts.Dices
{
    public class DiceManager : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        private bool _isFirstRoll = true;
        private List<Dice> _dices;
    
        private DiceRoller _diceRoller;
        private ScoreCalculator _scoreCalculator;
    
        [Inject]
        public void Construct(DiceRoller diceRoller, ScoreCalculator scoreCalculator)
        {
            _diceRoller = diceRoller;
            _scoreCalculator = scoreCalculator;;
        }
    
        public void Init(List<Dice> dices)
        {
            _dices = dices;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
    
        public void OnRoll(InputValue value)
        {
            if (_diceRoller.IsRolling) return;
            ExecuteRollAsync().Forget();
        }

        private async UniTaskVoid ExecuteRollAsync()
        {
            if (_isFirstRoll)
            {
                await _diceRoller.FirstRollAsync(_dices);
                _isFirstRoll = false;
            }
            else
            {
                var activeDices = _dices.Where(d => !d.IsLocked).ToList();
                if (activeDices.Count == 0) return;
                await _diceRoller.RerollAsync(activeDices);
            }
        
            var values = CollectDiceValues();
            Debug.Log($"Результат раунда: {string.Join(", ", values)}");
        }
    
        private List<int> CollectDiceValues()
        {
            var values = _dices.Select(d => d.Value).ToList();
            return values;
        }
        
        public async UniTaskVoid OnScoreButtonClicked()
        {
            if (_diceRoller.IsRolling) return;
            var score = _scoreCalculator.EvaluateHand(CollectDiceValues());
            
        }
    }
}
