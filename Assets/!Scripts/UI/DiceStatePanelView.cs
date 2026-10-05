using System.Collections.Generic;
using _Scripts.Dices;
using UnityEngine;

namespace _Scripts.UI
{
    public class DiceStatePanelView : MonoBehaviour
    {
        [SerializeField] private List<DiceStateView> _diceStateViews;

        public void InitDiceStateViews(List<Dice> dices)
        {
            for (int i = 0; i < _diceStateViews.Count; i++)
            {
                _diceStateViews[i].Bind(dices[i].Data);
            }
        }
    }
}