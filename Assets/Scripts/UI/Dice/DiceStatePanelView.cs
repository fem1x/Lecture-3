using System.Collections.Generic;
using _Scripts.Dices;
using DG.Tweening;
using UnityEngine;
using VContainer;

namespace _Scripts.UI
{
    public class DiceStatePanelView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private RectTransform _rect;
        [SerializeField] private List<DiceStateView> _diceStateViews;

        private Transform _defaultParent;
        private DiceInspectorView _inspectorView;

        public float HolderY => _defaultParent.position.y;

        [Inject]
        public void Construct(DiceInspectorView inspectorView)
        {
            _inspectorView = inspectorView;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            _defaultParent = transform.parent;
            
            foreach (var view in _diceStateViews)
                view.OnInspectRequested += data => _inspectorView.Open(data);
        }

        public void InitDiceStateViews(List<Dice> dices)
        {
            for (int i = 0; i < dices.Count; i++)
                _diceStateViews[i].Bind(dices[i].Data);
        }

        public void SetRepairMode(bool enable)
        {
            for (int i = 0; i < _diceStateViews.Count; i++)
                _diceStateViews[i].SetRepairMode(enable);
        }

        public void GrabBy(Transform slotTarget)
        {
            _rect.DOKill();
            _rect.SetParent(slotTarget, true);
            _rect.DOLocalMove(Vector3.zero, 0.1f).SetEase(Ease.OutSine);
        }

        public void Drop()
        {
            _rect.DOKill();
            _rect.SetParent(_defaultParent, true);
            _rect.DOAnchorPos(Vector2.zero, 0.1f).SetEase(Ease.OutSine);
        }
    }
}