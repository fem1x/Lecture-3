using System.Collections.Generic;
using System.Threading;
using _Scripts.Dices;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class DiceStatePanelView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private List<DiceStateView> _diceStateViews;
        
        private Vector3 _defaultPosition;
        private RectTransform _rectTransform;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _defaultPosition = _rectTransform.position;
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
        
        public Tween MoveToAnchorTween(Transform targetSlot, float duration, Ease ease)
        {
            _rectTransform.DOKill();
            return _rectTransform.DOMove(targetSlot.position, duration).SetEase(ease);
        }

        public Tween ResetPositionTween(float duration, Ease ease)
        {
            _rectTransform.DOKill();
            return _rectTransform.DOMove(_defaultPosition, duration).SetEase(ease);
        }
    }
}