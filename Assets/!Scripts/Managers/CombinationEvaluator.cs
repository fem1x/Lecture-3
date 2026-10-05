using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Dices;
using MoreLinq;

namespace _Scripts.Managers
{
    public class CombinationEvaluator
    {
        private readonly int[] _frequencies = new int[7];

        public List<FoundCombination> FindAllCombinations(List<Dice> tableDices)
        {
            var allFound = new List<FoundCombination>();
    
            for (int size = Math.Min(5, tableDices.Count); size >= 2; size--)
            {
                foreach (var subset in tableDices.Subsets(size))
                {
                    var diceList = subset.ToList();
                    var type = Evaluate(diceList);

                    if (type == CombinationType.HighestDice)
                        continue;

                    allFound.Add(new FoundCombination(type, diceList));
                }
            }

            //HighestDice
            if (tableDices.Count > 0)
            {
                var bestDice = tableDices.OrderByDescending(d => d.Data.RolledValue).First();
                allFound.Add(new FoundCombination(CombinationType.HighestDice, new List<Dice> { bestDice }));
            }

            //Best of each type
            return allFound
                .GroupBy(combo => combo.Type)
                .Select(group 
                    => group.OrderByDescending(c 
                        => c.Dices.Sum(d 
                            => d.Data.RolledValue)).First())
                .OrderByDescending(c => c.Type)
                .ToList();
        }
        
        
        public CombinationType Evaluate(List<Dice> dices)
        {
            var validDices = dices.Where(d => d != null && d.Data.RolledValue > 0).ToList();
            var count = validDices.Count;
            var valuesSum = ProcessDiceValues(validDices);

            if (count == 0) return CombinationType.HighestDice;
            switch (count)
            {
                case 5:
                    if (HasFrequency(5)) return CombinationType.Singularity;
                    if (valuesSum <= 9) return CombinationType.Underload;
                    if (valuesSum >= 24) return CombinationType.Overload;
                    if (IsSequence(5)) return CombinationType.FullSequence;
                    if (IsMonochrome()) return CombinationType.PureMonochrome;
                    break;
                
                case 4:
                    if (HasFrequency(4)) return CombinationType.Quad;
                    if (IsMonochrome()) return CombinationType.Monochrome;
                    if (IsBalance()) return CombinationType.Balance;
                    if (IsCross()) return CombinationType.Cross;
                    if (IsSequence(4)) return CombinationType.Sequence;
                    break;
                
                case 3:
                    if (HasFrequency(3)) return CombinationType.Triad;
                    break;

                case 2:
                    if (IsOpposites()) return CombinationType.Opposites;
                    if (HasFrequency(2)) return CombinationType.Duplet;
                    break;

                case 1:
                    return CombinationType.HighestDice;
            }
            
            return CombinationType.HighestDice;
        }

        /// <summary>
        /// Returns total values sum and fills _valueFrequencies array
        /// </summary>
        private int ProcessDiceValues(List<Dice> dices)
        {
            Array.Clear(_frequencies, 0, _frequencies.Length);
            
            var sum = 0;
            for (int i = 0; i < dices.Count; i++)
            {
                var val = dices[i].Data.RolledValue;
                if (val >= 1 && val <= 6)
                {
                    _frequencies[val]++;
                    sum += val;
                }
            }

            return sum;
        }

        
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        
        #region Combination Checks
        private bool HasFrequency(int targetFreq)
        {
            for (int i = 1; i < 7; i++)
                if (_frequencies[i] >= targetFreq)
                    return true;
            
            return false;
        }

        private bool IsMonochrome()
        {
            var evenCount = _frequencies[2] + _frequencies[4] + _frequencies[6];
            var oddCount = _frequencies[1] + _frequencies[3] + _frequencies[5];
            return evenCount == 0 ||  oddCount == 0;
        }

        private bool IsBalance()
        {
            int evenSum = (_frequencies[2] * 2) + (_frequencies[4] * 4) + (_frequencies[6] * 6);
            int oddSum = (_frequencies[1] * 1) + (_frequencies[3] * 3) + (_frequencies[5] * 5);
            return evenSum == oddSum;
        }

        private bool IsSequence(int targetLength)
        {
            int maxLength = 0;
            for (int i = 1; i <= 6; i++)
            {
                if (_frequencies[i] == 1)
                {
                    maxLength++;
                    if (maxLength == targetLength) return true;
                }
                else
                {
                    maxLength = 0;
                }
            }
            return false;
        }

        private bool IsCross()
        {
            int oppositePairs = 0;
            if (_frequencies[1] == 1 && _frequencies[6] == 1) oppositePairs++;
            if (_frequencies[2] == 1 && _frequencies[5] == 1) oppositePairs++;
            if (_frequencies[3] == 1 && _frequencies[4] == 1) oppositePairs++;

            return oppositePairs == 2;
        }
        
        private bool IsOpposites()
        {
            return (_frequencies[1] == 1 && _frequencies[6] == 1) ||
                   (_frequencies[2] == 1 && _frequencies[5] == 1) ||
                   (_frequencies[3] == 1 && _frequencies[4] == 1);
        }
        #endregion
    }
}