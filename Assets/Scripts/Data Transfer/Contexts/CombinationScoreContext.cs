using System.Collections.Generic;
using _Scripts.Combinations;
using _Scripts.Dices;
using _Scripts.UI.Charms;

namespace _Scripts.Data_Transfer.Contexts
{
    public class CombinationScoreContext
    {
        //In:
        public CombinationConfig Combination { get; }
        public IReadOnlyList<Dice> SelectedDices { get; }
        
        //Out:
        public List<CharmTriggerResult> Triggers { get; } = new();

        public CombinationScoreContext(CombinationConfig combination, IReadOnlyList<Dice> selectedDices)
        {
            Combination = combination;
            SelectedDices = selectedDices;
        }
        
        public void AddTrigger(CharmEquippedView view, int points = 0, int multiplier = 0, float xMultiplier = 1f)
        {
            Triggers.Add(new(view, points, multiplier, xMultiplier));
        }
    }
}