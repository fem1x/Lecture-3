using System.Collections.Generic;
using _Scripts.Dices;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.UI
{
    public class DiceStatePanelView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private RectTransform _rect;
        [SerializeField] private List<DiceStateView> _diceStateViews;
        
        private Transform _defaultParent;
        
        // Отдаем Y-координату родного холдера, чтобы мастерская знала, когда нас цеплять
        public float HolderY => _defaultParent.position.y;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        private void Awake()
        {
            _defaultParent = transform.parent;
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
            // true сохраняет мировую позицию. Это важно, так как WaitUntil может сработать 
            // между кадрами с небольшой погрешностью, и прямой сброс в Vector2.zero даст визуальный рывок.
            _rect.SetParent(slotTarget, true); 
            // Мягко и незаметно доводим панель ровно в центр слота (магнитный эффект)
            _rect.DOLocalMove(Vector3.zero, 0.1f).SetEase(Ease.OutSine);
        }

        public void Drop()
        {
            _rect.DOKill();
            _rect.SetParent(_defaultParent, true);
            // Доводим в ноль родного холдера
            _rect.DOAnchorPos(Vector2.zero, 0.1f).SetEase(Ease.OutSine);
        }
    }
}