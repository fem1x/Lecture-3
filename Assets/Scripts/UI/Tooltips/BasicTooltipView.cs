using TMPro;
using UnityEngine;

namespace _Scripts.UI.Tooltips
{
    public sealed class BasicTooltipView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        [SerializeField] private TMP_Text _content;
        [SerializeField] private RectTransform _arrow;

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void SetContent(string text, Vector2 direction)
        {
            _content.text = text;

            var nearEdge = (Vector2.one - direction) * 0.5f;
            ((RectTransform)transform).pivot = nearEdge;
            _arrow.anchorMin = nearEdge;
            _arrow.anchorMax = nearEdge;
            _arrow.anchoredPosition = Vector2.zero;
            _arrow.localRotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.up, -direction));
        }
    }
}
