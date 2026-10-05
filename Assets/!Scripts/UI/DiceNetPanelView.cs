using System.Collections.Generic;
using _Scripts.Dices;
using UnityEngine;
using VContainer;

namespace _Scripts.UI
{
    public class DiceNetPanelView : MonoBehaviour
    {
        [SerializeField] private List<DiceNetView> _diceNetViews;

        public void InitNetViews(List<Dice> dices)
        {
            for (int i = 0; i < _diceNetViews.Count && i < dices.Count; i++)
            {
                _diceNetViews[i].Bind(dices[i].Data);
            }
        }
    }
}