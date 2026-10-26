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
        public event Action OnSelectionChanged;
        
        public List<Dice> AllDices { get; private set; } = new();
        public List<Dice> PendingDices { get; private set; } = new();
        
        public bool HasPendingDices => PendingDices.Count > 0;
        public bool HasUnlockedDices => GetUnlockedDices().Count > 0;
        
        public Dice GetDiceById(int id) => AllDices.Find(d => d.Data.Id == id);
        public List<Dice> GetSelectedDices() => AllDices.Where(d => d.Data.IsLocked).ToList();
        public List<Dice> GetUnlockedDices() => AllDices.Where(d => !d.Data.IsLocked).ToList();
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        public void OnRmbClick(InputValue value) => ClearSelection();
        
        public void InitDiceList(List<Dice> dices)
        {
            AllDices = dices;
            SetPendingDices(dices);

            foreach (var dice in dices)
                dice.Data.OnLockChanged += HandleDiceLockChanged;
        }
        
        public void SetPendingDices(List<Dice> dices)
        {
            PendingDices.Clear();
            if (dices != null)
                PendingDices.AddRange(dices);
        }

        public void AddPendingDices(List<Dice> dices)
        {
            foreach (var dice in dices)
            {
                if (dice != null && !PendingDices.Contains(dice))
                    PendingDices.Add(dice);
            }
        }
        
        public List<Dice> ExtractPendingDices()
        {
            var result = new List<Dice>(PendingDices);
            PendingDices.Clear();
            return result;
        }
        
        public void ClearSelection()
        {
            foreach (var dice in AllDices)
                if (dice != null) dice.Data.SetLock(false);
        }

        public void ActivateAllDices(List<Dice> dices)
        {
            foreach (var dice in dices)
                dice.gameObject.SetActive(true);
        }
        
        public void DeactivateAllDices()
        {
            ClearSelection();
            
            if (AllDices != null)
            {
                foreach (var dice in AllDices)
                {
                    if (dice != null)
                        dice.gameObject.SetActive(false);
                }
            }
        }
        
        private void HandleDiceLockChanged(bool _) => OnSelectionChanged?.Invoke();
    }
}
