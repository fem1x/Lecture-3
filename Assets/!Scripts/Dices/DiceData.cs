using System;
using _Scripts.Configs;
using _Scripts.Interfaces;
using UnityEngine;

namespace _Scripts.Dices
{
    public class DiceData : IReadOnlyDiceData
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public int Id { get; }
        private readonly DiceDataConfig _config;

        public DiceData(int id, DiceDataConfig config)
        {
            Id = id;
            _config = config;
            ResetToDefault();
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public event Action OnDataChanged;
        public event Action<bool> OnLockChanged;
        public event Action<int> OnValueChanged;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public int RolledValue { get; private set; } = -1;
        public bool IsLocked { get; private set; }

        public int MaxDurability => _config.MaxDurability;
        public int CurrentDurability { get; private set; }

        public int DamageLevel { get; private set; }
        public bool IsDamaged => DamageLevel > 0;
        public bool NeedsRepair => IsDamaged || CurrentDurability < MaxDurability;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public Vector3 GetFaceDirection(int index) => _config.Faces[index].Direction;
        
        public int GetFaceValue(int index)
        {
            return Mathf.Max(0, _config.Faces[index].BaseValue - DamageLevel);
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public void SetValue(int value)
        {
            if (RolledValue == value) return;
            RolledValue = value;
            OnValueChanged?.Invoke(value);
        }

        public void SetLock(bool value)
        {
            if (IsLocked == value) return;
            IsLocked = value;
            OnLockChanged?.Invoke(value);
        }
        
        public void ToggleLock() => SetLock(!IsLocked);
        
        public void TakeDamage(int amount = 1)
        {
            CurrentDurability -= amount;
            if (CurrentDurability <= 0)
            {
                CurrentDurability = MaxDurability;
                DamageLevel++;
            }

            OnDataChanged?.Invoke();
        }

        public void Repair()
        {
            CurrentDurability = MaxDurability;
            if (DamageLevel > 0)
                DamageLevel--;

            OnDataChanged?.Invoke();
        }

        public void RoundReset()
        {
            SetValue(-1);
            SetLock(false);
        }
        
        public void ResetToDefault()
        {
            DamageLevel = 0;
            CurrentDurability = MaxDurability;
            RoundReset();
            OnDataChanged?.Invoke();
        }
    }
}