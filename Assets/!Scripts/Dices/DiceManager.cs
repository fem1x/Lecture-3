using System.Collections.Generic;
using System.Linq;
using _Scripts;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public class DiceManager : MonoBehaviour
{
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
    private bool _isFirstRoll = true;
    private List<Dice> _dices;
    
    private DiceRoller _roller;
    
    [Inject]
    public void Construct(DiceRoller roller)
    {
        _roller = roller;
    }
    
    public void Init(List<Dice> dices)
    {
        _dices = dices;
    }
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
    
    public void OnRoll(InputValue value)
    {
        if (_roller.IsRolling) return;
        ExecuteRollAsync().Forget();
    }

    private async UniTaskVoid ExecuteRollAsync()
    {
        if (_isFirstRoll)
        {
            await _roller.FirstRollAsync(_dices);
            _isFirstRoll = false;
        }
        else
        {
            var activeDices = _dices.Where(d => !d.IsLocked).ToList();
            if (activeDices.Count == 0) return;
            await _roller.RerollAsync(activeDices);
        }
        
        var values = CollectDiceValues();
        Debug.Log($"Результат раунда: {string.Join(", ", values)}");
    }
    
    private List<int> CollectDiceValues()
    {
        var values = _dices.Select(d => d.Value).ToList();
        return values;
    }
}
