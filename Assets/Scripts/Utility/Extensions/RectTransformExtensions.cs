using UnityEngine;
using _Scripts.UI;

namespace _Scripts.Utility.Extensions
{
    public static class RectTransformExtensions
    {
        public static Vector2 GetHiddenPosition(this RectTransform rect, SlideDirection direction, Vector2 shownAnchoredPos)
        {
            Canvas.ForceUpdateCanvases();

            var canvas = rect.GetComponentInParent<Canvas>(true);
            if (canvas == null) return shownAnchoredPos;

            var rootCanvas = canvas.rootCanvas;
            var canvasRect = rootCanvas.GetComponent<RectTransform>();
            var parentRect = rect.parent as RectTransform;

            var prevPos = rect.anchoredPosition;
            rect.anchoredPosition = shownAnchoredPos;

            var panelBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRect, rect);
            rect.anchoredPosition = prevPos;

            var shiftX = 0f;
            var shiftY = 0f;

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

            var scaleRatio = new Vector2(
                parentRect.lossyScale.x / canvasRect.lossyScale.x,
                parentRect.lossyScale.y / canvasRect.lossyScale.y
            );

            return shownAnchoredPos + new Vector2(shiftX / scaleRatio.x, shiftY / scaleRatio.y);
        }
    }
}