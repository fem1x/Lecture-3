using System;
using UnityEngine;

namespace _Scripts.Interfaces
{
    public interface IReadOnlyDiceData
    {
        int Id { get; }
        int RolledValue { get; }
        bool IsLocked { get; }
        int MaxDurability { get; }
        int CurrentDurability { get; }
        int DamageLevel { get; }
        bool IsDamaged { get; }
        bool NeedsRepair { get; }
    
        event Action OnDataChanged;
        event Action<bool> OnLockChanged;
        event Action<int> OnValueChanged;

        Vector3 GetFaceDirection(int index);
        int GetFaceValue(int index);
    }
}