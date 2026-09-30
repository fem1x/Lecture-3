using System;

namespace _Scripts.Dices
{
    [Serializable]
    public struct CombinationData
    {
        public CombinationType CombinationType;
        public int BasePoints;
        public int Multiplier;
        public int DiceRefund;
    }
}