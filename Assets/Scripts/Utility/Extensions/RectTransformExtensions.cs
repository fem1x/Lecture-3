using UnityEngine;
using _Scripts.UI;

namespace _Scripts.Utility.Extensions
{
    public static class RectTransformExtensions
    {
        public static Vector2 GetHiddenPosition(this RectTransform rect, SlideDirection direction)
        {
            var canvas = rect.GetComponentInParent<Canvas>();
            if (canvas == null) return Vector2.zero;

            var canvasRect = canvas.GetComponent<RectTransform>();
            
            Bounds panelBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRect, rect);
            
            float shiftX = 0f;
            float shiftY = 0f;

            switch (direction)
            {
                case SlideDirection.Right:
                    shiftX = canvasRect.rect.xMax - panelBounds.min.x;
                    break;
                case SlideDirection.Left:
                    shiftX = canvasRect.rect.xMin - panelBounds.max.x;
                    break;
                case SlideDirection.Top:
                    shiftY = canvasRect.rect.yMax - panelBounds.min.y;
                    break;
                case SlideDirection.Bottom:
                    shiftY = canvasRect.rect.yMin - panelBounds.max.y;
                    break;
            }
            
            return new Vector2(shiftX, shiftY);
        }
    }
}