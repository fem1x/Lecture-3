using System.Collections.Generic;
using _Scripts.Dices;

namespace _Scripts.Data_Transfer.Contexts
{
    public readonly struct DamageStep
    {
        public readonly Dice TargetDice;
        public readonly int DamageTaken;
        public readonly bool IsDegraded;

        public DamageStep(Dice targetDice, int damageTaken, bool isDegraded)
        {
            TargetDice = targetDice;
            DamageTaken = damageTaken;
            IsDegraded = isDegraded;
        }
    }

    public class DamageSequencePlan
    {
        public readonly List<DamageStep> Steps = new();
    }
}