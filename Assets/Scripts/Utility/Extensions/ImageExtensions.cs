using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Utility.Extensions
{
    public static class ImageExtensions
    {
        public static void SetVisible(this Image image, bool isVisible)
        {
            var col =  image.color;
            var alpha = isVisible ? 1f : 0f;
            image.color = new Color(col.r, col.g, col.b, alpha);
        }
    }
}