using System.Collections.Generic;
using _Scripts.Dices;
using _Scripts.UI.Charms;

namespace _Scripts.Data_Transfer.Contexts
{
    public class FinalizeScoreContext
    {
        // In:
        public int CurrentPoints { get; }
        public int CurrentMultiplier { get; }
        public int RemainingRerolls { get; }
        public int RemainingThrows { get; }
        public IReadOnlyList<Dice> AllTableDices { get; }
        public IReadOnlyList<Dice> SelectedDices { get; }
        
        // Out:
        public List<CharmTriggerResult> Triggers { get; } = new();
        
        public FinalizeScoreContext(
            int currentPoints, 
            int currentMultiplier, 
            int remainingRerolls, 
            int remainingThrows,
            IReadOnlyList<Dice> allTableDices,
            IReadOnlyList<Dice> selectedDices)
        {
            CurrentPoints = currentPoints;
            CurrentMultiplier = currentMultiplier;
            RemainingRerolls = remainingRerolls;
            RemainingThrows = remainingThrows;
            AllTableDices = allTableDices;
            SelectedDices = selectedDices;
        }
        
        public void AddTrigger(CharmEquippedView view, int points = 0, int multiplier = 0, float xMultiplier = 1f)
        {
            Triggers.Add(new(view, points, multiplier, xMultiplier));
        }
    }
}