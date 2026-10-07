using System.Collections.Generic;
using _Scripts.Dices;

namespace _Scripts.Combinations
{
    public class DiceListAnalyzer
    {
        public int[] Frequencies { get; private set; } = new int[7];
        public int TotalSum { get; private set; }
        public int ValidDiceCount { get; private set; }

        public DiceListAnalyzer(IReadOnlyList<Dice> dices)
        {
            if (dices == null) return;

            for (int i = 0; i < dices.Count; i++)
            {
                var dice = dices[i];
                if (dice == null) continue;

                var val = dice.Data.RolledValue;
                if (val >= 1 && val <= 6)
                {
                    Frequencies[val]++;
                    TotalSum += val;
                    ValidDiceCount++;
                }
            }
        }

        public bool HasFrequency(int targetCount)
        {
            for (int i = 1; i <= 6; i++)
            {
                if (Frequencies[i] >= targetCount)
                    return true;
            }
            return false;
        }
    }
}