using System.Collections.Generic;
using _Scripts;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using Random = UnityEngine.Random;

public class DiceManager : MonoBehaviour
{
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
    private bool _isFirstRoll = true;
    private DiceRoller _roller;
    private List<Dice> _dices;
    [Inject]
    public void Construct(DiceRoller roller, List<Dice> dices)
    {
        _roller = roller;
        _dices = dices;
    }
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
    
    public void OnRoll(InputValue value)
    {
        
    }
    
    private List<int> CollectDiceValues()
    {
        var values = new List<int>();
        foreach (var dice in _dices)
        {
            values.Add(dice.Value);
            Debug.Log(dice.Value);
        }
        return values;
    }
}
