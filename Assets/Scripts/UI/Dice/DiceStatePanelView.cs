using System.Collections.Generic;
using _Scripts.Dices;
using UnityEngine;
using VContainer;

namespace _Scripts.UI
{
    public class DiceStatePanelView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private List<DiceStateView> _diceStateViews = new();

        private DiceInspectorView _inspectorView;

        [Inject]
        public void Construct(DiceInspectorView inspectorView) => _inspectorView = inspectorView;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void Awake()
        {
            foreach (var view in _diceStateViews)
                view.OnInspectRequested += data => _inspectorView.Open(data);
        }

        public void InitDiceStateViews(List<Dice> dices)
        {
            for (var i = 0; i < dices.Count; i++)
                _diceStateViews[i].Bind(dices[i].Data);
        }
    }
}