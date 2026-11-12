using _Scripts.Dices;

namespace _Scripts.Data_Transfer.Contexts
{
    public enum DamageOutcome
    {
        None,
        Damaged,
        Degraded,
        Blocked
    }

    public readonly struct DamageStep
    {
        public readonly Dice TargetDice;
        public readonly DamageOutcome Outcome;

        public DamageStep(Dice targetDice, DamageOutcome outcome)
        {
            TargetDice = targetDice;
            Outcome = outcome;
        }
    }
}