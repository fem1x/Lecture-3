using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.Dices
{
    public class DiceTableController : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public event Action<List<Dice>> OnDicesStateChanged;
        
        private List<Dice> _allDices = new();
        private List<Dice> _pendingDicesToThrow = new();
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        public bool HasPendingDices => _pendingDicesToThrow.Count > 0;
        public bool HasUnlockedDices => GetUnlockedDices().Count > 0;
        
        public List<Dice> GetAllDices() => _allDices;
        public List<Dice> GetSelectedDices() => _allDices.Where(d => d.IsLocked).ToList();
        public List<Dice> GetUnlockedDices() => _allDices.Where(d => !d.IsLocked).ToList();
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 

        private void Start() => OnDicesStateChanged?.Invoke(_allDices);
        private void OnEnable() => CombinationButtonView.OnAnyButtonClicked += SelectCombination;
        private void OnDisable() => CombinationButtonView.OnAnyButtonClicked -= SelectCombination;
        public void OnRmbClick(InputValue value) => ClearSelection();
        
        public void InitDiceList(List<Dice> dices)
        {
            _allDices = dices;
            SetPendingDices(dices);
        }
        
        public void SetPendingDices(List<Dice> dices)
        {
            _pendingDicesToThrow.Clear();
            if (dices != null)
                _pendingDicesToThrow.AddRange(dices);
        }
        
        public List<Dice> ExtractPendingDices()
        {
            var result = new List<Dice>(_pendingDicesToThrow);
            _pendingDicesToThrow.Clear();
            return result;
        }
        
        public void ClearSelection()
        {
            foreach (var dice in _allDices)
                if (dice != null) dice.SetLock(false);
        }
        
        private void SelectCombination(FoundCombination selected)
        {
            ClearSelection();
            foreach (var dice in selected.Dices)
                if (dice != null) dice.SetLock(true);
        }
        
        public void DegradePlayedDices(List<Dice> dices, int amount = 1)
        {
            if (dices == null) return;
            foreach (var dice in dices)
                dice.TakeHit(amount);
            
            OnDicesStateChanged?.Invoke(dices);
        }
    }
}
