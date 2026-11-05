using UnityEngine;

namespace _Scripts.Utility.Extensions
{
    public static class CanvasGroupExtensions
    {
        public static void Show(this CanvasGroup group)
        {
            group.alpha = 1;
            group.blocksRaycasts = true;
            group.interactable = true;
        }

        public static void Hide(this CanvasGroup group)
        {
            group.alpha = 0;
            group.blocksRaycasts = false;
            group.interactable = false;
        }
        
        public static void SetState(this CanvasGroup group, bool isActive)
        {
            group.alpha = isActive ? 1f : 0f;
            group.blocksRaycasts = isActive;
            group.interactable = isActive;
        }
    }
}