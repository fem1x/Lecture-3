using System.Collections.Generic;
using _Scripts.Combinations;
using _Scripts.Dices;

namespace _Scripts.Structs___Enums.Contexts
{
    public class CombinationScoreContext
    {
        public CombinationConfig Combination { get; }
        public IReadOnlyList<Dice> SelectedDices { get; }

        public int BonusBasePoints { get; set; }
        public int BonusMultiplier { get; set; }

        public CombinationScoreContext(CombinationConfig combination, IReadOnlyList<Dice> selectedDices)
        {
            Combination = combination;
            SelectedDices = selectedDices;
        }
    }
}