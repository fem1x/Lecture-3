using System.Collections.Generic;
using _Scripts.Charms;
using _Scripts.Combinations;
using _Scripts.Dices;
using _Scripts.UI.Charms;

namespace _Scripts.Structs___Enums.Contexts
{
    public class CombinationScoreContext
    {
        //In:
        public CombinationConfig Combination { get; }
        
        //Out:
        public List<CharmTriggerResult> Triggers { get; } = new();

        public CombinationScoreContext(CombinationConfig combination)
        {
            Combination = combination;
        }
        
        public void AddTrigger(CharmView view, int points, int multiplier)
        {
            Triggers.Add(new CharmTriggerResult(view, points, multiplier));
        }
    }
}