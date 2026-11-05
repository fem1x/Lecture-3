using _Scripts.UI.Charms;
using UnityEngine;

namespace _Scripts.UI
{
    public class ConveyorCellView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public RectTransform CharmHolder { get; private set; }
        public CharmDragView CurrentCharm { get; private set; }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public void AttachCharm(CharmDragView card) => CurrentCharm = card;
        public void DetachCharm() => CurrentCharm = null;
        
        public void SetDraggable(bool isDraggable)
        {
            if (CurrentCharm != null)
                CurrentCharm.SetDraggable(isDraggable);
        }
    }
}