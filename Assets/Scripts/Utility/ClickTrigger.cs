using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Scripts.UI
{
    public class ClickTrigger : MonoBehaviour, IPointerClickHandler
    {
        public event Action Clicked;

        public void OnPointerClick(PointerEventData eventData) => Clicked?.Invoke();
    }
}