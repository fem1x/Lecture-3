using UnityEngine;
using _Scripts.UI;

namespace _Scripts.Utility.Extensions
{
    public static class RectTransformExtensions
    {
        public static Vector2 GetHiddenPosition(
            this RectTransform rect,
            SlideDirection direction,
            Vector2 shownAnchoredPos,
            RectTransform boundsRect = null)
        {
            Canvas.ForceUpdateCanvases();

            var referenceRect = boundsRect;
            if (referenceRect == null)
            {
                var canvas = rect.GetComponentInParent<Canvas>(true);
                referenceRect = canvas.rootCanvas.GetComponent<RectTransform>();
            }

            var parentRect = rect.parent as RectTransform;

            var prevPos = rect.anchoredPosition;
            rect.anchoredPosition = shownAnchoredPos;

            var panelBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(referenceRect, rect);
            rect.anchoredPosition = prevPos;

            var shiftX = 0f;
            var shiftY = 0f;

            switch (direction)
            {
                case SlideDirection.Right:
                    shiftX = referenceRect.rect.xMax - panelBounds.min.x;
                    break;
                case SlideDirection.Left:
                    shiftX = referenceRect.rect.xMin - panelBounds.max.x;
                    break;
                case SlideDirection.Top:
                    shiftY = referenceRect.rect.yMax - panelBounds.min.y;
                    break;
                case SlideDirection.Bottom:
                    shiftY = referenceRect.rect.yMin - panelBounds.max.y;
                    break;
            }

            var scaleRatio = new Vector2(
                parentRect.lossyScale.x / referenceRect.lossyScale.x,
                parentRect.lossyScale.y / referenceRect.lossyScale.y
            );

            return shownAnchoredPos + new Vector2(shiftX / scaleRatio.x, shiftY / scaleRatio.y);
        }
    }
}