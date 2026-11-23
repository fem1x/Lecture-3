using System;

namespace _Scripts.UI.Charms
{
    public class CharmDragState
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public event Action<bool> OnDragChanged;
        public bool IsDragging => _draggedCharm != null;

        private CharmDragView _draggedCharm;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public bool TryBegin(CharmDragView charm)
        {
            if (IsDragging) return false;

            _draggedCharm = charm;
            OnDragChanged?.Invoke(true);
            return true;
        }

        public void End(CharmDragView charm)
        {
            if (_draggedCharm != charm) return;

            _draggedCharm = null;
            OnDragChanged?.Invoke(false);
        }
    }
}
