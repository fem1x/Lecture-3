using System.Collections.Generic;
using _Scripts.Charms;
using _Scripts.Combinations;
using _Scripts.Dices;

namespace _Scripts.Structs___Enums.Contexts
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
        
        public void AddTrigger(CharmView view, int points, int multiplier)
        {
            Triggers.Add(new CharmTriggerResult(view, points, multiplier));
        }
    }
}