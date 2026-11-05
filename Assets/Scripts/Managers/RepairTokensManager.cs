using System;
using UnityEngine;

namespace _Scripts.Managers
{
    public class RepairTokensManager
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public event Action<int> OnTokensCountChange;
        
        private int _currentTokens;
        public int CurrentTokens
        {
            get => _currentTokens;
            private set 
            {
                _currentTokens = value;
                OnTokensCountChange?.Invoke(value);
            }
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public bool TrySpendTokens(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"Tokens to spend must be >= 0!");
                return false;
            }
            
            if (CurrentTokens >= amount)
            {
                CurrentTokens -= amount;
                return true;
            }

            return false;
        }

        public void AddTokens(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"Tokens to add must be >= 0!");
                return;
            }
            CurrentTokens += amount;
        }
        
        public void ResetTokens(int initialAmount = 0)
        {
            CurrentTokens = initialAmount;
        }
    }
}