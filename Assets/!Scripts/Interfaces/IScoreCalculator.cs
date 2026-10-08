using System.Collections.Generic;
using _Scripts.Charms;
using _Scripts.Dices;

namespace _Scripts.Interfaces
{
    public interface IScoreCalculator
    {
        public ScoreCalculationResult Calculate(List<Dice> selectedDices);
    }
}