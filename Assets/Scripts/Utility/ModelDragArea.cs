using _Scripts.Utility;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Scripts.UI.Workshop
{
    public class ModelDragArea : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private ModelRotator _rotator;
        
        public void OnBeginDrag(PointerEventData eventData) => _rotator.BeginDrag();
        public void OnDrag(PointerEventData eventData) => _rotator.Drag(eventData.delta);
        public void OnEndDrag(PointerEventData eventData) => _rotator.EndDrag();
    }
}