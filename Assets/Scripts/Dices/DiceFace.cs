using System;
using UnityEngine;

namespace _Scripts.Dices
{
    [Serializable]
    public class DiceFace
    {
        [field: SerializeField] public int BaseValue { get; private set; }
        [field: SerializeField] public Vector3 Direction { get; private set; }

        public DiceFace(int baseValue, Vector3 direction)
        {
            BaseValue = baseValue;
            Direction = direction;
        }
    }
}