using System.Collections.Generic;
using _Scripts.Charms;
using _Scripts.UI.Charms;

namespace _Scripts.Structs___Enums.Contexts
{
    public class FinalizeScoreContext
    {
        //In:
        public int CurrentPoints { get; }
        public int CurrentMultiplier { get; }
        
        //Out:
        public List<CharmTriggerResult> Triggers { get; } = new();
        
        public FinalizeScoreContext(int currentPoints, int currentMultiplier)
        {
            CurrentPoints = currentPoints;
            CurrentMultiplier = currentMultiplier;
        }
        
        public void AddTrigger(CharmView view, int points, int multiplier)
        {
            Triggers.Add(new CharmTriggerResult(view, points, multiplier));
        }
    }
}