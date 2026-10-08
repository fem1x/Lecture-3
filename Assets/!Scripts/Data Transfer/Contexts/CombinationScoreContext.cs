using System.Collections.Generic;
using _Scripts.Combinations;
using _Scripts.Dices;

namespace _Scripts.Structs___Enums.Contexts
{
    public class CombinationScoreContext
    {
        //In:
        public CombinationConfig Combination { get; }
        public IReadOnlyList<Dice> SelectedDices { get; }

        //Out:
        public int BonusPoints { get; set; }
        public int BonusMultiplier { get; set; }

        public CombinationScoreContext(CombinationConfig combination, IReadOnlyList<Dice> selectedDices)
        {
            Combination = combination;
            SelectedDices = selectedDices;
        }
    }
}