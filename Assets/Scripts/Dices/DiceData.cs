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
        private readonly int[] _faceBonuses = new int[6];

        private int _upgradeCount;

        public DiceData(int id, DiceDataConfig config)
        {
            Id = id;
            _config = config;
            ResetToDefault();
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public event Action OnDataChanged;
        public event Action<bool> OnLockChanged;
        public event Action<int, int> OnValueChanged; // oldVal, newVal
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public int RolledValue { get; private set; } = -1;
        public int RolledFaceIndex { get; private set; } = -1;
        public int RolledFaceBonus => RolledFaceIndex >= 0
            ? GetFaceBonus(RolledFaceIndex)
            : 0;

        public int UpgradeCount => _upgradeCount;

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

        public int GetFaceBonus(int index) => _faceBonuses[index];

        public void AddFaceBonus(int index, int bonus)
        {
            _faceBonuses[index] += bonus;
            _upgradeCount++;

            OnDataChanged?.Invoke();
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public void SetRolledValue(int value)
        {
            if (RolledValue == value) return;

            var oldValue = RolledValue;
            RolledValue = value;

            OnValueChanged?.Invoke(oldValue, value);
            OnDataChanged?.Invoke();
        }

        public void SetRolledFace(int index)
        {
            if (RolledFaceIndex == index) return;

            var oldValue = RolledValue;
            RolledFaceIndex = index;
            RolledValue = GetFaceValue(index);

            if (oldValue != RolledValue)
                OnValueChanged?.Invoke(oldValue, RolledValue);

            OnDataChanged?.Invoke();
        }

        private void ResetRolledFace()
        {
            if (RolledFaceIndex == -1 && RolledValue == -1) return;

            var oldValue = RolledValue;
            RolledFaceIndex = -1;
            RolledValue = -1;

            if (oldValue != -1)
                OnValueChanged?.Invoke(oldValue, -1);

            OnDataChanged?.Invoke();
        }

        public void SetLock(bool value)
        {
            if (IsLocked == value) return;

            IsLocked = value;
            OnLockChanged?.Invoke(value);
        }

        public void ToggleLock() => SetLock(!IsLocked);

        public void TakeDamage(int amount = 1, bool notify = true)
        {
            CurrentDurability -= amount;

            if (CurrentDurability <= 0)
            {
                Degrade(notify: notify);
                return;
            }

            if (notify) OnDataChanged?.Invoke();
        }

        public void Degrade(bool restoreHp = true, bool notify = true)
        {
            if (restoreHp)
                CurrentDurability = MaxDurability;

            DamageLevel++;

            if (notify) OnDataChanged?.Invoke();
        }

        public void NotifyDataChanged() => OnDataChanged?.Invoke();

        public void Repair()
        {
            CurrentDurability = MaxDurability;

            if (DamageLevel > 0)
                DamageLevel--;

            OnDataChanged?.Invoke();
        }

        public void RoundReset()
        {
            ResetRolledFace();
            SetLock(false);
        }

        public void ResetToDefault()
        {
            DamageLevel = 0;
            CurrentDurability = MaxDurability;

            Array.Clear(_faceBonuses, 0, _faceBonuses.Length);
            _upgradeCount = 0;

            RoundReset();
            OnDataChanged?.Invoke();
        }
    }
}