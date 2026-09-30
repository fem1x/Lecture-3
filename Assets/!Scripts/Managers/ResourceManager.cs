using System;
using _Scripts.Configs;
using UnityEngine;
using VContainer;

namespace _Scripts.Managers
{
    public class ResourceManager
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly ResourceConfig _config;

        [Inject]
        public ResourceManager(ResourceConfig config)
        {
            _config = config;
        }
        
        public event Action<int> OnRerollsChanged;
        public event Action<int> OnDicesChanged;
        
        private int _diceCount;
        public int DiceCount
        {
            get => _diceCount;
            private set
            {
                _diceCount = value;
                OnDicesChanged?.Invoke(_diceCount);
            }
        }
        
        private int _rerollsCount;
        public int RerollsCount
        {
            get => _rerollsCount;
            private set
            {
                _rerollsCount = value;
                OnRerollsChanged?.Invoke(_rerollsCount);
            }
        }
        
        public bool CanReroll => RerollsCount > 0;
        public bool HasDices => DiceCount > 0;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public void ResetForNewLevel()
        {
            DiceCount = _config.InitialDiceCount;
            ResetRerolls();
        }
        
        public bool TryUseReroll()
        {
            if (!CanReroll)
                return false;

            RerollsCount--;
            return true;
        }

        public void ResetRerolls()
        {
            RerollsCount = _config.RerollsPerRound;
        }
        
        public void SpendOneDice()
        {
            if (DiceCount > 0)
                DiceCount--;
        }

        public void AddRefundDice(int refundDice)
        {
            if (refundDice > 0)
                DiceCount += refundDice;
        }
    }
}