using System.Collections.Generic;
using _Scripts.Combinations;
using _Scripts.Dices;
using _Scripts.UI.Charms;

namespace _Scripts.Data_Transfer.Contexts
{
    public class DiceScoreContext
    {
        //In:
        public Dice Dice { get; }
        public CombinationConfig Combination { get; }

        //Out:
        public int BaseDicePoints { get; }
        public List<CharmTriggerResult> Triggers { get; } = new();

        public DiceScoreContext(Dice dice, CombinationConfig combination)
        {
            Dice = dice;
            Combination = combination;
            BaseDicePoints = dice.Data.RolledValue;
        }
        
        public void AddTrigger(CharmEquippedView view, int points = 0, int multiplier = 0, float xMultiplier = 1f)
        {
            Triggers.Add(new(view, points, multiplier, xMultiplier));
        }
    }
}