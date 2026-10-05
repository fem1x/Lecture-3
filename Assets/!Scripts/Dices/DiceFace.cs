using System;
using UnityEngine;

namespace _Scripts.Dices
{
    [Serializable]
    public class DiceFace
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public Vector3 Direction { get; private set; }
        [field: SerializeField] public int BaseValue { get; private set; }
        [field: SerializeField] public int CurrentValue { get; private set; }
        [field: SerializeField] public SpriteRenderer Renderer { get; private set; }
        
        public DiceFace(int baseValue, Vector3 direction)
        {
            BaseValue = baseValue;
            CurrentValue = baseValue;
            Direction = direction;
        } 
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public void ReduceValue(int amount = 1)
        {
            CurrentValue = Mathf.Max(0, CurrentValue - amount);
        }
        
        public void IncreaseValue(int amount = 1)
        {
            CurrentValue = Mathf.Min(BaseValue, CurrentValue + amount);
        }

        public void Reset()
        {
            CurrentValue = BaseValue;
        }
    }
}