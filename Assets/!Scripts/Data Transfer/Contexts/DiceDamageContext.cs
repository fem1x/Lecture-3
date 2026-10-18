using System.Collections.Generic;
using _Scripts.Dices;

namespace _Scripts.Data_Transfer.Contexts
{
    public class DiceDamageContext
    {
        //In:
        public Dice Dice { get; }
        public IReadOnlyList<Dice> PlayedDices { get; }
        
        //Out:
        public int DamageAmount { get; set; }
        public bool IsCancelled { get; private set; }
        public bool ShouldForceDegrade { get; private set; }

        public DiceDamageContext(Dice dice, int damageAmount, IReadOnlyList<Dice> playedDices)
        {
            Dice = dice;
            DamageAmount = damageAmount;
            PlayedDices = playedDices;
        }

        public void CancelDamage() => IsCancelled = true;
        public void  ForceDegrade() => ShouldForceDegrade = true;
    }
}