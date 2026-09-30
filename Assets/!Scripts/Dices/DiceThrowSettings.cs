using System;
using UnityEngine;

namespace _Scripts.Dices
{
    [Serializable]
    public struct DiceThrowSettings
    {
        public Vector3 Direction;
        public float DirectionRandomize;
        public float Force;
        public float ForceRandomize;
        public float Torque;
        public float TorqueRandomize;
        public float TimeBetweenThrows;
    }
}