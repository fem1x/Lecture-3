using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Managers;
using _Scripts.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace _Scripts.Dices
{
    public class DiceTableController : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public event Action OnSelectionChanged;
        public event Action OnSelectionAvailabilityChanged;
        public event Action OnPendingDicesChanged;
        
        public List<Dice> AllDices { get; private set; } = new();
        public List<Dice> PendingDices { get; private set; } = new();
        
        public bool HasPendingDices => PendingDices.Count > 0;
        public bool HasDicesOnTable => AllDices.Any(d => d.gameObject.activeInHierarchy && d.IsRolled);
        public bool HasUnlockedDices => AllDices.Exists(d => d.gameObject.activeInHierarchy && !d.Data.IsLocked);
        public bool HasSelectedDices => AllDices.Exists(d => d.gameObject.activeInHierarchy && d.Data.IsLocked);
        public bool IsInteractionLocked { get; private set; }
        
        private GameStateManager _gameStateManager;

        [Inject]
        public void Construct(GameStateManager gameStateManager)
        {
            _gameStateManager = gameStateManager;
            _gameStateManager.StateChanged += OnStateChanged;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        private void OnDestroy()
        {
            if (_gameStateManager != null)
                _gameStateManager.StateChanged -= OnStateChanged;

            foreach (var dice in AllDices)
            {
                if (dice == null) continue;
                dice.Data.OnLockChanged -= HandleDiceLockChanged;
                dice.Data.OnValueChanged -= HandleDiceValueChanged;
            }
        }

        private void OnStateChanged(GameState state)
        {
            if (state != GameState.Game)
            {
                DeactivateAllDices();
            }
        }
        
        public Dice GetDiceById(int id) => AllDices.Find(d => d.Data.Id == id);
        public List<Dice> GetSelectedDices() => AllDices.Where(d => d.gameObject.activeInHierarchy && d.Data.IsLocked).ToList();
        public List<Dice> GetUnlockedDices() => AllDices.Where(d => d.gameObject.activeInHierarchy && !d.Data.IsLocked).ToList();
        public bool CanSelectAll()
        {
            if (IsInteractionLocked) return false;

            var hasUnlockedDice = false;
            foreach (var dice in AllDices)
            {
                if (!dice.gameObject.activeInHierarchy || dice.Data.IsLocked) continue;
                if (!dice.CanBeSelected) return false;
                hasUnlockedDice = true;
            }

            return hasUnlockedDice;
        }

        public bool CanClearSelection() => !IsInteractionLocked &&
            AllDices.Exists(d => d.gameObject.activeInHierarchy && d.Data.IsLocked && d.CanBeSelected);
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        public void OnRmbClick(InputValue value)
        {
            if (SettingsMenuView.BlocksGameplayInput) return;
            ClearSelection();
        }
        
        public void InitDiceList(List<Dice> dices)
        {
            foreach (var dice in AllDices)
            {
                dice.Data.OnLockChanged -= HandleDiceLockChanged;
                dice.Data.OnValueChanged -= HandleDiceValueChanged;
            }

            AllDices = dices;
            SetPendingDices(dices);

            foreach (var dice in dices)
            {
                dice.Data.OnLockChanged += HandleDiceLockChanged;
                dice.Data.OnValueChanged += HandleDiceValueChanged;
            }

            OnSelectionAvailabilityChanged?.Invoke();
        }
        
        public void SetPendingDices(List<Dice> dices)
        {
            PendingDices.Clear();
            if (dices != null)
                PendingDices.AddRange(dices);
            OnPendingDicesChanged?.Invoke();
        }

        public void AddPendingDices(List<Dice> dices)
        {
            foreach (var dice in dices)
            {
                if (dice != null && !PendingDices.Contains(dice))
                    PendingDices.Add(dice);
            }
            OnPendingDicesChanged?.Invoke();
        }
        
        public List<Dice> ExtractPendingDices()
        {
            var result = new List<Dice>(PendingDices);
            PendingDices.Clear();
            OnPendingDicesChanged?.Invoke();
            return result;
        }
        
        public void SetInteractionLock(bool isLocked)
        {
            IsInteractionLocked = isLocked;
            OnSelectionChanged?.Invoke();
        }

        internal void NotifySelectionAvailabilityChanged() => OnSelectionAvailabilityChanged?.Invoke();
        
        public void ClearSelection()
        {
            if (IsInteractionLocked) return;
            foreach (var dice in AllDices)
                if (dice != null) dice.SetLock(false);
        }

        public void SelectAll()
        {
            if (IsInteractionLocked) return;
            if (!CanSelectAll()) return;
            foreach (var dice in GetUnlockedDices())
                dice.SetLock(true);
        }

        public void ActivateAllDices(List<Dice> dices)
        {
            foreach (var dice in dices)
                dice.gameObject.SetActive(true);
            OnSelectionAvailabilityChanged?.Invoke();
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
            OnSelectionAvailabilityChanged?.Invoke();
        }
        
        private void HandleDiceLockChanged(bool _) => OnSelectionChanged?.Invoke();
        private void HandleDiceValueChanged(int oldValue, int newValue) => OnSelectionAvailabilityChanged?.Invoke();
    }
}
