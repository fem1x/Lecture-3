using UnityEngine;

namespace _Scripts.Utility.Extensions
{
    public static class RectTransformExtensions
    {
        public static Vector2 GetHiddenPosition(this RectTransform rect, bool slideUp)
        {
            var canvas = rect.GetComponentInParent<Canvas>().rootCanvas;
            var canvasRect = (RectTransform)canvas.transform;

            var edgeY = slideUp ? canvasRect.rect.yMax : canvasRect.rect.yMin;
            var canvasEdge = canvasRect.TransformPoint(new Vector3(0f, edgeY));

            var parent = (RectTransform)rect.parent;
            var localEdge = parent.InverseTransformPoint(canvasEdge);

            var pivotOffset = rect.rect.height * (rect.pivot.y - (slideUp ? 0f : 1f));

            return new Vector2(rect.anchoredPosition.x, localEdge.y + pivotOffset);
        }
    }
}