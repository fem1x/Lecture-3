using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public sealed class CycleButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private ButtonScaleView _feedback;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake() => _feedback = transform.parent.GetComponent<ButtonScaleView>();

        public void OnPointerDown(PointerEventData eventData)
        {
            _feedback.OnPointerDown(eventData);
            _feedback.transform.Find("Press Hit Area").GetComponent<Image>().raycastTarget = false;
        }

        public void OnPointerUp(PointerEventData eventData) => _feedback.OnPointerUp(eventData);
    }
}
