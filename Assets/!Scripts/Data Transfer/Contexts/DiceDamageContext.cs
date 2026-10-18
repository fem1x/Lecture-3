using System.Collections.Generic;
using _Scripts.Dices;

namespace _Scripts.Data_Transfer.Contexts
{
    public class DiceDamageContext
    {
        //In:
        public Dice TargetDice { get; }
        public IReadOnlyList<Dice> PlayedDices { get; }
        
        //Out:
        public int DamageAmount { get; set; }
        public bool IsCancelled { get; private set; }

        public DiceDamageContext(Dice targetDice, int damageAmount, IReadOnlyList<Dice> playedDices)
        {
            TargetDice = targetDice;
            DamageAmount = damageAmount;
            PlayedDices = playedDices;
        }

        public void CancelDamage() => IsCancelled = true;
    }
}