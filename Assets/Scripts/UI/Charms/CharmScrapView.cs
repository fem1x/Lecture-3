using _Scripts.Charms;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI.Charms
{
    [RequireComponent(typeof(Image))]
    public class CharmScrapView : MonoBehaviour, IDropHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private CharmsConveyorService _conveyorService;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        [Inject]
        public void Construct(CharmsConveyorService conveyorService) => _conveyorService = conveyorService;

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null) return;
            if (!eventData.pointerDrag.TryGetComponent<CharmDragView>(out var dragView)) return;
            if (!dragView.IsDraggedBy(eventData.pointerId)) return;

            if (_conveyorService.TryScrap(dragView.GeneralView.Config))
                dragView.Consume();
        }
    }
}
