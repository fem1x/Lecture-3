using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class DiceThrower : MonoBehaviour
{
    [Header("Dices")]
    [SerializeField] private List<Dice> _dicesList = new();
    [SerializeField] private Dice _dicePrefab;
    
    [Header("Throw Stats")]
    [SerializeField] private Vector3 _throwDirection = new(0f, 0.5f, 1f);
    [SerializeField] private float _force = 10f;
    [SerializeField] private float _forceRandomize = 2f;
    [SerializeField] private float _torque = 10f;
    [SerializeField] private float _torqueRandomize = 2f;
    [SerializeField] private bool _randomizeStartingRotation = true;
    [SerializeField] private float _timeBetweenThrows = 0.2f;
    
    [Header("Spawn Position")]
    [SerializeField] private Transform _basePosition;
    [SerializeField] private float _spacing = 2f;
    
    private bool _isRolling;

    private void Awake() => InitDices();

    private void Start()
    {
        StartCoroutine(ThrowDices(_dicesList));
        Invoke(nameof(CollectDiceValues), 3);
    }
        

    private void InitDices()
    {
        for (int i = 0; i < 5; i++)
        {
            var startingRotation = _randomizeStartingRotation ? Random.rotation : Quaternion.identity;
            var dice = Instantiate(_dicePrefab, GetDiceStartingPosition(i), startingRotation);
            _dicesList.Add(dice);
        }
    }

    private Vector3 GetDiceStartingPosition(int i)
    {
        var position = _basePosition.position;
        var offset = Vector3.right * _spacing * (i-2);
        return (position + offset);
    }

    private IEnumerator ThrowDices(List<Dice> dices)
    {
        foreach (var dice in dices)
        {
            Throw(dice);
            yield return new WaitForSeconds(_timeBetweenThrows);
        }
    }

    private void Throw(Dice dice)
    {
        float forceMag = Utils.NumberInRange(_force, _forceRandomize);
        float torqueMag = Utils.NumberInRange(_torque, _torqueRandomize);
        
        var force = _throwDirection.normalized * forceMag;
        var torque = Random.insideUnitSphere.normalized * torqueMag;
        
        dice.Roll(force, torque);
    }

    private List<int> CollectDiceValues()
    {
        var values = new List<int>();
        foreach (var dice in _dicesList)
        {
            values.Add(dice.GetValue());
        }
        return values;
    }
}
