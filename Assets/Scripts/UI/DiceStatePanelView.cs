using System.Collections.Generic;
using System.Threading;
using _Scripts.Dices;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.UI
{
    public class DiceStatePanelView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private RectTransform _rect;
        [SerializeField] private List<DiceStateView> _diceStateViews;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
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
        
        public Tween MoveToPositionTween(Vector3 targetPosition, float duration, Ease ease)
        {
            _rect.DOKill();
            return _rect.DOMove(targetPosition, duration).SetEase(ease);
        }

        public Tween ResetPositionTween(float duration, Ease ease)
        {
            _rect.DOKill();
            return _rect.DOAnchorPos(Vector2.zero, duration).SetEase(ease);
        }
    }
}